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
            CargarLlaveTemp();
            mainWindow = pmainWindow;
            FocusManager.SetFocusedElement(this, Llave);
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

        private void CargarLlaveTemp()
        {
            Seguridad vSeguridad = new Seguridad();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string Codigo = config.AppSettings.Settings["CodigoActivacion"].Value;
            string Correo = vSeguridad.DecryptString(Codigo, config.AppSettings.Settings["Correo"].Value);
            string LlaveTemp = vSeguridad.DecryptString(Codigo, config.AppSettings.Settings["LlaveTemp"].Value);
            if (LlaveTemp.Length > 0)
            {
                try
                {
                    string cadena = vSeguridad.DecryptString(Codigo, LlaveTemp);
                    string[] subs = cadena.Split('|');
                    if (subs.Length >= 3)
                    {
                        if (subs[0].Equals(Correo) && subs[1].Equals(vSeguridad.numeroSerieHD()) && subs[2].Equals(vSeguridad.numeroSeriePlacaBase())
                            && subs[3].Equals(MainWindow.nombreApp))
                        {
                                Llave.Text = LlaveTemp;
                                if (!subs[4].Equals("0"))
                                {
                                    if (vSeguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[4]))
                                    {
                                        Llave.IsReadOnly = true;
                                        lblFecha.Foreground= new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                                        lblFecha.Content = "Fecha licencia: " + Convert.ToDateTime(subs[4]).Date.ToShortDateString();
                                    }
                                    else
                                    {
                                        Llave.Text = "";
                                        lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                                        lblFecha.Content = "Fecha licencia caducada: " + Convert.ToDateTime(subs[4]).Date.ToShortDateString();
                                    }

                                }
                                else
                                {
                                    Llave.IsReadOnly = true;
                                    lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                                    lblFecha.Content = "Fecha licencia: Permanete";
                                }
                            }
                    }
                }
                catch { }
            }       
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
                            && subs[3].Equals(MainWindow.nombreApp))
                        {
                            if(vSeguridad.GetNetworkTime().Date != new DateTime(1900, 1, 1))
                            {
                                if (!subs[4].Equals("0"))
                                {
                                    if (vSeguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[4]))
                                    {
                                        config.AppSettings.Settings["LlaveTemp"].Value = vSeguridad.EncryptString(Codigo, Llave.Text);
                                        config.Save(ConfigurationSaveMode.Modified);
                                        ConfigurationManager.RefreshSection("appSettings");

                                        DialogResult = true;
                                        Close();
                                    }
                                    else
                                    {
                                        Mensajes dialog = new Mensajes();
                                        dialog.lblNombre.Content = "¡Error!";
                                        dialog.lblTexto.Text = "La licencia ha caducado";
                                        dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                                        dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                                        dialog.ShowDialog();
                                    }
                                }
                                else
                                {
                                    config.AppSettings.Settings["LlaveTemp"].Value = vSeguridad.EncryptString(Codigo, Llave.Text);
                                    config.Save(ConfigurationSaveMode.Modified);
                                    ConfigurationManager.RefreshSection("appSettings");

                                    DialogResult = true;
                                    Close();
                                }

                            }
                            else
                            {
                                Mensajes dialog = new Mensajes();
                                dialog.lblNombre.Content = "¡Error!";
                                dialog.lblTexto.Text = "Se requiere de una conexión a internet para poder entrar en modo edición";
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
