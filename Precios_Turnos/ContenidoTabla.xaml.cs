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
        private string NombreControl;
        public ContenidoTabla(MainWindow pMainWindow, string pNombreControl)
        {
            InitializeComponent();
            mainWindow = pMainWindow;
            NombreControl = pNombreControl;
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

                if (config.AppSettings.Settings[NombreControl] == null)
                {
                    config.AppSettings.Settings.Add(NombreControl, Consulta.Text);
                }
                else
                {
                    config.AppSettings.Settings[NombreControl].Value = Consulta.Text;
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            CargarTabla();
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CargarTabla();
                Close();
            }
        }

        private void CargarInfo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[NombreControl] != null)
            {
                Consulta.Text = config.AppSettings.Settings[NombreControl].Value;
            }
        }

        private void btnProbar_Click(object sender, RoutedEventArgs e)
        {
            CargarTabla();
        }
        private void CargarTabla()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[NombreControl] == null)
            {
                config.AppSettings.Settings.Add(NombreControl, Consulta.Text);
            }
            else
            {
                config.AppSettings.Settings[NombreControl].Value = Consulta.Text;
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl);
            string[] datos = control.Tag.ToString().Split('|');
            if (datos[2].Equals("V"))
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
            control.ItemsSource = mainWindow.CargarTabla(NombreControl, control.Tag.ToString()).DefaultView;
            control.UpdateLayout();
            mainWindow.ColorFuenteFondoTabla(NombreControl, control.Tag.ToString());
        }
    }
}
