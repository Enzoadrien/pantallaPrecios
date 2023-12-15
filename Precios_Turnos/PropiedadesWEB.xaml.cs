using Microsoft.Web.WebView2.Wpf;
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
using static System.Net.Mime.MediaTypeNames;

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para PropiedadesImagen.xaml
    /// </summary>
    public partial class PropiedadesWEB : Window
    {
        private MainWindow mainWindow;
        public bool esInicio = true;
        private bool esCambio = true;

        public PropiedadesWEB(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
            FocusManager.SetFocusedElement(this, Ancho);
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

        private void Coordenada_LostFocus(object sender, RoutedEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if (cajaTexto.Text.Length == 0)
            {
                cajaTexto.Text = "0";
            }
        }

        private void AnchoLargo_LostFocus(object sender, RoutedEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if (cajaTexto.Text.Length == 0 || cajaTexto.Text.Equals("0"))
            {
                esCambio = false;
                var itemP = mainWindow.FindName(NombreControl.Text);
                if (cajaTexto.Name.Equals("Ancho"))
                {
                    cajaTexto.Text = Convert.ToInt32(Math.Round(((DockPanel)itemP).ActualWidth)).ToString();

                }
                else
                {
                    cajaTexto.Text = Convert.ToInt32(Math.Round(((DockPanel)itemP).ActualWidth)).ToString();
                }

                esCambio = true;
            }
        }

        private bool TextAllowed(string s)
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
            string s = (string)e.DataObject.GetData(typeof(string));
            if (!TextAllowed(s)) e.CancelCommand();
        }

        private void ResponseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;

            if (e.Key == Key.Space && cajaTexto.IsFocused == true)
                e.Handled = true;
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

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.BorarObjeto(NombreControl.Text))
                Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;

            ((DockPanel)item).SizeChanged += item_SizeChanged;

        }

        private void AnchoLargo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!esInicio)
            {
                var item = e.Source as UIElement;
                if (esCambio)
                {
                    var itemP = mainWindow.FindName(NombreControl.Text) as UIElement;

                    DockPanel webView2 = (DockPanel)itemP;

                    if (Alto.Text.Length > 0 && int.Parse(Alto.Text) > 0 && ((TextBox)item).Name.CompareTo("Alto") == 0)
                    {
                        if (webView2.MaxHeight >= int.Parse(Alto.Text))
                        {
                            webView2.Width = int.Parse(Ancho.Text);
                            webView2.Height = int.Parse(Alto.Text);
                        }
                        else
                        {
                            webView2.Width = int.Parse(Ancho.Text);
                            webView2.Height = webView2.MaxHeight;
                            esCambio = false;
                            Alto.Text = webView2.MaxHeight.ToString();
                            esCambio = true;
                        }
                    }
                    else if (Ancho.Text.Length > 0 && int.Parse(Ancho.Text) > 0 && ((TextBox)item).Name.CompareTo("Ancho") == 0)
                    {

                        if (webView2.MaxWidth >= int.Parse(Ancho.Text))
                        {
                            webView2.Width = int.Parse(Ancho.Text);
                            webView2.Height = int.Parse(Alto.Text);
                        }
                        else
                        {
                            webView2.Width = webView2.MaxWidth;
                            webView2.Height = int.Parse(Alto.Text);
                            esCambio = false;
                            Ancho.Text = webView2.MaxWidth.ToString();
                            esCambio = true;
                        }

                    }
                    esCambio = false;
                    if (Alto.Text.Length > 0)
                        Alto.Text = Convert.ToInt32(Alto.Text).ToString();
                    if (Ancho.Text.Length > 0)
                        Ancho.Text = Convert.ToInt32(Ancho.Text).ToString();
                    esCambio = true;
                }
                 ((TextBox)item).CaretIndex = ((TextBox)item).Text.Length;
            }
        }

        public void item_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!esInicio)
            {
                esCambio = false;
                var item = e.Source;

                if (!Ancho.Text.Equals(Convert.ToInt32(Math.Round(((DockPanel)item).ActualWidth)).ToString()))
                    Ancho.Text = Convert.ToInt32(Math.Round(((DockPanel)item).ActualWidth)).ToString();
                if (!Alto.Text.Equals(Convert.ToInt32(Math.Round(((DockPanel)item).ActualHeight)).ToString()))
                    Alto.Text = Convert.ToInt32(Math.Round(((DockPanel)item).ActualHeight)).ToString();
                esCambio = true;
            }
        }


        private void chkOcultar_Checked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;

            ((DockPanel)item).Visibility = Visibility.Hidden;
        }

        private void chkOcultar_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;

            ((DockPanel)item).Visibility = Visibility.Visible;
        }
    }
}
