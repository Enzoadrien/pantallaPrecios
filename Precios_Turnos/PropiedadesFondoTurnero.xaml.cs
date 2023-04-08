using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para PropiedadesFondoTurnero.xaml
    /// </summary>
    public partial class PropiedadesFondoTurnero : Window
    {
        private MostrarTurno mainWindow;
        public PropiedadesFondoTurnero(MostrarTurno pMostrarTurno)
        {
            InitializeComponent();
            mainWindow = pMostrarTurno;
            FocusManager.SetFocusedElement(this, btnColorFondo);
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            if (ContenidoTextBox.Text.Length > 0)
            {
                ContenidoTextBox.Text = "";
                Opacidad.IsEnabled = false;
            }

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
                mainWindow.Fondo.Background = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.ultimoColorFondo = colorPicker.SelectedColor;
            }
        }

        private void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            //dlg.DefaultExt = ".png";
            dlg.Filter = "Todos los archivos de imagen|*.jpeg;*.jpg;*.png;*.gif|JPEG (*.jpeg;*.jpg)|*.jpeg;*.jpg|PNG (*.png)|*.png|GIF (*.gif)|*.gif";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                ContenidoTextBox.Text = dlg.FileName;
                btnColorFondo.Fill = new SolidColorBrush(Colors.White);
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource =
                    new BitmapImage(new Uri(dlg.FileName));
                mainWindow.Fondo.Background = myBrush;
                Opacidad.IsEnabled = true;
                Opacidad.Value = 1;
            }

        }

        private void Opacidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource =
                new BitmapImage(new Uri(ContenidoTextBox.Text));
            myBrush.Opacity = Opacidad.Value;
            mainWindow.Fondo.Background = myBrush;
        }

        private void Opacidad_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int change = e.Delta / Math.Abs(e.Delta);
            Opacidad.Value = Opacidad.Value + (double)change / 10;
        }
    }
}
