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
        public bool esInicio = true;
        private bool esCambio = true;

        public PropiedadesMultimedia(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
            FocusManager.SetFocusedElement(this, Opacidad);
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
            else if(e.Key == Key.Enter)
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
                    switch (itemP.GetType().Name.ToString())
                    {
                        case "Image":
                            cajaTexto.Text = Convert.ToInt32(Math.Round(((Image)itemP).ActualWidth)).ToString();
                            break;
                        case "MediaElement":
                            cajaTexto.Text = Convert.ToInt32(Math.Round(((MediaElement)itemP).ActualWidth)).ToString();
                            break;
                    }
                }    
                else
                {
                    switch (itemP.GetType().Name.ToString())
                    {
                        case "Image":
                            cajaTexto.Text = Convert.ToInt32(Math.Round(((Image)itemP).ActualHeight)).ToString();
                            break;
                        case "MediaElement":
                            cajaTexto.Text = Convert.ToInt32(Math.Round(((MediaElement)itemP).ActualHeight)).ToString();
                            break;
                    }
                }
                    
                esCambio = true;
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

        private void Coordenada_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!esInicio)
            {
                var item = mainWindow.FindName(NombreControl.Text) as UIElement;
                try
                {
                    if (CoordenadaX.Text.Length > 0 && CoordenadaY.Text.Length > 0)
                    {

                        mainWindow.Principal.Children.Remove(item);
                        NameScope.GetNameScope(mainWindow).UnregisterName(NombreControl.Text);
                        switch (item.GetType().Name.ToString())
                        {
                            case "Image":
                                ((Image)item).Margin = new Thickness(int.Parse(CoordenadaX.Text), int.Parse(CoordenadaY.Text), 0, 0);
                                break;
                            case "MediaElement":
                                ((MediaElement)item).Margin = new Thickness(int.Parse(CoordenadaX.Text), int.Parse(CoordenadaY.Text), 0, 0);
                                break;
                            default: break;
                        }

                        NameScope.GetNameScope(mainWindow).RegisterName(NombreControl.Text, item);
                        mainWindow.Principal.Children.Add(item);
                    }
                }
                catch (Exception) { }
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
            switch (item.GetType().Name.ToString())
            {
                case "Image":
                    ((Image)item).SizeChanged += item_SizeChanged;
                    break;
                case "MediaElement":
                    ((MediaElement)item).SizeChanged += item_SizeChanged;
                    break;
                default: break;
            }

        }

        private void AnchoLargo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!esInicio)
            {
                var item = e.Source as UIElement;
                if (esCambio)
                {
                    var itemP = mainWindow.FindName(NombreControl.Text) as UIElement;

                    switch (itemP.GetType().Name.ToString())
                    {
                        case "Image":
                            Image image = (Image)itemP;

                            if (Largo.Text.Length > 0 && int.Parse(Largo.Text) > 0 && ((TextBox)item).Name.CompareTo("Largo") == 0)
                            {
                                if ((bool)chkRelacion.IsChecked)
                                {
                                    image.Stretch = Stretch.Uniform;
                                    image.Height = int.Parse(Largo.Text);
                                    if (image.ActualHeight != image.Height)
                                    {
                                        esCambio = false;
                                        Largo.Text = Convert.ToInt32(Math.Round(image.ActualHeight)).ToString();
                                        esCambio = true;
                                    }
                                }
                                else
                                {
                                    image.Stretch = Stretch.Fill;
                                    image.Width = int.Parse(Largo.Text);
                                    image.Height = int.Parse(Ancho.Text);
                                }
                                esCambio = true;
                            }
                            else if (Ancho.Text.Length > 0 && int.Parse(Ancho.Text) > 0 && ((TextBox)item).Name.CompareTo("Ancho") == 0)
                            {
                                if ((bool)chkRelacion.IsChecked)
                                {
                                    image.Stretch = Stretch.Uniform;
                                    image.Width = int.Parse(Ancho.Text);
                                    if (image.ActualWidth != image.Width)
                                    {
                                        esCambio = false;
                                        Ancho.Text = Convert.ToInt32(Math.Round(image.ActualWidth)).ToString();
                                        esCambio = true;
                                    }
                                }
                                else
                                {
                                    image.Stretch = Stretch.Fill;
                                    image.Width = int.Parse(Largo.Text);
                                    image.Height = int.Parse(Ancho.Text);
                                }
                            }
                            break;
                        case "MediaElement":
                            MediaElement mediaElement = (MediaElement)itemP;

                            if (Largo.Text.Length > 0 && int.Parse(Largo.Text) > 0 && ((TextBox)item).Name.CompareTo("Largo") == 0)
                            {
                                if ((bool)chkRelacion.IsChecked)
                                {
                                    mediaElement.Stretch = Stretch.Uniform;
                                    mediaElement.Height = int.Parse(Largo.Text);
                                    if (mediaElement.ActualHeight != mediaElement.Height)
                                    {
                                        esCambio = false;
                                        Largo.Text = Convert.ToInt32(Math.Round(mediaElement.ActualHeight)).ToString();
                                        esCambio = true;
                                    }
                                        
                                }
                                else
                                {
                                    mediaElement.Stretch = Stretch.Fill;
                                    mediaElement.Width = int.Parse(Largo.Text);
                                    mediaElement.Height = int.Parse(Ancho.Text);
                                }
                                esCambio = true;
                            }
                            else if (Ancho.Text.Length > 0 && int.Parse(Ancho.Text) > 0 && ((TextBox)item).Name.CompareTo("Ancho") == 0)
                            {
                                if ((bool)chkRelacion.IsChecked)
                                {
                                    mediaElement.Stretch = Stretch.Uniform;
                                    mediaElement.Width = int.Parse(Ancho.Text);
                                    if (mediaElement.ActualWidth != mediaElement.Width)
                                    {
                                        esCambio = false;
                                        Ancho.Text = Convert.ToInt32(Math.Round(mediaElement.ActualWidth)).ToString();
                                        esCambio = true;
                                    }
                                }
                                else
                                {
                                    mediaElement.Stretch = Stretch.Fill;
                                    mediaElement.Width = int.Parse(Largo.Text);
                                    mediaElement.Height = int.Parse(Ancho.Text);
                                }
                            }
                            break;
                        default: break;
                    }
                }
                 ((TextBox)item).CaretIndex = ((TextBox)item).Text.Length;
            }
        }

        public void item_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!esInicio)
            {
                if ((bool)chkRelacion.IsChecked)
                {
                    esCambio = false;
                    var item = e.Source;
                    switch (item.GetType().Name.ToString())
                    {
                        case "Image":
                            if(!Ancho.Text.Equals(Convert.ToInt32(Math.Round(((Image)item).ActualWidth)).ToString()))
                                Ancho.Text = Convert.ToInt32(Math.Round(((Image)item).ActualWidth)).ToString();
                            if (!Largo.Text.Equals(Convert.ToInt32(Math.Round(((Image)item).ActualHeight)).ToString()))
                                Largo.Text = Convert.ToInt32(Math.Round(((Image)item).ActualHeight)).ToString();
                            break;
                        case "MediaElement":
                            if (!Ancho.Text.Equals(Convert.ToInt32(Math.Round(((MediaElement)item).ActualWidth)).ToString()))
                                Ancho.Text = Convert.ToInt32(Math.Round(((MediaElement)item).ActualWidth)).ToString();
                            if (!Largo.Text.Equals(Convert.ToInt32(Math.Round(((MediaElement)item).ActualHeight)).ToString()))
                                Largo.Text = Convert.ToInt32(Math.Round(((MediaElement)item).ActualHeight)).ToString();
                            break;
                        default: break;
                    }
                    esCambio = true;
                }
            }
        }

        private void chkSonido_Checked(object sender, RoutedEventArgs e)
        {
            if (!esInicio)
            {
                var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            switch (item.GetType().Name.ToString())
            {
                case "MediaElement":
                    ((MediaElement)item).Volume = 1;
                        if (chkMaximizar.IsChecked == true)
                            ((MediaElement)item).Tag = "S|M|" + Cada.Text + "|" + Durar.Text;
                        else
                            ((MediaElement)item).Tag = "S";
                        break;
                default: break;
            }
            }
        }

        private void chkSonido_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            switch (item.GetType().Name.ToString())
            {
                case "MediaElement":
                    ((MediaElement)item).Volume = 0;
                    if (chkMaximizar.IsChecked == true)
                        ((MediaElement)item).Tag = "N|M|" + Cada.Text + "|" + Durar.Text;
                    else
                        ((MediaElement)item).Tag = "N";
                    break;
                default: break;
            }
        }

        private void chkMaximizar_Checked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            if (chkSonido.IsChecked == true)
                ((MediaElement)item).Tag = "S|M|" + Cada.Text + "|" + Durar.Text;
            else 
                ((MediaElement)item).Tag = "N|M|" + Cada.Text + "|" + Durar.Text;


            Cada.Text = "900";
            Durar.Text = "0";
            Cada.IsEnabled = true;
            Durar.IsEnabled = true;
        }

        private void chkMaximizar_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            if (chkSonido.IsChecked == true)
                ((MediaElement)item).Tag = "S";
            else
                ((MediaElement)item).Tag = "N";
            Cada.Text = "";
            Durar.Text = "";
            Cada.IsEnabled = false;
            Durar.IsEnabled = false;
        }

        private void CadaDurante_TextChanged(object sender, TextChangedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl.Text) as UIElement;
            if(chkSonido.IsChecked == true)
                ((MediaElement)item).Tag = "S|M|" + Cada.Text + "|" + Durar.Text;
            else
                ((MediaElement)item).Tag = "N|M|" + Cada.Text + "|" + Durar.Text;
        }

        private void btnAnimaciones_Click(object sender, RoutedEventArgs e)
        {
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
        }
    }
}
