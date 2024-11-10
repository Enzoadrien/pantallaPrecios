using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
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
using System.Xml.Linq;
using Priceio.ClasesGenericas;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para PropiedadesFondo.xaml
    /// </summary>
    public partial class PropiedadesFondo : Window
    {
        private MainWindow mainWindow;
        public PropiedadesFondo(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
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
                    FileInfo fileImg = new FileInfo(@".\data\objetos\multimedia\" + fi.Name);
                    if (File.Exists(@".\data\objetos\multimedia\" + fi.Name) && !fi.FullName.Equals(fileImg.FullName))
                    {
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, "Remplazar", "Mantener");
                        dialogMsg.lblNombre.Content = "¡Advertencia!";
                        dialogMsg.lblTexto.Text = "Ya existe un archivo con el mismo nombre y extension en la aplicación, ¿Desea remplazarlo o mantener la actual?. ¡Esta accion no se puede revertir!";
                        if (dialogMsg.ShowDialog() == true)
                        {
                            fi.CopyTo(@".\data\objetos\multimedia\" + fi.Name, true); 
                        }
                    }
                    else
                        fi.CopyTo(@".\data\objetos\multimedia\" + fi.Name, true);
                }
                catch {}

                FileInfo Img = new FileInfo(@".\data\objetos\multimedia\" + fi.Name);
                // Open document 
                ContenidoTextBox.Text = Img.Name;
                ContenidoTextBox.ToolTip = Img.Name;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@".\data\objetos\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
