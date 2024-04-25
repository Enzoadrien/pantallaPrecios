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
        private Licencia licencia = new Licencia();
        private Seguridad seguridad = new Seguridad();
        private ValidarLicencia validarLicencia = new ValidarLicencia();

        public EntrarDiseno(MainWindow pmainWindow)
        {
            InitializeComponent();
            CargarLicenciaApp();
            mainWindow = pmainWindow;
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
            try
            {
                string key = validarLicencia.cargarLicenciaDiseno(seguridad.DecryptString(licencia.Codigo, licencia.Correo), licencia.Codigo);
                Llave.Text = key;
                if (key.Length > 0)
                    ValidarLicenciaTemp();
            }
            catch{
                Mensaje.Text = "¡Error!\n" + "Ocurrio un error al cargar la licencia de edición, consulte al administrador.";
                Mensaje.Foreground = new SolidColorBrush(Colors.White);
                Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
            }
                
        }

        private void CargarLicenciaApp()
        {
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
        }

        private void ValidarLicenciaTemp()
        {
            if (licencia.Correo != null && licencia.Codigo != null && licencia.Llave != null && licencia.Key != null)
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                string Correo = seguridad.DecryptString(licencia.Codigo, licencia.Correo);
                if (Llave.Text.Length > 0)
                {
                    try
                    {
                        string cadena = seguridad.DecryptString(licencia.Codigo, Llave.Text);
                        string[] subs = cadena.Split('|');
                        if (subs.Length > 0)
                        {
                            if (subs[5].Equals(Correo) && subs[0].Equals(seguridad.numeroSerieHD()) && subs[1].Equals(seguridad.numeroSeriePlacaBase())
                                && subs[2].Equals(MainWindow.nombreApp))
                            {
                                if (!subs[3].Equals("0"))
                                {
                                    if (seguridad.GetNetworkTime().Date != new DateTime(1900, 1, 1))
                                    {
                                        string fechaActivacion = validarLicencia.cargarFechaActivacionLicenciaDiseno(Correo, licencia.Codigo, Llave.Text);

                                        if (fechaActivacion.Length > 0)
                                        {
                                            if(DateTime.Parse(fechaActivacion).Equals(new DateTime(1900, 1, 1)))
                                            {
                                                if(validarLicencia.activarLicenciaDiseno(Correo, licencia.Codigo, Llave.Text))
                                                {
                                                    DialogResult = true;
                                                    Close();
                                                }
                                                else
                                                {
                                                    Mensaje.Text = "¡Error!\n" + "La licencia no es válida, consulte al administrador.";
                                                    Mensaje.Foreground = new SolidColorBrush(Colors.White);
                                                    Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                                                }

                                            }
                                            else
                                            {
                                                if (seguridad.GetNetworkTime().Date <= DateTime.Parse(fechaActivacion).AddDays(int.Parse(subs[3])))
                                                {
                                                    DialogResult = true;
                                                    Close();
                                                }
                                                else
                                                {
                                                    Mensaje.Text = "Su licencia ha caducado.\nSu fecha de vencimiento fue el dia: " + DateTime.Parse(fechaActivacion).AddDays(int.Parse(subs[3])) + ", consulte al administrador.";
                                                    Mensaje.Foreground = new SolidColorBrush(Colors.White);
                                                    Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF6B73C"));
                                                }
                                            }
                                       
                                        }
                                        else {
                                            Mensaje.Text = "¡Error!\n" + "La licencia no es válida, consulte al administrador.";
                                            Mensaje.Foreground = new SolidColorBrush(Colors.White);
                                            Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                                        }                                       
                                    }
                                    else
                                    {
                                        Mensaje.Text = "¡Error!\n" + "Se requiere de una conexión a internet para poder entrar en modo edición, consulte al administrador.";
                                        Mensaje.Foreground = new SolidColorBrush(Colors.White);
                                        Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                                    }

                                }
                                else
                                {
                                    DialogResult = true;
                                    Close();
                                }
                            }
                            else
                            {
                                Mensaje.Text = "¡Error!\n" + "La licencia no es válida, consulte al administrador.";
                                Mensaje.Foreground = new SolidColorBrush(Colors.White);
                                Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                            }
                        }
                        else
                        {
                            Mensaje.Text = "¡Error!\n" + "La licencia no es válida, consulte al administrador.";
                            Mensaje.Foreground = new SolidColorBrush(Colors.White);
                            Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                        }
                    }
                    catch (Exception)
                    {
                        Mensaje.Text = "¡Error!\n" + "La licencia no es válida, consulte al administrador.";
                        Mensaje.Foreground = new SolidColorBrush(Colors.White);
                        Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                    }
                }
                else
                {
                    Mensaje.Text = "¡Error!\n" + "La licencia no es válida, consulte al administrador.";
                    Mensaje.Foreground = new SolidColorBrush(Colors.White);
                    Mensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                }
            }
        }
    }
}
