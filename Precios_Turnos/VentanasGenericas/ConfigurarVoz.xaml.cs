using Microsoft.Win32;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
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

namespace Priceio
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
            SQLiteClass.TextoVoz = lblTexto.Text;
            if (!new SQLiteClassManager().SetVozSplash(SQLiteClass))
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
                lblTexto.Text = SQLiteClass.TextoVoz;
                cbxTipoVoz.SelectedItem = SQLiteClass.TipoVoz;
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
                    synthesizer.SpeakAsync(lblTexto.Text);
                }
                catch { }
            }

        }
    }
}
