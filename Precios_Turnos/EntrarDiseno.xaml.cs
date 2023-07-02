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
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void CargarLlaveTemp()
        {
            Seguridad vSeguridad = new Seguridad();

            Licencia licencia = new Licencia();
            try
            {
                using (Stream stream = new FileStream(@".\LlaveEdicion.key", FileMode.Open))
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
                try
                {
                    string cadena = vSeguridad.DecryptString(licencia.Codigo, licencia.Key);
                    string Correo = vSeguridad.DecryptString(licencia.Codigo, licencia.Correo);
                    string[] subs = cadena.Split('|');
                    if (subs.Length > 1)
                    {
                        if (subs[5].Equals(Correo) && subs[0].Equals(vSeguridad.numeroSerieHD()) && subs[1].Equals(vSeguridad.numeroSeriePlacaBase())
                            && subs[2].Equals(MainWindow.nombreApp))
                        {
                            Llave.Text = licencia.Llave;
                            if (!subs[3].Equals("0"))
                            {
                                if (vSeguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[6]))
                                {
                                    Llave.IsReadOnly = true;
                                    lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                                    lblFecha.Content = "Fecha licencia: " + Convert.ToDateTime(subs[6]).Date.ToShortDateString();
                                }
                                else
                                {
                                    Llave.Text = "";
                                    lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                                    lblFecha.Content = "Fecha licencia caducada: " + Convert.ToDateTime(subs[6]).Date.ToShortDateString();
                                }

                            }
                            else
                            {
                                Llave.IsReadOnly = true;
                                lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                                lblFecha.Content = "Fecha licencia: Permanete.";
                            }
                        }
                    }
                    else
                    {
                        Llave.Text = "";
                        lblFecha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                        lblFecha.Content = "La licencia no es válida.";
                    }
                }
                catch { }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!Llave.IsReadOnly)
            {
                Seguridad vSeguridad = new Seguridad();

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
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                    string Correo = vSeguridad.DecryptString(licencia.Codigo, licencia.Correo);
                    if (Llave.Text.Length > 0)
                    {
                        try
                        {
                            string cadena = vSeguridad.DecryptString(licencia.Codigo, Llave.Text);
                            string[] subs = cadena.Split('|');
                            if (subs.Length > 0)
                            {
                                if (subs[5].Equals(Correo) && subs[0].Equals(vSeguridad.numeroSerieHD()) && subs[1].Equals(vSeguridad.numeroSeriePlacaBase())
                                    && subs[2].Equals(MainWindow.nombreApp))
                                {

                                    string strKey = vSeguridad.EncryptString(licencia.Codigo, cadena + '|' + DateTime.Now.Date.AddDays(int.Parse(subs[3])).ToShortDateString());
                                    if (!subs[3].Equals("0"))
                                    {
                                        if (vSeguridad.GetNetworkTime().Date != new DateTime(1900, 1, 1))
                                        {
                                        
                                            if (vSeguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[4]).AddDays(7))
                                            {

                                                Licencia licenciaEdicion = new Licencia();
                                                try
                                                {
                                                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Lista de precios 3K", true);
                                                    string LlaveReg = key.GetValue("KeyEdit").ToString();
                                                    licenciaEdicion = JsonSerializer.Deserialize<Licencia>(LlaveReg)!;
                                                }
                                                catch { }

                                                if (Correo.Equals(vSeguridad.DecryptString(licenciaEdicion.Codigo, licenciaEdicion.Correo)) && licencia.Codigo.Equals(licenciaEdicion.Codigo) && Llave.Text.Equals(licenciaEdicion.Llave))
                                                {
                                                    dialog.lblNombre.Content = "¡Error!";
                                                    dialog.lblTexto.Text = "Esta licencia ya se utilizó en este equipo.";
                                                    dialog.ShowDialog();
                                                }
                                                else
                                                {
                                                    GuardarLicencia(licencia.Codigo, Correo, strKey);
                                                    DialogResult = true;
                                                    Close();
                                                }

                                            }
                                            else
                                            {

                                                dialog.lblNombre.Content = "¡Error!";
                                                dialog.lblTexto.Text = "Su licencia ha caducado. \n Las licencias de edición tienen un tiempo maximo de activación de 7 días despues de ser generadas.";
                                                dialog.ShowDialog();
                                            }
                                        }
                                        else
                                        {

                                            dialog.lblNombre.Content = "¡Error!";
                                            dialog.lblTexto.Text = "Se requiere de una conexión a internet para poder entrar en modo edición.";
                                            dialog.ShowDialog();
                                        }

                                    }
                                    else
                                    {
                                        GuardarLicencia(licencia.Codigo, Correo, strKey);
                                        DialogResult = true;
                                        Close();
                                    }
                                }
                                else
                                {
                                    dialog.lblNombre.Content = "¡Error!";
                                    dialog.lblTexto.Text = "La licencia no es válida.";
                                    dialog.ShowDialog();
                                }
                            }
                            else
                            {
                                dialog.lblNombre.Content = "¡Error!";
                                dialog.lblTexto.Text = "La licencia no es válida.";
                                dialog.ShowDialog();
                            }
                        }
                        catch (Exception)
                        {
                            dialog.lblNombre.Content = "¡Error!";
                            dialog.lblTexto.Text = "La licencia no es válida.";
                            dialog.ShowDialog();
                        }
                    }
                    else
                    {
                        dialog.lblNombre.Content = "¡Error!";
                        dialog.lblTexto.Text = "Debes ingresar todos los datos.";
                        dialog.ShowDialog();
                    }
                }
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }

        private void GuardarLicencia(string pStrCodigo, string pStrCorreo, string pStrKey)
        {
            Seguridad vSeguridad = new Seguridad();

            var Licencia = new Licencia
            {
                Correo = vSeguridad.EncryptString(pStrCodigo, pStrCorreo),
                Codigo = pStrCodigo,
                Llave = Llave.Text,
                Key = pStrKey
            };

            string jsonString = JsonSerializer.Serialize(Licencia);

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Lista de precios 3K", true);
            key.SetValue("KeyEdit", jsonString);

            using (Stream stream = new FileStream(@".\LlaveEdicion.key", FileMode.Create))
            {
                stream.SetLength(0);
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

        }
    }
}
