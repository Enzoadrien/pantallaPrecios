using Microsoft.Win32;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.ObjectModel;
using System.Data.Odbc;
using System.Windows;
using System.Windows.Input;

namespace Priceio
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
            CargarDatos();
            FocusManager.SetFocusedElement(this, cbxODBC);
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
            GuardarDatos();
            Close();
        }

        private void btnProbar_Click(object sender, RoutedEventArgs e)
        {
            OdbcConnection connection = new OdbcConnection("DSN=" + cbxODBC.Text + ";uid=" + Usuario.Text + ";pwd=" + Contrasena.Password);
            try
            {
                connection.Open();
                connection.Close();
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ACEPTAR);
                dialog.lblNombre.Content = "¡Listo!";
                dialog.lblTexto.Text = "Pruebas completadas correctamente.";
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "No se puede establecer la conexión: \n" + ex.Message;
                dialog.ShowDialog();
            }
        }

        private void CargarDatos()
        {
            ConfiguracionODBC? SQLiteClass = new SQLiteClassManager().GetConfiguracionODBC();
            if (SQLiteClass != null)
            {
                cbxODBC.SelectedItem = SQLiteClass.ODBC;
                Usuario.Text = SQLiteClass.UsuarioODBC;
                Contrasena.Password = SQLiteClass.ContrasenaODBC;
            }
        }

        private void GuardarDatos()
        {
            ConfiguracionODBC SQLiteClass = new ConfiguracionODBC();
            SQLiteClass.ODBC = cbxODBC.Text;
            SQLiteClass.UsuarioODBC = Usuario.Text;
            SQLiteClass.ContrasenaODBC = vSeguridad.EncryptString(MainWindow.nombreApp, Contrasena.Password);
            if (!new SQLiteClassManager().SetConfiguracionODBC(SQLiteClass))
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                GuardarDatos();
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
            RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(@"Software\ODBC\ODBC.INI\ODBC Data Sources");
            if (regKey != null)
                foreach (string name in regKey.GetValueNames())
                    CmbContent.Add(name);

            regKey = Registry.LocalMachine.OpenSubKey(@"Software\ODBC\ODBC.INI\ODBC Data Sources");
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
