using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
    /// Lógica de interacción para PropiedadesImagen.xaml
    /// </summary>
    public partial class PropiedadesMultimedia : Window
    {
        private MainWindow mainWindow;
        private bool esInicio = true;
        private bool esCambio = true;
        public bool esMultimedia = false;
        public bool esWeb = false;
        public PropiedadesMultimedia(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        }

        private void Opacidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            item.Opacity = Opacidad.Value;
        }

        private void Opacidad_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int change = e.Delta / Math.Abs(e.Delta);
            Opacidad.Value = Opacidad.Value + (double)change / 10;
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
            if (!esInicio)
            {
                //Image control = (Image)mainWindow.FindName(NombreControl.Text);
                var item = mainWindow.FindName(NombreControl.Text) as UIElement;
                try
                {
                    if (CoordenadaX.Text.Length > 0 && CoordenadaY.Text.Length > 0)
                    {

                        mainWindow.Principal.Children.Remove(item);
                        NameScope.GetNameScope(mainWindow).UnregisterName(NombreControl.Text);
                        if (esMultimedia)
                            ((MediaElement)item).Margin = new Thickness(int.Parse(CoordenadaX.Text), int.Parse(CoordenadaY.Text), 0, 0);
                        else if (esWeb)
                            ((WebBrowser)item).Margin = new Thickness(int.Parse(CoordenadaX.Text), int.Parse(CoordenadaY.Text), 0, 0);
                        else
                            ((Image)item).Margin = new Thickness(int.Parse(CoordenadaX.Text), int.Parse(CoordenadaY.Text), 0, 0);

                        NameScope.GetNameScope(mainWindow).RegisterName(NombreControl.Text, item);
                        mainWindow.Principal.Children.Add(item);
                    }
                }
                catch (Exception ex) { }
            }
        }

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            mainWindow.Principal.Children.Remove(item);
            NameScope.GetNameScope(mainWindow).UnregisterName(NombreControl.Text);
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            esInicio = false;
        }

        private void Largo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!esInicio)
            {
                if (esCambio)
                    try
                    {
                        esCambio = false;
                        if (Largo.Text.Length > 0)
                        {
                            if (esMultimedia)
                            {
                                MediaElement mediaElement = (MediaElement)mainWindow.FindName(NombreControl.Text);
                                mediaElement.Width = int.Parse(Largo.Text);
                                mediaElement = (MediaElement)mainWindow.FindName(NombreControl.Text);
                                Ancho.Text = mediaElement.ActualHeight.ToString();
                            }
                            else if (esWeb)
                            {
                                WebBrowser webBrowser = (WebBrowser)mainWindow.FindName(NombreControl.Text);
                                webBrowser.Width = int.Parse(Largo.Text);
                                webBrowser = (WebBrowser)mainWindow.FindName(NombreControl.Text);
                                Ancho.Text = webBrowser.ActualHeight.ToString();
                            }
                            else
                            {
                                Image image = (Image)mainWindow.FindName(NombreControl.Text);
                                image.Width = int.Parse(Largo.Text);
                                Image image2 = (Image)mainWindow.FindName(NombreControl.Text);
                                Ancho.Text = image2.ActualHeight.ToString();
                            }
                        }
                    }
                    catch (Exception) { }
                esCambio = true;
            }

        }
        private void Ancho_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!esInicio)
            {
                if (esCambio)
                    try
                    {
                        esCambio = false;
                        if (Ancho.Text.Length > 0)
                        {
                            if (esMultimedia)
                            {
                                MediaElement mediaElement = (MediaElement)mainWindow.FindName(NombreControl.Text);
                                mediaElement.Height = int.Parse(Ancho.Text);
                                mediaElement = (MediaElement)mainWindow.FindName(NombreControl.Text);
                                Largo.Text = mediaElement.ActualWidth.ToString();
                            }
                            else if (esWeb)
                            {
                                WebBrowser webBrowser = (WebBrowser)mainWindow.FindName(NombreControl.Text);
                                webBrowser.Height = int.Parse(Ancho.Text);
                                webBrowser = (WebBrowser)mainWindow.FindName(NombreControl.Text);
                                Largo.Text = webBrowser.ActualWidth.ToString();
                            }
                            else
                            {
                                Image image = (Image)mainWindow.FindName(NombreControl.Text);
                                image.Height = int.Parse(Ancho.Text);
                                image = (Image)mainWindow.FindName(NombreControl.Text);
                                Largo.Text = image.ActualWidth.ToString();
                            }
                        }

                    }
                    catch (Exception) { }
                esCambio = true;
            }
        }
    }
}
