
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Speech.Synthesis;
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

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para Turnero.xaml
    /// </summary>
    public partial class Turnero : Window
    {
        private MainWindow mainWindow;
        bool esInicio = true;
        int count = 0;
        int countEncontrados = 0;
        public static SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        
        public Turnero(MainWindow pmainWindow)
        {
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

        private bool TextAllowed(string s)
        {
            foreach (char c in s.ToCharArray())
            {
                if (char.IsDigit(c) || c.Equals('.')) continue;
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
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            chkTurnero.IsChecked = config.AppSettings.Settings["Turnero"].Value.Equals("true")? true : false;
            cbxTipo.SelectedValue = config.AppSettings.Settings["TipoTurnero"].Value;
            cbxProtocolo.SelectedValue = config.AppSettings.Settings["ProtocoloTurnero"].Value;
            Puerto.Text = config.AppSettings.Settings["PuertoTurnero"].Value;
            Durar.Text = config.AppSettings.Settings["DuracionTurnero"].Value;
            cbxTurnosAnt.SelectedValue = config.AppSettings.Settings["TurnosAnteriores"].Value;
            cbxAudio.SelectedItem = config.AppSettings.Settings["AudioTurnero"].Value;
            chkVoz.IsChecked = config.AppSettings.Settings["VozTurnero"].Value.Equals("true") ? true : false;
            string[] clientes = config.AppSettings.Settings["Clientes"].Value.Split('|');
            if(clientes[0].Length > 0)
                foreach (string cliente in clientes)
                {
                    cbxTurneros.Items.Add(cliente);
                }
            lblEncontrados.Content = "Encontrados: "+ cbxTurneros.Items.Count;
            chkMostrarNombres.IsChecked = config.AppSettings.Settings["MostrarNombres"].Value.Equals("true") ? true : false;
        }

        private void GuardarDatos()
        {
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Turnero"].Value = chkTurnero.IsChecked == true ? "true" : "false";
            config.AppSettings.Settings["TipoTurnero"].Value = ((ComboBoxItem)cbxTipo.SelectedItem).Tag.ToString();
            config.AppSettings.Settings["ProtocoloTurnero"].Value = ((ComboBoxItem)cbxProtocolo.SelectedItem).Tag.ToString();
            config.AppSettings.Settings["PuertoTurnero"].Value = Puerto.Text;
            config.AppSettings.Settings["DuracionTurnero"].Value = Durar.Text;
            config.AppSettings.Settings["TurnosAnteriores"].Value = ((ComboBoxItem)cbxTurnosAnt.SelectedItem).Tag.ToString();
            config.AppSettings.Settings["AudioTurnero"].Value = cbxAudio.SelectedItem.ToString();
            config.AppSettings.Settings["VozTurnero"].Value = chkVoz.IsChecked == true ? "true" : "false";
            string clientes = string.Empty;
            foreach (string cliente in cbxTurneros.Items)
            {
                clientes += cliente+"|";
            }
            if(((ComboBoxItem)cbxTipo.SelectedItem).Tag.ToString().Equals("S") && clientes.Length > 0)
                config.AppSettings.Settings["Clientes"].Value = clientes.Substring(0, clientes.Length-1);
            else
                config.AppSettings.Settings["Clientes"].Value = "";

            config.AppSettings.Settings["MostrarNombres"].Value = chkMostrarNombres.IsChecked == true ? "true" : "false";

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings"); 

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            Close();
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            EntrarDiseno dialog = new EntrarDiseno(mainWindow);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            if (dialog.ShowDialog() == true)
            {
                MostrarTurno dialog2 = new MostrarTurno(true);
                dialog2.ShowDialog();
            }
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
                if(ipVal.Length == 4)
                {
                    if (ipVal[0].Length>0 && ipVal[1].Length > 0 && ipVal[2].Length > 0 && ipVal[3].Length > 0)
                    {
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        int puerto = int.Parse(config.AppSettings.Settings["PuertoTurnero"].Value);
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

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                // Simulate long running work
                Thread.Sleep(500);
                backgroundWorker.ReportProgress(count);
                if (count == 255)
                    break;
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This is called on the UI thread when ReportProgress method is called
            BarraP.Value = e.ProgressPercentage;
            Progress.Text = e.ProgressPercentage+"/255"; 
            lblEncontrados.Content = "Encontrados: "+countEncontrados;
        }

        private void cbxTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            dialog.ShowDialog();
        }

        private void chkVoz_Checked(object sender, RoutedEventArgs e)
        {
            btnVoz.Visibility = Visibility.Visible;
            if (!esInicio)
                Task.Run(() => vozDemo());
        }

        private void vozDemo() 
        {
            string line = string.Empty;
            try
            {
                synthesizer.SpeakAsyncCancelAll();
                using (Stream stream = new FileStream(@".\Recursos\vozTurnero.3k", FileMode.Open))
                {
                    var sr = new StreamReader(stream);

                    line = sr.ReadToEnd();
                    stream.Close();
                }
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (!config.AppSettings.Settings["TipoVozTurnero"].Value.Equals(""))
                    synthesizer.SelectVoice(config.AppSettings.Settings["TipoVozTurnero"].Value);
                synthesizer.SpeakAsync(line);
            }
            catch { }
        }

        private void chkVoz_Unchecked(object sender, RoutedEventArgs e)
        {
            synthesizer.SpeakAsyncCancelAll();
            btnVoz.Visibility = Visibility.Hidden;
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
