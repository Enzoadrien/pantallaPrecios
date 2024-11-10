using System;
using System.Collections.Generic;
using System.Configuration;
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
using Priceio.ClasesGenericas;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para PropiedadesFondo.xaml
    /// </summary>
    public partial class PropiedadesVentanaSplash : Window
    {
        private MostrarVentanaSplash mainWindow;
        internal string nombreControl;
        public PropiedadesVentanaSplash(MostrarVentanaSplash pmainWindow)
        {
            mainWindow = pmainWindow;
            InitializeComponent();
            CargarDatos();
            FocusManager.SetFocusedElement(this, btnColorFondo);
        }
        
        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void CargarDatos()
        {
            Ancho.Text = mainWindow.Width.ToString();
            Alto.Text = mainWindow.Height.ToString();
        }

        private void btnColorFondoBorde_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border control = (Border)mainWindow.FindName(nombreControl);
            ColorPicker colorPicker = new ColorPicker(mainWindow, mainWindow.ultimoColorLetra, mainWindow.ultimoColorFondo, (control.BorderBrush as SolidColorBrush).Color);
            // get the parent container

            // get the position within the container
            var mousePosition = e.GetPosition(mainWindow.Principal);

            if (mousePosition.Y + colorPicker.Height >= mainWindow.MaxHeight)
                colorPicker.Top = mousePosition.Y - colorPicker.Height;
            else
                colorPicker.Top = mousePosition.Y;

            if (mousePosition.X + colorPicker.Width >= mainWindow.MaxWidth)
                colorPicker.Left = mousePosition.X - colorPicker.Width;
            else
                colorPicker.Left = mousePosition.X;

            if ((bool)colorPicker.ShowDialog())
            {
                btnColorBorde.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                control.BorderBrush = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.ultimoColorFondo = colorPicker.SelectedColor;
            }
        }

        private void cbxGrosor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Border control = (Border)mainWindow.FindName(nombreControl);
            control.BorderThickness = new Thickness(int.Parse(((ComboBoxItem)cbxGrosor.SelectedItem).Tag.ToString()));
        }

        private void Ancho_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double ancho = double.Parse(Ancho.Text);
                if (mainWindow.Width != ancho)
                {
                    mainWindow.Width = ancho;
                }
            }
            catch{}
           
                

        }

        private void Alto_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double alto = double.Parse(Alto.Text);
            if (mainWindow.Height != alto)
            {
                mainWindow.Height = alto;
            }
               }
            catch{}
           
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

        private void ResponseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;

            if (e.Key == Key.Space && cajaTexto.IsFocused == true)
                e.Handled = true;
        }
        private Boolean TextAllowed(String s)
        {
            foreach (Char c in s.ToCharArray())
            {
                if (Char.IsDigit(c)) continue;
                else return false;
            }
            return true;
        }

        private void btnColorFondo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SolidColorBrush colorBase = new SolidColorBrush(Colors.White);
            ColorPicker colorPicker = new ColorPicker(mainWindow, mainWindow.ultimoColorLetra, mainWindow.ultimoColorFondo, (colorBase as SolidColorBrush).Color);
            // get the parent container

            // get the position within the container
            var mousePosition = e.GetPosition(mainWindow.Principal);

            if (mousePosition.Y + colorPicker.Height >= mainWindow.MaxHeight)
                colorPicker.Top = mousePosition.Y - colorPicker.Height;
            else
                colorPicker.Top = mousePosition.Y;

            if (mousePosition.X + colorPicker.Width >= mainWindow.MaxWidth)
                colorPicker.Left = mousePosition.X - colorPicker.Width;
            else
                colorPicker.Left = mousePosition.X;

            if ((bool)colorPicker.ShowDialog())
            {
                btnColorFondo.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.Fondo.Source = null;
                ContenidoTextBox.Text = "";
                ContenidoTextBox.ToolTip = "";
                mainWindow.Base.Background = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.ultimoColorFondo = colorPicker.SelectedColor;
                Opacidad.IsEnabled = false;
            }
        }

        private void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            //dlg.DefaultExt = ".png";
            dlg.Filter = "Todos los archivos de imagen|*.jpeg;*.jpg;*.png|JPEG (*.jpeg;*.jpg)|*.jpeg;*.jpg|PNG (*.png)|*.png";


            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                FileInfo fi = new FileInfo(dlg.FileName);
                try
                {
                    FileInfo fileImg = new FileInfo(@".\data\objetosSplash\multimedia\" + fi.Name);
                    if (File.Exists(@".\data\objetosSplash\multimedia\" + fi.Name) && !fi.FullName.Equals(fileImg.FullName))
                    {
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, "Remplazar", "Mantener");
                        dialogMsg.lblNombre.Content = "¡Advertencia!";
                        dialogMsg.lblTexto.Text = "Ya existe un archivo con el mismo nombre y extension en la aplicación, ¿Desea remplazarlo o mantener la actual?. ¡Esta accion no se puede revertir!";
                        if (dialogMsg.ShowDialog() == true)
                        {
                            fi.CopyTo(@".\data\objetosSplash\multimedia\" + fi.Name, true);
                        }
                    }
                    else
                        fi.CopyTo(@".\data\objetosSplash\multimedia\" + fi.Name, true);
                }
                catch
                {
                }
                FileInfo Img = new FileInfo(@".\data\objetosSplash\multimedia\" + fi.Name);
                // Open document 
                ContenidoTextBox.Text = Img.Name;
                ContenidoTextBox.ToolTip = Img.Name;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@".\data\objetosSplash\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                image.EndInit();
                mainWindow.Fondo.Source = image;
                Opacidad.IsEnabled = true;
                Opacidad.Value = 1;
            }

        }

        private void Opacidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mainWindow.Fondo.Opacity = Opacidad.Value;
        }

        private void Opacidad_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int change = e.Delta / Math.Abs(e.Delta);
            Opacidad.Value = Opacidad.Value + (double)change / 10;
        }
    }
}
