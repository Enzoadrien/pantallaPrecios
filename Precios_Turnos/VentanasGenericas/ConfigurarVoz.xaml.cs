using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para Mensajes.xaml
    /// </summary>
    public partial class ConfigurarVoz : Window
    {
        private bool esInicio = true;
        private SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        public ConfigurarVoz()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            synthesizer.SetOutputToDefaultAudioDevice();
            CargarDatos();

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
            GuardarDatos();
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void GuardarDatos()
        {
            VozSplash SQLiteClass = new VozSplash();
            SQLiteClass.TipoVoz = cbxTipoVoz.SelectedItem.ToString();
            SQLiteClass.TextoVoz1 = Voz1.Text;
            SQLiteClass.TextoVoz2 = Voz2.Text;
            SQLiteClass.TextoVoz3 = Voz3.Text;
            SQLiteClass.VozIngresoEfectivo = chkVozEfectivo.IsChecked;
            if (!new SQLiteClassManager().SetVozSplash(SQLiteClass))
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
            }
        }
        
        private void CargarDatos()
        {
            // show installed voices
            foreach (var v in synthesizer.GetInstalledVoices().Select(v => v.VoiceInfo))
            {
                cbxTipoVoz.Items.Add(v.Name);
            }

            VozSplash? SQLiteClass = new SQLiteClassManager().GetVozSplash();
            if (SQLiteClass != null)
            {
                Voz1.Text = SQLiteClass.TextoVoz1;
                Voz2.Text = SQLiteClass.TextoVoz2;
                Voz3.Text = SQLiteClass.TextoVoz3;
                chkVozEfectivo.IsChecked = SQLiteClass.VozIngresoEfectivo;
                cbxTipoVoz.SelectedItem = SQLiteClass.TipoVoz;

                synthesizer.SpeakAsyncCancelAll();
                synthesizer.SelectVoice(cbxTipoVoz.SelectedItem.ToString());
                synthesizer.SpeakAsync(Voz1.Text);
            }
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
                    synthesizer.SpeakAsync(Voz1.Text);
                }
                catch { }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                synthesizer.SpeakAsyncCancelAll();
            }
            catch { }

        }
    }
}
