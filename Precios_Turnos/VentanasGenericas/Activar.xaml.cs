using Microsoft.Win32;
using Priceio.ClasesGenericas;
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


namespace Priceio
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
            if (Codigo.Text.Length > 0 && Correo.Text.Length > 0)
            {
                ValidarLicencia validarLicencia = new ValidarLicencia();
                string key = validarLicencia.cargarLicenciaApp(Correo.Text, Codigo.Text);
                Llave.Text = key;
                if (key.Length > 0)
                    if (GuardarInfo())
                        Close();
            }
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
                            if (!subs[1].Equals("0"))
                            {
                                if (vSeguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[6]).Date)
                                {
                                    Correo.IsReadOnly = true;
                                    Llave.IsReadOnly = true;
                                    btnGenerar.IsEnabled = false;
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
                                lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                                lblFecha.Content = "Licencia: Permanente";
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
                        if (vSeguridad.GetNetworkTime().Date != new DateTime(1900, 1, 1))
                        {

                            GuardarLicencia(strKey);
                            mainWindow.ActivarControlesMenu();
                            mainWindow.CargarControles();
                            mainWindow.CargarArchivoLicencia();

                            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ACEPTAR);
                            dialog.lblNombre.Content = "¡Listo!";
                            dialog.lblTexto.Text = "Su producto se activo correctamente. \n Fecha: " + DateTime.Now.Date.AddDays(double.Parse(subs[1]));
                            dialog.ShowDialog();
                            return true;
                        }
                        else
                        {
                            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                            dialog.lblNombre.Content = "¡Error!";
                            dialog.lblTexto.Text = "Se requiere de una conexión a internet para activar la licencia";
                            dialog.ShowDialog();
                        }
                    }
                    else
                    {
                        GuardarLicencia(strKey);
                        mainWindow.ActivarControlesMenu();
                        mainWindow.CargarControles();
                        mainWindow.CargarArchivoLicencia();

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
