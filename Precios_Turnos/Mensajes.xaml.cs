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

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para Mensajes.xaml
    /// </summary>
    public partial class Mensajes : Window
    {

        public Mensajes(Recursos.TipoMensaje tipoMensaje, bool esPregunta=false)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            if (tipoMensaje == Recursos.TipoMensaje.ACEPTAR)
            {
                lblTexto.Foreground = new SolidColorBrush(Colors.White);
                lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
            }
            else if (tipoMensaje == Recursos.TipoMensaje.ADVERTENCIA)
            {
                lblTexto.Foreground = new SolidColorBrush(Colors.White);
                lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF6B73C"));
            }
            else if (tipoMensaje == Recursos.TipoMensaje.ERROR)
            {
                lblTexto.Foreground = new SolidColorBrush(Colors.White);
                lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
            }
            if(esPregunta)
                btnCancelar.Visibility = Visibility.Visible;

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
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
