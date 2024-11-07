using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Priceio.Cajero.Hopper;
using Priceio.Cajero.Payout;
using Priceio.ClasesGenericas;
using Priceio.Turnero;
using Priceio.Turnero.Kretz;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System.Linq;
using static Priceio.Cajero.ChannelData;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para Turnero.xaml
    /// </summary>
    public partial class ConfigurarVentanaSplash : Window
    {
        private MainWindow mainWindow;
        bool esInicio = true;
        int count = 0;
        int countEncontrados = 0;
        public static SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        private SMARTPayout? smartPayout;
        private SMARTHopper? smartHopper;

        internal ConfigurarVentanaSplash(MainWindow pmainWindow, SMARTPayout? payout = null, SMARTHopper? hopper = null)
        {
            smartPayout = payout;
            smartHopper = hopper;
            InitializeComponent();
            CargarDatos();

            mainWindow = pmainWindow;
            FocusManager.SetFocusedElement(this, cbxTipo);
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.ProgressChanged += ProgressChanged;
            backgroundWorker.DoWork += DoWork;
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

        private async void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (IP.Text.Length > 0)
            {
                string[] ipVal = IP.Text.Split('.');
                if (ipVal.Length == 4)
                {
                    if (ipVal[0].Length > 0 && ipVal[1].Length > 0 && ipVal[2].Length > 0 && ipVal[3].Length > 0)
                    {
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        int puerto = int.Parse(config.AppSettings.Settings["PuertoTCP"].Value);
                        List<string> lista = new List<string>();
                        string IPBase = IP.Text;
                        btnBuscar.IsEnabled = false;
                        cbxTurneros.IsEnabled = false;
                        backgroundWorker.RunWorkerAsync();
                        await Task.Run(async () =>
                        {
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
                                bool respuesta = await new AsynchronousClient().StartClient(adr, puerto, com.CrearComandoBascula("D018500"));
                                if (respuesta)
                                {
                                    lista.Add(adr);
                                    countEncontrados++;
                                }
                                count++;

                            }
                        }).ContinueWith(result => { Callback(lista); });
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

        private void Callback(List<string> lista)
        {
            backgroundWorker.CancelAsync();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                cbxTurneros.ItemsSource = null;
                if (lista.Count > 0)
                    cbxTurneros.ItemsSource = lista;
                btnBuscar.IsEnabled = true;
                cbxTurneros.IsEnabled = true;
            }));
        }

        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                // Simulate long running work
                backgroundWorker.ReportProgress(count);
                if (count == 255)
                    break;
                await Task.Delay(500);
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This is called on the UI thread when ReportProgress method is called
            BarraP.Value = e.ProgressPercentage;
            Progress.Text = e.ProgressPercentage + "/255";
            lblEncontrados.Content = "Encontrados: " + countEncontrados;
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
                            IP.IsEnabled = true;
                            string[] ip = new Seguridad().DisplayIPAddresses().Split('.');
                            if (ip.Length == 4)
                            {
                                IP.Text = ip[0] + "." + ip[1] + "." + ip[2] + ".0";
                            }
                            btnBuscar.IsEnabled = true;
                            cbxTurneros.IsEnabled = true;
                            lblEncontrados.Content = "Encontrados: " + cbxTurneros.Items.Count;
                        }
                        else
                        {
                            IP.IsEnabled = false;
                            IP.Text = "";
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
            Task.Run(() => vozDemo());
            ConfigurarVoz dialog = new ConfigurarVoz();
            if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("T"))
            {
                dialog.Texto1.Content = "*NumeroTurno          *NumeroEquipo           *NombreEquipo";
                dialog.Texto2.Content = "*NumeroTurnoAnt   *NumeroEquipoAnt    *NombreEquipoAnt";
                dialog.Texto3.Content = "";

            }
            else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("V"))
            {
                dialog.Texto1.Content = "* Para agregar campos recuperados de una tabla de datos";
                dialog.Texto2.Content = "   se deberá agregar la palabra TextoVoz seguido de la";
                dialog.Texto3.Content = "   posición del dato recuperado.(TextoVoz1, TextoVoz2, ...)";

            }
            else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("C"))
            {
                dialog.Texto1.Content = "";
                dialog.Texto2.Content = "";
                dialog.Texto3.Content = "";
            }
            dialog.ShowDialog();
        }

        private void chkVoz_Checked(object sender, RoutedEventArgs e)
        {
            btnVoz.Visibility = Visibility.Visible;
            if (!esInicio)
            {
                Task.Run(() => vozDemo());
                ConfigurarVoz dialog = new ConfigurarVoz();
                if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("T"))
                {
                    dialog.Texto1.Content = "*NumeroTurno          *NumeroEquipo           *NombreEquipo";
                    dialog.Texto2.Content = "*NumeroTurnoAnt   *NumeroEquipoAnt    *NombreEquipoAnt";
                    dialog.Texto3.Content = "";

                }
                else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("V"))
                {
                    dialog.Texto1.Content = "* Para agregar campos recuperados de una tabla de datos";
                    dialog.Texto2.Content = "   se deberá agregar la palabra TextoVoz seguido de la";
                    dialog.Texto3.Content = "   posición del dato recuperado.(TextoVoz1, TextoVoz2, ...)";

                }
                else if (((ComboBoxItem)cbxTipoSplash.SelectedItem).Tag.ToString().Equals("C"))
                {
                    dialog.Texto1.Content = "";
                    dialog.Texto2.Content = "";
                    dialog.Texto3.Content = "";
                }

                dialog.ShowDialog();
            }

        }

        private void vozDemo()
        {
            VozSplash? SQLiteClass = new SQLiteClassManager().GetVozSplash();
            if (SQLiteClass != null)
            {
                synthesizer.SelectVoice(SQLiteClass.TipoVoz);
                synthesizer.SpeakAsync(SQLiteClass.TextoVoz);
            }
        }

        private void chkVoz_Unchecked(object sender, RoutedEventArgs e)
        {
            synthesizer.SpeakAsyncCancelAll();
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
            PropiedadesImpresora dialog = new PropiedadesImpresora();
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
            IP.Text = "";
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
