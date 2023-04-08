using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
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
    /// Lógica de interacción para ConfigurarConexion.xaml
    /// </summary>
    public partial class ConfigurarConexion : Window
    {
        private Seguridad vSeguridad = new Seguridad();
        public ConfigurarConexion()
        {
            InitializeComponent();
            DataContext = new ViewModel();
            CargarInfo();
            FocusManager.SetFocusedElement(this, cbxODBC);
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
            GuardarInfo();
            Close();
        }

        private void btnProbar_Click(object sender, RoutedEventArgs e)
        {
            OdbcConnection connection = new OdbcConnection("DSN=" + cbxODBC.Text + ";uid=" + Usuario.Text + ";pwd=" + Contrasena.Password);
            try
            {
                connection.Open();
                connection.Close();
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Listo!";
                dialog.lblTexto.Text = "Pruebas completadas correctamente";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "No se puede establecer la conexión: \n"+ ex.Message;
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
        }

        private void CargarInfo()
        {
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cbxODBC.SelectedItem = config.AppSettings.Settings["ODBC"].Value;
            Usuario.Text = config.AppSettings.Settings["UsuarioODBC"].Value;
            Contrasena.Password = vSeguridad.DecryptString(config.AppSettings.Settings["CodigoActivacion"].Value, config.AppSettings.Settings["ContrasenaODBC"].Value);
        }

        private void GuardarInfo()
        {
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["ODBC"].Value = cbxODBC.Text;
            config.AppSettings.Settings["UsuarioODBC"].Value = Usuario.Text;
            config.AppSettings.Settings["ContrasenaODBC"].Value = vSeguridad.EncryptString(config.AppSettings.Settings["CodigoActivacion"].Value, Contrasena.Password);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                GuardarInfo();
                Close();
            }
        }
    }
    public class ViewModel
    {
        public ObservableCollection<string> CmbContent { get; private set; }

        public ViewModel()
        {
            CmbContent = new ObservableCollection<string>();
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\ODBC\ODBC.INI\ODBC Data Sources");
            if (regKey != null)
                foreach (string name in regKey.GetValueNames())
                    CmbContent.Add(name);
            regKey = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\ODBC\ODBC.INI\ODBC Data Sources");
            if (regKey != null)
                foreach (string name in regKey.GetValueNames())
                    CmbContent.Add(name);

        }
    }
}
