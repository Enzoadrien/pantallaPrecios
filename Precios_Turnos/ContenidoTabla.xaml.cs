using Precios_Turnos.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
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
    /// Lógica de interacción para ContenidoTabla.xaml
    /// </summary>
    public partial class ContenidoTabla : Window
    {
        private MainWindow mainWindow;
        private string NombreTabla;
        private int CantidadFilas;
        private int CantidadBloques;
        private string Orientacion;
        public ContenidoTabla(MainWindow pMainWindow, string pNombreTabla, int pCantidadBloques, int pCantidadFilas, string pOrientacion)
        {
            InitializeComponent();
            mainWindow = pMainWindow;
            NombreTabla = pNombreTabla;
            CantidadFilas = pCantidadFilas;
            CantidadBloques = pCantidadBloques;
            Orientacion = pOrientacion;
            CargarInfo();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }


        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            if (Consulta.Text.Length > 0)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings[NombreTabla] == null)
                {
                    config.AppSettings.Settings.Add(NombreTabla, Consulta.Text);
                }
                else
                {
                    config.AppSettings.Settings[NombreTabla].Value = Consulta.Text;
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void CargarInfo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[NombreTabla] != null)
            {
                Consulta.Text = config.AppSettings.Settings[NombreTabla].Value;
            }
        }

        private void btnProbar_Click(object sender, RoutedEventArgs e)
        {
            Seguridad vSeguridad = new Seguridad();
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string odbc = config.AppSettings.Settings["odbc"].Value;
            string usuario = config.AppSettings.Settings["usuarioODBC"].Value;
            string contrasena = vSeguridad.DecryptString(config.AppSettings.Settings["CodigoActivacion"].Value, config.AppSettings.Settings["contrasenaODBC"].Value);

            OdbcConnection connection = new OdbcConnection("DSN=" + odbc + ";uid=" + usuario + ";pwd=" + contrasena);
            try
            {
                connection.Open();
                OdbcCommand MyCommand = new OdbcCommand(Consulta.Text, connection);
                OdbcDataReader MyDataReader = MyCommand.ExecuteReader();
                if (MyDataReader.HasRows)
                {
                    DataGrid control = (DataGrid)mainWindow.FindName(NombreTabla);
                    DataTable dt = new DataTable();
                    dt.Load(MyDataReader);
                    mainWindow.LlenarListaTablas(NombreTabla, CantidadBloques, CantidadFilas, dt, Orientacion);
                    if (Orientacion.Equals("V"))
                    {
                        control.CellStyle = new Style(typeof(DataGridCell))
                        {
                            Setters = {
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center)
                    }
                        };
                    }
                    else
                    {
                        control.CellStyle = new Style(typeof(DataGridCell))
                        {
                            Setters = {
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Left)
                    }
                        };
                    }
                    control.ItemsSource = mainWindow.ListTablas[0].DefaultView;
                }
                else
                {
                    Mensajes dialog = new Mensajes();
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "No existen registros para mostrar";
                    dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                    dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                    dialog.ShowDialog();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Error en la consulta: \n" + ex.Message;
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
        }
    }
}
