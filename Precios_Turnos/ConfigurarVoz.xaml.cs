using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;
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
    /// Lógica de interacción para Mensajes.xaml
    /// </summary>
    public partial class ConfigurarVoz : Window
    {
        bool esInicio = true;
        public static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        public ConfigurarVoz()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CargarInfo();

        }
        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            GuardarTexto();
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void GuardarTexto()
        {
            using (Stream stream = new FileStream(@".\Recursos\voz.3k", FileMode.Open))
            {
                stream.SetLength(0);
                byte[] bytes = Encoding.UTF8.GetBytes(lblTexto.Text);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["TipoVoz"].Value = cbxTipoVoz.SelectedItem.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        private void CargarInfo()
        {

            string line = string.Empty;
            try
            {
                using (Stream stream = new FileStream(@".\Recursos\voz.3k", FileMode.Open))
                {
                    var sr = new StreamReader(stream);

                    line = sr.ReadToEnd();
                    stream.Close();
                }
            
            lblTexto.Text = line;

            // show installed voices
            foreach (var v in synthesizer.GetInstalledVoices().Select(v => v.VoiceInfo))
            {
                cbxTipoVoz.Items.Add(v.Name);
            }
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cbxTipoVoz.SelectedItem = config.AppSettings.Settings["TipoVoz"].Value;
            }
            catch { }
            esInicio = false;
        }

        private void cbxTipoVoz_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!esInicio)
            {
                try
                {
                    synthesizer.SpeakAsyncCancelAll();
                    synthesizer.SelectVoice(cbxTipoVoz.SelectedItem.ToString());
                    synthesizer.SpeakAsync(lblTexto.Text);
                }
                catch { }
            }
            
        }
    }
}
