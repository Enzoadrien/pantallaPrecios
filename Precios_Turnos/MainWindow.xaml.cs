using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Precios_Turnos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Point _positionInBlock;
        private TranslateTransform? _currentTT;
        private bool editar = false;
        public Color ultimoColorLetra;
        public Color ultimoColorFondo;

        public MainWindow()
        {
            InitializeComponent();
            Coordenadas.Visibility = Visibility.Hidden;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Visibility = Visibility.Collapsed;
                //Topmost = true;
                Menu.Visibility = Visibility.Hidden;

                //// re-show the window after changing style
                Visibility = Visibility.Visible;
                MaxHeight = SystemParameters.VirtualScreenHeight;
                MaxWidth = SystemParameters.VirtualScreenWidth;
            }

            else
            {
                Topmost = false;
                Menu.Visibility = Visibility.Visible;
                ResizeMode = ResizeMode.CanResize;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SalirEdicion();
            }
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void EditarDiseno_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            editar = true;
            Coordenadas.Visibility = Visibility.Visible;
            ModoEdicion.Content = "Modo edición";
            ModoEdicion.FontSize = 24;

        }

        private void Principal_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            if (editar)
                try
                {
                    var item = e.Source as UIElement;
                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        var container = VisualTreeHelper.GetParent(item) as UIElement;
                        _positionInBlock = e.GetPosition(container);
                        _currentTT = item.RenderTransform as TranslateTransform;
                        item.CaptureMouse();
                    }
                }
                catch (Exception) { }

        }

        private void Principal_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (editar)
                try
                {
                    var item = e.Source as UIElement;

                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        _currentTT = item.RenderTransform as TranslateTransform;
                        // release this control.
                        item.ReleaseMouseCapture();

                    }
                }
                catch (Exception) { }

        }

        private void Principal_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (editar)
                try
                {
                    var item = e.Source as UIElement;

                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        if (item.IsMouseCaptured)
                        {
                            // get the parent container
                            var container = VisualTreeHelper.GetParent(item) as UIElement;

                            // get the position within the container
                            var mousePosition = e.GetPosition(container);
                            Point point = item.TransformToAncestor(this).Transform(new Point(0, 0));


                            var offsetX = mousePosition.X - (_currentTT == null ? _positionInBlock.X : _positionInBlock.X - _currentTT.X);
                            var offsetY = mousePosition.Y - (_currentTT == null ? _positionInBlock.Y : _positionInBlock.Y - _currentTT.Y);


                            Coordenadas.Content = item.GetValue(NameProperty).ToString() + " - Coordenadas: " + Convert.ToInt32(point.X) + "X, " + Convert.ToInt32(point.Y) + "Y";
                            // move the usercontrol.

                            item.RenderTransform = new TranslateTransform(offsetX, offsetY);
                        }
                    }
                }
                catch (Exception) { }
        }

        private bool SeModificaControl(string name)
        {
            bool seModifica;
            switch (name)
            {
                case "Coordenadas":
                case "ModoEdicion":
                case "":
                    seModifica = false;
                    break;
                default:
                    seModifica = true;
                    break;
            }
            return seModifica;
        }

        private void Principal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (editar)
                try
                {
                    if (e.ClickCount == 2)
                    {
                        var item = e.Source as UIElement;

                        if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                        {
                            // get the parent container
                            var container = VisualTreeHelper.GetParent(item) as UIElement;

                            // get the position within the container
                            var mousePosition = e.GetPosition(container);
                            Point point = item.TransformToAncestor(this).Transform(new Point(0, 0));



                            switch (item.GetType().ToString())
                            {
                                case "System.Windows.Controls.Label":
                                    PropiedadesLabel propiedadesLabel = new PropiedadesLabel(this);
                                    propiedadesLabel.WindowStartupLocation = WindowStartupLocation.Manual;
                                    if (mousePosition.Y + 300 >= MaxHeight)
                                        propiedadesLabel.Top = mousePosition.Y - 300;
                                    else
                                        propiedadesLabel.Top = mousePosition.Y;

                                    if (mousePosition.X + 250 >= MaxWidth)
                                        propiedadesLabel.Left = mousePosition.X - 250;
                                    else
                                        propiedadesLabel.Left = mousePosition.X;

                                    propiedadesLabel.NombreControl.IsEnabled = false;
                                    propiedadesLabel.NombreControl.Text = item.GetValue(NameProperty).ToString();
                                    propiedadesLabel.cbxTipoControl.IsEnabled = false;
                                    propiedadesLabel.cbxTipoControl.SelectedIndex = 0;
                                    propiedadesLabel.Contenido.Text = ((Label)item).Content.ToString();
                                    propiedadesLabel.cbxFuente.SelectedItem = item.GetValue(FontFamilyProperty);
                                    propiedadesLabel.cbxTamano.SelectedValue = item.GetValue(FontSizeProperty);
                                    propiedadesLabel.chkNegrita.IsChecked = item.GetValue(FontWeightProperty).ToString().CompareTo("Bold") == 0 ? true : false;
                                    propiedadesLabel.chkCursiva.IsChecked = item.GetValue(FontStyleProperty).ToString().CompareTo("Italic") == 0 ? true : false;
                                    propiedadesLabel.btnColorFuente.Fill = new SolidColorBrush((((Label)item).Foreground as SolidColorBrush).Color);
                                    propiedadesLabel.btnColorFondo.Fill = new SolidColorBrush((((Label)item).Background as SolidColorBrush).Color);
                                    propiedadesLabel.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                                    propiedadesLabel.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
                                    propiedadesLabel.ShowDialog();
                                    break;
                                case "System.Windows.Controls.Image":
                                    //editarDiseno.cbxTipoControl.SelectedIndex = 1;
                                    break;
                                default:

                                    break;
                            }


                        }
                    }
                }
                catch (Exception) { }
        }

        private void Principal_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (editar)
                try
                {
                    var item = e.Source as UIElement;

                    if (!SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        ContextMenu cm = this.FindResource("cmdPrincipalContexMenu") as ContextMenu;
                        cm.PlacementTarget = sender as Button;
                        cm.IsOpen = true;
                    }
                    else
                    {
                        /*ContextMenu cm = this.FindResource("cmdContexMenu") as ContextMenu;
                        cm.PlacementTarget = sender as Button;
                        cm.IsOpen = true;*/
                    }

                }
                catch (Exception) { }
        }

        private void MenuSalir_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SalirEdicion();
        }

        private void SalirEdicion()
        {
            WindowState = WindowState.Normal;
            Coordenadas.Visibility = Visibility.Hidden;
            ModoEdicion.Content = "Vista previa";
            ModoEdicion.FontSize = 18;

            if (editar)
                editar = false;
        }

        private void MenuAgregarTexto_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CapturaTextoDialogo dialog = new CapturaTextoDialogo(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            var mousePosition = e.GetPosition(Principal);
            if (mousePosition.Y + 300 >= MaxHeight)
                dialog.Top = mousePosition.Y - 300;
            else
                dialog.Top = mousePosition.Y;

            if (mousePosition.X + 130 >= MaxWidth)
                dialog.Left = mousePosition.X - 130;
            else
                dialog.Left = mousePosition.X;

            dialog.ContenidoTextBox.IsEnabled = true;
            if (dialog.ShowDialog() == true)
            {
                Label lbl = new Label();
                lbl.Name = dialog.NombreText;
                lbl.Content = dialog.ContenidoText;
                lbl.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                lbl.VerticalContentAlignment = VerticalAlignment.Stretch;
                lbl.HorizontalAlignment = HorizontalAlignment.Center;
                lbl.VerticalAlignment = VerticalAlignment.Center;
                lbl.FontSize = 24;
                lbl.FontFamily = new FontFamily("Arial Rounded MT");
                NameScope.GetNameScope(this).RegisterName(lbl.Name, lbl);
                Principal.Children.Add(lbl);
                lbl.RenderTransform = new TranslateTransform(mousePosition.X-(300*3), mousePosition.Y-(130*3));
            }
        }

        private void MenuTraerFrente_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = e.Source as UIElement;

            //item.BringToFront();
        }
    }
}
