using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para MostrarTurno.xaml
    /// </summary>
    public partial class MostrarTurno : Window
    {
        private Point _positionInBlock;
        private TranslateTransform? _currentTT;
        private string controlClickName;
        public Color ultimoColorLetra;
        public Color ultimoColorFondo;
        private bool estaSaliendo = false;
        private bool esDiseno;

        public MostrarTurno(bool pEsDiseno = false)
        {
            esDiseno = pEsDiseno;
            InitializeComponent();
            double height = SystemParameters.FullPrimaryScreenHeight;
            double width = SystemParameters.FullPrimaryScreenWidth;
            Height = height - (height * .10);
            Width = width / 2;
            MaxHeight = Height;
            MaxWidth = Width;
            if (!esDiseno)
            {
                StartCloseTimer();
                ModoEdicion.Visibility = Visibility.Hidden;
                Coordenadas.Visibility = Visibility.Hidden;
            }

        }

        public async Task ActivarVoz()
        {
            var synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            string line = string.Empty;
            try
            {
                using (Stream stream = new FileStream(@".\vozTurnero.3k", FileMode.Open))
                {
                    var sr = new StreamReader(stream);

                    line = sr.ReadToEnd();
                    stream.Close();
                }
            }
            catch { }
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                line = line.Replace(@"NumeroTurno", NumeroTurno.Content.ToString());

                line = line.Replace(@"NumeroEquipo", NumeroEquipo.Content.ToString());

                line = line.Replace(@"NombreEquipo", NombreEquipo.Content.ToString());

                line = line.Replace(@"NumeroTurnoAnt", NumeroTurnoAnt.Content.ToString());

                line = line.Replace(@"NumeroEquipoAnt", NumeroEquipoAnt.Content.ToString());

                line = line.Replace(@"NombreEquipoAnt", NombreEquipoAnt.Content.ToString());
            }));
            synthesizer.SpeakAsync(line);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick;
            Close();
        }

        private void StartCloseTimer()
        {

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            double ms = double.Parse(config.AppSettings.Settings["DuracionTurnero"].Value);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(ms);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void Principal_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
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

        private void Principal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    var item = e.Source as UIElement;
                    controlClickName = item.GetValue(NameProperty).ToString();

                    if (SeModificaControl(controlClickName))
                    {
                        mostarPropiedadesObjetos(e);
                    }
                }
            }
            catch (Exception) { }
        }

        private void Principal_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = e.Source as UIElement;

                if (!SeModificaControl(item.GetValue(NameProperty).ToString()))
                {
                    ContextMenu cm = this.FindResource("cmdPrincipalContexMenu") as ContextMenu;

                    Label control = (Label)FindName("NumeroTurno");
                    if (control.Visibility == Visibility.Visible)
                        ((MenuItem)cm.Items[2]).Header = "Ocultar turno";
                    else
                        ((MenuItem)cm.Items[2]).Header = "Mostrar turno";

                    control = (Label)FindName("NumeroEquipo");
                    if (control.Visibility == Visibility.Visible)
                        ((MenuItem)cm.Items[3]).Header = "Ocultar equipo";
                    else
                        ((MenuItem)cm.Items[3]).Header = "Mostrar equipo";

                    control = (Label)FindName("NombreEquipo");
                    if (control.Visibility == Visibility.Visible)
                        ((MenuItem)cm.Items[4]).Header = "Ocultar nombre";
                    else
                        ((MenuItem)cm.Items[4]).Header = "Mostrar nombre";


                    control = (Label)FindName("NumeroTurnoAnt");
                    if (control.Visibility == Visibility.Visible)
                        ((MenuItem)cm.Items[5]).Header = "Ocultar turno anterior";
                    else
                        ((MenuItem)cm.Items[5]).Header = "Mostrar turno anterior";

                    control = (Label)FindName("NumeroEquipoAnt");
                    if (control.Visibility == Visibility.Visible)
                        ((MenuItem)cm.Items[6]).Header = "Ocultar equipo anterior";
                    else
                        ((MenuItem)cm.Items[6]).Header = "Mostrar equipo anterior";

                    control = (Label)FindName("NombreEquipoAnt");
                    if (control.Visibility == Visibility.Visible)
                        ((MenuItem)cm.Items[7]).Header = "Ocultar nombre anterior";
                    else
                        ((MenuItem)cm.Items[7]).Header = "Mostrar nombre anterior";



                    MenuItem itemCm = (MenuItem)cm.Items[9];
                    itemCm.Items.Clear();
                    foreach (var itemObjets in Principal.Children)
                    {
                        string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                        if (SeModificaControl(nombreControl) && (itemObjets as UIElement).Visibility == Visibility.Visible)
                        {
                            MenuItem itemControl = new MenuItem();
                            itemControl.Header = nombreControl + "-(" + (itemObjets as UIElement).GetType().Name + ")";
                            itemControl.Tag = nombreControl;
                            itemControl.PreviewMouseLeftButtonDown += MenuListaObjetos_PreviewMouseLeftButtonDown;
                            itemCm.Items.Add(itemControl);
                        }
                    }
                    cm.PlacementTarget = sender as Button;
                    cm.IsOpen = true;
                }
                else
                {
                    controlClickName = item.GetValue(NameProperty).ToString();
                    ContextMenu cm = this.FindResource("cmdContexMenu") as ContextMenu;
                    cm.PlacementTarget = sender as Button;
                    cm.IsOpen = true;

                }

            }
            catch (Exception) { }
        }

        private void MenuListaObjetos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem item = (MenuItem)e.Source;
            controlClickName = item.Tag.ToString();
            mostarPropiedadesObjetos(e);
        }

        private void MenuEliminar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BorarObjeto(controlClickName);
        }

        private bool SeEliminaControl(string name)
        {
            bool seElimina;
            switch (name)
            {
                case "Coordenadas":
                case "ModoEdicion":
                case "Principal":
                case "Fondo":
                case "Borde":
                case "NumeroTurno":
                case "NumeroEquipo":
                case "NombreEquipo":
                case "NumeroTurnoAnt":
                case "NumeroEquipoAnt":
                case "NombreEquipoAnt":
                    seElimina = false;
                    break;
                default:
                    seElimina = true;
                    break;
            }
            return seElimina;
        }

        private bool SeModificaControl(string name)
        {
            bool seModifica;
            switch (name)
            {
                case "Coordenadas":
                case "ModoEdicion":
                case "Principal":
                case "Fondo":
                case "Borde":
                    seModifica = false;
                    break;
                default:
                    seModifica = true;
                    break;
            }
            return seModifica;
        }

        private void mostarPropiedadesObjetos(MouseButtonEventArgs e)
        {
            var item = FindName(controlClickName) as UIElement;

            var mousePosition = e.GetPosition(Principal);
            var point = PointToScreen(mousePosition);
            switch (item.GetType().Name)
            {
                case "Label":
                    PropiedadesLabelTurno propiedadesLabel = new PropiedadesLabelTurno(this);
                    propiedadesLabel.WindowStartupLocation = WindowStartupLocation.Manual;

                    if (point.X + propiedadesLabel.Width >= MaxWidth)
                        propiedadesLabel.Left = point.X - propiedadesLabel.Width;
                    else
                        propiedadesLabel.Left = point.X;

                    if (point.Y + propiedadesLabel.Height >= MaxHeight)
                        propiedadesLabel.Top = point.Y - propiedadesLabel.Height;
                    else
                        propiedadesLabel.Top = point.Y;

                    propiedadesLabel.NombreControl.Text = item.GetValue(NameProperty).ToString();
                    propiedadesLabel.TipoControl.Text = item.GetType().Name;
                    propiedadesLabel.Contenido.Text = ((Label)item).Content.ToString();
                    if (item.GetValue(NameProperty).ToString().Equals("NumeroTurno") || item.GetValue(NameProperty).ToString().Equals("NumeroEquipo") || item.GetValue(NameProperty).ToString().Equals("NombreEquipo")
                        || item.GetValue(NameProperty).ToString().Equals("NumeroTurnoAnt") || item.GetValue(NameProperty).ToString().Equals("NumeroEquipoAnt") || item.GetValue(NameProperty).ToString().Equals("NombreEquipoAnt"))
                        propiedadesLabel.Contenido.IsReadOnly = true;
                    propiedadesLabel.cbxFuente.SelectedItem = item.GetValue(FontFamilyProperty);
                    propiedadesLabel.cbxTamano.SelectedValue = item.GetValue(FontSizeProperty);
                    propiedadesLabel.chkNegrita.IsChecked = item.GetValue(FontWeightProperty).ToString().CompareTo("Bold") == 0 ? true : false;
                    propiedadesLabel.chkCursiva.IsChecked = item.GetValue(FontStyleProperty).ToString().CompareTo("Italic") == 0 ? true : false;
                    propiedadesLabel.btnColorFuente.Fill = new SolidColorBrush((((Label)item).Foreground as SolidColorBrush).Color);
                    propiedadesLabel.btnColorFondo.Fill = new SolidColorBrush((((Label)item).Background as SolidColorBrush).Color);
                    propiedadesLabel.Opacidad.Value = item.Opacity;
                    propiedadesLabel.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                    propiedadesLabel.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
                    propiedadesLabel.ShowDialog();
                    break;
                case "Image":
                    PropiedadesMultimediaTurno propiedadesImagen = new PropiedadesMultimediaTurno(this);
                    propiedadesImagen.WindowStartupLocation = WindowStartupLocation.Manual;

                    if (point.X + propiedadesImagen.Width >= MaxWidth)
                        propiedadesImagen.Left = point.X - propiedadesImagen.Width;
                    else
                        propiedadesImagen.Left = point.X;

                    if (point.Y + propiedadesImagen.Height >= MaxHeight)
                        propiedadesImagen.Top = point.Y - propiedadesImagen.Height;
                    else
                        propiedadesImagen.Top = point.Y;

                    propiedadesImagen.Titulo.Content = "Propiedades \"" + item.GetValue(NameProperty).ToString() + "\"";
                    propiedadesImagen.NombreControl.Text = item.GetValue(NameProperty).ToString();
                    propiedadesImagen.TipoControl.Text = item.GetType().Name;
                    propiedadesImagen.Ruta.Text = ((Image)item).Source.ToString();
                    propiedadesImagen.Alto.Text = Math.Round(((Image)item).ActualHeight).ToString();
                    propiedadesImagen.Ancho.Text = Math.Round(((Image)item).ActualWidth).ToString();
                    propiedadesImagen.Opacidad.Value = item.Opacity;
                    propiedadesImagen.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                    propiedadesImagen.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
                    propiedadesImagen.chkRelacion.IsChecked = true;
                    propiedadesImagen.esInicio = false;
                    propiedadesImagen.ShowDialog();
                    break;

                case "MediaElement":
                    PropiedadesMultimediaTurno propiedadesMultimedia = new PropiedadesMultimediaTurno(this);
                    propiedadesMultimedia.WindowStartupLocation = WindowStartupLocation.Manual;

                    if (point.X + propiedadesMultimedia.Width >= MaxWidth)
                        propiedadesMultimedia.Left = point.X - propiedadesMultimedia.Width;
                    else
                        propiedadesMultimedia.Left = point.X;

                    if (point.Y + propiedadesMultimedia.Height >= MaxHeight)
                        propiedadesMultimedia.Top = point.Y - propiedadesMultimedia.Height;
                    else
                        propiedadesMultimedia.Top = point.Y;

                    propiedadesMultimedia.Titulo.Content = "Propiedades \"" + item.GetValue(NameProperty).ToString() + "\"";
                    propiedadesMultimedia.NombreControl.Text = item.GetValue(NameProperty).ToString();
                    propiedadesMultimedia.TipoControl.Text = item.GetType().Name;
                    propiedadesMultimedia.Ruta.Text = ((MediaElement)item).Source.ToString();
                    propiedadesMultimedia.Alto.Text = Math.Round(((MediaElement)item).ActualHeight).ToString();
                    propiedadesMultimedia.Ancho.Text = Math.Round(((MediaElement)item).ActualWidth).ToString();
                    propiedadesMultimedia.Opacidad.Value = item.Opacity;
                    propiedadesMultimedia.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                    propiedadesMultimedia.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
                    propiedadesMultimedia.chkRelacion.IsChecked = true;
                    propiedadesMultimedia.esInicio = false;
                    propiedadesMultimedia.ShowDialog();
                    break;

                default:

                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarControles();

            if (!esDiseno)
            {

                CargarAnimaciones();

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                try
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(@".\audios\" + config.AppSettings.Settings["AudioTurnero"].Value);
                    player.Play();
                }
                catch { }

                if (config.AppSettings.Settings["VozTurnero"].Value.Equals("true"))
                {
                    Task.Run(() => ActivarVoz());
                }
            }

        }

        public bool BorarObjeto(string pNombre)
        {
            if (SeEliminaControl(pNombre))
            {

                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
                dialog.lblNombre.Content = "¡Advertencia!";
                dialog.lblTexto.Text = "Se eliminará de forma permanete el objeto.";

                if (dialog.ShowDialog() == true)
                {
                    var item = FindName(pNombre) as UIElement;
                    Principal.Children.Remove(item);
                    NameScope.GetNameScope(this).UnregisterName(pNombre);
                    try
                    {
                        DirectoryInfo info = new DirectoryInfo(@"objetosTurno\");
                        foreach (var file in info.GetFiles())
                        {
                            string[] nombre = file.Name.Split('-');
                            if (nombre[1].Equals(pNombre + ".xaml"))
                                File.Delete(file.FullName);
                        }

                        info = new DirectoryInfo(@"objetosTurno\animaciones");

                        foreach (var file in info.GetFiles())
                        {
                            if (file.Name.Equals(pNombre + ".anim"))
                                File.Delete(file.FullName);
                        }

                    }
                    catch { }
                    return true;
                }
                return false;

            }
            else
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Este objeto no puede ser borrado.";
                dialog.ShowDialog();
                return false;
            }
        }

        private void Propiedades_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // get the position within the container
            var mousePosition = e.GetPosition(this);
            PropiedadesFondoTurnero propiedadesFondo = new PropiedadesFondoTurnero(this);
            propiedadesFondo.WindowStartupLocation = WindowStartupLocation.Manual;

            if (mousePosition.X + propiedadesFondo.Width >= MaxWidth)
                propiedadesFondo.Left = mousePosition.X - propiedadesFondo.Width;
            else
                propiedadesFondo.Left = mousePosition.X;

            if (mousePosition.Y + propiedadesFondo.Height >= MaxHeight)
                propiedadesFondo.Top = mousePosition.Y - propiedadesFondo.Height;
            else
                propiedadesFondo.Top = mousePosition.Y;
            try
            {
                propiedadesFondo.btnColorFondo.Fill = new SolidColorBrush(((SolidColorBrush)Principal.Background).Color);
            }
            catch (Exception)
            {
                propiedadesFondo.btnColorFondo.Fill = new SolidColorBrush(Colors.White);
            }
            propiedadesFondo.ShowDialog();
        }

        private void PropiedadesBorde_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // get the position within the container
            var mousePosition = e.GetPosition(this);
            PropiedadesBordeTurnero propiedadesBordeTurnero = new PropiedadesBordeTurnero(this);
            propiedadesBordeTurnero.WindowStartupLocation = WindowStartupLocation.Manual;

            if (mousePosition.X + propiedadesBordeTurnero.Width >= MaxWidth)
                propiedadesBordeTurnero.Left = mousePosition.X - propiedadesBordeTurnero.Width;
            else
                propiedadesBordeTurnero.Left = mousePosition.X;

            if (mousePosition.Y + propiedadesBordeTurnero.Height >= MaxHeight)
                propiedadesBordeTurnero.Top = mousePosition.Y - propiedadesBordeTurnero.Height;
            else
                propiedadesBordeTurnero.Top = mousePosition.Y;

            var item = FindName("Borde") as UIElement;

            propiedadesBordeTurnero.NombreControl.Text = item.GetValue(NameProperty).ToString();
            propiedadesBordeTurnero.TipoControl.Text = item.GetType().Name;
            propiedadesBordeTurnero.btnColorBorde.Fill = new SolidColorBrush((((Border)item).BorderBrush as SolidColorBrush).Color);
            propiedadesBordeTurnero.cbxGrosor.SelectedValue = ((Border)item).BorderThickness.Left;
            propiedadesBordeTurnero.ShowDialog();
        }

        private void MenuAgregarTexto_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CapturaTextoDialogo dialog = new CapturaTextoDialogo(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            var point = e.GetPosition(Principal);
            var mousePosition = PointToScreen(point);


            if (mousePosition.X + dialog.Width >= MaxWidth)
                dialog.Left = mousePosition.X - dialog.Width;
            else
                dialog.Left = mousePosition.X;

            if (mousePosition.Y + dialog.Height >= MaxHeight)
                dialog.Top = mousePosition.Y - dialog.Height;
            else
                dialog.Top = mousePosition.Y;

            dialog.btnAbrir.Visibility = Visibility.Hidden;
            dialog.Titulo.Content = "Agregar texto";
            if (dialog.ShowDialog() == true)
            {
                Label obj = new Label();
                obj.Name = dialog.NombreText.ToUpper();
                obj.ToolTip = dialog.NombreText.ToUpper();
                obj.Content = dialog.ContenidoText;
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
            }
        }

        private void MenuAgregarImagen_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CapturaTextoDialogo dialog = new CapturaTextoDialogo(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            var point = e.GetPosition(Principal);
            var mousePosition = PointToScreen(point);


            if (mousePosition.X + dialog.Width >= MaxWidth)
                dialog.Left = mousePosition.X - dialog.Width;
            else
                dialog.Left = mousePosition.X;

            if (mousePosition.Y + dialog.Height >= MaxHeight)
                dialog.Top = mousePosition.Y - dialog.Height;
            else
                dialog.Top = mousePosition.Y;

            dialog.lblTexto.Content = "Abrir";
            dialog.Titulo.Content = "Agregar imagen";
            dialog.ContenidoTextBox.IsEnabled = false;
            if (dialog.ShowDialog() == true)
            {
                string extension = System.IO.Path.GetExtension(dialog.ContenidoText).Replace(".", "").ToLower();
                if (extension.Equals("gif"))
                {
                    MediaElement obj = new MediaElement();
                    obj.Name = dialog.NombreText.ToUpper();
                    obj.ToolTip = dialog.NombreText.ToUpper();
                    obj.Source = new Uri(dialog.ContenidoText);
                    obj.MediaEnded += MediaElement_MediaEnded;
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.Stretch = Stretch.Uniform;
                    obj.MaxHeight = MaxHeight;
                    obj.MaxWidth = MaxWidth;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(dialog.ContenidoText);
                    bitmapImage.EndInit();

                    obj.Height = bitmapImage.Height;

                    obj.Tag = "";
                    NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                    Principal.Children.Add(obj);
                }
                else
                {
                    Image obj = new Image();
                    obj.Name = dialog.NombreText.ToUpper();
                    obj.ToolTip = dialog.NombreText.ToUpper();

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(dialog.ContenidoText);
                    bitmapImage.EndInit();

                    obj.Source = bitmapImage;
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.Stretch = Stretch.Uniform;
                    obj.MaxHeight = MaxHeight;
                    obj.MaxWidth = MaxWidth;
                    obj.Tag = "";
                    NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                    Principal.Children.Add(obj);

                }
            }
        }

        private void MenuTraerFrente_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = FindName(controlClickName) as UIElement;
            Principal.Children.Remove(item);
            NameScope.GetNameScope(this).UnregisterName(controlClickName);
            NameScope.GetNameScope(this).RegisterName(controlClickName, item);
            Principal.Children.Add(item);

        }

        private void MenuPropiedades_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mostarPropiedadesObjetos(e);
            }
            catch (Exception) { }
        }

        private void MenuSalir_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SalirEdicion();
        }

        private void MenuReiniciarDiseno_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
            dialog.lblNombre.Content = "¡Advertencia!";
            dialog.lblTexto.Text = "Se eliminará todo el diseño de forma permanente.";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(@".\objetosTurno");
                    foreach (FileInfo file in di.EnumerateFiles())
                    {
                        file.Delete();
                    }
                }
                catch { }
                esDiseno = false;
                Close();
                MostrarTurno dialog2 = new MostrarTurno(true);
                dialog2.ShowDialog();
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement item = (MediaElement)e.Source;
            item.Position = TimeSpan.FromMilliseconds(1);
        }
        private void GuardarControles()
        {
            try
            {
                if (!Directory.Exists(@".\objetosTurno"))
                {
                    Directory.CreateDirectory(@".\objetosTurno");
                }
                int x = 0;
                DirectoryInfo di = new DirectoryInfo(@".\objetosTurno");
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (var itemObjets in Principal.Children)
                {
                    bool esTabla;
                    string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                    if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo") || nombreControl.Equals("Borde"))
                    {
                        StringBuilder outstr = new StringBuilder();

                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.OmitXmlDeclaration = true;
                        settings.NewLineOnAttributes = true;


                        XamlDesignerSerializationManager dsm = new XamlDesignerSerializationManager(XmlWriter.Create(outstr, settings));
                        dsm.XamlWriterMode = XamlWriterMode.Expression;

                        XamlWriter.Save(itemObjets, dsm);
                        string savedControls = outstr.ToString();
                        Seguridad vSeguridad = new Seguridad();
                        File.WriteAllText(@"objetosTurno\" + ++x + "-" + nombreControl + ".xaml", vSeguridad.EncryptString(MainWindow.nombreApp, savedControls));
                    }
                }
            }
            catch (Exception) { }
        }

        private void CargarControles()
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(@"objetosTurno\");
                foreach (var file in info.GetFiles())
                {

                    try
                    {
                        StreamReader sR = new StreamReader(@file.FullName);
                        string text = sR.ReadToEnd();
                        sR.Close();
                        Seguridad vSeguridad = new Seguridad();
                        StringReader stringReader = new StringReader(vSeguridad.DecryptString(MainWindow.nombreApp, text));
                        XmlReader xmlReader = XmlReader.Create(stringReader);
                        object ob = System.Windows.Markup.XamlReader.Load(xmlReader);
                        var item = ob as UIElement;
                        if (item.GetValue(NameProperty).ToString().Equals("Fondo"))
                        {
                            Fondo.Background = ((Grid)item).Background;
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("Borde"))
                        {
                            Principal.Children.Remove(Borde);
                            NameScope.GetNameScope(this).UnregisterName(Borde.Name);

                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("NumeroTurno"))
                        {
                            Principal.Children.Remove(NumeroTurno);
                            NameScope.GetNameScope(this).UnregisterName(NumeroTurno.Name);
                            ((Label)item).Content = NumeroTurno.Content;
                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("NumeroEquipo"))
                        {
                            Principal.Children.Remove(NumeroEquipo);
                            NameScope.GetNameScope(this).UnregisterName(NumeroEquipo.Name);
                            ((Label)item).Content = NumeroEquipo.Content;
                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("NombreEquipo"))
                        {
                            Principal.Children.Remove(NombreEquipo);
                            NameScope.GetNameScope(this).UnregisterName(NombreEquipo.Name);
                            ((Label)item).Content = NombreEquipo.Content;
                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("NumeroTurnoAnt"))
                        {
                            Principal.Children.Remove(NumeroTurnoAnt);
                            NameScope.GetNameScope(this).UnregisterName(NumeroTurnoAnt.Name);
                            ((Label)item).Content = NumeroTurnoAnt.Content;
                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("NumeroEquipoAnt"))
                        {
                            Principal.Children.Remove(NumeroEquipoAnt);
                            NameScope.GetNameScope(this).UnregisterName(NumeroEquipoAnt.Name);
                            ((Label)item).Content = NumeroEquipoAnt.Content;
                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("NombreEquipoAnt"))
                        {
                            Principal.Children.Remove(NombreEquipoAnt);
                            NameScope.GetNameScope(this).UnregisterName(NombreEquipoAnt.Name);
                            ((Label)item).Content = NombreEquipoAnt.Content;
                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        else
                        {
                            try
                            {
                                NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                                Principal.Children.Add(item);
                            }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
        }

        private void SalirEdicion()
        {
            GuardarControles();
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !estaSaliendo)
            {
                estaSaliendo = true;
                SalirEdicion();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (esDiseno)
                GuardarControles();
        }

        private void MenuMostrarOcultarTurno_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroTurno");

            if (control.Visibility == Visibility.Visible)
            {
                control.Visibility = Visibility.Hidden;
                itemCm.Header = "Ocultar turno";
            }
            else
            {
                control.Visibility = Visibility.Visible;
                itemCm.Header = "Mostrar turno";
            }
        }

        private void MenuMostrarOcultarEquipo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroEquipo");
            if (control.Visibility == Visibility.Visible)
            {
                control.Visibility = Visibility.Hidden;
                itemCm.Header = "Ocultar equipo";
            }
            else
            {
                control.Visibility = Visibility.Visible;
                itemCm.Header = "Mostrar equipo";
            }
        }

        private void MenuMostrarOcultarNombreEquipo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NombreEquipo");
            if (control.Visibility == Visibility.Visible)
            {
                control.Visibility = Visibility.Hidden;
                itemCm.Header = "Ocultar nombre";
            }
            else
            {
                control.Visibility = Visibility.Visible;
                itemCm.Header = "Mostrar nombre";
            }
        }

        private void MenuMostrarOcultarTurnoAnt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroTurnoAnt");
            if (control.Visibility == Visibility.Visible)
            {
                control.Visibility = Visibility.Hidden;
                itemCm.Header = "Ocultar turno anterior";
            }
            else
            {
                control.Visibility = Visibility.Visible;
                itemCm.Header = "Mostrar turno anterior";
            }


        }

        private void MenuMostrarOcultarEquipoAnt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroEquipoAnt");
            if (control.Visibility == Visibility.Visible)
            {
                control.Visibility = Visibility.Hidden;
                itemCm.Header = "Ocultar equipo anterior";
            }
            else
            {
                control.Visibility = Visibility.Visible;
                itemCm.Header = "Mostrar equipo anterior";
            }
        }

        private void MenuMostrarOcultarNombreEquipoAnt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NombreEquipoAnt");
            if (control.Visibility == Visibility.Visible)
            {
                control.Visibility = Visibility.Hidden;
                itemCm.Header = "Ocultar nombre anterior";
            }
            else
            {
                control.Visibility = Visibility.Visible;
                itemCm.Header = "Mostrar nombre anterior";
            }
        }

        private void CargarAnimaciones()
        {
            foreach (UIElement item in Principal.Children)
            {
                if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    Animacion(item.GetValue(NameProperty).ToString());
            }
        }

        private void Animacion(string pNombreControl)
        {
            var item = FindName(pNombreControl) as UIElement;

            TranslateTransform? _currentTTEscalar = new TranslateTransform();
            try
            {

                for (int x = 0; x < ((TransformGroup)item.RenderTransform).Children.Count; x++)
                {
                    switch (((TransformGroup)item.RenderTransform).Children[x].GetType().Name)
                    {
                        case "TranslateTransform":
                            item.RenderTransform = (TranslateTransform)((TransformGroup)item.RenderTransform).Children[x];
                            break;
                    }
                }
            }
            catch
            {
                if (item.RenderTransform != null)
                    _currentTTEscalar = item.RenderTransform as TranslateTransform;
            }

            TransformGroup myTransformGroup = new TransformGroup();

            string lectura = string.Empty;
            DirectoryInfo info = new DirectoryInfo(@"objetosTurno\animaciones");
            foreach (var file in info.GetFiles())
            {
                if (@file.Name.Equals(pNombreControl + ".anim"))
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
                                        Storyboard storyboard = new Storyboard();

                                        if (datos[3].Equals("S"))
                                        {
                                            DoubleAnimation growAnimation = new DoubleAnimation();
                                            growAnimation.Duration = TimeSpan.FromMilliseconds(int.Parse(datos[5]));
                                            growAnimation.From = 0;
                                            growAnimation.To = datos[4].Equals("D") ? double.Parse(datos[6]) : -double.Parse(datos[6]);
                                            growAnimation.AutoReverse = datos[7].Equals("S");
                                            growAnimation.RepeatBehavior = RepeatBehavior.Forever;
                                            storyboard.Children.Add(growAnimation);

                                            Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.X"));
                                            Storyboard.SetTarget(growAnimation, item);

                                        }
                                        if (datos[9].Equals("S"))
                                        {
                                            DoubleAnimation growAnimation2 = new DoubleAnimation();
                                            growAnimation2.Duration = TimeSpan.FromMilliseconds(int.Parse(datos[11]));
                                            growAnimation2.From = 0;
                                            growAnimation2.To = datos[10].Equals("B") ? double.Parse(datos[12]) : -double.Parse(datos[12]);
                                            growAnimation2.AutoReverse = datos[13].Equals("S");
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
                                    break;
                                case "E":
                                    if (datos[1].Equals("S"))
                                    {
                                        Storyboard storyboard = new Storyboard();

                                        DoubleAnimation growAnimation = new DoubleAnimation();
                                        growAnimation.Duration = TimeSpan.FromMilliseconds(int.Parse(datos[3]));
                                        growAnimation.From = 1;
                                        growAnimation.To = 1 + double.Parse(datos[2]);
                                        growAnimation.AutoReverse = true;
                                        growAnimation.RepeatBehavior = RepeatBehavior.Forever;
                                        storyboard.Children.Add(growAnimation);

                                        Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.ScaleX"));
                                        Storyboard.SetTarget(growAnimation, item);

                                        DoubleAnimation growAnimation2 = new DoubleAnimation();
                                        growAnimation2.Duration = TimeSpan.FromMilliseconds(int.Parse(datos[3]));
                                        growAnimation2.From = 1;
                                        growAnimation2.To = 1 + double.Parse(datos[2]);
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
                                    break;
                                case "G":
                                    if (datos[1].Equals("S"))
                                    {
                                        RotateTransform rotate = new RotateTransform();

                                        DoubleAnimation anim = new DoubleAnimation(0, datos[2].Equals("D") ? 360 : -360, TimeSpan.FromMilliseconds(int.Parse(datos[3])));
                                        anim.RepeatBehavior = RepeatBehavior.Forever;
                                        rotate.BeginAnimation(RotateTransform.AngleProperty, anim);

                                        item.RenderTransform = rotate;

                                        myTransformGroup.Children.Add(rotate);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                }
            }

            if (_currentTTEscalar != null)
                myTransformGroup.Children.Add(new TranslateTransform(_currentTTEscalar.X, _currentTTEscalar.Y));
            item.RenderTransformOrigin = new Point(.5, .5);
            item.RenderTransform = myTransformGroup;
        }
    }
}
