using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Priceio.VentanasGenericas
{
    /// <summary>
    /// Lógica de interacción para ConfigurarGeneral.xaml
    /// </summary>
    public partial class ConfigurarGeneral : Window
    {
        public ConfigurarGeneral()
        {
            InitializeComponent();
        }
        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool TextAllowed(string s)
        {
            foreach (Char c in s.ToCharArray())
            {
                if (Char.IsDigit(c)) continue;
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
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            Close();
        }
        private void CargarDatos()
        {
            ConfiguracionGeneral? SQLiteClass = new SQLiteClassManager().GetConfiguracionGeneral();
            if (SQLiteClass != null)
            {
                cbxTamano.SelectedValue = SQLiteClass.TamanoMensaje;
                chkCierreAutomatico.IsChecked = SQLiteClass.CerradoAutomatico;
                Duracion.Text = SQLiteClass.TiempoMensaje.ToString();
                chkResolucion.IsChecked = SQLiteClass.ForzarResolucion;
                Ancho.Text = SQLiteClass.AnchoResolucion.ToString();
                Alto.Text = SQLiteClass.AltoResolucion.ToString();
            }
            if (Ancho.Text.Length==0)
                Ancho.Text = SystemParameters.VirtualScreenWidth.ToString();
            if (Alto.Text.Length == 0)
                Alto.Text = SystemParameters.VirtualScreenHeight.ToString();
        }

        private void GuardarDatos()
        {
            ConfiguracionGeneral SQLiteClass = new ConfiguracionGeneral();
            SQLiteClass.TamanoMensaje = ((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString();
            SQLiteClass.CerradoAutomatico = chkCierreAutomatico.IsChecked;
            SQLiteClass.TiempoMensaje = int.Parse(Duracion.Text);
            SQLiteClass.ForzarResolucion = chkResolucion.IsChecked;
            SQLiteClass.AnchoResolucion = int.Parse(Ancho.Text);
            SQLiteClass.AltoResolucion = int.Parse(Alto.Text);
            if (!new SQLiteClassManager().SetConfiguracionGeneral(SQLiteClass))
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {

                GuardarDatos();
                Close();
            }
        }

        private void chkCierreAutomatico_Checked(object sender, RoutedEventArgs e)
        {
            Duracion.IsEnabled = true;
        }

        private void chkCierreAutomatico_Unchecked(object sender, RoutedEventArgs e)
        {
            Duracion.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void lblResolucion_Checked(object sender, RoutedEventArgs e)
        {
            Ancho.IsEnabled = true;
            Alto.IsEnabled = true;
        }

        private void lblResolucion_Unchecked(object sender, RoutedEventArgs e)
        {
            Ancho.IsEnabled = false;
            Alto.IsEnabled = false;
            Ancho.Text = SystemParameters.VirtualScreenWidth.ToString();
            Alto.Text = SystemParameters.VirtualScreenHeight.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (chkResolucion.IsChecked == true)
            {
                if (int.Parse(Ancho.Text) != SystemParameters.VirtualScreenWidth
                    || int.Parse(Alto.Text) != SystemParameters.VirtualScreenHeight)
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, false);
                    dialog.lblNombre.Content = "¡Advertencia!";
                    dialog.lblTexto.Text = "La resolución de pantalla se forzara a una resolución diferente la aplicacion se reinicara";
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.ShowDialog();
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    Application.Current.Shutdown();
                }

            }
        }
    }
}
