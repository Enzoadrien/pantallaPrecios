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

namespace Priceio.VentanasPropiedadesSplash
{
    /// <summary>
    /// Lógica de interacción para PropiedadesBoton.xaml
    /// </summary>
    public partial class PropiedadesBoton : Window
    {
        private MostrarVentanaSplash mainWindow;
        bool esInicio = true;
        private bool esCambio = true;
        public PropiedadesBoton(MostrarVentanaSplash pMainWindow)
        {
            mainWindow = pMainWindow;
            InitializeComponent();

        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }


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
            else if (e.Key == Key.Enter)
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

        private void chkNegrita_Checked(object sender, RoutedEventArgs e)
        {
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontWeightProperty, FontWeights.Bold);
        }

        private void chkNegrita_Unchecked(object sender, RoutedEventArgs e)
        {
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontWeightProperty, FontWeights.Normal);
        }

        private void chkCursiva_Checked(object sender, RoutedEventArgs e)
        {
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontStyleProperty, FontStyles.Italic);

        }

        private void chkCursiva_Unchecked(object sender, RoutedEventArgs e)
        {
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontStyleProperty, FontStyles.Normal);
        }

        private void btnColorFuente_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Opacity = 0.5;
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            ColorPicker colorPicker = new ColorPicker(mainWindow, mainWindow.ultimoColorLetra, mainWindow.ultimoColorFondo, (control.Foreground as SolidColorBrush).Color);
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
                btnColorFuente.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                control.Foreground = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.ultimoColorLetra = colorPicker.SelectedColor;
            }
            Opacity = 0.9;
        }

        private void btnColorFondo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Opacity = 0.5;
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            ColorPicker colorPicker = new ColorPicker(mainWindow, mainWindow.ultimoColorLetra, mainWindow.ultimoColorFondo, (control.Background as SolidColorBrush).Color);
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
                control.Background = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.ultimoColorFondo = colorPicker.SelectedColor;
            }
            Opacity = 0.9;
        }

        private void btnColorFuenteActivo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Opacity = 0.5;
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            ColorPicker colorPicker = new ColorPicker(mainWindow, mainWindow.ultimoColorLetra, mainWindow.ultimoColorFondo, (control.Foreground as SolidColorBrush).Color);
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
                btnColorFuenteActivo.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                control.Foreground = new SolidColorBrush(colorPicker.SelectedColor);
                mainWindow.ultimoColorLetra = colorPicker.SelectedColor;
            }
            Opacity = 0.9;
        }

        private void btnColorFondoActivo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Opacity = 0.5;
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            ColorPicker colorPicker = new ColorPicker(mainWindow, mainWindow.ultimoColorLetra, mainWindow.ultimoColorFondo, (control.Background as SolidColorBrush).Color);
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
                btnColorFondoActivo.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                Style buttonStyle = new Style(typeof(Button));

                // Create a trigger for IsMouseOver
                Trigger mouseOverTrigger = new Trigger();
                mouseOverTrigger.Property = Button.IsMouseOverProperty;
                mouseOverTrigger.Value = true;

                // Set the background to yellow when the mouse is over the button
                mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(colorPicker.SelectedColor)));
                buttonStyle.Triggers.Add(mouseOverTrigger);

                // Apply the style to the button
                control.Style = buttonStyle;
                mainWindow.ultimoColorFondo = colorPicker.SelectedColor;
            }
            Opacity = 0.9;
        }


        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.BorarObjeto(NombreControl.Text))
                Close();
        }

        private void Coordenada_LostFocus(object sender, RoutedEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if (cajaTexto.Text.Length == 0)
            {
                Point point = item.TransformToAncestor(mainWindow.Principal).Transform(new Point(0, 0));
                double pX = Math.Round(point.X);
                double pY = Math.Round(point.Y);

                if (cajaTexto.Name.Equals("CoordenadaX"))
                    cajaTexto.Text = Convert.ToInt32(pX).ToString();
                else
                    cajaTexto.Text = Convert.ToInt32(pY).ToString();
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

            var item = e.Source as UIElement;
            if ((((TextBox)item).Name.CompareTo("CoordenadaX") == 0 || ((TextBox)item).Name.CompareTo("CoordenadaY") == 0)
                && e.Text.Equals("-") && !((TextBox)item).Text.Contains('-') && ((TextBox)item).CaretIndex == 0)
                e.Handled = false;
            else
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
            if (!esInicio)
            {
                if (esCambio)
                {
                    var item = mainWindow.FindName(NombreControl.Text) as UIElement;
                    try
                    {
                        if (CoordenadaX.Text.Length > 0 && CoordenadaY.Text.Length > 0)
                        {

                            Point point = item.TransformToAncestor(mainWindow.Principal).Transform(new Point(0, 0));
                            double pX = Math.Round(point.X);
                            double pY = Math.Round(point.Y);

                            double currentX = 0;
                            double currentY = 0;
                            try
                            {
                                TranslateTransform _currentTT = item.RenderTransform as TranslateTransform;
                                currentX = Math.Round(_currentTT.X);
                                currentY = Math.Round(_currentTT.Y);
                            }
                            catch { }

                            double x = double.Parse(CoordenadaX.Text);
                            double y = double.Parse(CoordenadaY.Text);
                            double movX = 0;
                            double movY = 0;

                            if (pX != x && currentX == 0)
                                movX = -pX + x;
                            else if (pX == x)
                                movX = currentX;
                            else
                                movX = currentX - pX + x;

                            if (pY != y && currentY == 0)
                                movY = -pY + y;
                            else if (pY == y)
                                movY = currentY;
                            else
                                movY = currentY - pY + y;

                            item.RenderTransform = new TranslateTransform(movX, movY);
                        }

                    }
                    catch (Exception) { }
                    esCambio = false;
                    if (CoordenadaX.Text.Length > 0)
                        CoordenadaX.Text = Convert.ToInt32(CoordenadaX.Text).ToString();
                    if (CoordenadaY.Text.Length > 0)
                        CoordenadaY.Text = Convert.ToInt32(CoordenadaY.Text).ToString();
                    esCambio = true;
                }
            }
        }

        private void Opacidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Button control = (Button)mainWindow.FindName(NombreControl.Text);
            control.Opacity = Opacidad.Value;
        }

        private void Opacidad_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int change = e.Delta / Math.Abs(e.Delta);
            Opacidad.Value = Opacidad.Value + (double)change / 10;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            esInicio = false;
        }

        private void btnAnimaciones_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.5;
            Animaciones dialog = new Animaciones(mainWindow, NombreControl.Text);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = Mouse.GetPosition(this);

            // get the position within the container
            var mousePosition = this.PointToScreen(relativeCenterParent);

            if (mousePosition.Y + dialog.Height >= mainWindow.MaxHeight)
                dialog.Top = mousePosition.Y - dialog.Height;
            else
                dialog.Top = mousePosition.Y;

            if (mousePosition.X + dialog.Width >= mainWindow.MaxWidth)
                dialog.Left = mousePosition.X - dialog.Width;
            else
                dialog.Left = mousePosition.X;

            dialog.ShowDialog();
            Opacity = 0.9;
        }
    }
}
