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
                    propiedadesImagen.Largo.Text = Math.Round(((Image)item).ActualHeight).ToString();
                    propiedadesImagen.Ancho.Text = Math.Round(((Image)item).ActualWidth).ToString();
                    propiedadesImagen.Opacidad.Value = item.Opacity;
                    propiedadesImagen.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                    propiedadesImagen.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
                    propiedadesImagen.chkSonido.IsEnabled = false;
                    propiedadesImagen.Cada.IsEnabled = false;
                    propiedadesImagen.Durar.IsEnabled = false;
                    propiedadesImagen.chkMaximizar.IsEnabled = false;
                    propiedadesImagen.chkRelacion.IsChecked = true;
                    propiedadesImagen.esInicio = false;
                    propiedadesImagen.ShowDialog();
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

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    if (config.AppSettings.Settings[pNombre] != null)
                        config.AppSettings.Settings.Remove(pNombre);

                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    try
                    {
                        DirectoryInfo info = new DirectoryInfo(@"objetosTurno\");
                        foreach (var file in info.GetFiles())
                        {
                            string[] nombre = file.Name.Split('-');
                            if (nombre[1].Equals(pNombre + ".xaml"))
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
                Image obj = new Image();
                obj.Name = dialog.NombreText.ToUpper();
                obj.ToolTip = dialog.NombreText.ToUpper();
                obj.Source = new BitmapImage(new Uri(dialog.ContenidoText));
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.Stretch = Stretch.Uniform;
                obj.Height = 800;
                obj.MaxHeight = MaxHeight;
                obj.MaxWidth = MaxHeight;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
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
    }
}
