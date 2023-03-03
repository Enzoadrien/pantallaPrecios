using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Precios_Turnos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Point _positionInBlock;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Visibility = Visibility.Collapsed;
                Topmost = true;
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
            }

        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditarDiseno_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;

        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = e.Source as UIElement;
                if (item.GetType() == typeof(Label) || item.GetType() == typeof(Image))
                {
                    _positionInBlock = Mouse.GetPosition(item);
                    item.CaptureMouse();

                }
            }
            catch (Exception) { }

        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = e.Source as UIElement;

                if (item.GetType() == typeof(Label) || item.GetType() == typeof(Image))
                {
                    // release this control.
                    item.ReleaseMouseCapture();

                }
            }
            catch (Exception) { }

        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var item = e.Source as UIElement;

                if (item.GetType() == typeof(Label) || item.GetType() == typeof(Image))
                {
                    if (item.IsMouseCaptured)
                    {
                        // get the parent container
                        var container = VisualTreeHelper.GetParent(item) as UIElement;

                        // get the position within the container
                        var mousePosition = e.GetPosition(container);

                        // move the usercontrol.
                        item.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, mousePosition.Y - _positionInBlock.Y);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
