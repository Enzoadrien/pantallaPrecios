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
            if (CargarTabla())
                Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if(CargarTabla())
                    Close();
            }
        }

        private void CargarInfo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[NombreControl] != null)
            {
                Seguridad vSeguridad = new Seguridad();
                string[] datos = vSeguridad.DecryptString(mainWindow.nombreApp, config.AppSettings.Settings[NombreControl].Value).Split('|');
                Consulta.Text = datos[0];
                if (datos.Length > 1)
                {
                    chkImagen.IsChecked = true;
                    Imagen.Text = datos[1];
                }
                else
                {
                    lblImagen.Visibility = Visibility.Hidden;
                    Imagen.Visibility = Visibility.Hidden;
                }

            }
        }

        private void btnProbar_Click(object sender, RoutedEventArgs e)
        {
            CargarTabla();
        }
        private bool CargarTabla()
        {
            if (chkImagen.IsChecked == false || (chkImagen.IsChecked == true && Imagen.Text.Length > 0 && Consulta.Text.Contains(Imagen.Text)))
            {

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                Seguridad vSeguridad = new Seguridad();

                if (config.AppSettings.Settings[NombreControl] == null)
                {
                    if (chkImagen.IsChecked == true)
                        config.AppSettings.Settings.Add(NombreControl, vSeguridad.EncryptString(mainWindow.nombreApp, Consulta.Text + "|" + Imagen.Text));
                    else
                        config.AppSettings.Settings.Add(NombreControl, vSeguridad.EncryptString(mainWindow.nombreApp, Consulta.Text));
                }
                else
                {
                    if (chkImagen.IsChecked == true)
                        config.AppSettings.Settings[NombreControl].Value = vSeguridad.EncryptString(mainWindow.nombreApp, Consulta.Text + "|" + Imagen.Text);
                    else
                        config.AppSettings.Settings[NombreControl].Value = vSeguridad.EncryptString(mainWindow.nombreApp, Consulta.Text);
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
                control.ItemsSource = mainWindow.CargarListaTablas(NombreControl, control.Tag.ToString())[0].DefaultView;
                control.UpdateLayout();
                mainWindow.ColorFuenteFondoTabla(NombreControl, control.Tag.ToString());
                return true;
            }
            else
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "El campo de la imagen no existe en la consulta";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
            return false;
        }

        private void chkImagen_Checked(object sender, RoutedEventArgs e)
        {
            lblImagen.Visibility = Visibility.Visible;
            Imagen.Visibility = Visibility.Visible;
        }

        private void chkImagen_Unchecked(object sender, RoutedEventArgs e)
        {
            lblImagen.Visibility = Visibility.Hidden;
            Imagen.Visibility = Visibility.Hidden;
            Imagen.Text = "";
        }
        private void ResponseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            var item = e.Source as UIElement;
            if (e.Key == Key.Space && item.IsFocused == true)
                e.Handled = true;
        }
        private Boolean TextAllowed(String s)
        {
            string strAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçñÑ";
            foreach (Char c in s.ToCharArray())
            {
                if (strAcentos.IndexOf(c) > 0)
                    return false;
                else if (Char.IsLetterOrDigit(c) || Char.IsControl(c))
                    continue;
                else
                    return false;
            }
            return true;
        }

        private void ResponseTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            e.Handled = !TextAllowed(e.Text);
        }
        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            // more error handling would be needed here - this is asking for trouble!
            String s = (String)e.DataObject.GetData(typeof(String));
            if (!TextAllowed(s)) e.CancelCommand();
        }
    }
}
