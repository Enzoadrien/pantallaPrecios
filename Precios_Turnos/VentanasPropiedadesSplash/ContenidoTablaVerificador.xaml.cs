using Priceio;
using Priceio.ClasesGenericas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para ContenidoTabla.xaml
    /// </summary>
    public partial class ContenidoTablaVerificador : Window
    {
        private MostrarVentanaSplash mainWindow;
        private string NombreControl;
        public ContenidoTablaVerificador(MostrarVentanaSplash pMainWindow, string pNombreControl)
        {
            InitializeComponent();
            mainWindow = pMainWindow;
            NombreControl = pNombreControl;
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
    
        private void CargarInfo()
        {
            DirectoryInfo info = new DirectoryInfo(@".\objetosSplash\consultasSQL");
            foreach (var file in info.GetFiles())
            {
                if (@file.Name.Equals(NombreControl + ".sql"))
                {
                    StreamReader sR = new StreamReader(@file.FullName);
                    string lectura = sR.ReadToEnd();
                    sR.Close();
                    string[] datos = new Seguridad().DecryptString(MainWindow.nombreApp, lectura).Split('|');
                    if (datos.Length > 1)
                    {
                        Consulta.Text = datos[0];
                        cbxTipoCampo.SelectedValue = datos[1];
                        DatoPrueba.Text = datos[2];
                        chkImagen.IsChecked = bool.Parse(datos[3]);
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
                Seguridad vSeguridad = new Seguridad();

                string cadenaGuardar = string.Empty;
          
                cadenaGuardar = Consulta.Text + "|" + ((ComboBoxItem)cbxTipoCampo.SelectedItem).Tag.ToString() +"|" + DatoPrueba.Text +"|" + chkImagen.IsChecked;

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

            string pNombre = "ImgTablaDatos";
            var item = mainWindow.FindName(pNombre) as UIElement;

            if (item != null)
            {

                try 
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = new Uri(@".\objetosSplash\TablaDatos\" + DatoPrueba.Text + ".png", UriKind.RelativeOrAbsolute);
                    bitmapImage.EndInit();

                    ((Image)item).Source = bitmapImage;
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

            mainWindow.ColorFuenteFondoTabla(NombreControl, control.Tag.ToString());
                return true;
        }

        private bool GuardarInfo(string pvStrConsulta, string pvStrNombreObjeto)
        {
            try
            {
                using (Stream stream = new FileStream(@".\objetosSplash\consultasSQL\" + pvStrNombreObjeto + ".sql", FileMode.Create))
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

        private void chkImagen_Checked(object sender, RoutedEventArgs e)
        {
            btnAbrir.Visibility = Visibility.Visible;
            //Carpetas de animaciones
            if (!Directory.Exists(@".\objetosSplash\TablaDatos"))
            {
                Directory.CreateDirectory(@".\objetosSplash\TablaDatos");
            }
            string pNombre = "ImgTablaDatos";
            var item = mainWindow.FindName(pNombre) as UIElement;

            if (item == null)
            {
                Image obj = new Image();
                obj.Name = "ImgTablaDatos";
                obj.ToolTip = "ImgTablaDatos";
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
                string pNombre = "ImgTablaDatos";
                var item = mainWindow.FindName(pNombre) as UIElement;

                if (item != null)
                {
                    mainWindow.Principal.Children.Remove(item);
                    NameScope.GetNameScope(mainWindow).UnregisterName(pNombre);

                    if (Directory.Exists(@".\objetosSplash\TablaDatos"))
                    {
                        Directory.Delete(@".\objetosSplash\TablaDatos", true);
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

            Process.Start("explorer.exe", @".\objetosSplash\TablaDatos");
        }
    }
}
