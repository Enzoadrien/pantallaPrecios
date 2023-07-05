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
            FocusManager.SetFocusedElement(this, Ruta);
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
                    cajaTexto.Text = Convert.ToInt32(Math.Round(((WebView2)itemP).ActualWidth)).ToString();

                }
                else
                {
                    cajaTexto.Text = Convert.ToInt32(Math.Round(((WebView2)itemP).ActualWidth)).ToString();
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

        private void Coordenada_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.BorarObjeto(NombreControl.Text))
                Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;

            ((WebView2)item).SizeChanged += item_SizeChanged;

        }

        private void AnchoLargo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!esInicio)
            {
                var item = e.Source as UIElement;
                if (esCambio)
                {
                    var itemP = mainWindow.FindName(NombreControl.Text) as UIElement;

                    WebView2 webView2 = (WebView2)itemP;

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

                if (!Ancho.Text.Equals(Convert.ToInt32(Math.Round(((WebView2)item).ActualWidth)).ToString()))
                    Ancho.Text = Convert.ToInt32(Math.Round(((WebView2)item).ActualWidth)).ToString();
                if (!Alto.Text.Equals(Convert.ToInt32(Math.Round(((WebView2)item).ActualHeight)).ToString()))
                    Alto.Text = Convert.ToInt32(Math.Round(((WebView2)item).ActualHeight)).ToString();
                esCambio = true;
            }
        }

        private void chkMaximizar_Checked(object sender, RoutedEventArgs e)
        {
            if (!esInicio)
            {
                var item = mainWindow.FindName(NombreControl.Text) as UIElement;
                ((WebView2)item).Tag = "M|" + Cada.Text + "|" + Durar.Text;


                Cada.Text = "900";
                Durar.Text = "0";
                Cada.IsEnabled = true;
                Durar.IsEnabled = true;
            }
        }

        private void chkMaximizar_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            ((WebView2)item).Tag = "";
            Cada.Text = "";
            Durar.Text = "";
            Cada.IsEnabled = false;
            Durar.IsEnabled = false;
        }

        private void CadaDurante_TextChanged(object sender, TextChangedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            if (chkMaximizar.IsChecked == true)
            {
                ((WebView2)item).Tag = "M|" + Cada.Text + "|" + Durar.Text;
            }
            else
            {
                ((WebView2)item).Tag = "";
            }
        }


        private void chkOcultar_Checked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;

            ((WebView2)item).Visibility = Visibility.Hidden;
        }

        private void chkOcultar_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;

            ((WebView2)item).Visibility = Visibility.Visible;
        }
    }
}
