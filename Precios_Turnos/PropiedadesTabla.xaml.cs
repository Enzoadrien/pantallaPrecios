using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para PropiedadesTabla.xaml
    /// </summary>
    public partial class PropiedadesTabla : Window
    {
        private MainWindow mainWindow;
        bool esInicio = true;
        public PropiedadesTabla(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
            FocusManager.SetFocusedElement(this, btnContenido);
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
            else if (e.Key == Key.Enter)
            {
                Close();
            }
        }

        private void cbxFuente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = mainWindow.FindName(NombreControl.Text) as UIElement;
            control.SetValue(FontFamilyProperty, new FontFamily(cbxFuente.SelectedItem.ToString()));
            if (!esInicio)
                RecargarTablas();
        }

        private void cbxTamano_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = mainWindow.FindName(NombreControl.Text) as UIElement;
            control.SetValue(FontSizeProperty, Double.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()));
            if (!esInicio)
                RecargarTablas();
        }

        private void chkNegrita_Checked(object sender, RoutedEventArgs e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontWeightProperty, FontWeights.Bold);
        }

        private void chkNegrita_Unchecked(object sender, RoutedEventArgs e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontWeightProperty, FontWeights.Normal);
        }

        private void chkCursiva_Checked(object sender, RoutedEventArgs e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontStyleProperty, FontStyles.Italic);

        }

        private void chkCursiva_Unchecked(object sender, RoutedEventArgs e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
            control.SetValue(FontStyleProperty, FontStyles.Normal);
        }

        private void chkDoble_Checked(object sender, RoutedEventArgs e)
        {
            btnColorFuente2.Visibility = Visibility.Visible;
            btnColorFondo2.Visibility = Visibility.Visible;
            lblCFuenteUno.Visibility = Visibility.Visible;
            lblCFuenteDos.Visibility = Visibility.Visible;
            lblCFondoUno.Visibility = Visibility.Visible;
            lblCFondoDos.Visibility = Visibility.Visible;
            Grid.SetColumnSpan(btnColorFuente, 1);
            Grid.SetColumnSpan(btnColorFondo, 1);
            if (!esInicio)
                ColorFuenteFondo();

        }

        private void chkDoble_Unchecked(object sender, RoutedEventArgs e)
        {
            btnColorFuente2.Visibility = Visibility.Hidden;
            btnColorFondo2.Visibility = Visibility.Hidden;
            lblCFuenteUno.Visibility = Visibility.Hidden;
            lblCFuenteDos.Visibility = Visibility.Hidden;
            lblCFondoUno.Visibility = Visibility.Hidden;
            lblCFondoDos.Visibility = Visibility.Hidden;
            Grid.SetColumnSpan(btnColorFuente, 3);
            Grid.SetColumnSpan(btnColorFondo, 3);
            if (!esInicio)
                ColorFuenteFondo();

        }

        private void btnColorFuente_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
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
                ColorFuenteFondo();
                mainWindow.ultimoColorLetra = colorPicker.SelectedColor;
            }
        }

        private void ColorFuenteFondo()
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);

            if (chkDoble.IsChecked == true)
            {
                foreach (var item in control.ItemsSource as IEnumerable)
                {
                    DataGridRow row = (DataGridRow)control.ItemContainerGenerator.ContainerFromItem(item);

                    if (row != null)
                        if (row.GetIndex() % 2 == 0)
                        {
                            row.Foreground = btnColorFuente.Fill;
                            row.Background = btnColorFondo.Fill;
                        }
                        else
                        {
                            row.Foreground = btnColorFuente2.Fill;
                            row.Background = btnColorFondo2.Fill;
                        }
                }
            }
            else
            {
                control.Foreground = btnColorFuente.Fill;
                control.Background = btnColorFondo.Fill;
                control.RowStyle = new Style(typeof(DataGridRow))
                {
                    Setters = {
                        new Setter(BackgroundProperty, btnColorFondo.Fill)
                    }
                };
            }
            ((DataGrid)mainWindow.FindName(NombreControl.Text)).Tag = int.Parse(((ComboBoxItem)cbxCBloques.SelectedItem).Tag.ToString()) + "|" +
                   int.Parse(((ComboBoxItem)cbxCRegistros.SelectedItem).Tag.ToString()) + "|" +
                   ((ComboBoxItem)cbxOrientacion.SelectedItem).Tag.ToString() + "|" +
                    ((ComboBoxItem)cbxCambiar.SelectedItem).Tag.ToString() +
                   (chkDoble.IsChecked == true ? "|" + btnColorFuente.Fill + "|" + btnColorFondo.Fill + "|" + btnColorFuente2.Fill + "|" + btnColorFondo2.Fill : "");
        }

        private void btnColorFondo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
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
                ColorFuenteFondo();
                mainWindow.ultimoColorFondo = colorPicker.SelectedColor;
            }
        }

        private void btnColorFuente2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
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
                btnColorFuente2.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                ColorFuenteFondo();
                mainWindow.ultimoColorLetra = colorPicker.SelectedColor;
            }
        }

        private void btnColorFondo2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
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
                btnColorFondo2.Fill = new SolidColorBrush(colorPicker.SelectedColor);
                ColorFuenteFondo();
                mainWindow.ultimoColorFondo = colorPicker.SelectedColor;
            }
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
                DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
                try
                {
                    if (CoordenadaX.Text.Length > 0 && CoordenadaY.Text.Length > 0)
                    {

                        var point = control.TranslatePoint(new Point(), mainWindow.Principal);
                        control.RenderTransform = new TranslateTransform(-point.X, -point.Y);
                        //control.RenderTransform = new TranslateTransform(double.Parse(CoordenadaX.Text),double.Parse(CoordenadaY.Text));
                    }
                }
                catch (Exception) { }
            }
        }

        private void Opacidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
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

        private void cbxCRegistros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!esInicio)
            {
                ((DataGrid)mainWindow.FindName(NombreControl.Text)).Tag = int.Parse(((ComboBoxItem)cbxCBloques.SelectedItem).Tag.ToString()) + "|" +
                   int.Parse(((ComboBoxItem)cbxCRegistros.SelectedItem).Tag.ToString()) + "|" +
                   ((ComboBoxItem)cbxOrientacion.SelectedItem).Tag.ToString() +
                   (chkDoble.IsChecked == true ? "|" + btnColorFuente.Fill + "|" + btnColorFondo.Fill + "|" + btnColorFuente2.Fill + "|" + btnColorFondo2.Fill : "");
                RecargarTablas();
            }
        }

        private void chkLineas_Checked(object sender, RoutedEventArgs e)
        {
            if (!esInicio)
            {
                DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
                control.GridLinesVisibility = DataGridGridLinesVisibility.All;
                control.BorderBrush = new SolidColorBrush(Colors.Black);
                ((DataGrid)mainWindow.FindName(NombreControl.Text)).Tag = int.Parse(((ComboBoxItem)cbxCBloques.SelectedItem).Tag.ToString()) + "|" +
                       int.Parse(((ComboBoxItem)cbxCRegistros.SelectedItem).Tag.ToString()) + "|" +
                       ((ComboBoxItem)cbxOrientacion.SelectedItem).Tag.ToString() + "|" +
                        ((ComboBoxItem)cbxCambiar.SelectedItem).Tag.ToString() +
                       (chkDoble.IsChecked == true ? "|" + btnColorFuente.Fill + "|" + btnColorFondo.Fill + "|" + btnColorFuente2.Fill + "|" + btnColorFondo2.Fill : "");
                RecargarTablas();
            }
        }

        private void chkLineas_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!esInicio)
            {
                DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
                control.GridLinesVisibility = DataGridGridLinesVisibility.None;
                control.BorderBrush = new SolidColorBrush(Colors.Transparent);
                ((DataGrid)mainWindow.FindName(NombreControl.Text)).Tag = int.Parse(((ComboBoxItem)cbxCBloques.SelectedItem).Tag.ToString()) + "|" +
                       int.Parse(((ComboBoxItem)cbxCRegistros.SelectedItem).Tag.ToString()) + "|" +
                       ((ComboBoxItem)cbxOrientacion.SelectedItem).Tag.ToString() + "|" +
                        ((ComboBoxItem)cbxCambiar.SelectedItem).Tag.ToString() +
                       (chkDoble.IsChecked == true ? "|" + btnColorFuente.Fill + "|" + btnColorFondo.Fill + "|" + btnColorFuente2.Fill + "|" + btnColorFondo2.Fill : "");
                RecargarTablas();
            }
        }

        private void btnContenido_Click(object sender, RoutedEventArgs e)
        {
            ContenidoTabla dialog = new ContenidoTabla(mainWindow, NombreControl.Text);
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
            ColorFuenteFondo();
        }

        private void cbxCBloques_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!esInicio)
            {
                ((DataGrid)mainWindow.FindName(NombreControl.Text)).Tag = int.Parse(((ComboBoxItem)cbxCBloques.SelectedItem).Tag.ToString()) + "|" +
                   int.Parse(((ComboBoxItem)cbxCRegistros.SelectedItem).Tag.ToString()) + "|" +
                   ((ComboBoxItem)cbxOrientacion.SelectedItem).Tag.ToString() + "|" +
                    ((ComboBoxItem)cbxCambiar.SelectedItem).Tag.ToString() +
                   (chkDoble.IsChecked == true ? "|" + btnColorFuente.Fill + "|" + btnColorFondo.Fill + "|" + btnColorFuente2.Fill + "|" + btnColorFondo2.Fill : "");
                RecargarTablas();
            }
        }

        private void cbxOrientacion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!esInicio)
            {
                ((DataGrid)mainWindow.FindName(NombreControl.Text)).Tag = int.Parse(((ComboBoxItem)cbxCBloques.SelectedItem).Tag.ToString()) + "|" +
                   int.Parse(((ComboBoxItem)cbxCRegistros.SelectedItem).Tag.ToString()) + "|" +
                   ((ComboBoxItem)cbxOrientacion.SelectedItem).Tag.ToString() + "|" +
                    ((ComboBoxItem)cbxCambiar.SelectedItem).Tag.ToString() +
                   (chkDoble.IsChecked == true ? "|" + btnColorFuente.Fill + "|" + btnColorFondo.Fill + "|" + btnColorFuente2.Fill + "|" + btnColorFondo2.Fill : "");
                RecargarTablas();
            }
        }

        private void RecargarTablas()
        {
            DataGrid control = (DataGrid)mainWindow.FindName(NombreControl.Text);
            if (((ComboBoxItem)cbxOrientacion.SelectedItem).Tag.ToString().Equals("V"))
            {
                control.CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters = {
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center)
                    }
                };
            }
            else
            {
                control.CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters = {
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Left)
                    }
                };
            }
            control.ItemsSource = mainWindow.CargarListaTablas(NombreControl.Text, control.Tag.ToString())[0].DefaultView;
            control.UpdateLayout();
            ColorFuenteFondo();
        }

        private void cbxCambiar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!esInicio)
            {
                ((DataGrid)mainWindow.FindName(NombreControl.Text)).Tag = int.Parse(((ComboBoxItem)cbxCBloques.SelectedItem).Tag.ToString()) + "|" +
                   int.Parse(((ComboBoxItem)cbxCRegistros.SelectedItem).Tag.ToString()) + "|" +
                   ((ComboBoxItem)cbxOrientacion.SelectedItem).Tag.ToString() + "|" +
                    ((ComboBoxItem)cbxCambiar.SelectedItem).Tag.ToString() +
                   (chkDoble.IsChecked == true ? "|" + btnColorFuente.Fill + "|" + btnColorFondo.Fill + "|" + btnColorFuente2.Fill + "|" + btnColorFondo2.Fill : "");
            }
        }
    }
}
