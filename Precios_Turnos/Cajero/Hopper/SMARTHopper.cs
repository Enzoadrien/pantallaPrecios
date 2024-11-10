using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Priceio.Cajero.Hopper
{
    internal class SMARTHopper
    {
        internal CHopper Hopper;
        private string? ComPort = "COM1";
        private byte SSPAddress = 16;
        private int pollTimer = 250;
        private int reconnectionAttempts = 10;
        internal bool RunningHopper = false;
        internal string logPagoHopper = string.Empty;
        private Pago? pago;
        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer reconnectionTimer = new DispatcherTimer();
        internal bool ConfigCargada = false;
        internal bool BoolResetHopper = false;
        internal bool BoolEnableCoinMech = false;
        internal bool BoolDisableCoinMech = false;
        internal bool BoolCalculatePayoutHopper = false;
        internal bool BoolCalculatePayoutDenomination = false;
        internal bool BoolEmptyCash = false;
        internal bool BoolRouteCash = false;
        internal bool BoolDisableRouteCash = false;
        internal int chanelRoute = 0;
        private string moneda = "MXN";

        internal SMARTHopper()
        {
            Hopper = new CHopper(); ConfiguracionCajero? configuracionCajero = new SQLiteClassManager().GetConfiguracionCajero();
            if (configuracionCajero != null)
            {
                ComPort = configuracionCajero.COMHopper;
                SSPAddress = byte.Parse(configuracionCajero.SSPHopper.ToString());
            }
            timer.Interval = TimeSpan.FromMilliseconds(pollTimer);
            timer.Tick += new EventHandler(TimerTick);
            reconnectionTimer.Tick += new EventHandler(reconnectionTimer_Tick);
            reconnectionTimer.Start();
        }

        internal async Task RunHopper()
        {
            Hopper.CommandStructure.ComPort = ComPort;
            Hopper.CommandStructure.SSPAddress = SSPAddress;
            Hopper.CommandStructure.Timeout = 2000;
            Hopper.CommandStructure.RetryLevel = 3;
            // First connect to the hopper
            if (ConnectToHopper(reconnectionAttempts, 3).Result)
            {
                RunningHopper = true;

                logPagoHopper += "\r\nPoll Loop\r\n"
                + "*********************************\r\n";
            }
            Hopper.DisableCoinMech(ref logPagoHopper);
            // This loop won't run until the hopper is connected
            while (RunningHopper)
            {
                if (BoolResetHopper)
                {
                    ConfigCargada = false;
                    ResetHopper();
                    BoolResetHopper = false;
                }
                if (BoolEnableCoinMech)
                {
                    EnableCoinMech();
                    BoolEnableCoinMech = false;
                }
                if (BoolDisableCoinMech)
                {
                    DisableCoinMech();
                    BoolDisableCoinMech = false;
                }
                if (BoolCalculatePayoutHopper)
                {
                    CalculatePayoutHopper(pago.MonedasCambio.ToString(), moneda.ToCharArray());
                    BoolCalculatePayoutHopper = false;
                }
                if (BoolCalculatePayoutDenomination)
                {
                    CalculatePayoutDenomination(pago.MonedasCambio.ToString(), pago.Canal, moneda.ToCharArray());
                    BoolCalculatePayoutDenomination = false;
                }
                if (BoolEmptyCash)
                {
                    EmptyCash();
                    BoolEmptyCash = false;
                }
                if (BoolRouteCash)
                {
                    RouteCash();
                    chanelRoute = 0;
                    BoolRouteCash = false;
                }
                if (BoolDisableRouteCash)
                {
                    DisableRouteCash();
                    chanelRoute = 0;
                    BoolDisableRouteCash = false;
                }
                // poll the hopper
                if (!Hopper.DoPoll(ref logPagoHopper, ref pago))
                {
                    ConfigCargada = false;
                    // If the poll fails, try to reconnect
                    logPagoHopper += "Attempting to reconnect...\r\n";
                    if (!ConnectToHopper(reconnectionAttempts, 3).Result)
                    {
                        // If it fails after 5 attempts, exit the loop
                        break;
                    }
                    logPagoHopper += "Reconnected\r\n";
                }
                timer.Start();

                while (timer.IsEnabled)
                {
                    await Task.Delay(1); // Yield to free up CPU
                }
            }
            //close com port
            Hopper.SSPComms.CloseComPort();
            if (RunningHopper)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Ocurrio un error en la conexion del SMART Hopper y se ha desabilitado, consulte al administrador.";
                    new Recursos().ventanaMensajesGrande800x600(dialog);
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.ShowDialog();
                }));
            }

            RunningHopper = false;
        }

        private async Task<bool> ConnectToHopper(int attempts, int interval)
        {
            reconnectionTimer.Interval = TimeSpan.FromMilliseconds(interval * 1000);

            // run for number of attempts specified
            for (int i = 0; i < attempts; i++)
            {
                reconnectionTimer.Start();
                // close com port in case it was open
                Hopper.SSPComms.CloseComPort();

                // turn encryption off for first stage
                Hopper.CommandStructure.EncryptionStatus = false;

                // if the key negotiation is successful then set the rest up
                if (Hopper.OpenComPort(ref logPagoHopper) && Hopper.NegotiateKeys(ref logPagoHopper) == true)
                {
                    Hopper.CommandStructure.EncryptionStatus = true; // now encrypting
                                                                     // find the max protocol version this hopper supports
                    byte maxPVersion = FindMaxProtocolVersionHopper();
                    if (maxPVersion >= 6)
                    {
                        Hopper.SetProtocolVersion(maxPVersion, ref logPagoHopper);
                    }
                    else
                    {
                        logPagoHopper += "This program does not support hoppers under protocol 6!\r\n";
                        //MessageBox.Show("This program does not support hoppers under protocol 6!", "ERROR");
                        return false;
                    }
                    // get info from the hopper and store useful vars
                    Hopper.HopperSetupRequest(ref logPagoHopper);
                    // Get serial number.
                    Hopper.GetSerialNumber(ref logPagoHopper);
                    // check unit is valid type
                    if (!IsUnitValidHopper(Hopper.UnitType))
                    {
                        logPagoHopper += "Unsupported type shown by SMART Hopper, this SDK supports the SMART Hopper only\r\n";
                        //MessageBox.Show("Unsupported type shown by SMART Hopper, this SDK supports the SMART Hopper only");
                        return false;
                    }
                    // inhibits, this sets which channels can receive coins
                    Hopper.SetInhibits(ref logPagoHopper);
                    // enable, this allows the hopper to operate
                    Hopper.EnableHopper(ref logPagoHopper);

                    //Hopper.SetHopperOptions(0x00, 0x01, 0x01, 0x01, textBox1); 

                    Hopper.SetHopperOptions(0x01, 0x01, 0x01, 0x00, ref logPagoHopper);


                    Hopper.GetHopperOptions(ref logPagoHopper);

                    ConfigCargada = true;
                    return true;
                }
                while (reconnectionTimer.IsEnabled)
                {
                    await Task.Delay(1); // Yield to free up CPU
                }
            }
            return false;
        }

        internal void detenerHopper()
        {
            RunningHopper = false;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void reconnectionTimer_Tick(object sender, EventArgs e)
        {
            reconnectionTimer.Stop();
        }

        internal void ActualizaPago(ref Pago pPago)
        {
            pago = pPago;
        }

        private byte FindMaxProtocolVersionHopper()
        {
            // not dealing with protocol under level 6
            // attempt to set in hopper
            byte b = 0x06;
            while (true)
            {
                if (!Hopper.SetProtocolVersion(b, ref logPagoHopper) || b > 20)
                    return 0x06; // return default
                // If it fails then it can't be set so fall back to previous iteration and return it
                if (Hopper.CommandStructure.ResponseData[0] == CCommands.SSP_RESPONSE_FAIL)
                    return --b;
                b++;
            }
        }

        private bool IsUnitValidHopper(char unitType)
        {
            if (unitType == (char)0x03) // 0x03 is Hopper, only Hopper supported here
                return true;
            return false;
        }

        private bool CalculatePayoutHopper(string amount, char[] currency)
        {
            // Split string by decimal point
            string[] s = amount.Split('.');
            string final = "";
            int payoutAmount = 0;

            // If there was a decimal point
            if (s.Length > 1)
            {
                // Add a trailing zero if necessary
                if (s[1].Length == 1)
                    s[1] += "0";
                // If more than 2 decimal places, cull end
                else if (s[1].Length > 2)
                    s[1] = s[1].Substring(0, 2);

                final += s[0] + s[1]; // Add to final result string
            }
            else
                final += s[0] + "00"; // Add two zeros if there is no decimal point entered

            try
            {
                // Parse it to a number
                payoutAmount = int.Parse(final);
            }
            catch
            {
                return false;
            }

            // Pay it out
            return Hopper.PayoutAmount(payoutAmount, currency, ref logPagoHopper);
        }

        private bool CalculatePayoutDenomination(string amount, int chanel, char[] currency)
        {
            bool payoutRequired = false;
            byte[] data = new byte[9]; // create to size of maximum possible
            byte length = 9;
            byte denomsToPayout = byte.Parse(amount);
            // For each denomination
            try
            {
                payoutRequired = true; // need to do a payout as there is now > 0 denoms

                // Number of this denomination to payout
                UInt16 numToPayout = UInt16.Parse(amount);
                byte[] b = CHelpers.ConvertIntToBytes(numToPayout);
                data[0] = b[0];
                data[1] = b[1];

                // Value of this denomination
                ChannelData d = Hopper.UnitDataList[chanel];
                b = CHelpers.ConvertIntToBytes(d.Value);
                data[2] = b[0];
                data[3] = b[1];
                data[4] = b[2];
                data[5] = b[3];

                // Currency of this denomination
                data[6] = (Byte)d.Currency[0];
                data[7] = (Byte)d.Currency[1];
                data[8] = (Byte)d.Currency[2];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                payoutRequired = false; // don't payout on exception
            }

            if (payoutRequired)
            {
                // Send payout command and shut this form
                return Hopper.PayoutByDenomination(denomsToPayout, data, length, ref logPagoHopper);
            }
            return false;
        }
        private void ResetHopper()
        {
            Hopper.Reset(ref logPagoHopper);
            Hopper.SSPComms.CloseComPort();
        }

        private void EnableCoinMech()
        {
            Hopper.EnableCoinMech(ref logPagoHopper);
        }

        private void DisableCoinMech()
        {
            Hopper.DisableCoinMech(ref logPagoHopper);
        }
        
        private void EmptyCash()
        {
            Hopper.SmartEmpty(ref logPagoHopper);
        }

        private void RouteCash()
        {

           Hopper.RouteChannelToStorage(chanelRoute, ref logPagoHopper);
        }

        private void DisableRouteCash()
        {
            Hopper.RouteChannelToCashbox(chanelRoute, ref logPagoHopper);
        }
    }

}
