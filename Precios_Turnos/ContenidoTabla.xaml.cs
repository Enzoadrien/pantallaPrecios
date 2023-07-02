using Microsoft.Win32;
using Precios_Turnos.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.IO;
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
            CargarComboCampoMostrar();
            CargarInfo();
            
            FocusManager.SetFocusedElement(this, Consulta);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        private void CargarComboCampoMostrar()
        {
            foreach (var itemObjets in mainWindow.Principal.Children)
            {
                
                switch (itemObjets.GetType().Name)
                {
                    case "Label":
                        string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                        if (mainWindow.SeModificaControl(nombreControl))
                            cbxLabels.Items.Add(nombreControl);
                        break;
                }
            }
        }

        private void CargarInfo()
        {
            DirectoryInfo info = new DirectoryInfo(@".\objetos\consultasSQL");
            foreach (var file in info.GetFiles())
            {
                if (@file.Name.Equals(NombreControl + ".sql"))
                {
                    StreamReader sR = new StreamReader(@file.FullName);
                    string lectura = sR.ReadToEnd();
                    sR.Close();
                    string[] datos = new Seguridad().DecryptString(MainWindow.nombreApp, lectura).Split('|');
                    Consulta.Text = datos[0];
                    if (datos.Length > 1)
                    {
                        chkOrganizar.IsChecked = true;
                        Organizar.Text = datos[1];
                        if (datos.Length > 2)
                        {
                            chkMostrarTitulo.IsChecked = true;
                            cbxLabels.SelectedItem = datos[2];
                        }
                    }
                    else
                    {
                        lblOrganizar.Visibility = Visibility.Hidden;
                        Organizar.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void btnProbar_Click(object sender, RoutedEventArgs e)
        {
            CargarTabla();
        }
       
        private bool CargarTabla()
        {
            if (chkOrganizar.IsChecked == false || (chkOrganizar.IsChecked == true && Organizar.Text.Length > 0 && Consulta.Text.Contains(Organizar.Text)))
            {

                Seguridad vSeguridad = new Seguridad();

                string cadenaGuardar = string.Empty;

                if (chkOrganizar.IsChecked == true)
                {
                    cadenaGuardar = Consulta.Text + "|" + Organizar.Text;
                    if(chkMostrarTitulo.IsChecked == true)
                        cadenaGuardar += "|" + cbxLabels.SelectedItem;
                }  
                else
                    cadenaGuardar = Consulta.Text;


                GuardarInfo(new Seguridad().EncryptString(MainWindow.nombreApp, cadenaGuardar), NombreControl);


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
                List<DataTable> tablas = mainWindow.CargarListaTablas(NombreControl, control.Tag.ToString(), true);
                control.ItemsSource = tablas[0].DefaultView;
                //control.UpdateLayout();
                if (chkMostrarTitulo.IsChecked == true)
                {
                    if (cbxLabels.SelectedIndex != -1)
                    {
                       Label item = (Label)mainWindow.FindName(cbxLabels.SelectedItem.ToString());
                        item.Content = tablas[0].TableName;
                    }
                    
                }
                    

                mainWindow.ColorFuenteFondoTabla(NombreControl, control.Tag.ToString());
                return true;
            }
            else
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "El campo a organizar no existe en la consulta.";
                dialog.ShowDialog();
            }
            return false;
        }

        private bool GuardarInfo(string pvStrConsulta, string pvStrNombreObjeto)
        {
            try
            {
                using (Stream stream = new FileStream(@".\objetos\consultasSQL\" + pvStrNombreObjeto + ".sql", FileMode.Create))
                {
                    stream.SetLength(0);
                    byte[] bytes = Encoding.UTF8.GetBytes(pvStrConsulta);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                    return true;
                }
            }
            catch { return false; }
        }

        private void chkOrganizar_Checked(object sender, RoutedEventArgs e)
        {
            lblOrganizar.Visibility = Visibility.Visible;
            Organizar.Visibility = Visibility.Visible;
            chkMostrarTitulo.Visibility = Visibility.Visible;

        }

        private void chkOrganizar_Unchecked(object sender, RoutedEventArgs e)
        {
            lblOrganizar.Visibility = Visibility.Hidden;
            Organizar.Visibility = Visibility.Hidden;
            Organizar.Text = "";
            chkMostrarTitulo.IsChecked = false;
            chkMostrarTitulo.Visibility = Visibility.Hidden;
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
            string s = (string)e.DataObject.GetData(typeof(string));
            if (!TextAllowed(s)) e.CancelCommand();
        }

        private void chkMostrarTitulo_Checked(object sender, RoutedEventArgs e)
        {
            lblCampoTitulo.Visibility = Visibility.Visible;
            cbxLabels.Visibility= Visibility.Visible;
        }

        private void chkMostrarTitulo_Unchecked(object sender, RoutedEventArgs e)
        {
            lblCampoTitulo.Visibility = Visibility.Hidden;
            cbxLabels.Visibility = Visibility.Hidden;
        }
    }
}
