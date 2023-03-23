using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para EntrarDiseno.xaml
    /// </summary>
    public partial class EntrarDiseno : Window
    {
        private MainWindow mainWindow;
        public EntrarDiseno(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
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
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Seguridad vSeguridad = new Seguridad();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string Codigo = config.AppSettings.Settings["CodigoActivacion"].Value;
            string Correo = vSeguridad.DecryptString(Codigo, config.AppSettings.Settings["Correo"].Value);
            if (Llave.Text.Length > 0)
            {
                try
                {
                    string cadena = vSeguridad.DecryptString(Codigo, Llave.Text);
                    string[] subs = cadena.Split('|');
                    if (subs.Length >= 3)
                    {
                        if (subs[0].Equals(Correo) && subs[1].Equals(vSeguridad.numeroSerieHD()) && subs[2].Equals(vSeguridad.numeroSeriePlacaBase())
                            && subs[3].Equals(mainWindow.nombreApp) && DateTime.Now.Date <= Convert.ToDateTime(subs[4]))
                        {
                            DialogResult = true;
                            Close();
                        }
                        else
                        {
                            Mensajes dialog = new Mensajes();
                            dialog.lblNombre.Content = "¡Error!";
                            dialog.lblTexto.Text = "Los datos ingresados no son correctos";
                            dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                            dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                            dialog.ShowDialog();
                        }
                    }
                    else
                    {
                        Mensajes dialog = new Mensajes();
                        dialog.lblNombre.Content = "¡Error!";
                        dialog.lblTexto.Text = "Los datos ingresados no son correctos";
                        dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                        dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                        dialog.ShowDialog();
                    }
                }
                catch (Exception)
                {
                    Mensajes dialog = new Mensajes();
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Los datos ingresados no son correctos";
                    dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                    dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                    dialog.ShowDialog();
                }
            }
            else
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Los datos ingresados no son correctos";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
        }
    }
}
