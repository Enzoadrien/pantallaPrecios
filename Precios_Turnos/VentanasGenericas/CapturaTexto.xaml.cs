using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para CapturaTexto.xaml
    /// </summary>
    public partial class CapturaTexto : Window
    {
        ConfiguracionGeneral? SQLiteClass = new SQLiteClassManager().GetConfiguracionGeneral();
        public CapturaTexto(bool tamanoEspecial = false)
        {
            InitializeComponent();
            {
                if (SQLiteClass != null)
                {
                    switch (SQLiteClass.TamanoMensaje)
                    {
                        case "E":
                            ventanaCapturaTextoExtraGrande();
                            break;
                        case "G":
                            ventanaCapturaTextoGrande();
                            break;
                        default:
                            break;

                    }
                }
            }
            FocusManager.SetFocusedElement(this, Texto);
            
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
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
        
        internal void ventanaCapturaTextoExtraGrande()
        {
            Width = 800;
            Height = 600;
            Salir.Width = 100;
            Salir.Height = 100;
            Salir.FontSize = 80;
            lblNombre.FontSize = 100;
            Texto.FontSize = 60;
            btnOK.FontSize = 60;
            btnOK.Width = 300;
            btnOK.Height = 100;
            btnCancelar.FontSize = 60;
            btnCancelar.Width = 300;
            btnCancelar.Height = 100;
            btnCancelar.HorizontalAlignment = HorizontalAlignment.Left;
            btnCancelar.Margin = new Thickness(5, 5, 5, 5);
        }
        
        internal void ventanaCapturaTextoGrande()
        {
            Width = 600;
            Height = 400;
            Salir.Width = 80;
            Salir.Height = 80;
            Salir.FontSize = 60;
            lblNombre.FontSize = 64;
            Texto.FontSize = 44;
            btnOK.FontSize = 40;
            btnOK.Width = 200;
            btnOK.Height = 70;
            btnCancelar.FontSize = 40;
            btnCancelar.Width = 200;
            btnCancelar.Height = 70;
            btnCancelar.HorizontalAlignment = HorizontalAlignment.Left;
            btnCancelar.Margin = new Thickness(5, 5, 5, 5);
        }
    }
}
