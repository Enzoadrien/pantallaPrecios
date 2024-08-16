using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
using System.Windows.Threading;
using System.Xml;
using static Precios_Turnos.Recursos;

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para MostrarTurno.xaml
    /// </summary>
    public partial class MostrarTurno : Window
    {
        private Point _positionInBlock;
        private TranslateTransform? _currentTT;
        private string? controlClickName;
        public Color ultimoColorLetra;
        public Color ultimoColorFondo;
        private bool estaSaliendo = false;
        private bool esDiseno;
        private SolidColorBrush ultimoColor;
        private double ultimaOpacidad;
        private string? controlSelectedName;
        private MainWindow? mainWindow;
        public static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        Recursos.TipoVentana tipoVentana;

        public MostrarTurno(Recursos.TipoVentana pTipoVentana, bool pEsDiseno = false, MainWindow? parentWindow = null)
        {
            tipoVentana = pTipoVentana;
            Owner = parentWindow;
            mainWindow = parentWindow;
            esDiseno = pEsDiseno;
            InitializeComponent();
            if (tipoVentana == Recursos.TipoVentana.CAJERO)
            {
                Title = "Cajero";
            }
            else if (tipoVentana == Recursos.TipoVentana.VERIFICADOR)
            {
                Title = "Verificador";
            }
            else
            {
                Title = "Turno";
            }
            double height = SystemParameters.FullPrimaryScreenHeight;
            double width = SystemParameters.FullPrimaryScreenWidth;
            Height = height - (height * .10);
            Width = width / 2;
            MaxHeight = Height;
            MaxWidth = Width;
            if (!esDiseno)
            {
                IsHitTestVisible = false;
                ModoEdicion.Visibility = Visibility.Hidden;
                Coordenadas.Visibility = Visibility.Hidden;
                StartCloseTimer();
            }

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
            double ms = double.Parse(config.AppSettings.Settings["Duracion"].Value);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(ms);
            timer.Tick += TimerTick;
            timer.Start();
        }

        public void ActivarVoz()
        {
            try
            {
                synthesizer.SetOutputToDefaultAudioDevice();
                string line = string.Empty;
                try
                {
                    using (Stream stream = new FileStream(@".\Recursos\voz.3k", FileMode.Open))
                    {
                        var sr = new StreamReader(stream);

                        line = sr.ReadToEnd();
                        stream.Close();
                    }
                }
                catch { }
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Label NumeroTurno = (Label)FindName("NumeroTurno");
                    if(NumeroTurno != null)
                        line = line.Replace(@"NumeroTurno", NumeroTurno.Content.ToString());
                    Label NumeroEquipo = (Label)FindName("NumeroEquipo");
                    if (NumeroEquipo != null)
                        line = line.Replace(@"NumeroEquipo", NumeroEquipo.Content.ToString());
                    Label NombreEquipo = (Label)FindName("NombreEquipo");
                    if (NombreEquipo != null)
                        line = line.Replace(@"NombreEquipo", NombreEquipo.Content.ToString());
                    Label NumeroTurnoAnt = (Label)FindName("NumeroTurnoAnt");
                    if (NumeroTurnoAnt != null)
                        line = line.Replace(@"NumeroTurnoAnt", NumeroTurnoAnt.Content.ToString());
                    Label NumeroEquipoAnt = (Label)FindName("NumeroEquipoAnt");
                    if (NumeroEquipoAnt != null)
                        line = line.Replace(@"NumeroEquipoAnt", NumeroEquipoAnt.Content.ToString());
                }));

                synthesizer.SpeakAsync(line);
            }
            catch { }
            
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
                        switch (item.GetType().Name.ToString())
                        {
                            case "Label":
                                item.SetValue(BackgroundProperty, ultimoColor);
                                break;
                            case "MediaElement":
                            case "Image":
                                item.SetValue(OpacityProperty, ultimaOpacidad);
                                break;
                        }
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
                    if (control != null)
                        ((MenuItem)cm.Items[2]).Header = "Eliminar turno";
                    else
                        ((MenuItem)cm.Items[2]).Header = "Agregar turno";

                    control = (Label)FindName("NumeroEquipo");
                    if (control != null)
                        ((MenuItem)cm.Items[3]).Header = "Eliminar equipo";
                    else
                        ((MenuItem)cm.Items[3]).Header = "Agregar equipo";

                    control = (Label)FindName("NombreEquipo");
                    if (control != null)
                        ((MenuItem)cm.Items[4]).Header = "Eliminar nombre equipo";
                    else
                        ((MenuItem)cm.Items[4]).Header = "Agregar nombre equipo";


                    control = (Label)FindName("NumeroTurnoAnt");
                    if (control != null)
                        ((MenuItem)cm.Items[5]).Header = "Eliminar turno anterior";
                    else
                        ((MenuItem)cm.Items[5]).Header = "Agregar turno anterior";

                    control = (Label)FindName("NumeroEquipoAnt");
                    if (control != null)
                        ((MenuItem)cm.Items[6]).Header = "Eliminar equipo anterior";
                    else
                        ((MenuItem)cm.Items[6]).Header = "Agregar equipo anterior";



                    MenuItem itemCm = (MenuItem)cm.Items[8];
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
                case "Base":
                case "Coordenadas":
                case "ModoEdicion":
                case "Principal":
                case "Fondo":
                case "Borde":
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
                case "Base":
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
            Point pointItem = item.TransformToAncestor(this).Transform(new Point(0, 0));

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
                    propiedadesLabel.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();
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
                    propiedadesLabel.CoordenadaX.Text = Convert.ToInt32(pointItem.X).ToString();
                    propiedadesLabel.CoordenadaY.Text = Convert.ToInt32(pointItem.Y).ToString();
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
                    propiedadesImagen.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();
                    propiedadesImagen.TipoControl.Text = item.GetType().Name;
                    propiedadesImagen.Ruta.Text = Path.GetFileName(((Image)item).Source.ToString());
                    propiedadesImagen.Ruta.ToolTip = Path.GetFileName(((Image)item).Source.ToString());
                    propiedadesImagen.Alto.Text = Math.Round(((Image)item).ActualHeight).ToString();
                    propiedadesImagen.Ancho.Text = Math.Round(((Image)item).ActualWidth).ToString();
                    propiedadesImagen.Opacidad.Value = item.Opacity;
                    propiedadesImagen.CoordenadaX.Text = Convert.ToInt32(pointItem.X).ToString();
                    propiedadesImagen.CoordenadaY.Text = Convert.ToInt32(pointItem.Y).ToString();
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
                    propiedadesMultimedia.CoordenadaX.Text = Convert.ToInt32(pointItem.X).ToString();
                    propiedadesMultimedia.CoordenadaY.Text = Convert.ToInt32(pointItem.Y).ToString();
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
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(@".\Recursos\audios\" + config.AppSettings.Settings["Audio"].Value);
                    player.Play();
                }
                catch { }

                if (config.AppSettings.Settings["Voz"].Value.Equals("true"))
                {
                    Task.Run(() => ActivarVoz());
                }
            }

        }

        public bool BorarObjeto(string pNombre)
        {
            if (SeEliminaControl(pNombre))
            {
                var item = FindName(pNombre) as UIElement;
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
                dialog.lblNombre.Content = "¡Advertencia!";
                dialog.lblTexto.Text = "Se eliminará de forma permanete el objeto " + pNombre+ "("+ item.GetType().Name + ").";

                if (dialog.ShowDialog() == true)
                {
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
                        switch (item.GetType().Name.ToString())
                        {
                            case "Image":
                                File.Delete(new Uri(((Image)item).Source.ToString()).AbsolutePath);
                                break;
                            case "MediaElement":
                                File.Delete(new Uri(((MediaElement)item).Source.ToString()).AbsolutePath);
                                break;
                            default:
                                break;
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

        private void objeto_MouseLeave(object sender, MouseEventArgs e)
        {
            var control = e.Source as UIElement;
            control.SetValue(BackgroundProperty, ultimoColor);
        }

        private void objeto_MouseEnter(object sender, MouseEventArgs e)
        {
            var control = e.Source as UIElement;
            ultimoColor = new SolidColorBrush((control.GetValue(BackgroundProperty) as SolidColorBrush).Color);
            control.SetValue(BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffbee6fd")));
            controlSelectedName = control.GetValue(NameProperty).ToString();
        }

        private void objetoMedia_MouseLeave(object sender, MouseEventArgs e)
        {
            var control = e.Source as UIElement;
            control.SetValue(OpacityProperty, ultimaOpacidad);
        }

        private void objetoMedia_MouseEnter(object sender, MouseEventArgs e)
        {
            var control = e.Source as UIElement;
            ultimaOpacidad = control.Opacity;
            control.SetValue(OpacityProperty, ultimaOpacidad > .5 ? ultimaOpacidad - .3 : ultimaOpacidad + .3);
            controlSelectedName = control.GetValue(NameProperty).ToString();
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
                propiedadesFondo.btnColorFondo.Fill = new SolidColorBrush(((SolidColorBrush)Base.Background).Color);
            }
            catch (Exception)
            {
                propiedadesFondo.btnColorFondo.Fill = new SolidColorBrush(Colors.White);
            }
            try
            {
                propiedadesFondo.ContenidoTextBox.Text = Path.GetFileName(Fondo.Source.ToString());
                propiedadesFondo.ContenidoTextBox.ToolTip = Path.GetFileName(Fondo.Source.ToString());
                propiedadesFondo.Opacidad.IsEnabled = true;
                propiedadesFondo.Opacidad.Value = Fondo.Opacity;
            }
            catch { }
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
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
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
            dialog.ContenidoTextBox.IsReadOnly = true;
            if (dialog.ShowDialog() == true)
            {
                string extension = System.IO.Path.GetExtension(dialog.ContenidoText).Replace(".", "").ToLower();

                FileInfo fi = new FileInfo(dialog.ContenidoText);
                try
                {
                    FileInfo fileImg = new FileInfo(@".\objetosTurno\multimedia\" + fi.Name);
                    if (File.Exists(@".\objetosTurno\multimedia\" + fi.Name) && !fi.FullName.Equals(fileImg.FullName))
                    {
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, "Remplazar", "Mantener");
                        dialogMsg.lblNombre.Content = "¡Advertencia!";
                        dialogMsg.lblTexto.Text = "Ya existe un archivo con el mismo nombre y extension en la aplicación, ¿Desea remplazarlo o mantener la actual?. ¡Esta accion no se puede revertir!";
                        if (dialogMsg.ShowDialog() == true)
                        {
                            fi.CopyTo(@".\objetosTurno\multimedia\" + fi.Name, true);
                            foreach (var itemObjets in Principal.Children)
                            {
                                string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                                if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo"))
                                {
                                    switch (itemObjets.GetType().Name.ToString())
                                    {
                                        case "Image":
                                            if (((BitmapImage)((Image)itemObjets).Source).UriSource.Equals(@".\objetosTurno\multimedia\" + fi.Name))
                                            {
                                                BitmapImage bitmapImage = new BitmapImage();
                                                bitmapImage.BeginInit();
                                                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                                bitmapImage.UriSource = new Uri(@".\objetosTurno\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                                                bitmapImage.EndInit();

                                                ((Image)itemObjets).Source = bitmapImage;
                                            }
                                            break;
                                        case "MediaElement":
                                            if (((MediaElement)itemObjets).Source.Equals(@".\objetosTurno\multimedia\" + fi.Name))
                                            {
                                                Application.Current.Dispatcher.Invoke(new Action(async () =>
                                                {
                                                    ((MediaElement)itemObjets).Source = null;
                                                    await Task.Delay(100);
                                                    ((MediaElement)itemObjets).Source = new Uri(@".\objetosTurno\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                                                }));
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    else
                        fi.CopyTo(@".\objetosTurno\multimedia\" + fi.Name, true);
                }
                catch
                {
                }
                if (extension.Equals("gif"))
                {
                    MediaElement obj = new MediaElement();
                    obj.Name = dialog.NombreText.ToUpper();
                    obj.ToolTip = dialog.NombreText.ToUpper();
                    obj.Source = new Uri(@".\objetosTurno\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                    obj.MediaEnded += MediaElement_MediaEnded;
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.Stretch = Stretch.Uniform;
                    obj.MaxHeight = MaxHeight;
                    obj.MaxWidth = MaxWidth;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = new Uri(@".\objetosTurno\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                    bitmapImage.EndInit();
                    
                    obj.Height = bitmapImage.Height;
                    obj.Tag = "";
                    obj.MouseLeave += objetoMedia_MouseLeave;
                    obj.MouseEnter += objetoMedia_MouseEnter;

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
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = new Uri(@".\objetosTurno\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                    bitmapImage.EndInit();

                    obj.Source = bitmapImage;
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.Stretch = Stretch.Uniform;
                    obj.MaxHeight = MaxHeight;
                    obj.MaxWidth = MaxWidth;
                    obj.Tag = "";
                    obj.MouseLeave += objetoMedia_MouseLeave;
                    obj.MouseEnter += objetoMedia_MouseEnter;

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
                    foreach (var item in Directory.GetFiles(@".\objetosTurno\multimedia", "*.*"))
                    {
                        File.SetAttributes(item, FileAttributes.Normal);
                        File.Delete(item);
                    }
                }
                catch { }
                esDiseno = false;
                Close();
                MostrarTurno dialog2 = new MostrarTurno(tipoVentana, true);
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
                var controlSelected = FindName(controlSelectedName) as UIElement;
                switch (controlSelected.GetType().Name.ToString())
                {
                    case "Image":
                    case "MediaElement":
                    case "DataGrid":
                        controlSelected.SetValue(OpacityProperty, ultimaOpacidad);
                        break;
                    case "DockPanel":
                    case "Label":
                        controlSelected.SetValue(BackgroundProperty, ultimoColor);
                        break;
                }

            }
            catch { }

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
                    string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                    if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo") || nombreControl.Equals("Base") || nombreControl.Equals("Borde"))
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

        public void CargarControles()
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(@"objetosTurno\");
                foreach (var file in info.GetFiles().OrderBy(x => int.Parse(x.Name.Substring(0, x.Name.IndexOf('-')))).ToArray())
                {

                    try
                    {
                        StreamReader sR = new StreamReader(@file.FullName);
                        string text = sR.ReadToEnd();
                        sR.Close();
                        Seguridad vSeguridad = new Seguridad();
                        StringReader stringReader = new StringReader(vSeguridad.DecryptString(MainWindow.nombreApp, text));
                        text = stringReader.ReadToEnd();
                        ParserContext pc = new ParserContext();
                        pc.BaseUri = new Uri(Directory.GetCurrentDirectory() + "/", UriKind.Absolute);
                        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text));

                        object ob = XamlReader.Load(ms, pc);
                        var item = ob as UIElement;
                        if (item.GetValue(NameProperty).ToString().Equals("Fondo"))
                        {
                            Fondo.Source = ((Image)item).Source;
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("Base"))
                        {
                            Base.Background = ((Grid)item).Background;
                        }
                        else if (item.GetValue(NameProperty).ToString().Equals("Borde"))
                        {
                            Principal.Children.Remove(Borde);
                            NameScope.GetNameScope(this).UnregisterName(Borde.Name);

                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        else
                        {
                            try
                            {

                                NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                                Principal.Children.Add(item);

                                switch (item.GetType().Name.ToString())
                                {
                                    case "Label":
                                        item.MouseLeave += objeto_MouseLeave;
                                        item.MouseEnter += objeto_MouseEnter;
                                        break;
                                    case "MediaElement":
                                        MediaElement media = (MediaElement)FindName(item.GetValue(NameProperty).ToString());
                                        media.MediaEnded += MediaElement_MediaEnded;
                                        item.MouseLeave += objetoMedia_MouseLeave;
                                        item.MouseEnter += objetoMedia_MouseEnter;
                                        break;
                                    case "Image":
                                        item.MouseLeave += objetoMedia_MouseLeave;
                                        item.MouseEnter += objetoMedia_MouseEnter;
                                        break;
                                }
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
            if(esDiseno)
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

            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar turno";
            }
            else
            {
                Label obj = new Label();
                obj.Name = "NumeroTurno";
                obj.ToolTip = "NumeroTurno";
                obj.Content = "NumeroTurno";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar turno";
            }
        }

        private void MenuMostrarOcultarEquipo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroEquipo");
            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar equipo";
            }
            else
            {
                Label obj = new Label();
                obj.Name = "NumeroEquipo";
                obj.ToolTip = "NumeroEquipo";
                obj.Content = "NumeroEquipo";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar equipo";
            }
        }

        private void MenuMostrarOcultarNombreEquipo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NombreEquipo");
            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar nombre equipo";
            }
            else
            {
                Label obj = new Label();
                obj.Name = "NombreEquipo";
                obj.ToolTip = "NombreEquipo";
                obj.Content = "NombreEquipo";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar nombre equipo";
            }
        }

        private void MenuMostrarOcultarTurnoAnt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroTurnoAnt");
            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar turno anterior";
            }
            else
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                int TurnosAnt = int.Parse(config.AppSettings.Settings["TurnosAnteriores"].Value);

                Label obj = new Label();
                obj.Name = "NumeroTurnoAnt";
                obj.ToolTip = "NumeroTurnoAnt";

                string turnosAntLista = string.Empty;
                for (int x = 1; x <= TurnosAnt; x++)
                    turnosAntLista += "Turno " + x + "\n";

                obj.Content = turnosAntLista.Substring(0, turnosAntLista.Length - 1);
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar turno anterior";
            }


        }

        private void MenuMostrarOcultarEquipoAnt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroEquipoAnt");
            if (control != null)
            {
                control.Visibility = Visibility.Hidden;
                itemCm.Header = "Eliminar equipo anterior";
            }
            else
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                int TurnosAnt = int.Parse(config.AppSettings.Settings["TurnosAnteriores"].Value);

                Label obj = new Label();
                obj.Name = "NumeroEquipoAnt";
                obj.ToolTip = "NumeroEquipoAnt";

                string turnosAntLista = string.Empty;
                for (int x = 1; x <= TurnosAnt; x++)
                    if (x == 10)
                        turnosAntLista += "Equipo" + x + "\n";
                    else
                        turnosAntLista += "Equipo 0" + x + "\n";

                obj.Content = turnosAntLista.Substring(0, turnosAntLista.Length - 1);
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar equipo anterior";
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
                if (item.RenderTransform != null)
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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
                Task.Run(() => ProcesarTecla(e));
        }
        
        internal void ProcesarTecla(KeyEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                        switch (e.Key)
                        {
                            case Key.Left:
                                mainWindow.listaTurnosTeclas.Add(new Random().NextInt64(), new Dictionary<bool, string>() { { false, "00" } });
                                break;
                            case Key.Right:
                                mainWindow.listaTurnosTeclas.Add(new Random().NextInt64(), new Dictionary<bool, string>() { { true, "00" } });
                                break;
                            case Key.Down:
                                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
                                dialog.lblNombre.Content = "¡Advertencia!";
                                dialog.lblTexto.Text = "Se reinicia el turno al numero 1, ¿Está seguro que desea continuar?.";
                                dialog.btnCancelar.Visibility = Visibility.Visible;
                                new Recursos().ventanaMensajesGrande800x600(dialog);
                                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                if (dialog.ShowDialog() == true)
                                {
                                    SetearNumeroTurno(0);
                                    File.WriteAllText(@".\Recursos\turnoAnt.3k", string.Empty);

                                var numeroTurnoAnt = mainWindow.FindName("NumeroTurnoAnt") as UIElement;
                                var numeroEquipoAnt = mainWindow.FindName("NumeroEquipoAnt") as UIElement;
                                if(numeroTurnoAnt != null)
                                    numeroTurnoAnt.SetValue(ContentProperty, "");
                                if (numeroTurnoAnt != null)
                                    numeroEquipoAnt.SetValue(ContentProperty, "");

                            }
                                break;
                            case Key.Up:
                                CapturaTexto dialog2 = new CapturaTexto(mainWindow);
                                dialog2.lblNombre.Content = "¡Seteo de turno!";
                                new Recursos().ventanaCapturaTextoGrande800x600(dialog2);
                                dialog2.lblNombre.FontSize = 80;
                                dialog2.Texto.FontSize = 200;
                                dialog2.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                if (dialog2.ShowDialog() == true)
                                    try
                                    {
                                        SetearNumeroTurno(int.Parse(dialog2.Texto.Text));
                                    }
                                    catch (Exception)
                                    {
                                        Mensajes dialog3 = new Mensajes(Recursos.TipoMensaje.ERROR, true);
                                        dialog3.lblNombre.Content = "¡Error!";
                                        dialog3.lblTexto.Text = "No se pudo setear el turno, el valor introducido no es correcto. ¡Intente nuevamente!";
                                        new Recursos().ventanaMensajesGrande800x600(dialog3);
                                        dialog3.ShowDialog();
                                    }
                                break;

                            default:
                                if (new Recursos().NumericKeys.ContainsKey(e.Key))
                                    mainWindow.listaTurnosTeclas.Add(new Random().NextInt64(), new Dictionary<bool, string>() { { true, "0" + new Recursos().NumericKeys[e.Key] } });
                                break;
                        }
                }));
            }
            catch (Exception) { }
        }
     
        public void SetearNumeroTurno(int numeroTurno)
        {
            new Recursos().GuardarNumeroTurno(numeroTurno);
        }
    }
}
