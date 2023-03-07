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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para ConfigDiseno.xaml
    /// </summary>
    public partial class PropiedadesLabel : Window
    {
        private MainWindow mainWindow;
        bool esInicio = true;

        public PropiedadesLabel(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }catch(Exception) { }
            
            
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void cbxFuente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = mainWindow.FindName(NombreControl.Text) as UIElement;
            control.SetValue(FontFamilyProperty, new FontFamily(cbxFuente.SelectedItem.ToString()));
        }

        private void cbxTamano_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = mainWindow.FindName(NombreControl.Text) as UIElement;
            control.SetValue(FontSizeProperty, Double.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()));
        }

        private void Contenido_TextChanged(object sender, TextChangedEventArgs e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            control.Content = Contenido.Text;
        }

        private void chkNegrita_Checked(object sender, RoutedEventArgs e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontWeightProperty, FontWeights.Bold);
        }

        private void chkNegrita_Unchecked(object sender, RoutedEventArgs e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontWeightProperty, FontWeights.Normal);
        }

        private void chkCursiva_Checked(object sender, RoutedEventArgs e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontStyleProperty, FontStyles.Italic);
            
        }

        private void chkCursiva_Unchecked(object sender, RoutedEventArgs e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontStyleProperty, FontStyles.Normal);
        }

        private void btnColorFuente_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            ColorPicker colorPicker = new ColorPicker(mainWindow, (control.Foreground as SolidColorBrush).Color);
            // get the parent container

            // get the position within the container
            var mousePosition = e.GetPosition(mainWindow);

            if (mousePosition.Y + 480 >= mainWindow.MaxHeight)
                colorPicker.Top = mousePosition.Y - 480;
            else
                colorPicker.Top = mousePosition.Y;

            if (mousePosition.X + 330 >= mainWindow.MaxWidth)
                colorPicker.Left = mousePosition.X - 330;
            else
                colorPicker.Left = mousePosition.X;

            if ((bool)colorPicker.ShowDialog()) {
                btnColorFuente.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                control.Foreground = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.ultimoColorLetra = colorPicker.SelectedColor;
            }
        }

        private void btnColorFondo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            ColorPicker colorPicker = new ColorPicker(mainWindow, (control.Background as SolidColorBrush).Color);
            // get the parent container

            // get the position within the container
            var mousePosition = e.GetPosition(mainWindow);

            if (mousePosition.Y + 480 >= mainWindow.MaxHeight)
                colorPicker.Top = mousePosition.Y - 480;
            else
                colorPicker.Top = mousePosition.Y;

            if (mousePosition.X + 330 >= mainWindow.MaxWidth)
                colorPicker.Left = mousePosition.X - 330;
            else
                colorPicker.Left = mousePosition.X;

            if ((bool)colorPicker.ShowDialog())
            {
                btnColorFondo.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                control.Background = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.ultimoColorFondo = colorPicker.SelectedColor;
            }
        }

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            mainWindow.Principal.Children.Remove(control);
            NameScope.GetNameScope(mainWindow).UnregisterName(NombreControl.Text);
            Close();
        }

        private void Coordenada_LostFocus(object sender, RoutedEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if (cajaTexto.Text.Length == 0)
            {
                cajaTexto.Text = "0";
            }
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

        private void txtAlpha_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if (cajaTexto.Text.Length > 0)
            {
                bool moverCursor = false;
                if (cajaTexto.Text.Substring(0, 1).CompareTo("0") == 0 && cajaTexto.Text.Length > 1)
                    moverCursor = true;
                cajaTexto.Text = int.Parse(cajaTexto.Text).ToString();
                if (moverCursor)
                    cajaTexto.CaretIndex = cajaTexto.Text.Length;
               
                }
        }

        private void Coordenada_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!esInicio) {
                Label control = (Label)mainWindow.FindName(NombreControl.Text);
                try {
                    if(CoordenadaX.Text.Length > 0 && CoordenadaY.Text.Length > 0) {

                        mainWindow.Principal.Children.Remove(control);
                        NameScope.GetNameScope(mainWindow).UnregisterName(NombreControl.Text);
                        control.Margin = new Thickness(int.Parse(CoordenadaX.Text), int.Parse(CoordenadaY.Text), 0, 0);
                        NameScope.GetNameScope(mainWindow).RegisterName(control.Name, control);
                        mainWindow.Principal.Children.Add(control);
                    }
                }
                catch(Exception ex) { }
            }
        }

        private void Opacidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Label control = (Label)mainWindow.FindName(NombreControl.Text);
            control.Opacity = Opacidad.Value;
        }
        
        private void Opacidad_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int change = e.Delta / Math.Abs(e.Delta);
            Opacidad.Value = Opacidad.Value + (double)change/10;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            esInicio = false;
        }
    }
}
