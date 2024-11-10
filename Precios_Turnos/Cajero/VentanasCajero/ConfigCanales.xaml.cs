using Precios_Turnos;
using Priceio.Cajero;
using Priceio.Cajero.Hopper;
using Priceio.Cajero.Payout;
using Priceio.Cajero.VentanasCajero;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
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
        internal bool corriendo = false;
        private SMARTPayout smartPayout;
        private SMARTHopper smartHopper;
        private DispatcherTimer timer = new DispatcherTimer();
        private int pollTimer = 250;
        private bool runLog = false;
        private VentanaLog dialog;

        private int canales = 0;
        private bool cargando = true;

        internal ConfigCanales(TipoSMART pTipoSMART, SMARTPayout pSmartPayout, SMARTHopper pSmartHopper)
        {
            smartPayout = pSmartPayout;
            smartHopper = pSmartHopper;
            InitializeComponent();
            tipoSMART = pTipoSMART;

            timer.Interval = TimeSpan.FromMilliseconds(pollTimer);
            timer.Tick += new EventHandler(TimerTick);
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                Titulo.Content = "Configurar SMART Payout";
                Task.Run( ()=> smartPayout.RunPayout());
            }
            else
            {
                Titulo.Content = "Configurar SMART Hopper";
                Task.Run(() => smartHopper.RunHopper());
            }
            bloquearControles();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
        }

        internal async Task RacargarNiveles()
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
                    if (smartPayout.ConfigCargada)
                    {
                        if (canales == 0)
                            desbloquearControles();
                        else
                            cargando = false;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (ChannelData d in smartPayout.Payout.UnitDataList)
                            {
                                string s = string.Empty;
                                s += (d.Value / 100f).ToString() + " " + d.Currency[0] + d.Currency[1] + d.Currency[2];
                                s += " [" + d.Level + "] = " + (d.Level * d.Value / 100f).ToString();
                                s += " " + d.Currency[0] + d.Currency[1] + d.Currency[2];
                                Niveles.Items.Add(s);
                                if (cargando)
                                {
                                    cbxDenominacion.Items.Add((d.Value / 100f).ToString() + " " + d.Currency[0] + d.Currency[1] + d.Currency[2]);
                                    cargarCheck(d.Channel, d.Recycling);
                                }
                            }
                        }));
                    }
                }
                else
                {
                    if (smartHopper.ConfigCargada)
                    {
                        if (canales == 0)
                            desbloquearControles();
                        else
                            cargando = false;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (ChannelData d in smartHopper.Hopper.UnitDataList)
                            {
                                string s = string.Empty;
                                s += (d.Value / 100f).ToString() + " " + d.Currency[0] + d.Currency[1] + d.Currency[2];
                                s += " [" + d.Level + "] = " + (d.Level * d.Value / 100f).ToString();
                                s += " " + d.Currency[0] + d.Currency[1] + d.Currency[2];
                                Niveles.Items.Add(s);
                                if (cargando)
                                {
                                    cbxDenominacion.Items.Add((d.Value / 100f).ToString() + " " + d.Currency[0] + d.Currency[1] + d.Currency[2]);
                                    cargarCheck(d.Channel, d.Recycling);
                                }
                            }
                        }));
                    }
                }
                await Task.Delay(250);
            }
        }

        private void cargarCheck(int canal, bool activar)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                switch (canal)
                {
                    case 1:
                        chkCh1.IsChecked = activar;
                        break;
                    case 2:
                        chkCh2.IsChecked = activar;
                        break;
                    case 3:
                        chkCh3.IsChecked = activar;
                        break;
                    case 4:
                        chkCh4.IsChecked = activar;
                        break;
                    case 5:
                        chkCh5.IsChecked = activar;
                        break;
                    case 6:
                        chkCh6.IsChecked = activar;
                        break;
                    case 7:
                        chkCh6.IsChecked = activar;
                        break;
                    case 8:
                        chkCh6.IsChecked = activar;
                        break;
                    default:
                        break;
                }
            }));
        }

        private void desbloquearControles()
        {
            if (tipoSMART == TipoSMART.PAYOUT)
                canales = smartPayout.Payout.NumberOfChannels;
            else
                canales = smartHopper.Hopper.NumberOfChannels;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (canales)
                {
                    case 1:
                        Ch1Min.IsEnabled = true;
                        Ch1Max.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        break;
                    case 2:
                        Ch1Min.IsEnabled = true;
                        Ch1Max.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        Ch2Max.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        break;
                    case 3:
                        Ch1Min.IsEnabled = true;
                        Ch1Max.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        Ch2Max.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        Ch3Max.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        break;
                    case 4:
                        Ch1Min.IsEnabled = true;
                        Ch1Max.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        Ch2Max.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        Ch3Max.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        Ch4Min.IsEnabled = true;
                        Ch4Max.IsEnabled = true;
                        chkCh4.IsEnabled = true;
                        break;
                    case 5:
                        Ch1Min.IsEnabled = true;
                        Ch1Max.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        Ch2Max.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        Ch3Max.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        Ch4Min.IsEnabled = true;
                        Ch4Max.IsEnabled = true;
                        chkCh4.IsEnabled = true;
                        Ch5Min.IsEnabled = true;
                        Ch5Max.IsEnabled = true;
                        chkCh5.IsEnabled = true;
                        break;
                    case 6:
                        Ch1Min.IsEnabled = true;
                        Ch1Max.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        Ch2Max.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        Ch3Max.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        Ch4Min.IsEnabled = true;
                        Ch4Max.IsEnabled = true;
                        chkCh4.IsEnabled = true;
                        Ch5Min.IsEnabled = true;
                        Ch5Max.IsEnabled = true;
                        chkCh5.IsEnabled = true;
                        Ch6Min.IsEnabled = true;
                        Ch6Max.IsEnabled = true;
                        chkCh6.IsEnabled = true;
                        break;
                    case 7:
                        Ch1Min.IsEnabled = true;
                        Ch1Max.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        Ch2Max.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        Ch3Max.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        Ch4Min.IsEnabled = true;
                        Ch4Max.IsEnabled = true;
                        chkCh4.IsEnabled = true;
                        Ch5Min.IsEnabled = true;
                        Ch5Max.IsEnabled = true;
                        chkCh5.IsEnabled = true;
                        Ch6Min.IsEnabled = true;
                        Ch6Max.IsEnabled = true;
                        chkCh6.IsEnabled = true;
                        Ch7Min.IsEnabled = true;
                        Ch7Max.IsEnabled = true;
                        chkCh7.IsEnabled = true;
                        break;
                    case 8:
                        Ch1Min.IsEnabled = true;
                        Ch1Max.IsEnabled = true;
                        chkCh1.IsEnabled = true;
                        Ch2Min.IsEnabled = true;
                        Ch2Max.IsEnabled = true;
                        chkCh2.IsEnabled = true;
                        Ch3Min.IsEnabled = true;
                        Ch3Max.IsEnabled = true;
                        chkCh3.IsEnabled = true;
                        Ch4Min.IsEnabled = true;
                        Ch4Max.IsEnabled = true;
                        chkCh4.IsEnabled = true;
                        Ch5Min.IsEnabled = true;
                        Ch5Max.IsEnabled = true;
                        chkCh5.IsEnabled = true;
                        Ch6Min.IsEnabled = true;
                        Ch6Max.IsEnabled = true;
                        chkCh6.IsEnabled = true;
                        Ch7Min.IsEnabled = true;
                        Ch7Max.IsEnabled = true;
                        chkCh7.IsEnabled = true;
                        Ch8Min.IsEnabled = true;
                        Ch8Max.IsEnabled = true;
                        chkCh8.IsEnabled = true;
                        break;
                    default:
                        break;
                }
            }));
        }

        private void bloquearControles()
        {
            Ch1Min.IsEnabled = false;
            Ch1Max.IsEnabled = false;
            chkCh1.IsEnabled = false;
            Ch2Min.IsEnabled = false;
            Ch2Max.IsEnabled = false;
            chkCh2.IsEnabled = false;
            Ch3Min.IsEnabled = false;
            Ch3Max.IsEnabled = false;
            chkCh3.IsEnabled = false;
            Ch4Min.IsEnabled = false;
            Ch4Max.IsEnabled = false;
            chkCh4.IsEnabled = false;
            Ch5Min.IsEnabled = false;
            Ch5Max.IsEnabled = false;
            chkCh5.IsEnabled = false;
            Ch6Min.IsEnabled = false;
            Ch6Max.IsEnabled = false;
            chkCh6.IsEnabled = false;
            Ch7Min.IsEnabled = false;
            Ch7Max.IsEnabled = false;
            chkCh7.IsEnabled = false;
            Ch8Min.IsEnabled = false;
            Ch8Max.IsEnabled = false;
            chkCh8.IsEnabled = false;
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
            if (!cargando)
            {
                CheckBox ck = ((CheckBox)sender);
                if (tipoSMART == TipoSMART.PAYOUT)
                {
                    smartPayout.chanelRoute = int.Parse(ck.Tag.ToString());
                    smartPayout.BoolDisableRouteNote = true;

                }
                else
                {
                    smartHopper.chanelRoute = int.Parse(ck.Tag.ToString());
                    smartHopper.BoolDisableRouteCash = true;
                }
            }
        }

        private void chkCh_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!cargando)
            {
                CheckBox ck = ((CheckBox)sender);
                if (tipoSMART == TipoSMART.PAYOUT)
                {
                    smartPayout.chanelRoute = int.Parse(ck.Tag.ToString());
                    smartPayout.BoolRouteNote = true;

                }
                else
                {
                    smartHopper.chanelRoute = int.Parse(ck.Tag.ToString());
                    smartHopper.BoolRouteCash = true;
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            runLog = false;
            corriendo = false;
            if (tipoSMART == TipoSMART.PAYOUT)
                smartPayout.detenerPayout();
            else
                smartHopper.detenerHopper();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => RacargarNiveles());
        }

        private void btnRecargar_Click(object sender, RoutedEventArgs e)
        {
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                if (smartPayout.Payout.ValidatorEnabled)
                {
                    btnRecargar.Content = "Recargar";
                    smartPayout.BoolDisablePayout = true;
                }
                else
                {

                    btnRecargar.Content = "Finalizar";
                    smartPayout.BoolEnablePayout = true;
                }
            }

            else
            {
                if (smartHopper.Hopper.CoinMechEnabled)
                {
                    btnRecargar.Content = "Recargar";
                    smartHopper.BoolDisableCoinMech = true;
                }
                else
                {
                    btnRecargar.Content = "Finalizar";
                    smartHopper.BoolEnableCoinMech = true;
                }
            }
        }

        private void btnRestablecer_Click(object sender, RoutedEventArgs e)
        {
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                smartPayout.BoolResetPayout = true;
                bloquearControles();
                cargando = true;
                canales = 0;
            }
            else
            {
                smartHopper.BoolResetHopper = true;
                bloquearControles();
                cargando = true;
                canales = 0;
            }
            btnRecargar.Content = "Finalizar";
        }

        private void btnVaciar_Click(object sender, RoutedEventArgs e)
        {
            if (tipoSMART == TipoSMART.PAYOUT)
            {
                smartPayout.BoolEmptyCash = true;
            }
            else
            {
                smartHopper.BoolEmptyCash = true; ;
            }
        }

        private void btnRetirar_Click(object sender, RoutedEventArgs e)
        {
            if (Monto.Text.Length > 0)
            {
                int cantidad = int.Parse(Monto.Text);
                if (cantidad > 0)
                {
                    ValidarCambio validarCambio = new ValidarCambio();
                    Pago pago = new Pago();
                    pago.Cambio = cantidad;
                    bool seEntrego = false;
                    if (tipoSMART == TipoSMART.PAYOUT)
                    {
                        int revisar = validarCambio.cambioPayout(cantidad, smartPayout.Payout.UnitDataList);
                        if (revisar == 0)
                        {
                            pago.BilletesCambio = cantidad;
                            smartPayout.ActualizaPago(ref pago);
                            smartPayout.BoolCalculatePayout = true;
                            seEntrego = true;
                        }
                        else
                            seEntrego = false;
                    }
                    else
                    {
                        bool revisar = validarCambio.cambioHopper(cantidad, smartHopper.Hopper.UnitDataList);
                        if (revisar)
                        {
                            pago.MonedasCambio = cantidad;
                            smartHopper.ActualizaPago(ref pago);
                            smartHopper.BoolCalculatePayoutHopper = true;
                            seEntrego = true;
                        }
                        else
                            seEntrego = false;
                    }
                    if (!seEntrego)
                    {
                        Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, false);
                        dialog.lblNombre.Content = "¡Advertencia!";
                        dialog.lblTexto.Text = "El cajero no puede entregar la cantidad indicada, por favor revise que los datos sean correctos";
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        dialog.ShowDialog();
                        FocusManager.SetFocusedElement(this, Monto);
                    }
                }
                else
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Indique un valor diferente de cero en el retiro";
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.ShowDialog();
                    FocusManager.SetFocusedElement(this, Monto);
                }
            }
            else
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Indique un valor en el retiro";
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
                FocusManager.SetFocusedElement(this, Monto);
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

        internal async Task recargarlog(VentanaLog dialog)
        {
            runLog = true;
            while (runLog)
            {
                if (tipoSMART == TipoSMART.PAYOUT)
                    dialog.recargarlog(smartPayout.logPagoPayout);
                else
                    dialog.recargarlog(smartHopper.logPagoHopper);

                timer.Start();
                while (timer.IsEnabled)
                {
                    await Task.Delay(1); // Yield to free up CPU
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

        private void btnRetirarDenominacion_Click(object sender, RoutedEventArgs e)
        {
            if (Cantidad.Text.Length > 0 && cbxDenominacion.SelectedIndex != -1)
            {
                int cantidad = int.Parse(Cantidad.Text);
                if (cantidad > 0)
                {
                    ValidarCambio validarCambio = new ValidarCambio();
                    Pago pago = new Pago();
                    pago.Cambio = cantidad;
                    pago.Canal = cbxDenominacion.SelectedIndex;
                    bool seEntrego = false;
                    if (tipoSMART == TipoSMART.PAYOUT)
                    {
                        seEntrego = validarCambio.cambioPayoutByChanel(cantidad, cbxDenominacion.SelectedIndex + 1, smartPayout.Payout.UnitDataList);
                        if (seEntrego)
                        {
                            pago.BilletesCambio = cantidad;
                            smartPayout.ActualizaPago(ref pago);
                            smartPayout.BoolCalculatePayoutDenomination = true;
                        }
                    }
                    else
                    {
                        seEntrego = validarCambio.cambioPayoutByChanel(cantidad, cbxDenominacion.SelectedIndex + 1, smartHopper.Hopper.UnitDataList);
                        if (seEntrego)
                        {
                            pago.MonedasCambio = cantidad;
                            smartHopper.ActualizaPago(ref pago);
                            smartHopper.BoolCalculatePayoutDenomination = true;
                        }
                    }
                    if (!seEntrego)
                    {
                        Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, false);
                        dialog.lblNombre.Content = "¡Advertencia!";
                        dialog.lblTexto.Text = "El cajero no puede entregar la cantidad indicada, por favor revise que los datos sean correctos";
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        dialog.ShowDialog();
                        FocusManager.SetFocusedElement(this, Cantidad);
                    }
                }
                else
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Indique un valor diferente de cero en el retiro";
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.ShowDialog();
                    FocusManager.SetFocusedElement(this, Cantidad);
                }
            }
            else
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Indique un valor en el retiro";
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
                FocusManager.SetFocusedElement(this, Cantidad);
            }
        }
    }
}
