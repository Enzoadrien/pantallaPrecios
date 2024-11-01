using Microsoft.VisualBasic.Logging;
using Org.BouncyCastle.Asn1.X509;
using Priceio.ClasesGenericas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Priceio.Cajero.Payout
{
    internal class SMARTPayout
    {
        internal CPayout Payout;
        private string ComPort;
        private byte SSPAddress;
        internal bool RunningPayout = false;
        private int pollTimer = 500; // timer in ms
        private int reconnectionAttempts = 5;
        internal string logPagoPayout = string.Empty;
        private Pago? pago;
        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer reconnectionTimer = new DispatcherTimer();
        internal bool ConfigCargada = false;
        internal bool BoolResetPayout = false;
        internal bool BoolEnablePayout = false;
        internal bool BoolDisablePayout = false;
        internal bool BoolCalculatePayout = false;
        internal bool BoolEmptyCash = false;
        private string moneda = "MXN";

        internal SMARTPayout()
        {
            Payout = new CPayout();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ComPort = config.AppSettings.Settings["COMNV22"].Value;
            SSPAddress = byte.Parse(config.AppSettings.Settings["SSPNV22"].Value);
            timer.Interval = TimeSpan.FromMilliseconds(pollTimer);
            timer.Tick += new EventHandler(TimerTick);
            reconnectionTimer.Tick += new EventHandler(reconnectionTimer_Tick);
        }

        internal async Task RunPayout()
        {
            Payout.CommandStructure.ComPort = ComPort;
            Payout.CommandStructure.SSPAddress = SSPAddress;
            Payout.CommandStructure.Timeout = 3000;
            // connect to validator
            if (ConnectToValidator(reconnectionAttempts, 2).Result)
            {
                RunningPayout = true;
                logPagoPayout += "\r\nPoll Loop\r\n*********************************\r\n";
                Payout.ConfigureBezel(0x00, 0x00, 0xFF, ref logPagoPayout);
            }
            Payout.DisableValidator(ref logPagoPayout);
            while (RunningPayout)
            {
                if (BoolResetPayout)
                {
                    ResetPayout();
                    BoolResetPayout = false;
                }
                if (BoolEnablePayout)
                {
                    EnableValidator();
                    BoolEnablePayout = false;
                }
                if (BoolDisablePayout)
                {
                    DisableValidator();
                    BoolDisablePayout = false;
                }
                if (BoolCalculatePayout)
                {
                    CalculatePayout(pago.BilletesCambio.ToString(), moneda.ToCharArray());
                    BoolCalculatePayout = false;
                }
                if (BoolEmptyCash)
                {
                    EmptyCash();
                    BoolEmptyCash = false;
                }
                // if the poll fails, try to reconnect
                if (!Payout.DoPoll(ref logPagoPayout, ref pago))
                {
                    ConfigCargada = false;
                    logPagoPayout += "Poll failed, attempting to reconnect...\r\n";
                    // attempt reconnect, pass over number of reconnection attempts
                    if (!ConnectToValidator(reconnectionAttempts, 2).Result)
                        break;
                    logPagoPayout += "Reconnected\r\n";
                }
                timer.Start();

                while (timer.IsEnabled)
                {
                    await Task.Delay(1);
                }
            }
            Payout.SSPComms.CloseComPort();
            if (RunningPayout)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Ocurrio un error en la conexion del SMART Payout y se ha desabilitado, consulte al administrador.";
                    new Recursos().ventanaMensajesGrande800x600(dialog);
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.ShowDialog();
                }));
            }
            RunningPayout = false;
        }

        private async Task<bool> ConnectToValidator(int attempts, int interval)
        {
            reconnectionTimer.Interval = TimeSpan.FromMilliseconds(interval * 1000);
            // run for number of attempts specified
            for (int i = 0; i < attempts; i++)
            {
                // close com port in case it was open
                Payout.SSPComms.CloseComPort();

                // turn encryption off for first stage
                Payout.CommandStructure.EncryptionStatus = false;

                // if the key negotiation is successful then set the rest up
                if (Payout.OpenComPort(ref logPagoPayout) && Payout.NegotiateKeys(ref logPagoPayout))
                {
                    Payout.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    byte maxPVersion = FindMaxProtocolVersionPayout();
                    if (maxPVersion >= 6)
                    {
                        Payout.SetProtocolVersion(maxPVersion, ref logPagoPayout);
                    }
                    else
                    {
                        logPagoPayout += "This program does not support slaves under protocol 6!\r\n";
                        //MessageBox.Show("This program does not support slaves under protocol 6!", "ERROR");
                        return false;
                    }
                    // get info from the validator and store useful vars
                    Payout.PayoutSetupRequest(ref logPagoPayout);
                    // check this unit is supported
                    if (!IsUnitValidPayout(Payout.UnitType))
                    {
                        logPagoPayout += "Unsupported type shown by SMART Payout, this SDK supports the SMART Payout only\r\n";
                        //MessageBox.Show("Unsupported type shown by SMART Payout, this SDK supports the SMART Payout only");
                        return false;
                    }
                    // inhibits, this sets which channels can receive notes
                    Payout.SetInhibits(ref logPagoPayout);
                    // Get serial number
                    Payout.GetSerialNumber(ref logPagoPayout);
                    // enable, this allows the validator to operate
                    Payout.EnableValidator(ref logPagoPayout);
                    // enable the payout system on the validator
                    Payout.EnablePayout(ref logPagoPayout);
                    ConfigCargada = true;
                    return true;
                }

                reconnectionTimer.Start();
                while (reconnectionTimer.IsEnabled)
                {
                    await Task.Delay(1);
                }
            }
            return false;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void reconnectionTimer_Tick(object sender, EventArgs e)
        {
            reconnectionTimer.Stop();
        }

        internal void detenerPayout()
        {
            RunningPayout = false;
        }

        internal void ActualizaPago(ref Pago pPago)
        {
            pago = pPago;
        }

        private byte FindMaxProtocolVersionPayout()
        {
            // not dealing with protocol under level 6
            // attempt to set in validator
            byte b = 0x06;
            while (true)
            {
                Payout.SetProtocolVersion(b, ref logPagoPayout);
                if (Payout.CommandStructure.ResponseData[0] == CCommands.SSP_RESPONSE_FAIL)
                    return --b;
                b++;

                // catch runaway
                if (b > 12)
                    return 0x06; // return default
            }
        }

        private bool IsUnitValidPayout(char unitType)
        {
            if (unitType == (char)0x06) // 0x06 is Payout, no other types supported by this program
                return true;
            return false;
        }

        private bool CalculatePayout(string amount, char[] currency)
        {
            string[] s = amount.Split('.');
            // only need to deal with whole numbers so we can always just deal
            // with s[0]
            int n = 0;
            try
            {
                n = int.Parse(s[0]) * 100; // Multiply by 100 for penny value
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "EXCEPTION");
                return false;
            }
            // Make payout
            return Payout.PayoutAmount(n, currency, ref logPagoPayout);
        }

        private void ResetPayout()
        {
            Payout.Reset(ref logPagoPayout);
            Payout.SSPComms.CloseComPort();
        }

        private void EnableValidator()
        {
            Payout.EnableValidator(ref logPagoPayout);
        }

        private void DisableValidator()
        {
            Payout.DisableValidator(ref logPagoPayout);
        }

        private void EmptyCash()
        {
            Payout.SmartEmpty(ref logPagoPayout);
        }
    }
}
