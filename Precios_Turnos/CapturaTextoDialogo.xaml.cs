using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
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
        private Window mainWindow;
        public bool esVideo = false;
        public CapturaTextoDialogo(Window pMainWindow)
        {
            InitializeComponent();
            mainWindow = pMainWindow;
            FocusManager.SetFocusedElement(this, NombreTextBox);
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
            if (Char.IsDigit(NombreText.FirstOrDefault()))
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "El nombre no debe comenzar con números";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
            else if(NombreText.Length == 0 || ContenidoText.Length == 0)
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Débes ingresar todos los datos";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
            else { 
            if (mainWindow.FindName(NombreText) == null)
            {
                DialogResult = true;
            }
            else {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ya existe un objeto con ese nombre";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
            }
        }

        private void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if(esVideo)
                dlg.Filter = "Todos los archivos de video|*.mp3;*.mp4;*.asf;*.mov|MP3 (*.mp3)|*.mp3|MP4 (*.mp4)|*.mp4|ASF (*.wmv;*.wma)|*.wmv;*wma|MOV (*.mov)|*.mov";
            else
                dlg.Filter = "Todos los archivos de imagen|*.jpeg;*.jpg;*.png;*.gif|JPEG (*.jpeg;*.jpg)|*.jpeg;*.jpg|PNG (*.png)|*.png|GIF (*.gif)|*.gif";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                ContenidoTextBox.Text = dlg.FileName;
            }

        }

        private Boolean TextAllowed(String s)
        {
            string strAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçñÑ";
            foreach (Char c in s.ToCharArray())
            {
                if (strAcentos.IndexOf(c) > 0)
                    return false;
                else if (Char.IsLetterOrDigit(c) || Char.IsControl(c)) 
                    continue;
                else 
                    return false;
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
