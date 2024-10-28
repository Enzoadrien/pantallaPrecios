using Precios_Turnos;
using Priceio.Cajero;
using Priceio.Cajero.Payout;
using Priceio.Cajero.VentanasCajero;
using Priceio.ClasesGenericas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Priceio.Cajero.ChannelData;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para Sobre.xaml
    /// </summary>
    public partial class ConfigCanales : Window
    {
        internal TipoSMART tipoSMART;
        internal HiloContolCajero hiloContolCajero;
        internal bool corriendo = false;

        private DispatcherTimer timer = new DispatcherTimer();
        private int pollTimer = 250;
        private bool runLog = false;
        private VentanaLog dialog;

        private int canales = 0;

        public ConfigCanales(TipoSMART pTipoSMART)
        {
            InitializeComponent();
            tipoSMART = pTipoSMART;

            timer.Interval = TimeSpan.FromMilliseconds(pollTimer);
            timer.Tick += new EventHandler(TimerTick);
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                Titulo.Content = "Configurar SMART Payout";
                hiloContolCajero = new HiloContolCajero();
                hiloContolCajero.iniciarPayout();
            }
            else
            {
                Titulo.Content = "Configurar SMART Hopper";
                hiloContolCajero = new HiloContolCajero();
                hiloContolCajero.iniciarHopper();
            }
            bloquearControles();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
        }

        internal void RacargarNiveles()
        {
            corriendo = true;
            while (corriendo)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Niveles.Items.Clear();
                }));
                if (tipoSMART == TipoSMART.PAYOUT)
                {
                    if (hiloContolCajero.smartPayout.ConfigCargada)
                    {
                        if (canales == 0)
                            desbloquearControles();
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (ChannelData d in hiloContolCajero.NivelesPayout())
                            {
                                string s = string.Empty;
                                s += (d.Value / 100f).ToString() + " " + d.Currency[0] + d.Currency[1] + d.Currency[2];
                                s += " [" + d.Level + "] = " + (d.Level * d.Value / 100f).ToString();
                                s += " " + d.Currency[0] + d.Currency[1] + d.Currency[2];
                                Niveles.Items.Add(s);
                            }
                        }));
                    }
                }
                else
                {
                    if (hiloContolCajero.smartHopper.ConfigCargada)
                    {
                        if (canales == 0)
                            desbloquearControles();
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (ChannelData d in hiloContolCajero.NivelesHopper())
                            {
                                string s = string.Empty;
                                s += (d.Value / 100f).ToString() + " " + d.Currency[0] + d.Currency[1] + d.Currency[2];
                                s += " [" + d.Level + "] = " + (d.Level * d.Value / 100f).ToString();
                                s += " " + d.Currency[0] + d.Currency[1] + d.Currency[2];
                                Niveles.Items.Add(s);
                            }
                        }));
                    }
                }
                Thread.Sleep(250);
            }
        }

        private void desbloquearControles()
        {
            if (tipoSMART == TipoSMART.PAYOUT)
                canales = hiloContolCajero.smartPayout.Payout.NumberOfChannels;
            else
                canales = hiloContolCajero.smartHopper.Hopper.NumberOfChannels;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (canales)
                {
                    case 1:
                        Ch1Min.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        break;
                    case 2:
                        Ch1Min.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        break;
                    case 3:
                        Ch1Min.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        break;
                    case 4:
                        Ch1Min.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        Ch4Min.IsEnabled = true;
                        chkCh4.IsEnabled = true;
                        break;
                    case 5:
                        Ch1Min.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        Ch4Min.IsEnabled = true;
                        chkCh4.IsEnabled = true;
                        Ch5Min.IsEnabled = true;
                        chkCh5.IsEnabled = true;
                        break;
                    case 6:
                        Ch1Min.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        Ch4Min.IsEnabled = true;
                        chkCh4.IsEnabled = true;
                        Ch5Min.IsEnabled = true;
                        chkCh5.IsEnabled = true;
                        Ch6Min.IsEnabled = true;
                        chkCh6.IsEnabled = true;
                        break;
                    default:
                        break;
                }
            }));
        }

        private void bloquearControles()
        {
            Ch1Min.IsEnabled = false;
            chkCh1.IsEnabled = false;
            Ch2Min.IsEnabled = false;
            chkCh2.IsEnabled = false;
            Ch3Min.IsEnabled = false;
            chkCh3.IsEnabled = false;
            Ch4Min.IsEnabled = false;
            chkCh4.IsEnabled = false;
            Ch5Min.IsEnabled = false;
            chkCh5.IsEnabled = false;
            Ch6Min.IsEnabled = false;
            chkCh6.IsEnabled = false;
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void chkCh_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox ck = ((CheckBox)sender);
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                // Get the data from the payout
                ChannelData d = new ChannelData();
                hiloContolCajero.smartPayout.Payout.GetDataByChannel(Int32.Parse(ck.Tag.ToString()), ref d);
                hiloContolCajero.smartPayout.Payout.ChangeNoteRoute(d.Value, d.Currency, false, ref hiloContolCajero.smartPayout.logPagoPayout);

            }
            else
                hiloContolCajero.smartHopper.Hopper.RouteChannelToStorage(Int32.Parse(ck.Tag.ToString()), ref hiloContolCajero.smartHopper.logPagoHopper);
        }

        private void chkCh_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox ck = ((CheckBox)sender);
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                // Get the data from the payout
                ChannelData d = new ChannelData();
                hiloContolCajero.smartPayout.Payout.GetDataByChannel(Int32.Parse(ck.Tag.ToString()), ref d);
                hiloContolCajero.smartPayout.Payout.ChangeNoteRoute(d.Value, d.Currency, true, ref hiloContolCajero.smartPayout.logPagoPayout);

            }
            else
                hiloContolCajero.smartHopper.Hopper.RouteChannelToCashbox(Int32.Parse(ck.Tag.ToString()), ref hiloContolCajero.smartHopper.logPagoHopper);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            corriendo = false;

            if (tipoSMART == TipoSMART.PAYOUT)
                hiloContolCajero.detenerPayout();
            else
                hiloContolCajero.detenerHopper();

            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            corriendo = false;
            runLog = false;

            if (tipoSMART == TipoSMART.PAYOUT)
                hiloContolCajero.detenerPayout();
            else
                hiloContolCajero.detenerHopper();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => RacargarNiveles());
        }

        private void btnRecargar_Click(object sender, RoutedEventArgs e)
        {
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                if (hiloContolCajero.smartPayout.Payout.ValidatorEnabled)
                {

                    if (hiloContolCajero.smartPayout.Payout.DisableValidator(ref hiloContolCajero.smartPayout.logPagoPayout))
                        btnRecargar.Content = "Recargar";
                }
                else
                    if (hiloContolCajero.smartPayout.Payout.EnableValidator(ref hiloContolCajero.smartPayout.logPagoPayout))
                        btnRecargar.Content = "Finalizar";
            }

            else
            {
                if (hiloContolCajero.smartHopper.Hopper.CoinMechEnabled)
                {
                    if (hiloContolCajero.smartHopper.Hopper.DisableCoinMech(ref hiloContolCajero.smartHopper.logPagoHopper))
                        btnRecargar.Content = "Recargar";
                }
                else
                    if (hiloContolCajero.smartHopper.Hopper.EnableCoinMech(ref hiloContolCajero.smartHopper.logPagoHopper))
                        btnRecargar.Content = "Finalizar";
            }
        }

        private void btnRestablecer_Click(object sender, RoutedEventArgs e)
        {
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                hiloContolCajero.smartPayout.Payout.Reset(ref hiloContolCajero.smartPayout.logPagoPayout);
                hiloContolCajero.smartPayout.Payout.SSPComms.CloseComPort();
            }
            else
            {
                hiloContolCajero.smartHopper.Hopper.Reset(ref hiloContolCajero.smartHopper.logPagoHopper);
                hiloContolCajero.smartHopper.Hopper.SSPComms.CloseComPort();
            }
        }

        private void btnVaciar_Click(object sender, RoutedEventArgs e)
        {
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                hiloContolCajero.smartPayout.Payout.SmartEmpty(ref hiloContolCajero.smartPayout.logPagoPayout);
            }
            else
            {
                hiloContolCajero.smartHopper.Hopper.SmartEmpty(ref hiloContolCajero.smartHopper.logPagoHopper);
            }
        }

        private void btnRetirar_Click(object sender, RoutedEventArgs e)
        {
            if (Retiro.Text.Length > 0)
            {
                int cantidad = int.Parse(Retiro.Text);
                if (cantidad > 0)
                {
                    bool seEntrego = false;
                    if (tipoSMART == TipoSMART.PAYOUT)
                    {
                        seEntrego = hiloContolCajero.smartPayout.CalculatePayout(cantidad.ToString(), CHelpers.moneda.ToCharArray());
                    }
                    else
                    {
                        seEntrego = hiloContolCajero.smartHopper.CalculatePayoutHopper(cantidad.ToString(), CHelpers.moneda.ToCharArray());
                    }
                    if (!seEntrego)
                    {
                        Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, false);
                        dialog.lblNombre.Content = "¡Advertencia!";
                        dialog.lblTexto.Text = "El cajero no puede entregar la cantidad indicada, por favor revise que los datos sean correctos";
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        dialog.ShowDialog();
                        FocusManager.SetFocusedElement(this, Retiro);
                    }

                }
                else
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Indique un valor diferente de cero en el retiro";
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.ShowDialog();
                    FocusManager.SetFocusedElement(this, Retiro);
                }
            }
            else
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Indique un valor en el retiro";
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
                FocusManager.SetFocusedElement(this, Retiro);
            }
        }

        private void chkLog_Checked(object sender, RoutedEventArgs e)
        {
            dialog = new VentanaLog(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = Mouse.GetPosition(this);

            // get the position within the container
            var mousePosition = this.PointToScreen(relativeCenterParent);

            if (mousePosition.Y + dialog.Height >= this.MaxHeight)
                dialog.Top = mousePosition.Y - dialog.Height;
            else
                dialog.Top = mousePosition.Y;

            if (mousePosition.X + dialog.Width >= this.MaxWidth)
                dialog.Left = mousePosition.X - dialog.Width;
            else
                dialog.Left = mousePosition.X;

            if (tipoSMART == TipoSMART.PAYOUT)
                dialog.Titulo.Content = "Log SMART Payout";
            else
                dialog.Titulo.Content = "Log SMART Hopper";
            dialog.Show();
            Task.Run(() => recargarlog(dialog));
        }

        internal void recargarlog(VentanaLog dialog)
        {
            runLog = true;
            while (runLog)
            {
                if (tipoSMART == TipoSMART.PAYOUT)
                    dialog.recargarlog(hiloContolCajero.smartPayout.logPagoPayout);
                else
                    dialog.recargarlog(hiloContolCajero.smartHopper.logPagoHopper);

                timer.Start();
                while (timer.IsEnabled)
                {
                    Thread.Sleep(1); // Yield to free up CPU
                }
            }
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                dialog.Close();
            }));
        }

        private void chkLog_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dialog != null)
            {
                runLog = false;
            }
        }
    }
}
