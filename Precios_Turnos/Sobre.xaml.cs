using Precios_Turnos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
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

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para Sobre.xaml
    /// </summary>
    public partial class Sobre : Window
    {
        private MainWindow mainWindow;
        public Sobre(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                Version.Content = "Version "+ config.AppSettings.Settings["Version"].Value;
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
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }
    }
}
