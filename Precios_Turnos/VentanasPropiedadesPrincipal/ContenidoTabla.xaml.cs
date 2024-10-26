using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Priceio;
using System.Linq;
using Azure;
using Priceio.ClasesGenericas;

namespace Priceio
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
                    chkImagen.IsChecked = bool.Parse(datos[1]);
                    ID.Text = datos[2];
                    if (datos.Length > 3)
                    {
                        chkOrganizar.IsChecked = true;
                        Organizar.Text = datos[3];
                        if (datos.Length > 4)
                        {
                            chkMostrarTitulo.IsChecked = true;
                            cbxLabels.SelectedItem = datos[4];
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
            bool validaOrganizar = true;
            if (chkOrganizar.IsChecked == true)
            {
                if(Organizar.Text.Length > 0)
                {
                    var punctuation = Consulta.Text.Where(Char.IsPunctuation).Distinct().ToArray();
                    var words = Consulta.Text.Split().Select(x => x.Trim(punctuation));
                    validaOrganizar = words.Contains(Organizar.Text, StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Debe de escribir el campo a organizar contenido en la consulta.";
                    dialog.ShowDialog();
                    return false;
                }
            }
            if (chkOrganizar.IsChecked == false || validaOrganizar)
            {

                if (chkImagen.IsChecked == true)
                {
                    if (ID.Text.Length > 0)
                    {
                        var punctuation = Consulta.Text.Where(Char.IsPunctuation).Distinct().ToArray();
                        var words = Consulta.Text.Split().Select(x => x.Trim(punctuation));

                        if(!words.Contains(ID.Text, StringComparer.OrdinalIgnoreCase))
                        {
                            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                            dialog.lblNombre.Content = "¡Error!";
                            dialog.lblTexto.Text = "El campo a buscar para imagen no existe en la consulta.";
                            dialog.ShowDialog();
                            return false;
                        }
                    }
                    else
                    {
                        Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                        dialog.lblNombre.Content = "¡Error!";
                        dialog.lblTexto.Text = "Debe de escribir el campo a buscar de la imagen contenido en la consulta.";
                        dialog.ShowDialog();
                        return false;
                    } 

                }

                Seguridad vSeguridad = new Seguridad();

                string cadenaGuardar  = Consulta.Text + "|" + chkImagen.IsChecked + "|" + ID.Text;

                if (chkOrganizar.IsChecked == true)
                {
                    cadenaGuardar +=  "|" + Organizar.Text;
                    if(chkMostrarTitulo.IsChecked == true)
                        cadenaGuardar += "|" + cbxLabels.SelectedItem;
                }  
                    


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
                List<DataTable> tablas = mainWindow.CargarListaTablas(NombreControl, control.Tag.ToString(), ID.Text, true);
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
                if (chkImagen.IsChecked == true)
                {
                    string pNombre = "Img_" + NombreControl;
                    var item = mainWindow.FindName(pNombre) as UIElement;

                    if(item != null)
                    {
                        try
                        {

                            if (datos[2].Equals("V"))
                            {

                                String imagenBuscar = tablas[0].Columns[0].ColumnName;
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.UriSource = new Uri(@".\Objetos\" + NombreControl + "\\" +imagenBuscar + ".png", UriKind.RelativeOrAbsolute);
                                bitmapImage.EndInit();

                                ((Image)item).Source = bitmapImage;
                            }
                            else 
                            {
                                String imagenBuscar = tablas[0].Rows[0][ID.Text].ToString();
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.UriSource = new Uri(@".\Objetos\" + NombreControl + "\\" + imagenBuscar + ".png", UriKind.RelativeOrAbsolute);
                                bitmapImage.EndInit();

                                ((Image)item).Source = bitmapImage;
                            }  

                        }
                        catch
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.UriSource = new Uri(@".\Recursos\pictureAdd.png", UriKind.RelativeOrAbsolute);
                            bitmapImage.EndInit();

                            ((Image)item).Source = bitmapImage;
                        }
                    } 

                }

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

        private void chkImagen_Checked(object sender, RoutedEventArgs e)
        {
            btnAbrir.Visibility = Visibility.Visible;
            lblID.Visibility = Visibility.Visible;
            ID.Visibility = Visibility.Visible;
            //Carpetas de animaciones
            if (!Directory.Exists(@".\objetos\"+NombreControl))
            {
                Directory.CreateDirectory(@".\objetos\"+ NombreControl);
            }
            string pNombre = "Img_"+NombreControl;
            var item = mainWindow.FindName(pNombre) as UIElement;

            if (item == null)
            {
                Image obj = new Image();
                obj.Name = pNombre;
                obj.ToolTip = pNombre;
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.Stretch = Stretch.Uniform;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(@".\Recursos\pictureAdd.png", UriKind.RelativeOrAbsolute);
                bitmapImage.EndInit();

                obj.Source = bitmapImage;
                obj.Height = bitmapImage.Height;
                obj.Tag = "";
                obj.MouseLeave += objetoMedia_MouseLeave;
                obj.MouseEnter += objetoMedia_MouseEnter;

                NameScope.GetNameScope(mainWindow).RegisterName(obj.Name, obj);
                mainWindow.Principal.Children.Add(obj);
            }

        }

        private void objetoMedia_MouseLeave(object sender, MouseEventArgs e)
        {
            var control = e.Source as UIElement;
            control.SetValue(OpacityProperty, mainWindow.ultimaOpacidad);
        }

        private void objetoMedia_MouseEnter(object sender, MouseEventArgs e)
        {
            var control = e.Source as UIElement;
            mainWindow.ultimaOpacidad = control.Opacity;
            control.SetValue(OpacityProperty, mainWindow.ultimaOpacidad > .5 ? mainWindow.ultimaOpacidad - .3 : mainWindow.ultimaOpacidad + .3);
            mainWindow.controlSelectedName = control.GetValue(NameProperty).ToString();
        }

        private void chkImagen_Unchecked(object sender, RoutedEventArgs e)
        {
            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
            dialog.lblNombre.Content = "¡Advertencia!";
            dialog.lblTexto.Text = "Se eliminará de forma permanente todas las imágenes agregadas para mostrar en la tabla. ¿Está seguro que desea continuar?.";

            if (dialog.ShowDialog() == true)
            {
                btnAbrir.Visibility = Visibility.Hidden;
                lblID.Visibility = Visibility.Hidden;
                ID.Visibility = Visibility.Hidden;
                ID.Text = "";
                string pNombre = "Img_"+ NombreControl;
                var item = mainWindow.FindName(pNombre) as UIElement;

                if (item != null)
                {
                    mainWindow.Principal.Children.Remove(item);
                    NameScope.GetNameScope(mainWindow).UnregisterName(pNombre);

                    if (Directory.Exists(@".\objetos\" + NombreControl))
                    {
                        Directory.Delete(@".\objetos\" + NombreControl, true);
                    }
                }
            }
            else
                chkImagen.IsChecked = true;


        }

        private void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA);
            dialog.lblNombre.Content = "¡Alerta!";
            dialog.lblTexto.Text = "Las imágenes cargadas deberán tener por nombre el campo a buscar y deberán estar en formato png";
            dialog.ShowDialog();

            Process.Start("explorer.exe", @".\objetos\" + NombreControl);
        }
    }
}
