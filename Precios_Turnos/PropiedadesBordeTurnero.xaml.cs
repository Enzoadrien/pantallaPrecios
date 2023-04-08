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
    /// Lógica de interacción para PropiedadesFondo.xaml
    /// </summary>
    public partial class PropiedadesBordeTurnero : Window
    {
        private MostrarTurno mainWindow;
        public PropiedadesBordeTurnero(MostrarTurno pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
            FocusManager.SetFocusedElement(this, btnColorBorde);
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
            Border control = (Border)mainWindow.FindName(NombreControl.Text);
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
            Border control = (Border)mainWindow.FindName(NombreControl.Text);
            control.BorderThickness = new Thickness(int.Parse(((ComboBoxItem)cbxGrosor.SelectedItem).Tag.ToString()));
        }
    }
}
