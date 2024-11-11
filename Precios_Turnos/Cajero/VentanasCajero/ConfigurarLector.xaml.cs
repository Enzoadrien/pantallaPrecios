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

namespace Priceio.Cajero.VentanasCajero
{
    /// <summary>
    /// Lógica de interacción para ConfigurarLector.xaml
    /// </summary>
    public partial class ConfigurarLector : Window
    {
        private string formatoImpresion;
        public ConfigurarLector()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void CargarDatos()
        {
            ConfiguracionLector? SQLiteClass = new SQLiteClassManager().GetConfiguracionLector();
            if (SQLiteClass != null)
            {

                chkLector.IsChecked = SQLiteClass.Activo;
                Formato.Text = SQLiteClass.FormatoCodigo;
                chkImprimirPago.IsChecked = SQLiteClass.Imprmir;
                formatoImpresion = SQLiteClass.FormatoImpresora;
            }
        }

        private bool GuardarDatos()
        {
            ConfiguracionLector SQLiteClass = new ConfiguracionLector();
            SQLiteClass.Activo = chkLector.IsChecked;
            SQLiteClass.FormatoCodigo = Formato.Text;
            SQLiteClass.Imprmir = chkImprimirPago.IsChecked;
            SQLiteClass.FormatoImpresora = formatoImpresion;

            if (!new SQLiteClassManager().SetConfiguracionLector(SQLiteClass))
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                return false;
            }
            return true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if(GuardarDatos())
                Close();
        }

        private void chkImprimirPago_Checked(object sender, RoutedEventArgs e)
        {
            btnFormato.Visibility = Visibility.Visible;
        }

        private void chkImprimirPago_Unchecked(object sender, RoutedEventArgs e)
        {
            btnFormato.Visibility = Visibility.Hidden;
        }

        private void btnFormato_Click(object sender, RoutedEventArgs e)
        {
            FormatoPagoEscaner dialog = new FormatoPagoEscaner(formatoImpresion);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            dialog.ShowDialog();
            formatoImpresion = dialog.formatoImpresora;
        }
    }
}
