using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
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
        private Window mainWindow;
        private string NombreControl;
        private TranslateTransform? _currentTT;
        public Animaciones(Window pMainWindow, string pNombreControl)
        {
            InitializeComponent();
            mainWindow = pMainWindow;
            NombreControl = pNombreControl;

            var item = mainWindow.FindName(NombreControl) as UIElement;
            _currentTT = item.RenderTransform as TranslateTransform;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private bool TextAllowed(string s)
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

        private void chkMover_Checked(object sender, RoutedEventArgs e)
        {
            chkHorizontal.IsEnabled = true;
            chkVertical.IsEnabled = true;

            Animar();
        }

        private void chkMover_Unchecked(object sender, RoutedEventArgs e)
        {
            chkHorizontal.IsEnabled = false;
            cbxDireccionMH.IsEnabled = false;
            VelocidadMH.IsReadOnly = true;
            CantidadMH.IsReadOnly = true;
            chkHorizontal.IsChecked = false;
            chkReversaMH.IsEnabled = false;
            chkReversaMH.IsChecked = false;

            chkVertical.IsEnabled = false;
            cbxDireccionMV.IsEnabled = false;
            VelocidadMV.IsReadOnly = true;
            CantidadMV.IsReadOnly = true;
            chkVertical.IsChecked = false;
            chkReversaMV.IsEnabled = false;
            chkReversaMV.IsChecked = false;
            Animar();

        }

        private void chkHorizontal_Checked(object sender, RoutedEventArgs e)
        {
            cbxDireccionMH.IsEnabled = true;
            chkReversaMH.IsEnabled = true;
            VelocidadMH.IsReadOnly = false;
            CantidadMH.IsReadOnly = false;

            if (chkMover.IsChecked == true)
                Animar();


        }

        private void chkHorizontal_Unchecked(object sender, RoutedEventArgs e)
        {
            cbxDireccionMH.IsEnabled = false;
            VelocidadMH.IsReadOnly = true;
            CantidadMH.IsReadOnly = true;
            chkReversaMH.IsEnabled = false;
            chkReversaMH.IsChecked = false;

            if (chkMover.IsChecked == true)
                Animar();
        }

        private void chkVertical_Unchecked(object sender, RoutedEventArgs e)
        {
            cbxDireccionMV.IsEnabled = false;
            VelocidadMV.IsReadOnly = true;
            CantidadMV.IsReadOnly = true;
            chkReversaMV.IsEnabled = false;
            chkReversaMV.IsChecked = false;

            if (chkMover.IsChecked == true)
                Animar();
        }

        private void chkVertical_Checked(object sender, RoutedEventArgs e)
        {
            cbxDireccionMV.IsEnabled = true;
            VelocidadMV.IsReadOnly = false;
            CantidadMV.IsReadOnly = false;
            chkReversaMV.IsEnabled = true;

            if (chkMover.IsChecked == true)
                Animar();


        }

        private void cbxDireccionMH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chkMover.IsChecked == true && chkHorizontal.IsChecked == true)
                Animar();
        }

        private void cbxDireccionMV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chkMover.IsChecked == true && chkVertical.IsChecked == true)
                Animar();
        }

        private void VelocidadMH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VelocidadMH.Text.Length > 0)
                if (chkMover.IsChecked == true)
                    Animar();
        }

        private void VelocidadMH_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VelocidadMH.Text.Length == 0)
                VelocidadMH.Text = "0";
        }

        private void VelocidadMV_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VelocidadMV.Text.Length > 0)
                if (chkMover.IsChecked == true)
                    Animar();
        }

        private void VelocidadMV_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VelocidadMV.Text.Length == 0)
                VelocidadMV.Text = "0";
        }

        private void CantidadMH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CantidadMH.Text.Length > 0)
                if (chkMover.IsChecked == true)
                    Animar();
        }

        private void CantidadMH_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CantidadMH.Text.Length == 0)
                CantidadMH.Text = "0";
        }

        private void CantidadMV_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CantidadMV.Text.Length > 0)
                if (chkMover.IsChecked == true)
                    Animar();
        }

        private void CantidadMV_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CantidadMV.Text.Length == 0)
                CantidadMV.Text = "0";
        }

        private void chkReversaM_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            Animar();
        }

        private void chkEscalar_Checked(object sender, RoutedEventArgs e)
        {
            VelocidadE.IsReadOnly = false;
            cbxTamanoE.IsEnabled = true;
            Animar();
        }

        private void chkEscalar_Unchecked(object sender, RoutedEventArgs e)
        {
            VelocidadE.IsReadOnly = true;
            cbxTamanoE.IsEnabled = false;
            Animar();
        }

        private void cbxTamanoE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chkEscalar.IsChecked == true)
                Animar();
        }

        private void VelocidadE_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VelocidadE.Text.Length > 0)
                if (chkEscalar.IsChecked == true)
                    Animar();
        }

        private void VelocidadE_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VelocidadE.Text.Length == 0)
                VelocidadE.Text = "0";
        }

        private void chkGirar_Checked(object sender, RoutedEventArgs e)
        {
            VelocidadG.IsReadOnly = false;
            cbxDireccionG.IsEnabled = true;
            Animar();
        }

        private void chkGirar_Unchecked(object sender, RoutedEventArgs e)
        {
            VelocidadG.IsReadOnly = true;
            cbxDireccionG.IsEnabled = false;
            Animar();
        }

        private void cbxDireccionG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chkGirar.IsChecked == true)
                Animar();
        }

        private void VelocidadG_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VelocidadG.Text.Length > 0)
                if (chkGirar.IsChecked == true)
                    Animar();
        }

        private void VelocidadG_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VelocidadG.Text.Length == 0)
                VelocidadG.Text = "0";
        }

        private void CargarControles()
        {
            string lectura = string.Empty;
            DirectoryInfo info = new DirectoryInfo((mainWindow.Name.Equals("VentanaPrincipal") ? "objetos" : "objetosTurno") + @"\animaciones");
            foreach (var file in info.GetFiles())
            {
                if (@file.Name.Equals(NombreControl + ".anim"))
                {
                    StreamReader sR = new StreamReader(@file.FullName);
                    lectura = sR.ReadToEnd();
                    sR.Close();
                    string[] animaciones = new Seguridad().DecryptString(MainWindow.nombreApp, lectura).Split('-');
                    if (animaciones.Length > 1)
                    {
                        foreach (string animacion in animaciones)
                        {
                            string[] datos = animacion.Split('|');
                            switch (datos[0])
                            {
                                case "M":
                                    if (datos[1].Equals("S"))
                                    {
                                        if (datos[3].Equals("S"))
                                        {
                                            cbxDireccionMH.SelectedValue = datos[4];
                                            VelocidadMH.Text = datos[5];
                                            CantidadMH.Text = datos[6];
                                            if (datos[7].Equals("S"))
                                                chkReversaMH.IsChecked = true;
                                            chkHorizontal.IsChecked = true;

                                        }
                                        if (datos[9].Equals("S"))
                                        {
                                            cbxDireccionMV.SelectedValue = datos[10];
                                            VelocidadMV.Text = datos[11];
                                            CantidadMV.Text = datos[12];
                                            if (datos[13].Equals("S"))
                                                chkReversaMV.IsChecked = true;
                                            chkVertical.IsChecked = true;
                                        }
                                        chkMover.IsChecked = true;
                                    }
                                    break;
                                case "E":
                                    if (datos[1].Equals("S"))
                                    {
                                        cbxTamanoE.SelectedValue = datos[2];
                                        VelocidadE.Text = datos[3];
                                        chkEscalar.IsChecked = true;
                                    }
                                    break;
                                case "G":
                                    if (datos[1].Equals("S"))
                                    {
                                        cbxDireccionG.SelectedValue = datos[2];
                                        VelocidadG.Text = datos[3];
                                        chkGirar.IsChecked = true;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void Animar()
        {
            var item = mainWindow.FindName(NombreControl) as UIElement;

            TransformGroup myTransformGroup = new TransformGroup();
            double tamanoE = double.Parse(((ComboBoxItem)cbxTamanoE.SelectedItem).Tag.ToString());
            string direccionMH = ((ComboBoxItem)cbxDireccionMH.SelectedItem).Tag.ToString();
            string direccionMV = ((ComboBoxItem)cbxDireccionMV.SelectedItem).Tag.ToString();
            string direccionG = ((ComboBoxItem)cbxDireccionG.SelectedItem).Tag.ToString();
            string Animaciones = "";

            if (chkGirar.IsChecked == true)
                Animaciones += "G|S|" + direccionG + "|" + VelocidadG.Text;
            else
                Animaciones += "G|N";

            if (chkEscalar.IsChecked == true)
                Animaciones += "-E|S|" + tamanoE + "|" + VelocidadE.Text;
            else
                Animaciones += "-E|N";

            if (chkMover.IsChecked == true)
                Animaciones += "-M|S|H|" + (chkHorizontal.IsChecked == true ? "S" : "N") + "|" + direccionMH + "|" + VelocidadMH.Text + "|" + CantidadMH.Text + "|" + (chkReversaMH.IsChecked == true ? "S" : "N")
                                      + "|V|" + (chkVertical.IsChecked == true ? "S" : "N") + "|" + direccionMV + "|" + VelocidadMV.Text + "|" + CantidadMV.Text + "|" + (chkReversaMV.IsChecked == true ? "S" : "N");
            else
                Animaciones += "-M|N";

            GuardarInfo(new Seguridad().EncryptString(MainWindow.nombreApp, Animaciones), NombreControl);

            if (chkGirar.IsChecked == true)
            {

                RotateTransform rotate = new RotateTransform();

                DoubleAnimation anim = new DoubleAnimation(0, direccionG.Equals("D") ? 360 : -360, TimeSpan.FromMilliseconds(int.Parse(VelocidadG.Text)));
                anim.RepeatBehavior = RepeatBehavior.Forever;
                rotate.BeginAnimation(RotateTransform.AngleProperty, anim);

                item.RenderTransform = rotate;

                myTransformGroup.Children.Add(rotate);
            }

            if (chkEscalar.IsChecked == true)
            {
                Storyboard storyboard = new Storyboard();

                DoubleAnimation growAnimation = new DoubleAnimation();
                growAnimation.Duration = TimeSpan.FromMilliseconds(int.Parse(VelocidadE.Text));
                growAnimation.From = 1;
                growAnimation.To = 1 + tamanoE;
                growAnimation.AutoReverse = true;
                growAnimation.RepeatBehavior = RepeatBehavior.Forever;
                storyboard.Children.Add(growAnimation);

                Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.ScaleX"));
                Storyboard.SetTarget(growAnimation, item);

                DoubleAnimation growAnimation2 = new DoubleAnimation();
                growAnimation2.Duration = TimeSpan.FromMilliseconds(int.Parse(VelocidadE.Text));
                growAnimation2.From = 1;
                growAnimation2.To = 1 + tamanoE;
                growAnimation2.AutoReverse = true;
                growAnimation2.RepeatBehavior = RepeatBehavior.Forever;
                storyboard.Children.Add(growAnimation2);

                Storyboard.SetTargetProperty(growAnimation2, new PropertyPath("RenderTransform.ScaleY"));
                Storyboard.SetTarget(growAnimation2, item);

                ScaleTransform scale = new ScaleTransform();
                item.RenderTransform = scale;
                storyboard.Begin();

                myTransformGroup.Children.Add(scale);
            }
            if (chkMover.IsChecked == true)
            {
                if (chkHorizontal.IsChecked == true || chkVertical.IsChecked == true)
                {
                    Storyboard storyboard = new Storyboard();

                    if (chkHorizontal.IsChecked == true)
                    {
                        DoubleAnimation growAnimation = new DoubleAnimation();
                        growAnimation.Duration = TimeSpan.FromMilliseconds(int.Parse(VelocidadMH.Text));
                        growAnimation.From = 0;
                        growAnimation.To = direccionMH.Equals("D") ? double.Parse(CantidadMH.Text) : -double.Parse(CantidadMH.Text);
                        growAnimation.AutoReverse = chkReversaMH.IsChecked == true;
                        growAnimation.RepeatBehavior = RepeatBehavior.Forever;
                        storyboard.Children.Add(growAnimation);

                        Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.X"));
                        Storyboard.SetTarget(growAnimation, item);
                    }

                    if (chkVertical.IsChecked == true)
                    {
                        DoubleAnimation growAnimation2 = new DoubleAnimation();
                        growAnimation2.Duration = TimeSpan.FromMilliseconds(int.Parse(VelocidadMV.Text));
                        growAnimation2.From = 0;
                        growAnimation2.To = direccionMV.Equals("B") ? double.Parse(CantidadMV.Text) : -double.Parse(CantidadMV.Text);
                        growAnimation2.AutoReverse = chkReversaMV.IsChecked == true;
                        growAnimation2.RepeatBehavior = RepeatBehavior.Forever;
                        storyboard.Children.Add(growAnimation2);

                        Storyboard.SetTargetProperty(growAnimation2, new PropertyPath("RenderTransform.Y"));
                        Storyboard.SetTarget(growAnimation2, item);
                    }


                    TranslateTransform traslate = new TranslateTransform();
                    item.RenderTransform = traslate;
                    storyboard.Begin();

                    myTransformGroup.Children.Add(traslate);
                }
            }

            if (_currentTT != null)
                myTransformGroup.Children.Add(new TranslateTransform(_currentTT.X, _currentTT.Y));
            item.RenderTransformOrigin = new Point(.5, .5);
            item.RenderTransform = myTransformGroup;

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
            CargarControles();
        }

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
            dialog.lblNombre.Content = "¡Advertencia!";
            dialog.lblTexto.Text = "Se eliminarán las animaciones de forma permanete el objeto.";
            if (dialog.ShowDialog() == true)
            {
                chkEscalar.IsChecked = false;
                chkGirar.IsChecked = false;
                chkMover.IsChecked = false;
            }
        }

        private bool GuardarInfo(string pvStrAnimacion, string pvStrNombreObjeto)
        {
            try
            {
                using (Stream stream = new FileStream(@".\" + (mainWindow.Name.Equals("VentanaPrincipal") ? "objetos" : "objetosTurno") + @"\animaciones\" + pvStrNombreObjeto + ".anim", FileMode.Create))
                {
                    stream.SetLength(0);
                    byte[] bytes = Encoding.UTF8.GetBytes(pvStrAnimacion);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                    return true;
                }
            }
           catch { return false; }
        }
    }
}
