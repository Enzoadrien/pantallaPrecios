using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para CapturaTextoDialogo.xaml
    /// </summary>
    public partial class CapturaTextoDialogo : Window
    {
        private MainWindow mainWindow;
        public CapturaTextoDialogo(MainWindow pmainWindow)
        {
            InitializeComponent();
            mainWindow = pmainWindow;
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
        public string NombreText
        {
            get { return NombreTextBox.Text; }
            set { NombreTextBox.Text = value; }
        }
        public string ContenidoText
        {
            get { return ContenidoTextBox.Text; }
            set { ContenidoTextBox.Text = value; }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

            if (mainWindow.FindName(NombreText) == null)
            {
                DialogResult = true;
            }
            else {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "Error";
                dialog.lblTexto.Content = "Ya existe un objeto con ese nombre";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
        }

        private Boolean TextAllowed(String s)
        {
            foreach (Char c in s.ToCharArray())
            {
                if (Char.IsLetterOrDigit(c) || Char.IsControl(c)) continue;
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
            if (e.Key == Key.Space && item.IsFocused == true)
                e.Handled = true;
        }
    }
}
