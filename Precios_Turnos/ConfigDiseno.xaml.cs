using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Lógica de interacción para ConfigDiseno.xaml
    /// </summary>
    public partial class ConfigDiseno : Window
    {
        public ConfigDiseno()
        {
            InitializeComponent();
            double MaxHeightScreen = SystemParameters.PrimaryScreenHeight;
            double MaxWidthScreen = SystemParameters.PrimaryScreenWidth;
            Top = (MaxHeightScreen/2) - (Height/2);
            Left = MaxWidthScreen - Width;

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        } 
    }
}
