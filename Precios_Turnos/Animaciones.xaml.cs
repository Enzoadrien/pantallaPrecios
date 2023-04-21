using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para Animaciones.xaml
    /// </summary>
    public partial class Animaciones : Window
    {
        private MainWindow mainWindow;
        private string NombreControl;
        private TranslateTransform? _currentTT;
        private ScaleTransform? _currentST;
        public Animaciones(MainWindow pMainWindow, string pNombreControl)
        {
            InitializeComponent();
            mainWindow = pMainWindow;
            NombreControl = pNombreControl;

            var item = mainWindow.FindName(NombreControl) as UIElement;
            _currentTT = item.RenderTransform as TranslateTransform;
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

        private void chkEscalar_Checked(object sender, RoutedEventArgs e)
        {
            Escalar();
        }

        private void CargarControles(string sParametros)
        {
            string[] datos = sParametros.Split('|');
            if (datos.Length == 3)
            {
                if (datos[0].Equals("E")) {
                    cbxTamanoE.SelectedValue = datos[1];
                    VelocidadE.Text = datos[2];
                    chkEscalar.IsChecked = true;
                }
                
            }
        }

        private void Escalar()
        {
            var item = mainWindow.FindName(NombreControl) as UIElement;

            double tamano = double.Parse(((ComboBoxItem)cbxTamanoE.SelectedItem).Tag.ToString());

            Storyboard storyboard = new Storyboard();

            DoubleAnimation growAnimation = new DoubleAnimation();
            growAnimation.Duration = TimeSpan.FromMilliseconds(int.Parse(VelocidadE.Text));
            growAnimation.From = 1;
            growAnimation.To = 1 + tamano;
            growAnimation.AutoReverse = true;
            growAnimation.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.Children.Add(growAnimation);

            Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(growAnimation, item);

            DoubleAnimation growAnimation2 = new DoubleAnimation();
            growAnimation2.Duration = TimeSpan.FromMilliseconds(int.Parse(VelocidadE.Text));
            growAnimation2.From = 1;
            growAnimation2.To = 1 + tamano;
            growAnimation2.AutoReverse = true;
            growAnimation2.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.Children.Add(growAnimation2);

            Storyboard.SetTargetProperty(growAnimation2, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimation2, item);

            ScaleTransform scale = new ScaleTransform();
            item.RenderTransform = scale;
            storyboard.Begin();

            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(scale);
            if(_currentTT != null)
                myTransformGroup.Children.Add(new TranslateTransform(_currentTT.X, _currentTT.Y));
            item.RenderTransformOrigin = new Point(.5, .5);
            item.RenderTransform = myTransformGroup;
            switch (item.GetType().Name)
            {
                case "Label":
                    break;

                case "Image":
                    ((Image)item).Tag = "E|"+ tamano + "|"+ VelocidadE.Text;
                    break;

                default:

                    break;
            }

            }


        private void chkEscalar_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl) as UIElement;
            if (_currentTT != null)
                item.RenderTransform = new TranslateTransform(_currentTT.X, _currentTT.Y);
            ((Image)item).Tag = "";
        }

        private void cbxTamano_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(chkEscalar.IsChecked ==  true)
                Escalar();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var item = mainWindow.FindName(NombreControl) as UIElement;
            if (_currentTT != null)
                item.RenderTransform = new TranslateTransform(_currentTT.X, _currentTT.Y);
            else
            {
                item.RenderTransform = null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var item = mainWindow.FindName(NombreControl) as UIElement;
            CargarControles(item.GetValue(TagProperty).ToString());
        }
    }
}
