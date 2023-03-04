using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private bool editar = false
            ;

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
                WindowState = WindowState.Normal;
                Coordenadas.Visibility = Visibility.Hidden;
                ModoEdicion.Content = "Vista previa";
                ModoEdicion.FontSize = 18;

                if (editar)
                    editar = false;
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
                        item.CaptureMouse();
                        _currentTT = item.RenderTransform as TranslateTransform;
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
                        // release this control.
                        item.ReleaseMouseCapture();
                        _currentTT = item.RenderTransform as TranslateTransform;

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


                            var offsetX = mousePosition.X - (_currentTT == null ? _positionInBlock.X : _positionInBlock.X - _currentTT.X);
                            var offsetY = mousePosition.Y - (_currentTT == null ? _positionInBlock.Y : _positionInBlock.Y - _currentTT.Y);

                            //var offsetX = mousePosition.X - (_currentTT == null ? _positionInBlock.X : _positionInBlock.X - _currentTT.X);
                            //var offsetY = mousePosition.Y - (_currentTT == null ? _positionInBlock.Y : _positionInBlock.Y - _currentTT.Y);


                            Coordenadas.Content = "Coordenadas: " + mousePosition.X + "X, " + mousePosition.Y + "Y";
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

                            

                            switch (item.GetType().ToString())
                            {
                                case "System.Windows.Controls.Label":
                                    EditarDiseno editarDiseno = new EditarDiseno(this);
                                    editarDiseno.WindowStartupLocation = WindowStartupLocation.Manual;
                                    editarDiseno.Top = mousePosition.Y;
                                    editarDiseno.Left = mousePosition.X;
                                    editarDiseno.NombreControl.IsEnabled = false;
                                    editarDiseno.NombreControl.Text = item.GetValue(NameProperty).ToString();
                                    editarDiseno.cbxTipoControl.IsEnabled = false;
                                    editarDiseno.cbxTipoControl.SelectedIndex = 0;
                                    editarDiseno.Contenido.Text = ((Label)item).Content.ToString();
                                    editarDiseno.cbxFuente.SelectedItem = item.GetValue(FontFamilyProperty);
                                    editarDiseno.cbxTamano.SelectedValue = item.GetValue(FontSizeProperty);
                                    editarDiseno.chkNegrita.IsChecked = item.GetValue(FontWeightProperty).ToString().CompareTo("Bold") == 0 ? true: false;
                                    editarDiseno.chkCursiva.IsChecked = item.GetValue(FontStyleProperty).ToString().CompareTo("Italic") == 0 ? true : false;
                                    editarDiseno.ShowDialog();
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
    }
}
