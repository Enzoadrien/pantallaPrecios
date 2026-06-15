using Priceio.Cajero.Hopper;
using Priceio.Cajero.Payout;
using Priceio.Cajero.VentanasCajero;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using Priceio.Turnero;
using Priceio.Turnero.Kretz;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Priceio.Cajero.ChannelData;
using static Priceio.PropiedadesImpresora;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para Turnero.xaml
    /// </summary>
    public partial class ConfigurarVentanaSplash : Window
    {
        private MainWindow mainWindow;
        private bool esInicio = true;
        private int count = 0;
        private int countEncontrados = 0;
        private bool bucando = false;

        private SMARTPayout? smartPayout;
        private SMARTHopper? smartHopper;

        internal ConfigurarVentanaSplash(MainWindow pmainWindow, SMARTPayout? payout = null, SMARTHopper? hopper = null)
        {
            smartPayout = payout;
            smartHopper = hopper;
            InitializeComponent();
            CargarIPs();
            CargarDatos();

            mainWindow = pmainWindow;
            FocusManager.SetFocusedElement(this, cbxTipo);
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            esInicio = false;
            cambioTipoSplash();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private bool TextAllowedIP(string s)
        {
            foreach (char c in s.ToCharArray())
            {
                if (char.IsDigit(c) || c.Equals('.')) continue;
                else return false;
            }
            return true;
        }

        private void ResponseTextBox_PreviewTextInputIP(object sender, TextCompositionEventArgs e)
        {

            e.Handled = !TextAllowedIP(e.Text);

        }

        private void PastingHandlerIP(object sender, DataObjectPastingEventArgs e)
        {
            // more error handling would be needed here - this is asking for trouble!
            String s = (String)e.DataObject.GetData(typeof(String));
            if (!TextAllowedIP(s)) e.CancelCommand();
        }

        private bool TextAllowed(string s)
        {
            foreach (char c in s.ToCharArray())
            {
                if (char.IsDigit(c)) continue;
                else return false;
            }
            return true;
        }

        private void ResponseTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            e.Handled = !TextAllowed(e.Text);

        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            // more error handling would be needed here - this is asking for trouble!
            String s = (String)e.DataObject.GetData(typeof(String));
            if (!TextAllowed(s)) e.CancelCommand();
        }

        private void ResponseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;

            if (e.Key == Key.Space && cajaTexto.IsFocused == true)
                e.Handled = true;
        }

        private void CargarDatos()
        {
            ConfiguracionVentanaSplash? SQLiteClass = new SQLiteClassManager().GetConfiguracionVentanaSplash();
            if (SQLiteClass != null)
            {
                cbxTipoSplash.SelectedValue = SQLiteClass.TipoSplash;
                chkActivarSplash.IsChecked = SQLiteClass.ActivarSplash;
                cbxAudio.SelectedItem = SQLiteClass.Audio;
                Durar.Text = SQLiteClass.Duracion.ToString();
                Ancho.Text = SQLiteClass.Ancho.ToString();
                Alto.Text = SQLiteClass.Alto.ToString();
                chkVoz.IsChecked = SQLiteClass.Voz;
            }
            ConfiguracionTurnero? SQLiteClassTurnero = new SQLiteClassManager().GetConfiguracionTurnero();
            if (SQLiteClassTurnero != null)
            {
                cbxTipo.SelectedValue = SQLiteClassTurnero.TipoTurnero;
                cbxProtocolo.SelectedValue = SQLiteClassTurnero.ProtocoloTurnero;
                Puerto.Text = SQLiteClassTurnero.PuertoTCP.ToString();
                cbxTurnosAnt.SelectedValue = SQLiteClassTurnero.TurnosAnteriores;
                List<ClientesTurnero>? ListClientesTurnero = new SQLiteClassManager().GetClientesTurnero();
                if (ListClientesTurnero != null)
                    cbxTurneros.ItemsSource = ListClientesTurnero.Select(i => i.Cliente);
                lblEncontrados.Content = "Encontrados: " + cbxTurneros.Items.Count;
                chkMostrarNombres.IsChecked = SQLiteClassTurnero.MostrarNombres;
            }
            ConfiguracionVerificador? SQLiteClassVerificador = new SQLiteClassManager().GetConfiguracionVerificador();
            if (SQLiteClassVerificador != null)
            {
            }
            ConfiguracionCajero? SQLiteClassCajero = new SQLiteClassManager().GetConfiguracionCajero();
            if (SQLiteClassCajero != null)
            {
                PuertoTCPCajero.Text = SQLiteClassCajero.PuertoTCP.ToString();
                cbxPuertoComNV22.SelectedValue = SQLiteClassCajero.COMPayout;
                SSPNV22Spectral.Text = SQLiteClassCajero.SSPPayout.ToString();
                cbxPuertoComSMARTHopper.SelectedValue = SQLiteClassCajero.COMHopper;
                SSPSMARTHopper.Text = SQLiteClassCajero.SSPHopper.ToString();
                chkLogPagos.IsChecked = SQLiteClassCajero.LogPagos;
            }
        }

        private void GuardarDatos()
        {
            if (!GuardarDatosVentanaSplash() || !GuardarDatosTurnero()
                || !GuardarDatosClientesTurnero() || !GuardarDatosCajero())
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
            }
        }

        private bool GuardarDatosVentanaSplash()
        {
            ConfiguracionVentanaSplash SQLiteClass = new ConfiguracionVentanaSplash();
            SQLiteClass.TipoSplash = ((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString();
            SQLiteClass.Audio = cbxAudio.SelectedItem.ToString();
            SQLiteClass.Duracion = int.Parse(Durar.Text);
            SQLiteClass.Ancho = int.Parse(Ancho.Text);
            SQLiteClass.Alto = int.Parse(Alto.Text);
            SQLiteClass.Voz = chkVoz.IsChecked;
            SQLiteClass.ActivarSplash = chkActivarSplash.IsChecked;
            return new SQLiteClassManager().SetConfiguracionVentanaSplash(SQLiteClass);
        }

        private void CargarIPs()
        {
            String strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostByName(strHostName);

            // Enumerate IP addresses
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                string[] ipVal = ipaddress.ToString().Split('.');
                if (ipVal.Length == 4)
                {
                    cbxIP.Items.Add(ipVal[0] + "." + ipVal[1] + "." + ipVal[2] + ".0");
                }       
            }
        }

        private bool GuardarDatosTurnero()
        {
            ConfiguracionTurnero SQLiteClassTurnero = new ConfiguracionTurnero();
            SQLiteClassTurnero.TipoTurnero = ((ComboBoxItem)cbxTipo.SelectedItem).Tag.ToString();
            SQLiteClassTurnero.ProtocoloTurnero = ((ComboBoxItem)cbxProtocolo.SelectedItem).Tag.ToString();
            SQLiteClassTurnero.PuertoTCP = int.Parse(Puerto.Text);
            SQLiteClassTurnero.TurnosAnteriores = int.Parse(((ComboBoxItem)cbxTurnosAnt.SelectedItem).Tag.ToString());
            SQLiteClassTurnero.MostrarNombres = chkMostrarNombres.IsChecked;
            return new SQLiteClassManager().SetConfiguracionTurnero(SQLiteClassTurnero);
        }

        private bool GuardarDatosClientesTurnero()
        {
            List<ClientesTurnero> ListClientesTurnero = new List<ClientesTurnero>();
            foreach (string cliente in cbxTurneros.Items)
            {
                ListClientesTurnero.Add(new ClientesTurnero { Cliente = cliente });
            }
            if (ListClientesTurnero.Count > 0)
                return new SQLiteClassManager().SetClientesTurnero(ListClientesTurnero);
            else
                return true;
        }

        private bool GuardarDatosCajero()
        {
            ConfiguracionCajero SQLiteClassCajero = new ConfiguracionCajero();
            SQLiteClassCajero.PuertoTCP = int.Parse(PuertoTCPCajero.Text);
            SQLiteClassCajero.COMPayout = ((ComboBoxItem)cbxPuertoComNV22.SelectedItem).Tag.ToString();
            SQLiteClassCajero.SSPPayout = int.Parse(SSPNV22Spectral.Text);
            SQLiteClassCajero.COMHopper = ((ComboBoxItem)cbxPuertoComSMARTHopper.SelectedItem).Tag.ToString();
            SQLiteClassCajero.SSPHopper = int.Parse(SSPSMARTHopper.Text);
            SQLiteClassCajero.LogPagos = chkLogPagos.IsChecked;
            return new SQLiteClassManager().SetConfiguracionCajero(SQLiteClassCajero);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            Close();
        }

        private void cbxAudio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!esInicio)
            {
                try
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(@".\Recursos\audios\" + cbxAudio.SelectedItem.ToString());
                    player.Play();
                }
                catch { }
            }

        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (cbxIP.SelectedIndex != -1)
            {
                string[] ipVal = cbxIP.Text.Split('.');
                if (ipVal.Length == 4)
                {
                    if (ipVal[0].Length > 0 && ipVal[1].Length > 0 && ipVal[2].Length > 0 && ipVal[3].Length > 0)
                    {
                        btnBuscar.IsEnabled = false;
                        cbxTurneros.IsEnabled = false;
                        btnDetener.Visibility = Visibility.Visible;
                        bucando = true;
                        Task.Run(() => encontrarTurneros());
                    }
                    else
                    {
                        Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                        dialog.lblNombre.Content = "¡Error!";
                        dialog.lblTexto.Text = "No es una dirección IP valida.";
                        dialog.ShowDialog();
                    }

                }
                else
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "No es una dirección IP valida.";
                    dialog.ShowDialog();
                }
            }
        }

        private async void encontrarTurneros()
        {
            int puerto = 0;
            string IPBase = string.Empty;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                puerto = int.Parse(Puerto.Text);

                IPBase = cbxIP.Text;
            }));
            List<string> lista = new List<string>();
            count = 0;
            string[] ip = IPBase.Split('.');
            string baseIP = ip[0] + "." + ip[1] + "." + ip[2] + ".";

            List<string> Equipos = new List<string>();
            for (int x = 1; x <= 255; x++)
            {
                Equipos.Add(baseIP + x.ToString());
            }
            Comunicacion com = new Comunicacion();

            foreach (string adr in Equipos)
            {
                if (!bucando)
                    break;
                bool respuesta = await new AsynchronousClient().StartClient(adr, puerto, com.CrearComandoBascula("D018500"));
                if (respuesta)
                {
                    lista.Add(adr);
                    countEncontrados++;
                }
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    BarraP.Value = count++;
                    lblIPS.Content = adr;
                    Progress.Text = count + "/255";
                    lblEncontrados.Content = "Encontrados: " + countEncontrados;
                }));
            }
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                cbxTurneros.ItemsSource = null;
                if (lista.Count > 0)
                    cbxTurneros.ItemsSource = lista;
                btnBuscar.IsEnabled = true;
                cbxTurneros.IsEnabled = true;

                BarraP.Value = 0;
                lblIPS.Content = string.Empty;
                Progress.Text = 0 + "/255";
            }));
            bucando = false;
        }

        private void cbxTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!esInicio)
            {
                try
                {
                    if ((ComboBoxItem)cbxTipo.SelectedItem != null)
                        if (((ComboBoxItem)cbxTipo.SelectedItem).Tag.ToString().Equals("S"))
                        {
                            cbxIP.IsEnabled = true;
                            string[] ip = new Seguridad().DisplayIPAddresses().Split('.');
                            if (ip.Length == 4)
                            {
                                cbxIP.Text = ip[0] + "." + ip[1] + "." + ip[2] + ".0";
                            }
                            btnBuscar.IsEnabled = true;
                            cbxTurneros.IsEnabled = true;
                            lblEncontrados.Content = "Encontrados: " + cbxTurneros.Items.Count;
                        }
                        else
                        {
                            cbxIP.IsEnabled = false;
                            cbxIP.SelectedIndex = -1;
                            btnBuscar.IsEnabled = false;
                            cbxTurneros.IsEnabled = false;
                            lblEncontrados.Content = "Encontrados: 0";
                        }
                }
                catch { }
            }
        }

        private void btnNombresEquipos_Click(object sender, RoutedEventArgs e)
        {
            ConfigurarNombres dialog = new ConfigurarNombres();
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            dialog.ShowDialog();
        }

        private void btnVoz_Click(object sender, RoutedEventArgs e)
        {
            ConfigurarVoz dialog = new ConfigurarVoz();
            if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("T"))
            {
                dialog.Texto1.Content = "*NumeroTurno          *NumeroEquipo           *NombreEquipo";

            }
            else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("V"))
            {
                dialog.Texto1.Content = "* Para agregar campos recuperados de una tabla de datos";
                dialog.Texto2.Content = "   se deberá agregar la palabra TextoVoz seguido de la";
                dialog.Texto3.Content = "   posición del dato recuperado.(TextoVoz1, TextoVoz2, ...)";

            }
            else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("C"))
            {
                dialog.lblVoz1.Content = "Voz de pago";
                dialog.lblVoz2.Content = "Voz de retiro";
                dialog.lblVoz3.Visibility = Visibility.Visible;
                dialog.Voz3.Visibility = Visibility.Visible;
                dialog.Texto1.Content = "*CantidadTotal           *Cambio";
                dialog.chkVozEfectivo.Visibility = Visibility.Visible;
            }
            dialog.ShowDialog();
        }

        private void chkVoz_Checked(object sender, RoutedEventArgs e)
        {
            btnVoz.Visibility = Visibility.Visible;
            if (!esInicio)
            {
                ConfigurarVoz dialog = new ConfigurarVoz();
                if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("T"))
                {
                    dialog.Texto1.Content = "*NumeroTurno          *NumeroEquipo           *NombreEquipo";

                }
                else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("V"))
                {
                    dialog.Texto1.Content = "* Para agregar campos recuperados de una tabla de datos";
                    dialog.Texto2.Content = "   se deberá agregar la palabra TextoVoz seguido de la";
                    dialog.Texto3.Content = "   posición del dato recuperado.(TextoVoz1, TextoVoz2, ...)";

                }
                else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("C"))
                {
                    dialog.lblVoz1.Content = "Voz de pago";
                    dialog.lblVoz2.Content = "Voz de retiro";
                    dialog.lblVoz3.Visibility = Visibility.Visible;
                    dialog.Voz3.Visibility = Visibility.Visible;
                    dialog.Texto1.Content = "*CantidadTotal           *Cambio";
                    dialog.chkVozEfectivo.Visibility = Visibility.Visible;
                }

                dialog.ShowDialog();
            }

        }

        private void chkVoz_Unchecked(object sender, RoutedEventArgs e)
        {
            btnVoz.Visibility = Visibility.Hidden;
        }

        private void cbxTipoSplash_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!esInicio)
                cambioTipoSplash();
        }

        private void cambioTipoSplash()
        {
            if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("T"))
            {
                TiposVentana.SelectedIndex = 0;
                Turnero.IsEnabled = true;
                Verificador.IsEnabled = false;
                CajeroATM.IsEnabled = false;
            }
            else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("V"))
            {
                TiposVentana.SelectedIndex = 1;
                Turnero.IsEnabled = false;
                Verificador.IsEnabled = true;
                CajeroATM.IsEnabled = false;
            }
            else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("C"))
            {
                TiposVentana.SelectedIndex = 2;
                Turnero.IsEnabled = false;
                Verificador.IsEnabled = false;
                CajeroATM.IsEnabled = true;
            }
        }

        private void btnImpresora_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            PropiedadesImpresora dialog = new PropiedadesImpresora(TipoImpresion.Cajero);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            dialog.ShowDialog();
        }

        private void btnPayout_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            ConfigCanales dialog = new ConfigCanales(TipoSMART.PAYOUT, smartPayout, smartHopper);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            dialog.ShowDialog();
        }

        private void btnHopper_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            ConfigCanales dialog = new ConfigCanales(TipoSMART.HOPPER, smartPayout, smartHopper);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            dialog.ShowDialog();
        }

        private void btnRestablecer_Click(object sender, RoutedEventArgs e)
        {
            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
            dialog.lblNombre.Content = "¡Advertencia!";
            dialog.lblTexto.Text = "Se establecerá la configuración de fábrica, se perderá la configuración actual. ¿Está seguro que desea continuar?.";
            dialog.btnCancelar.Visibility = Visibility.Visible;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (dialog.ShowDialog() == true)
            {
                if (!new SQLiteClassManager().ResetConfigSplash())
                {
                    dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                    dialog.btnCancelar.Visibility = Visibility.Visible;
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                else
                    LimpiarDatosVentana();
            }
        }

        private void LimpiarDatosVentana()
        {
            cbxTipoSplash.SelectedIndex = 0;
            esInicio = true;
            cbxAudio.SelectedIndex = 0;
            Durar.Text = "5";
            Ancho.Text = "600";
            Alto.Text = "600";
            chkVoz.IsChecked = false;
            cbxTipo.SelectedIndex = 0;
            cbxProtocolo.SelectedIndex = 0;
            Puerto.Text = "9101";
            cbxTurnosAnt.SelectedIndex = 0;
            cbxIP.SelectedIndex = -1;
            cbxTurneros.ItemsSource = null; ;
            lblEncontrados.Content = "Encontrados: 0";
            chkMostrarNombres.IsChecked = false;
            PuertoTCPCajero.Text = "9101";
            cbxPuertoComNV22.SelectedIndex = 0;
            SSPNV22Spectral.Text = "0";
            cbxPuertoComSMARTHopper.SelectedIndex = 0;
            SSPSMARTHopper.Text = "0";
            chkLogPagos.IsChecked = false;
            chkActivarSplash.IsChecked = false;

            esInicio = false;
            count = 0;
            countEncontrados = 0;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            bucando = false;
        }

        private void btnDetener_Click(object sender, RoutedEventArgs e)
        {
            bucando = false;
            btnDetener.Visibility = Visibility.Hidden;
        }

        private void btnLector_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            ConfigurarLector dialog = new ConfigurarLector();
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            dialog.ShowDialog();
        }

        private void btnImpresoraTurnero_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            PropiedadesImpresora dialog = new PropiedadesImpresora(TipoImpresion.Turnero); 
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            dialog.Left = centerParent.X - dialog.Width / 2;
            dialog.Top = centerParent.Y - dialog.Height / 2;
            dialog.ShowDialog();
        }
    }
    public class ViewModelAudio
    {
        public ObservableCollection<string> CmbContentAudio { get; private set; }

        public ViewModelAudio()
        {
            CmbContentAudio = new ObservableCollection<string>
            {
                "Silencio"
            };
            DirectoryInfo di = new DirectoryInfo(@".\Recursos\audios");
            foreach (FileInfo file in di.GetFiles("*.wav"))
            {
                CmbContentAudio.Add(file.Name);
            }

        }
    }
}
