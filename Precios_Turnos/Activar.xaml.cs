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
    /// Lógica de interacción para Activar.xaml
    /// </summary>
    public partial class Activar : Window
    {
        private Seguridad vSeguridad = new Seguridad();
        private MainWindow mainWindow;
        public Activar(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
            CargarInfo();
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if(GuardarInfo())
                Close();
        }

        private void CargarInfo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Codigo.Text = config.AppSettings.Settings["CodigoActivacion"].Value;
            Correo.Text = vSeguridad.DecryptString(Codigo.Text, config.AppSettings.Settings["Correo"].Value);
            Llave.Text = vSeguridad.DecryptString(Codigo.Text, config.AppSettings.Settings["Llave"].Value);
            if(Llave.Text.Length > 0)
            {
                string cadena = vSeguridad.DecryptString(Codigo.Text, Llave.Text);
                string[] subs = cadena.Split('|');
                if (subs.Length >= 3)
                {
                    if (subs[0].Equals(Correo.Text) && subs[1].Equals(vSeguridad.numeroSerieHD()) && subs[2].Equals(vSeguridad.numeroSeriePlacaBase())
                        && subs[3].Equals(MainWindow.nombreApp))
                    {
                        if (!subs[4].Equals("0"))
                        {
                            if (vSeguridad.GetNetworkTime().Date < Convert.ToDateTime(subs[4]).Date)
                            {
                                Correo.IsReadOnly = true;
                                Llave.IsReadOnly = true;
                                btnGenerar.IsEnabled = false;
                                lblFecha.Content = "Fecha licencia: " + Convert.ToDateTime(subs[4]).Date.ToShortDateString();
                            }
                        }
                        else
                        {
                            Correo.IsReadOnly = true;
                            Llave.IsReadOnly = true;
                            btnGenerar.IsEnabled = false;
                            lblFecha.Content = "Fecha licencia: Permanete";
                        }
                    }
                }
                
            }
        }

        private bool GuardarInfo()
        {
            string cadena = vSeguridad.DecryptString(Codigo.Text, Llave.Text);
            string[] subs = cadena.Split('|');
            if (subs.Length >= 3)
            {
                if(subs[0].Equals(Correo.Text) && subs[1].Equals(vSeguridad.numeroSerieHD()) && subs[2].Equals(vSeguridad.numeroSeriePlacaBase()) 
                    && subs[3].Equals(MainWindow.nombreApp)) {


                    if (!subs[4].Equals("0"))
                    {
                        if (vSeguridad.GetNetworkTime() < Convert.ToDateTime(subs[4]))
                        {
                            //Create the object
                            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                            config.AppSettings.Settings["Correo"].Value = vSeguridad.EncryptString(Codigo.Text, Correo.Text);
                            config.AppSettings.Settings["CodigoActivacion"].Value = Codigo.Text;
                            config.AppSettings.Settings["Llave"].Value = vSeguridad.EncryptString(Codigo.Text, Llave.Text);
                            config.Save(ConfigurationSaveMode.Modified);
                            ConfigurationManager.RefreshSection("appSettings");

                            mainWindow.Conexion.IsEnabled = true;
                            mainWindow.Turnero.IsEnabled = true;
                            mainWindow.EditarDiseno.IsEnabled = true;
                            mainWindow.ResizeMode = ResizeMode.CanResize;

                            Mensajes dialog = new Mensajes();
                            dialog.lblNombre.Content = "¡Listo!";
                            dialog.lblTexto.Text = "Su producto se activo correctamente. \n Fecha: "+Convert.ToDateTime(subs[4]);
                            dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                            dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                            dialog.ShowDialog();
                            return true;
                        }
                        else
                        {
                            Mensajes dialog = new Mensajes();
                            dialog.lblNombre.Content = "¡Error!";
                            dialog.lblTexto.Text = "Su licencia ha caducado. \n Fecha: "+Convert.ToDateTime(subs[4]);
                            dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                            dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                            dialog.ShowDialog();
                        }
                    }
                    else
                    {
                        //Create the object
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        config.AppSettings.Settings["Correo"].Value = vSeguridad.EncryptString(Codigo.Text, Correo.Text);
                        config.AppSettings.Settings["CodigoActivacion"].Value = Codigo.Text;
                        config.AppSettings.Settings["Llave"].Value = vSeguridad.EncryptString(Codigo.Text, Llave.Text);
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");

                        mainWindow.Conexion.IsEnabled = true;
                        mainWindow.Turnero.IsEnabled = true;
                        mainWindow.EditarDiseno.IsEnabled = true;
                        mainWindow.ResizeMode = ResizeMode.CanResize;

                        Mensajes dialog = new Mensajes();
                        dialog.lblNombre.Content = "¡Listo!";
                        dialog.lblTexto.Text = "Su producto se activo correctamente de forma permante. ";
                        dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                        dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                        dialog.ShowDialog();
                        return true;
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
            else
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Los datos ingresados no son correctos";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
            return false;
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidEmail(Correo.Text))
                Codigo.Text = vSeguridad.EncryptString(Correo.Text, vSeguridad.numeroSerieHD() + "|" +
                                                                    vSeguridad.numeroSeriePlacaBase() + "|" +
                                                                    MainWindow.nombreApp);
            else
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ingrese un correo electrónico valido";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            } 
        }

        private void btnCopiar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Codigo.Text);
            }
            catch (Exception)
            {

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
