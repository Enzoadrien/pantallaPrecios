using Priceio.ClasesGenericas;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para Animaciones.xaml
    /// </summary>
    public partial class Animaciones : Window
    {
        private Window mainWindow;
        private string NombreControl;
        private TranslateTransform? _currentTT;
        private CancellationTokenSource cts = new();
        double opacidadOriginal;
        public Animaciones(Window pMainWindow, string pNombreControl)
        {
            InitializeComponent();
            mainWindow = pMainWindow;
            NombreControl = pNombreControl;

            var item = mainWindow.FindName(NombreControl) as UIElement;
            opacidadOriginal = item.Opacity;
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

        private void chkDesplazar_Checked(object sender, RoutedEventArgs e)
        {
            chkHorizontal.IsEnabled = true;
            chkVertical.IsEnabled = true;

            Animar();
        }

        private void chkDesplazar_Unchecked(object sender, RoutedEventArgs e)
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

            if (chkDesplazar.IsChecked == true)
                Animar();


        }

        private void chkHorizontal_Unchecked(object sender, RoutedEventArgs e)
        {
            cbxDireccionMH.IsEnabled = false;
            VelocidadMH.IsReadOnly = true;
            CantidadMH.IsReadOnly = true;
            chkReversaMH.IsEnabled = false;
            chkReversaMH.IsChecked = false;

            if (chkDesplazar.IsChecked == true)
                Animar();
        }

        private void chkVertical_Unchecked(object sender, RoutedEventArgs e)
        {
            cbxDireccionMV.IsEnabled = false;
            VelocidadMV.IsReadOnly = true;
            CantidadMV.IsReadOnly = true;
            chkReversaMV.IsEnabled = false;
            chkReversaMV.IsChecked = false;

            if (chkDesplazar.IsChecked == true)
                Animar();
        }

        private void chkVertical_Checked(object sender, RoutedEventArgs e)
        {
            cbxDireccionMV.IsEnabled = true;
            VelocidadMV.IsReadOnly = false;
            CantidadMV.IsReadOnly = false;
            chkReversaMV.IsEnabled = true;

            if (chkDesplazar.IsChecked == true)
                Animar();


        }

        private void cbxDireccionMH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chkDesplazar.IsChecked == true && chkHorizontal.IsChecked == true)
                Animar();
        }

        private void cbxDireccionMV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chkDesplazar.IsChecked == true && chkVertical.IsChecked == true)
                Animar();
        }

        private void VelocidadMH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VelocidadMH.Text.Length > 0)
                if (chkDesplazar.IsChecked == true)
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
                if (chkDesplazar.IsChecked == true)
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
                if (chkDesplazar.IsChecked == true)
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
                if (chkDesplazar.IsChecked == true)
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
            cbxAngulo.IsEnabled = true;
            chkGirarAutomatico.IsEnabled = true;
        }

        private void chkGirar_Unchecked(object sender, RoutedEventArgs e)
        {
            cbxAngulo.IsEnabled = false;
            cbxAngulo.SelectedIndex = 40;
            chkGirarAutomatico.IsEnabled = false;
            chkGirarAutomatico.IsChecked = false;
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
            DirectoryInfo info = new DirectoryInfo(@".\data\" + (mainWindow.Name.Equals("VentanaPrincipal") ? "objetos" : "objetosSplash") + @"\animaciones");
            foreach (var file in info.GetFiles())
            {
                if (@file.Name.Equals(NombreControl + ".anim"))
                {
                    try
                    {
                        ConfiguracionAnimaciones configuracionAnimaciones = JsonSerializer.Deserialize<ConfiguracionAnimaciones>(new Seguridad().DecryptString(MainWindow.nombreApp, File.ReadAllText(file.FullName).ToString()));
                        chkDesplazar.IsChecked = configuracionAnimaciones.Desplazar;
                        chkHorizontal.IsChecked = configuracionAnimaciones.Horizontal;
                        cbxDireccionMH.SelectedValue = configuracionAnimaciones.DirecionHorizontal;
                        VelocidadMH.Text = configuracionAnimaciones.VelocidadHorizontal.ToString();
                        CantidadMH.Text = configuracionAnimaciones.CantidadHorizontal.ToString();
                        chkReversaMH.IsChecked = configuracionAnimaciones.ReversaHorizontal;
                        chkVertical.IsChecked = configuracionAnimaciones.Vertical;
                        cbxDireccionMV.SelectedValue = configuracionAnimaciones.DirecionVertical;
                        VelocidadMV.Text = configuracionAnimaciones.VelocidadVertical.ToString();
                        CantidadMH.Text = configuracionAnimaciones.CantidadVertical.ToString();
                        chkReversaMV.IsChecked = configuracionAnimaciones.ReversaVertical;
                        chkEscalar.IsChecked = configuracionAnimaciones.Escalar;
                        cbxTamanoE.SelectedValue = configuracionAnimaciones.TamanoEscalar;
                        VelocidadE.Text = configuracionAnimaciones.VelocidadEscalar.ToString();
                        chkDesvanecer.IsChecked = configuracionAnimaciones.Desvanecer;
                        VelocidadDesvanecer.Text = configuracionAnimaciones.VelocidadDesvanecer.ToString();
                        chkReversaDesvanecer.IsChecked = configuracionAnimaciones.ReversaDesvanecer;
                        chkGirar.IsChecked = configuracionAnimaciones.Girar;
                        cbxAngulo.SelectedValue = configuracionAnimaciones.AnguloGirar;
                        chkGirarAutomatico.IsChecked = configuracionAnimaciones.AutoGirar;
                        cbxAnguloAuto.SelectedValue = configuracionAnimaciones.AnguloAutoGirar;
                        VelocidadG.Text = configuracionAnimaciones.VelocidadGirar.ToString();
                        chkReversaGiro.IsChecked = configuracionAnimaciones.ReversaGirar;
                    }
                    catch { }
                }
            }
        }

        private void GuardarDatos()
        {
            GuardarInfo(new Seguridad().EncryptString(MainWindow.nombreApp, JsonSerializer.Serialize(ActualizaConfiguracionAnimaciones())), NombreControl);
        }

        private ConfiguracionAnimaciones ActualizaConfiguracionAnimaciones()
        {
            ConfiguracionAnimaciones configuracionAnimaciones = new ConfiguracionAnimaciones();
            configuracionAnimaciones.Desplazar = chkDesplazar.IsChecked;
            configuracionAnimaciones.Horizontal = chkHorizontal.IsChecked;
            configuracionAnimaciones.DirecionHorizontal = char.Parse(((ComboBoxItem)cbxDireccionMH.SelectedItem).Tag.ToString());
            configuracionAnimaciones.VelocidadHorizontal = int.Parse(VelocidadMH.Text);
            configuracionAnimaciones.CantidadHorizontal = int.Parse(CantidadMH.Text);
            configuracionAnimaciones.ReversaHorizontal = chkReversaMH.IsChecked;
            configuracionAnimaciones.Vertical = chkVertical.IsChecked;
            configuracionAnimaciones.DirecionVertical = char.Parse(((ComboBoxItem)cbxDireccionMV.SelectedItem).Tag.ToString());
            configuracionAnimaciones.VelocidadVertical = int.Parse(VelocidadMV.Text);
            configuracionAnimaciones.CantidadVertical = int.Parse(CantidadMH.Text);
            configuracionAnimaciones.ReversaVertical = chkReversaMV.IsChecked;
            configuracionAnimaciones.Escalar = chkEscalar.IsChecked;
            configuracionAnimaciones.TamanoEscalar = double.Parse(((ComboBoxItem)cbxTamanoE.SelectedItem).Tag.ToString());
            configuracionAnimaciones.VelocidadEscalar = int.Parse(VelocidadE.Text);
            configuracionAnimaciones.Desvanecer = chkDesvanecer.IsChecked;
            configuracionAnimaciones.VelocidadDesvanecer = int.Parse(VelocidadDesvanecer.Text);
            configuracionAnimaciones.ReversaDesvanecer = chkReversaDesvanecer.IsChecked;
            configuracionAnimaciones.Girar = chkGirar.IsChecked;
            configuracionAnimaciones.AnguloGirar = double.Parse(((ComboBoxItem)cbxAngulo.SelectedItem).Tag.ToString());
            configuracionAnimaciones.AutoGirar = chkGirarAutomatico.IsChecked;
            configuracionAnimaciones.AnguloAutoGirar = double.Parse(((ComboBoxItem)cbxAnguloAuto.SelectedItem).Tag.ToString());
            configuracionAnimaciones.VelocidadGirar = int.Parse(VelocidadG.Text);
            configuracionAnimaciones.ReversaGirar = chkReversaGiro.IsChecked;


            return configuracionAnimaciones;
        }

        private async Task Desvanecer(int velocidad, bool reversa, CancellationToken cancellationToken)
        {
            bool baja = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var item = mainWindow.FindName(NombreControl) as UIElement;
                if (VelocidadDesvanecer.Text.Length > 0)
                {
                    double disminucion = .001;
                    if (item.Opacity > -.01 && baja)
                    {
                        item.Opacity -= disminucion;
                    }
                    else if (item.Opacity < opacidadOriginal && !baja && reversa)
                    {
                        item.Opacity += disminucion;
                    }
                    else if (item.Opacity < opacidadOriginal && !baja && !reversa)
                    {
                        item.Opacity = opacidadOriginal;
                    }
                    else
                    {
                        if (baja)
                            baja = false;
                        else
                            baja = true;
                    }
                }

            }));
                if(reversa && velocidad > 1)
                    await Task.Delay(velocidad/2, cancellationToken);
                else
                    await Task.Delay(velocidad, cancellationToken);
            }
        }

        private void Animar()
        {

            var item = mainWindow.FindName(NombreControl) as UIElement;

            cts.Cancel();
            if (chkDesvanecer.IsChecked == true)
            {
                item.Opacity = opacidadOriginal;
                int velocidad = int.Parse(VelocidadDesvanecer.Text);
                bool reversa = chkReversaDesvanecer.IsChecked == true;
                cts = new();
                Task.Run(() => Desvanecer(velocidad, reversa, cts.Token));
            }
            else
                item.Opacity = opacidadOriginal;


            TransformGroup myTransformGroup = new TransformGroup();

            if (chkGirar.IsChecked == true && chkGirarAutomatico.IsChecked == false)
            {

                RotateTransform rotate = new RotateTransform(double.Parse(((ComboBoxItem)cbxAngulo.SelectedItem).Tag.ToString()));

                item.RenderTransform = rotate;

                myTransformGroup.Children.Add(rotate);
            }

            if (chkGirar.IsChecked == true &&  chkGirarAutomatico.IsChecked == true)
            {

                RotateTransform rotate = new RotateTransform();

                DoubleAnimation anim = new DoubleAnimation(double.Parse(((ComboBoxItem)cbxAngulo.SelectedItem).Tag.ToString()), double.Parse(((ComboBoxItem)cbxAnguloAuto.SelectedItem).Tag.ToString()), TimeSpan.FromMilliseconds(int.Parse(VelocidadG.Text)));
                anim.RepeatBehavior = RepeatBehavior.Forever;
                anim.AutoReverse = chkReversaGiro.IsChecked == true;
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
                growAnimation.To = 1 + double.Parse(((ComboBoxItem)cbxTamanoE.SelectedItem).Tag.ToString());
                growAnimation.AutoReverse = true;
                growAnimation.RepeatBehavior = RepeatBehavior.Forever;
                storyboard.Children.Add(growAnimation);

                Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.ScaleX"));
                Storyboard.SetTarget(growAnimation, item);

                DoubleAnimation growAnimation2 = new DoubleAnimation();
                growAnimation2.Duration = TimeSpan.FromMilliseconds(int.Parse(VelocidadE.Text));
                growAnimation2.From = 1;
                growAnimation2.To = 1 + double.Parse(((ComboBoxItem)cbxTamanoE.SelectedItem).Tag.ToString());
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
            if (chkDesplazar.IsChecked == true)
            {
                if (chkHorizontal.IsChecked == true || chkVertical.IsChecked == true)
                {
                    Storyboard storyboard = new Storyboard();

                    if (chkHorizontal.IsChecked == true)
                    {
                        DoubleAnimation growAnimation = new DoubleAnimation();
                        growAnimation.Duration = TimeSpan.FromMilliseconds(int.Parse(VelocidadMH.Text));
                        growAnimation.From = 0;
                        growAnimation.To = ((ComboBoxItem)cbxDireccionMH.SelectedItem).Tag.ToString().Equals("D") ? double.Parse(CantidadMH.Text) : -double.Parse(CantidadMH.Text);
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
                        growAnimation2.To = ((ComboBoxItem)cbxDireccionMV.SelectedItem).Tag.ToString().Equals("B") ? double.Parse(CantidadMV.Text) : -double.Parse(CantidadMV.Text);
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

            cts.Cancel();
            GuardarDatos();
            var item = mainWindow.FindName(NombreControl) as UIElement;
            item.Opacity = opacidadOriginal;
            if (_currentTT != null)
            {

                if (chkGirar.IsChecked == true && chkGirarAutomatico.IsChecked == false)
                {

                    TransformGroup myTransformGroup = new TransformGroup();
                    RotateTransform rotate = new RotateTransform(double.Parse(((ComboBoxItem)cbxAngulo.SelectedItem).Tag.ToString()));

                    item.RenderTransform = rotate;

                    myTransformGroup.Children.Add(rotate);

                    myTransformGroup.Children.Add(new TranslateTransform(_currentTT.X, _currentTT.Y));
                    item.RenderTransformOrigin = new Point(.5, .5);
                    item.RenderTransform = myTransformGroup;
                }
                else
                    item.RenderTransform = new TranslateTransform(_currentTT.X, _currentTT.Y);
            }

            else
            {
                if (chkGirar.IsChecked == true && chkGirarAutomatico.IsChecked == false)
                {
                    RotateTransform rotate = new RotateTransform(double.Parse(((ComboBoxItem)cbxAngulo.SelectedItem).Tag.ToString()));

                    item.RenderTransform = rotate;
                }
                else
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
                chkDesplazar.IsChecked = false;
            }
        }

        private bool GuardarInfo(string pvStrAnimacion, string pvStrNombreObjeto)
        {
            try
            {
                using (Stream stream = new FileStream(@".\data\" + (mainWindow.Name.Equals("VentanaPrincipal") ? "objetos" : "objetosSplash") + @"\animaciones\" + pvStrNombreObjeto + ".anim", FileMode.Create))
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

        private void chkGirarAutomatico_Checked(object sender, RoutedEventArgs e)
        {
            VelocidadG.IsReadOnly = false;
            chkReversaGiro.IsEnabled = true;
            cbxAnguloAuto.IsEnabled = true;
            Animar();
        }

        private void chkGirarAutomatico_Unchecked(object sender, RoutedEventArgs e)
        {
            VelocidadG.IsReadOnly = true;
            chkReversaGiro.IsEnabled = false;
            chkReversaGiro.IsChecked = false;
            cbxAnguloAuto.IsEnabled = false;
            cbxAnguloAuto.SelectedIndex = 40;
            Animar();
        }

        private void cbxAngulo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chkGirar.IsChecked == true)
                Animar();
        }

        private void chkReversaGiro_Checked(object sender, RoutedEventArgs e)
        {
            if (chkGirar.IsChecked == true)
                Animar();
        }

        private void chkReversaGiro_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkGirar.IsChecked == true)
                Animar();
        }

        private void chkDesvanecer_Checked(object sender, RoutedEventArgs e)
        {
            VelocidadDesvanecer.IsReadOnly = false;
            chkReversaDesvanecer.IsEnabled = true;
            Animar();
        }

        private void chkDesvanecer_Unchecked(object sender, RoutedEventArgs e)
        {
            VelocidadDesvanecer.IsReadOnly = true;
            chkReversaDesvanecer.IsEnabled = false;
            chkReversaDesvanecer.IsChecked = false;
            Animar();
        }

        private void VelocidadDesvanecer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VelocidadDesvanecer.Text.Length > 0)
                if (chkDesvanecer.IsChecked == true)
                    Animar();
        }

        private void VelocidadDesvanecer_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VelocidadE.Text.Length == 0)
                VelocidadE.Text = "0";
            Animar();
        }

        private void chkReversaDesvanecer_Checked(object sender, RoutedEventArgs e)
        {
            Animar();
        }

        private void chkReversaDesvanecer_Unchecked(object sender, RoutedEventArgs e)
        {
            Animar();
        }
    }
}
