using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.Generic;
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
            }
        }

        private void GuardarDatos()
        {
            ConfiguracionGeneral SQLiteClass = new ConfiguracionGeneral();
            SQLiteClass.TamanoMensaje = ((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString();
            SQLiteClass.CerradoAutomatico = chkCierreAutomatico.IsChecked;
            SQLiteClass.TiempoMensaje = int.Parse(Duracion.Text);
            if (!new SQLiteClassManager().SetConfiguracionGeneral(SQLiteClass))
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
    }
}
