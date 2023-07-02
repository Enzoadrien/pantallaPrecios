using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Numerics;
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
            FocusManager.SetFocusedElement(this, Correo);
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
            if (GuardarInfo())
                Close();
        }

        private void CargarInfo()
        {
            Licencia licencia = new Licencia();
            try
            {
                using (Stream stream = new FileStream(@".\Llave.key", FileMode.Open))
                {
                    var sr = new StreamReader(stream);

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        try
                        {
                            licencia = JsonSerializer.Deserialize<Licencia>(line)!;
                        }
                        catch { }

                    }
                    stream.Close();
                }
            }
            catch { }

            if (licencia.Correo != null && licencia.Codigo != null && licencia.Llave != null && licencia.Key != null)
            {
                Codigo.Text = licencia.Codigo;
                Correo.Text = vSeguridad.DecryptString(licencia.Codigo, licencia.Correo);
                Llave.Text = licencia.Llave;

                string cadena = vSeguridad.DecryptString(Codigo.Text, licencia.Key);
                string[] subs = cadena.Split('|');
                if (subs.Length > 1)
                {
                    if (subs[0].Equals(Correo.Text) && subs[3].Equals(vSeguridad.numeroSerieHD()) && subs[4].Equals(vSeguridad.numeroSeriePlacaBase())
                        && subs[5].Equals(MainWindow.nombreApp))
                    {
                        try
                        {
                            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Lista de precios 3K", true);
                            string LlaveReg = key.GetValue("Key").ToString();
                            Licencia licencia2 = JsonSerializer.Deserialize<Licencia>(LlaveReg)!;

                            if (licencia.Correo.Equals(licencia2.Correo) || licencia.Codigo.Equals(licencia2.Codigo)
                                || licencia.Llave.Equals(licencia2.Llave) || licencia.Key.Equals(licencia2.Key))
                            {
                                if (!subs[1].Equals("0"))
                                {
                                    if (vSeguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[6]).Date)
                                    {
                                        Correo.IsReadOnly = true;
                                        Llave.IsReadOnly = true;
                                        btnGenerar.IsEnabled = false;
                                        btnOK.IsEnabled = false;
                                        lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                                        lblFecha.Content = "Licencia: " + Convert.ToDateTime(subs[6]).Date.ToShortDateString();

                                    }
                                    else
                                    {
                                        Llave.Text = "";
                                        lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                                        lblFecha.Content = "Licencia caducada: " + Convert.ToDateTime(subs[6]).Date.ToShortDateString();
                                    }

                                }
                                else
                                {
                                    Correo.IsReadOnly = true;
                                    Llave.IsReadOnly = true;
                                    btnGenerar.IsEnabled = false;
                                    btnOK.IsEnabled = false;
                                    lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                                    lblFecha.Content = "Licencia: Permanete";
                                }
                            }
                            else
                            {
                                Llave.Text = "";
                                lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                                lblFecha.Content = "Licencia no válida";
                            }
                        }
                        catch
                        {
                            Llave.Text = "";
                            lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                            lblFecha.Content = "Licencia no válida";
                        }
                    }
                }
            }
        }

        private bool GuardarInfo()
        {
            string cadena = vSeguridad.DecryptString(Codigo.Text, Llave.Text);
            string[] subs = cadena.Split('|');
            if (subs.Length > 0)
            {
                if (subs[0].Equals(Correo.Text) && subs[3].Equals(vSeguridad.numeroSerieHD()) && subs[4].Equals(vSeguridad.numeroSeriePlacaBase())
                    && subs[5].Equals(MainWindow.nombreApp))
                {

                    string strKey = vSeguridad.EncryptString(Codigo.Text, cadena + '|' + DateTime.Now.Date.AddDays(int.Parse(subs[1])).ToShortDateString());
                    if (!subs[1].Equals("0"))
                    {
                        if (vSeguridad.GetNetworkTime().Date != new DateTime(1900, 1, 1)
                            && vSeguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[2]).AddDays(30))
                        {
                            Licencia licencia = new Licencia();
                            try
                            {
                                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Lista de precios 3K", true);
                                string LlaveReg = key.GetValue("Key").ToString();
                                licencia = JsonSerializer.Deserialize<Licencia>(LlaveReg)!;
                            }
                            catch { }

                            if (Correo.Text.Equals(vSeguridad.DecryptString(Codigo.Text, licencia.Correo)) && Codigo.Text.Equals(licencia.Codigo) && Llave.Text.Equals(licencia.Llave))
                            {

                                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                                dialog.lblNombre.Content = "¡Error!";
                                dialog.lblTexto.Text = "Esta licencia ya se utilizó en este equipo.";
                                dialog.ShowDialog();
                            }
                            else
                            {
                                GuardarLicencia(strKey);
                                mainWindow.Conexion.IsEnabled = true;
                                mainWindow.Turnero.IsEnabled = true;
                                mainWindow.EditarDiseno.IsEnabled = true;
                                mainWindow.ResizeMode = ResizeMode.CanResize;
                                mainWindow.CargarControles();

                                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ACEPTAR);
                                dialog.lblNombre.Content = "¡Listo!";
                                dialog.lblTexto.Text = "Su producto se activo correctamente. \n Fecha: " + DateTime.Now.Date.AddDays(double.Parse(subs[1]));
                                dialog.ShowDialog();
                                return true;
                            }
                        }
                        else
                        {
                            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                            dialog.lblNombre.Content = "¡Error!";
                            dialog.lblTexto.Text = "Su licencia ha caducado. \n Las licencias tienen un tiempo maximo de activación de 30 días despues de ser generadas.";
                            dialog.ShowDialog();
                        }
                    }
                    else
                    {
                        GuardarLicencia(strKey);

                        mainWindow.Conexion.IsEnabled = true;
                        mainWindow.Turnero.IsEnabled = true;
                        mainWindow.EditarDiseno.IsEnabled = true;
                        mainWindow.ResizeMode = ResizeMode.CanResize;
                        mainWindow.CargarControles();

                        Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ACEPTAR);
                        dialog.lblNombre.Content = "¡Listo!";
                        dialog.lblTexto.Text = "Su producto se activo correctamente de forma permante.";
                        dialog.ShowDialog();
                        return true;
                    }
                }
            }
            Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
            dialogError.lblNombre.Content = "¡Error!";
            dialogError.lblTexto.Text = "Licencia no válida.";
            dialogError.ShowDialog();
            return false;
        }

        private void GuardarLicencia(string pStrKey)
        {
            var Licencia = new Licencia
            {
                Correo = vSeguridad.EncryptString(Codigo.Text, Correo.Text),
                Codigo = Codigo.Text,
                Llave = Llave.Text,
                Key = pStrKey
            };

            string jsonString = JsonSerializer.Serialize(Licencia);

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Lista de precios 3K", true);
            key.SetValue("Key", jsonString);

            using (Stream stream = new FileStream(@".\Llave.key", FileMode.Create))
            {
                stream.SetLength(0);
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

        }

        private bool IsValidEmail(string email)
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
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ingrese un correo electrónico valido.";
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
