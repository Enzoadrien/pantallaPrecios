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

namespace Priceio.Cajero.Hopper
{
    internal class SMARTHopper
    {
        internal CHopper Hopper;
        private string ComPort;
        private byte SSPAddress;
        private int pollTimer = 250;
        private int reconnectionAttempts = 5;
        internal bool RunningHopper = false;
        internal string logPagoHopper = string.Empty;
        private Pago? pago;
        DispatcherTimer timer = new DispatcherTimer();

        internal SMARTHopper()
        {
            Hopper = new CHopper();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ComPort = config.AppSettings.Settings["COMHopper"].Value;
            SSPAddress = byte.Parse(config.AppSettings.Settings["SSPHopper"].Value);
            timer.Interval = TimeSpan.FromMilliseconds(pollTimer);
            timer.Tick += TimerTick;
        }

        internal void RunHooper()
        {
            Hopper.CommandStructure.ComPort = ComPort;
            Hopper.CommandStructure.SSPAddress = SSPAddress;
            Hopper.CommandStructure.Timeout = 2000;
            Hopper.CommandStructure.RetryLevel = 3;
            // First connect to the hopper
            if (ConnectToHopper(10, 3))
            {
                RunningHopper = true;

                logPagoHopper += "\r\nPoll Loop\r\n"
                + "*********************************\r\n";
            }
            Hopper.DisableCoinMech(logPagoHopper);
            // This loop won't run until the hopper is connected
            while (RunningHopper)
            {
                // poll the hopper
                if (!Hopper.DoPoll(logPagoHopper, ref pago))
                {
                    // If the poll fails, try to reconnect
                    logPagoHopper += "Attempting to reconnect...\r\n";
                    if (ConnectToHopper(reconnectionAttempts, 3))
                    {
                        // If it fails after 5 attempts, exit the loop
                        //RunningHopper = false;
                    }
                }
                timer.Start();

                while (timer.IsEnabled)
                {
                    Thread.Sleep(pollTimer); // Yield to free up CPU
                }
            }
            //close com port
            Hopper.SSPComms.CloseComPort();
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

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
        }

        internal void actualizaPago(ref Pago pPago)
        {
            pago = pPago;
        }

        private bool ConnectToHopper(int attempts, int interval)
        {

            // run for number of attempts specified
            for (int i = 0; i < attempts; i++)
            {

                // close com port in case it was open
                Hopper.SSPComms.CloseComPort();

                // turn encryption off for first stage
                Hopper.CommandStructure.EncryptionStatus = false;

                // if the key negotiation is successful then set the rest up
                if (Hopper.OpenComPort(logPagoHopper) && Hopper.NegotiateKeys(logPagoHopper) == true)
                {
                    Hopper.CommandStructure.EncryptionStatus = true; // now encrypting
                                                                     // find the max protocol version this hopper supports
                    byte maxPVersion = FindMaxProtocolVersionHopper();
                    if (maxPVersion >= 6)
                    {
                        Hopper.SetProtocolVersion(maxPVersion, logPagoHopper);
                    }
                    else
                    {
                        MessageBox.Show("This program does not support hoppers under protocol 6!", "ERROR");
                        return false;
                    }
                    // get info from the hopper and store useful vars
                    Hopper.HopperSetupRequest(logPagoHopper);
                    // Get serial number.
                    Hopper.GetSerialNumber(logPagoHopper);
                    // check unit is valid type
                    if (!IsUnitValidHopper(Hopper.UnitType))
                    {
                        MessageBox.Show("Unsupported type shown by SMART Hopper, this SDK supports the SMART Hopper only");
                        return false;
                    }
                    // inhibits, this sets which channels can receive coins
                    Hopper.SetInhibits(logPagoHopper);
                    // enable, this allows the hopper to operate
                    Hopper.EnableHopper(logPagoHopper);

                    //Hopper.SetHopperOptions(0x00, 0x01, 0x01, 0x01, textBox1); 

                    Hopper.SetHopperOptions(0x01, 0x01, 0x01, 0x00, logPagoHopper);


                    Hopper.GetHopperOptions(logPagoHopper);

                    return true;
                }
            }
            return false;
        }

        private byte FindMaxProtocolVersionHopper()
        {
            // not dealing with protocol under level 6
            // attempt to set in hopper
            byte b = 0x06;
            while (true)
            {
                if (!Hopper.SetProtocolVersion(b) || b > 20)
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

        internal bool CalculatePayoutHopper(string amount, char[] currency)
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
            return Hopper.PayoutAmount(payoutAmount, currency, logPagoHopper);
        }
    }

}
