using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using static Precios_Turnos.MainWindow;

namespace Precios_Turnos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string nombreApp = "Precios_Turnos";
        private Point _positionInBlock;
        private TranslateTransform? _currentTT;
        private bool esAplicacion = false;
        private bool editar = false;
        private bool animaciones = false;
        private bool maximizado = false;
        private bool activarTurnero = false;
        private bool estaSaliendo = false;
        public Color ultimoColorLetra;
        public Color ultimoColorFondo;
        private string controlClickName;
        public MainWindow()
        {
            InitializeComponent();
            Coordenadas.Visibility = Visibility.Hidden;
            CargarVistaPrevia();

            MaxHeight = SystemParameters.VirtualScreenHeight;
            MaxWidth = SystemParameters.VirtualScreenWidth;

            try
            {
                if (ValidarActivar())
                {
                    CargarControles();
                    WindowState = WindowState.Maximized;
                }
            }
            catch (Exception) { }
        }

        private bool ValidarActivar()
        {
            Seguridad vSeguridad = new Seguridad();

            Licencia licencia = new Licencia();
            try
            {
                using (Stream stream = new FileStream(@".\Llave.key", FileMode.Open))
                {
                    var sr = new StreamReader(stream);

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        try
                        {
                            licencia = JsonSerializer.Deserialize<Licencia>(line)!;
                        }
                        catch { }

                    }
                    stream.Close();
                }
            }
            catch { }


            if (licencia.Correo != null && licencia.Codigo != null && licencia.Llave != null && licencia.Key != null)
            {

                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialog.lblNombre.Content = "¡Error!";

                string cadena = vSeguridad.DecryptString(licencia.Codigo, licencia.Key);
                string[] subs = cadena.Split('|');
                if (subs.Length > 0)
                {
                    if (subs[0].Equals(vSeguridad.DecryptString(licencia.Codigo, licencia.Correo))
                        && subs[3].Equals(vSeguridad.numeroSerieHD()) && subs[4].Equals(vSeguridad.numeroSeriePlacaBase())
                        && subs[5].Equals(nombreApp))
                    {
                        try
                        {
                            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Lista de precios 3K", true);
                            string Llave = key.GetValue("Key").ToString();
                            Licencia licencia2 = new Licencia();
                            licencia2 = JsonSerializer.Deserialize<Licencia>(Llave)!;

                            if (licencia.Correo.Equals(licencia2.Correo) || licencia.Codigo.Equals(licencia2.Codigo)
                                || licencia.Llave.Equals(licencia2.Llave) || licencia.Key.Equals(licencia2.Key))
                            {
                                if (!subs[1].Equals("0"))
                                {
                                    if (vSeguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[6]).Date)
                                    {
                                        Conexion.IsEnabled = true;
                                        Turnero.IsEnabled = true;
                                        EditarDiseno.IsEnabled = true;
                                        return true;
                                    }
                                    dialog.lblTexto.Text = "La licencia ha caducado.";
                                    dialog.ShowDialog();
                                }
                                else
                                {
                                    Conexion.IsEnabled = true;
                                    Turnero.IsEnabled = true;
                                    EditarDiseno.IsEnabled = true;
                                    return true;
                                }
                            }
                        }
                        catch
                        {
                            dialog.lblTexto.Text = "La licencia no es válida.";
                            dialog.ShowDialog();
                        }
                    }
                }
                else
                {
                    dialog.lblTexto.Text = "La licencia no es válida.";
                    dialog.ShowDialog();
                }
            }

            Conexion.IsEnabled = false;
            Turnero.IsEnabled = false;
            EditarDiseno.IsEnabled = false;
            ResizeMode = ResizeMode.NoResize;

            return false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized && !maximizado && !editar)
            {
                maximizado = true;
                esAplicacion = true;
                //Topmost = true;

                Principal.IsHitTestVisible = false;
                ModoEdicion.Visibility = Visibility.Hidden;

                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Visibility = Visibility.Collapsed;
                Menu.Visibility = Visibility.Hidden;

                //// re-show the window after changing style
                Visibility = Visibility.Visible;
                LimpiarVistaPrevia();
                PausarVideos(true);
                animaciones = true;
                CargarAnimaciones();
                Task.Run(() => ComportamientoObjetos());

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                activarTurnero = config.AppSettings.Settings["Turnero"].Value.Equals("true") ? true : false;
                Task.Run(() => EscuhcarTurnos());

            }

            else if (WindowState == WindowState.Maximized && editar)
            {
                maximizado = true;
                animaciones = false;
                ModoEdicion.Visibility = Visibility.Visible;
                Principal.IsHitTestVisible = true;

                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Visibility = Visibility.Collapsed;
                Menu.Visibility = Visibility.Hidden;

                //// re-show the window after changing style
                Visibility = Visibility.Visible;
                LimpiarVistaPrevia();
                PausarVideos(true);

            }
            else if (WindowState != WindowState.Maximized && !editar && maximizado)
            {
                maximizado = false;
                animaciones = false;
                AsynchronousSocketListener.StopListening();
                Topmost = false;
                Menu.Visibility = Visibility.Visible;
                ResizeMode = ResizeMode.CanResize;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                Principal.IsHitTestVisible = true;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (esAplicacion)
                {
                    QuitarAnimaciones();
                    esAplicacion = false;
                }

                CargarVistaPrevia();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && maximizado && !estaSaliendo)
            {
                estaSaliendo = true;
                SalirMaximizar();
                estaSaliendo = false;
            }
            else if ((e.Key == Key.Right || e.Key == Key.Left) && maximizado)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings["TeclasDemo"].Value.Equals("true"))
                {

                    Random rand = new Random();
                    int turno = rand.Next(1000);

                    MostrarTurno mostrarTurno = new MostrarTurno();
                    mostrarTurno.WindowStyle = WindowStyle.None;
                    mostrarTurno.ShowInTaskbar = false;
                    mostrarTurno.NumeroTurno.Content = turno;
                    mostrarTurno.NumeroEquipo.Content = "01";
                    mostrarTurno.NumeroTurnoAnt.Content = turno - 1;
                    mostrarTurno.NumeroEquipoAnt.Content = "02";

                    if (config.AppSettings.Settings["MostrarNombres"].Value.Equals("true"))
                    {
                        mostrarTurno.NombreEquipo.Content = "VENDEDOR 1";
                        mostrarTurno.NombreEquipoAnt.Content = "VENDEDOR 2";
                    }
                    else
                    {
                        mostrarTurno.NombreEquipo.Content = "";
                        mostrarTurno.NombreEquipoAnt.Content = "";
                    }
                    mostrarTurno.Show();

                }
            }
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditarDiseno_Click(object sender, RoutedEventArgs e)
        {
            EntrarDiseno dialog = new EntrarDiseno(this);
            if (dialog.ShowDialog() == true)
            {
                editar = true;
                WindowState = WindowState.Maximized;
                Coordenadas.Visibility = Visibility.Visible;
                ModoEdicion.Content = "Modo edición";
                ModoEdicion.FontSize = 24;
            }
        }

        private void Principal_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            if (editar)
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
            if (editar)
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
            if (editar)
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

        internal bool SeModificaControl(string name)
        {
            bool seModifica;
            switch (name)
            {
                case "Coordenadas":
                case "ModoEdicion":
                case "Principal":
                case "Menu":
                case "LogoPrincipal":
                case "Fondo":
                case "VistaPrevia":
                case "Video":
                case "Tabla":
                    seModifica = false;
                    break;
                default:
                    seModifica = true;
                    break;
            }
            return seModifica;
        }

        private void Principal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (editar)
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
            if (editar)
                try
                {
                    var item = e.Source as UIElement;

                    if (!SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        ContextMenu cm = this.FindResource("cmdPrincipalContexMenu") as ContextMenu;
                        MenuItem itemCm = (MenuItem)cm.Items[5];
                        itemCm.Items.Clear();
                        foreach (var itemObjets in Principal.Children)
                        {
                            string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                            if (SeModificaControl(nombreControl))
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

        private void MenuSalir_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SalirMaximizar();
        }

        private void SalirMaximizar()
        {
            PausarVideos(false);
            MediaElement control = (MediaElement)FindName("FullScreamVideo");
            if (control != null)
            {
                Video.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                Video.Background = null;
                //Video.UpdateLayout();
            }


            Coordenadas.Visibility = Visibility.Hidden;
            ModoEdicion.Visibility = Visibility.Hidden;
            LogoPrincipal.Visibility = Visibility.Hidden;
            if (editar)
            {
                SaveFrameworkElementToPng(Principal, 900, 557, @".\Principal.png");
                GuardarControles();
                editar = false;
            }

            WindowState = WindowState.Normal;
            LogoPrincipal.Visibility = Visibility.Visible;
            ModoEdicion.Content = "Vista previa";
            ModoEdicion.FontSize = 18;
            ModoEdicion.Visibility = Visibility.Visible;

        }

        private void CargarVistaPrevia()
        {
            try
            {
                ImageBrush myBrush = new ImageBrush();
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@"./Principal.png", UriKind.Relative);
                image.EndInit();
                myBrush.ImageSource = image;

                VistaPrevia.Background = myBrush;
            }
            catch (Exception) { VistaPrevia.Background = null; }


        }

        private void LimpiarVistaPrevia()
        {
            VistaPrevia.Background = null;
        }

        private void MenuAgregarTexto_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CapturaTextoDialogo dialog = new CapturaTextoDialogo(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            var mousePosition = e.GetPosition(Principal);


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
            var mousePosition = e.GetPosition(Principal);


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

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(dialog.ContenidoText);
                bitmapImage.EndInit();

                obj.Source = bitmapImage;
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.Stretch = Stretch.Uniform;
                obj.Height = bitmapImage.Height;
                obj.MaxHeight = MaxHeight;
                obj.MaxWidth = MaxHeight;
                obj.Tag = "";
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
            if (editar)
                try
                {
                    mostarPropiedadesObjetos(e);
                }
                catch (Exception) { }
        }

        private void mostarPropiedadesObjetos(MouseButtonEventArgs e)
        {
            var item = FindName(controlClickName) as UIElement;

            var point = e.GetPosition(Principal);

            switch (item.GetType().Name)
            {
                case "Label":
                    PropiedadesLabel propiedadesLabel = new PropiedadesLabel(this);
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
                    PropiedadesMultimedia propiedadesImagen = new PropiedadesMultimedia(this);
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
                    propiedadesImagen.chkOcultar.IsEnabled = false;
                    propiedadesImagen.esInicio = false;
                    propiedadesImagen.ShowDialog();
                    break;
                case "MediaElement":
                    PropiedadesMultimedia propiedadesVideo = new PropiedadesMultimedia(this);
                    propiedadesVideo.WindowStartupLocation = WindowStartupLocation.Manual;

                    if (point.X + propiedadesVideo.Width >= MaxWidth)
                        propiedadesVideo.Left = point.X - propiedadesVideo.Width;
                    else
                        propiedadesVideo.Left = point.X;

                    if (point.Y + propiedadesVideo.Height >= MaxHeight)
                        propiedadesVideo.Top = point.Y - propiedadesVideo.Height;
                    else
                        propiedadesVideo.Top = point.Y;

                    propiedadesVideo.Titulo.Content = "Propiedades \"" + item.GetValue(NameProperty).ToString() + "\"";
                    propiedadesVideo.NombreControl.Text = item.GetValue(NameProperty).ToString();
                    propiedadesVideo.TipoControl.Text = item.GetType().Name;
                    propiedadesVideo.Ruta.Text = ((MediaElement)item).Source.ToString();

                    string extension = System.IO.Path.GetExtension(((MediaElement)item).Source.ToString()).Replace(".", "").ToLower();
                    bool esAudio = false;
                    if (extension.Equals("wav") || extension.Equals("mp3"))
                        esAudio = true;
                    if (esAudio)
                    {
                        propiedadesVideo.chkRelacion.IsEnabled = false;
                        propiedadesVideo.Largo.IsEnabled = false;
                        propiedadesVideo.Ancho.IsEnabled = false;
                        propiedadesVideo.Opacidad.IsEnabled = false;
                        propiedadesVideo.chkSonido.IsEnabled = false;
                        propiedadesVideo.chkMaximizar.IsEnabled = false;
                        propiedadesVideo.chkOcultar.IsEnabled = false;
                    }
                        

                    propiedadesVideo.chkRelacion.IsChecked = true;
                    propiedadesVideo.Largo.Text = Convert.ToInt32(((MediaElement)item).ActualHeight).ToString();
                    propiedadesVideo.Ancho.Text = Convert.ToInt32(((MediaElement)item).ActualWidth).ToString();
                    propiedadesVideo.Opacidad.Value = item.Opacity;
                    propiedadesVideo.chkSonido.IsChecked = ((MediaElement)item).Volume == 1 ? true : false;
                    propiedadesVideo.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                    propiedadesVideo.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
                    string[] datosTag = ((MediaElement)item).Tag.ToString().Split('|');
                    if (datosTag.Length > 2)
                    {
                        propiedadesVideo.chkMaximizar.IsChecked = true;
                        propiedadesVideo.Cada.Text = datosTag[2];
                        propiedadesVideo.Durar.Text = datosTag[3];
                    }
                    else
                    {
                        propiedadesVideo.chkMaximizar.IsChecked = false;
                        propiedadesVideo.Cada.IsEnabled = false;
                        propiedadesVideo.Durar.IsEnabled = false;
                    }
                    propiedadesVideo.chkOcultar.IsChecked = ((MediaElement)item).Visibility == Visibility.Hidden;

                    propiedadesVideo.esInicio = false;
                    propiedadesVideo.ShowDialog();
                    break;
                case "DataGrid":
                    PropiedadesTabla propiedadesTabla = new PropiedadesTabla(this);
                    propiedadesTabla.WindowStartupLocation = WindowStartupLocation.Manual;

                    if (point.X + propiedadesTabla.Width >= MaxWidth)
                        propiedadesTabla.Left = point.X - propiedadesTabla.Width;
                    else
                        propiedadesTabla.Left = point.X;

                    if (point.Y + propiedadesTabla.Height >= MaxHeight)
                        propiedadesTabla.Top = point.Y - propiedadesTabla.Height;
                    else
                        propiedadesTabla.Top = point.Y;

                    propiedadesTabla.NombreControl.Text = item.GetValue(NameProperty).ToString();
                    propiedadesTabla.TipoControl.Text = item.GetType().Name;

                    propiedadesTabla.cbxFuente.SelectedItem = item.GetValue(FontFamilyProperty);
                    propiedadesTabla.cbxTamano.SelectedValue = item.GetValue(FontSizeProperty);
                    propiedadesTabla.chkNegrita.IsChecked = item.GetValue(FontWeightProperty).ToString().CompareTo("Bold") == 0 ? true : false;
                    propiedadesTabla.chkCursiva.IsChecked = item.GetValue(FontStyleProperty).ToString().CompareTo("Italic") == 0 ? true : false;
                    propiedadesTabla.cbxCRegistros.SelectedValue = ((DataGrid)item).Items.Count * 2;
                    propiedadesTabla.chkLineas.IsChecked = ((DataGrid)item).GridLinesVisibility == DataGridGridLinesVisibility.All ? true : false;
                    string[] datos = ((DataGrid)item).Tag.ToString().Split('|');
                    propiedadesTabla.cbxCBloques.SelectedValue = datos[0];
                    propiedadesTabla.cbxCRegistros.SelectedValue = datos[1];
                    propiedadesTabla.cbxOrientacion.SelectedValue = datos[2];
                    propiedadesTabla.cbxCambiar.SelectedValue = datos[3];
                    if (datos.Length == 8)
                    {
                        propiedadesTabla.chkDoble.IsChecked = true;
                        propiedadesTabla.btnColorFuente.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(datos[4]);
                        propiedadesTabla.btnColorFondo.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(datos[5]);
                        propiedadesTabla.btnColorFuente2.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(datos[6]);
                        propiedadesTabla.btnColorFondo2.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(datos[7]);
                    }
                    else
                    {
                        propiedadesTabla.btnColorFuente2.Visibility = Visibility.Hidden;
                        propiedadesTabla.btnColorFondo2.Visibility = Visibility.Hidden;
                        propiedadesTabla.lblCFuenteUno.Visibility = Visibility.Hidden;
                        propiedadesTabla.lblCFuenteDos.Visibility = Visibility.Hidden;
                        propiedadesTabla.lblCFondoUno.Visibility = Visibility.Hidden;
                        propiedadesTabla.lblCFondoDos.Visibility = Visibility.Hidden;
                        Grid.SetColumnSpan(propiedadesTabla.btnColorFuente, 3);
                        Grid.SetColumnSpan(propiedadesTabla.btnColorFondo, 3);

                        propiedadesTabla.btnColorFuente.Fill = ((DataGrid)item).Foreground;
                        propiedadesTabla.btnColorFondo.Fill = ((DataGrid)item).Background;
                        propiedadesTabla.btnColorFuente2.Fill = ((DataGrid)item).Foreground;
                        propiedadesTabla.btnColorFondo2.Fill = ((DataGrid)item).Background;
                    }

                    propiedadesTabla.Opacidad.Value = item.Opacity;
                    propiedadesTabla.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                    propiedadesTabla.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
                    propiedadesTabla.ShowDialog();
                    break;

                default:

                    break;
            }
        }

        private void MenuEliminar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BorarObjeto(controlClickName);
        }

        public bool BorarObjeto(string pNombre, bool pMuestraMensaje = true)
        {
            bool seBorra = false;
            if (pMuestraMensaje)
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
                dialog.lblNombre.Content = "¡Advertencia!";
                dialog.lblTexto.Text = "Se eliminará de forma permanete el objeto.";
                if (dialog.ShowDialog() == true)
                    seBorra = true;

            }
            
            if (seBorra || !pMuestraMensaje)
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
                    DirectoryInfo info = new DirectoryInfo(@"objetos\");


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

        private void Propiedades_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // get the position within the container
            var mousePosition = e.GetPosition(this);
            PropiedadesFondo propiedadesFondo = new PropiedadesFondo(this);
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

        private void MenuAgregarVideo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CapturaTextoDialogo dialog = new CapturaTextoDialogo(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            var mousePosition = e.GetPosition(Principal);


            if (mousePosition.X + dialog.Width >= MaxWidth)
                dialog.Left = mousePosition.X - dialog.Width;
            else
                dialog.Left = mousePosition.X;

            if (mousePosition.Y + dialog.Height >= MaxHeight)
                dialog.Top = mousePosition.Y - dialog.Height;
            else
                dialog.Top = mousePosition.Y;

            dialog.lblTexto.Content = "Abrir";
            dialog.Titulo.Content = "Agregar video/audio";
            dialog.ContenidoTextBox.IsEnabled = false;
            dialog.esVideo = true;
            if (dialog.ShowDialog() == true)
            {

                MediaElement obj = new MediaElement();
                obj.Name = dialog.NombreText.ToUpper();
                obj.ToolTip = dialog.NombreText.ToUpper();
                obj.Source = new Uri(dialog.ContenidoText);
                obj.LoadedBehavior = MediaState.Play;
                obj.MediaEnded += MediaElement_MediaEnded;
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.Stretch = Stretch.Uniform;
                //obj.Height = obj.NaturalVideoHeight;
                obj.MaxHeight = MaxHeight;
                obj.MaxWidth = MaxHeight;
                obj.Volume = 1;
                obj.Tag = "";
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement item = (MediaElement)e.Source;
            item.Position = TimeSpan.FromMilliseconds(1);
        }

        private void MenuAgregarTabla_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CapturaTextoDialogo dialog = new CapturaTextoDialogo(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            var mousePosition = e.GetPosition(Principal);


            if (mousePosition.X + dialog.Width >= MaxWidth)
                dialog.Left = mousePosition.X - dialog.Width;
            else
                dialog.Left = mousePosition.X;

            if (mousePosition.Y + dialog.Height >= MaxHeight)
                dialog.Top = mousePosition.Y - dialog.Height;
            else
                dialog.Top = mousePosition.Y;

            dialog.lblTexto.Content = "Titulo";
            dialog.Titulo.Content = "Agregar tabla";
            dialog.btnAbrir.Visibility = Visibility.Hidden;
            if (dialog.ShowDialog() == true)
            {
                DataGrid obj = new DataGrid();
                obj.Name = dialog.NombreText.ToUpper();
                obj.ToolTip = dialog.NombreText.ToUpper();
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 28;
                obj.FontFamily = new FontFamily("Arial");
                obj.CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters = {
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Left)
                    }
                };
                obj.HeadersVisibility = DataGridHeadersVisibility.None;
                obj.CanUserAddRows = false;
                obj.Tag = "2|15|H|15";
                obj.ItemsSource = CargarListaTablas(dialog.NombreText, obj.Tag.ToString())[0].DefaultView;

                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);

            }
        }

        public List<DataTable> LlenarListaTablas(int bloques, int cantFilas, DataTable dt, string orientacion, string pNombreControl)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            int index=0;
            bool tieneCampoOrganizar = false;
            if (config.AppSettings.Settings[pNombreControl] != null)
            {
                Seguridad vSeguridad = new Seguridad();
                string[] datos = vSeguridad.DecryptString(nombreApp, config.AppSettings.Settings[pNombreControl].Value).Split('|');
                if (datos.Length > 1)
                {
                    index = dt.Columns.IndexOf(datos[1]);
                    tieneCampoOrganizar = true;
                }
            }
            List<DataTable> ListTablas = new List<DataTable>();
            int x = 1;
            int rowCont = 0;
            int rowContTotal = 0;

            DataTable dtFinal = new DataTable();
            for (int z = 0; z < (dt.Columns.Count * bloques) + (bloques - 1); z++)
                dtFinal.Columns.Add();

            object[] arrayTemp = new object[0];
            object[] arrayResult = new object[0];
            foreach (DataRow row in dt.Rows)
            {
                bool seAgrego = false;
                rowCont++;
                rowContTotal++;
                if (x <= cantFilas)
                {
                    switch (bloques)
                    {
                        case 1:
                            seAgrego = true;
                            dtFinal.Rows.Add(row.ItemArray);
                            x++;
                            break;
                        case 2:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 2 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        case 3:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 3 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        case 4:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 4 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        case 5:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 5 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        case 6:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 6 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        case 7:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 7 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        case 8:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 8 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        case 9:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 9 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        case 10:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 10 || rowContTotal == dt.Rows.Count)
                            {
                                seAgrego = true;
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        default:
                            break;
                    }

                    bool cambioCampoOrganizar = false;
                    if (tieneCampoOrganizar)
                    {
                        try
                        {
                            if (!row[index].ToString().Equals(dt.Rows[rowContTotal][index].ToString()))
                            {
                                cambioCampoOrganizar = true;
                                if (!seAgrego)
                                    {
                                    dtFinal.Rows.Add(arrayResult);
                                    arrayTemp = new object[0];
                                    x++;
                                    rowCont = 0;
                                }
                                    
                            }
                                
                        }
                        catch { }
                    }

                    if (x == cantFilas + 1 || rowContTotal == dt.Rows.Count || cambioCampoOrganizar)
                    {
                        while(x < cantFilas + 1)
                        {
                            dtFinal.Rows.Add();
                            x++;
                        }
                        rowCont = 0;

                        if (tieneCampoOrganizar)
                        {
                            dtFinal.TableName = row[index].ToString();
                            switch (bloques)
                            {
                                case 1:
                                    dtFinal.Columns.RemoveAt(index);
                                    break;
                                case 2:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    break;
                                case 3:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 2) + (index));
                                    break;
                                case 4:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 2) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 3) + (index));
                                    break;
                                case 5:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 2) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 3) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 4) + (index));
                                    break;
                                case 6:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 2) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 3) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 4) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 5) + (index));
                                    break;
                                case 7:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 2) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 3) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 4) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 5) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 6) + (index));
                                    break;
                                case 8:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 2) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 3) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 4) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 5) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 6) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 7) + (index));
                                    break;
                                case 9:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 2) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 3) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 4) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 5) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 6) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 7) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 8) + (index));
                                    break;
                                case 10:
                                    dtFinal.Columns.RemoveAt(index);
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 2) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 3) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 4) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 5) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 6) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 7) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 8) + (index));
                                    dtFinal.Columns.RemoveAt((dt.Columns.Count * 9) + (index));
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (orientacion.Equals("V"))
                        {
                            
                            ListTablas.Add(PivotTable(dtFinal));
                        }
                        else
                        {
                            
                            ListTablas.Add(dtFinal);
                        }
                        dtFinal = new DataTable();
                        for (int z = 0; z < (dt.Columns.Count * bloques) + (bloques - 1); z++)
                        {
                            dtFinal.Columns.Add();
                        }
                        x = 1;
                    }

                }
            }
            if (ListTablas.Count == 0)
            {
                dtFinal.Columns.Add();
                dtFinal.Rows.Add(new object[1] { "Sin datos" }.ToArray());
                ListTablas.Add(dtFinal);
            }
            return ListTablas;
        }

        public byte[] getJPGFromImageControl(BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }

        private DataTable PivotTable(DataTable origTable)
        {

            DataTable newTable = new DataTable();
            DataRow dr = null;

            //Add Columns to new Table
            for (int i = 0; i < origTable.Rows.Count; i++)
            {
                newTable.Columns.Add();
            }

            //Execute the Pivot Method
            for (int cols = 0; cols < origTable.Columns.Count; cols++)
            {
                dr = newTable.NewRow();
                for (int rows = 0; rows < origTable.Rows.Count; rows++)
                {
                    dr[rows] = origTable.Rows[rows][cols];
                }
                newTable.Rows.Add(dr); //add the DataRow to the new Table rows collection
            }
            return newTable;
        }

        private void MenuListaObjetos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem item = (MenuItem)e.Source;
            controlClickName = item.Tag.ToString();
            mostarPropiedadesObjetos(e);
        }

        private void ConfigurarConexion_Click(object sender, RoutedEventArgs e)
        {
            ConfigurarConexion dialog = new ConfigurarConexion();
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            dialog.ShowDialog();
        }

        private void Activar_Click(object sender, RoutedEventArgs e)
        {
            Activar dialog = new Activar(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;

            var relativeCenterParent = new Point(ActualWidth / 2, ActualHeight / 2);
            var centerParent = this.PointToScreen(relativeCenterParent);
            //This calculates the relative center of the child form.
            var hCenterChild = dialog.Width / 2;
            var vCenterChild = dialog.Height / 2;
            dialog.Left = centerParent.X - hCenterChild;
            dialog.Top = centerParent.Y - vCenterChild;

            dialog.ShowDialog();
        }

        private void GuardarControles()
        {
            try
            {
                if (!Directory.Exists(@".\objetos"))
                {
                    Directory.CreateDirectory(@".\objetos");
                }
                DirectoryInfo di = new DirectoryInfo(@".\objetos");
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                int x = 0;
                foreach (var itemObjets in Principal.Children)
                {
                    bool esTabla;
                    string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                    if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo"))
                    {
                        switch (itemObjets.GetType().Name.ToString())
                        {
                            case "DataGrid":
                                esTabla = true;
                                ((DataGrid)itemObjets).ItemsSource = null;
                                break;
                            default:
                                esTabla = false;
                                break;
                        }

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
                        File.WriteAllText(@"objetos\" + ++x + "-" + nombreControl + ".xaml", vSeguridad.EncryptString(nombreApp, savedControls));
                        if (esTabla)
                        {
                            ((DataGrid)itemObjets).ItemsSource = CargarListaTablas(nombreControl, ((DataGrid)itemObjets).Tag.ToString())[0].DefaultView;
                            //((DataGrid)itemObjets).UpdateLayout();
                            ColorFuenteFondoTabla(nombreControl, ((DataGrid)itemObjets).Tag.ToString());
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        internal void CargarControles()
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(@"objetos\");
                foreach (var file in info.GetFiles())
                {

                    StreamReader sR = new StreamReader(@file.FullName);
                    string text = sR.ReadToEnd();
                    sR.Close();
                    Seguridad vSeguridad = new Seguridad();
                    StringReader stringReader = new StringReader(vSeguridad.DecryptString(nombreApp, text));
                    XmlReader xmlReader = XmlReader.Create(stringReader);

                    object ob = System.Windows.Markup.XamlReader.Load(xmlReader);
                    var item = ob as UIElement;
                    if (item.GetValue(NameProperty).ToString().Equals("Fondo"))
                    {
                        Fondo.Background = ((Grid)item).Background;
                    }
                    else
                    {
                        try
                        {
                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);
                        }
                        catch (Exception) { }

                        switch (item.GetType().Name.ToString())
                        {
                            case "DataGrid":
                                string[] datos = ((DataGrid)item).Tag.ToString().Split('|');
                                DataGrid control = (DataGrid)FindName(item.GetValue(NameProperty).ToString());
                                if (datos[2].Equals("V"))
                                {
                                    control.CellStyle = new Style(typeof(DataGridCell))
                                    {
                                        Setters = {
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center)
                    }
                                    };
                                }
                                else
                                {
                                    control.CellStyle = new Style(typeof(DataGridCell))
                                    {
                                        Setters = {
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Left)
                    }
                                    };
                                }
                                List<DataTable> list = CargarListaTablas(control.Name, control.Tag.ToString());
                                if (!list[0].Rows[0][0].ToString().Equals("Sin datos"))
                                    control.ItemsSource = list[0].DefaultView;
                                //control.UpdateLayout();
                                ColorFuenteFondoTabla(control.Name, control.Tag.ToString());
                                break;
                            case "MediaElement":
                                MediaElement video = (MediaElement)FindName(item.GetValue(NameProperty).ToString());
                                video.MediaEnded += MediaElement_MediaEnded;
                                break;
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        public List<DataTable> CargarListaTablas(string pNombre, string pTag, bool pMostrarMensaje = false)
        {
            string[] datos = pTag.Split('|');
            List<DataTable> ListaTablas;
            try
            {
                Seguridad vSeguridad = new Seguridad();
                //Create the object
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string odbc = config.AppSettings.Settings["ODBC"].Value;
                string usuario = config.AppSettings.Settings["UsuarioODBC"].Value;
                string contrasena = vSeguridad.DecryptString(nombreApp, config.AppSettings.Settings["ContrasenaODBC"].Value);
                string consulta = string.Empty;

                if (config.AppSettings.Settings[pNombre] != null)
                {
                    string[] datosC = vSeguridad.DecryptString(nombreApp, config.AppSettings.Settings[pNombre].Value).Split('|');
                    consulta = datosC[0];

                    OdbcConnection connection = new OdbcConnection("DSN=" + odbc + ";uid=" + usuario + ";pwd=" + contrasena);

                    connection.Open();
                    OdbcCommand MyCommand = new OdbcCommand(consulta, connection);
                    OdbcDataReader MyDataReader = MyCommand.ExecuteReader();
                    if (MyDataReader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(MyDataReader);
                        ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), dt, datos[2], pNombre);

                    }
                    else
                    {
                        ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2], pNombre);
                    }
                    connection.Close();
                }
                else
                {
                    ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2], pNombre);
                }
            }
            catch (Exception ex)
            {
                if (pMostrarMensaje)
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Error al cargar la consulta: \n" + ex.Message; ;
                    dialog.ShowDialog();
                }
                ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2], pNombre);
            }
            return ListaTablas;
        }

        public void ColorFuenteFondoTabla(string pNombre, string pTag)
        {
            string[] datos = pTag.Split('|');
            DataGrid control = (DataGrid)FindName(pNombre);
            if (datos.Length == 8)
            {
                foreach (DataRowView item in control.ItemsSource)
                {
                    DataGridRow row = (DataGridRow)control.ItemContainerGenerator.ContainerFromItem(item);

                    if (row != null)
                        if (row.GetIndex() % 2 == 0)
                        {
                            row.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(datos[4]);
                            row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(datos[5]);
                        }
                        else
                        {
                            row.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(datos[6]);
                            row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(datos[7]);
                        }
                }
            }
            else
            {
                control.Foreground = control.Foreground;
                control.Background = control.Background;
                control.RowStyle = new Style(typeof(DataGridRow))
                {
                    Setters = {
                        new Setter(BackgroundProperty, control.Background)
                    }
                };
            }
            //control.UpdateLayout();
        }

        private void SaveFrameworkElementToPng(FrameworkElement frameworkElement,
                                       int width,
                                       int height,
                                       string filePath)
        {
            BitmapImage bitmapImage = VisualToBitmapImage(frameworkElement);
            SaveImage(bitmapImage, width, height, filePath);
        }

        public BitmapImage VisualToBitmapImage(FrameworkElement frameworkElement)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)frameworkElement.ActualWidth,
                                                            (int)frameworkElement.ActualHeight,
                                                            96d,
                                                            96d,
                                                            PixelFormats.Default);
            rtb.Render(frameworkElement);

            MemoryStream stream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public void SaveImage(BitmapImage sourceImage,
                        int width,
                        int height,
                        string filePath)
        {
            try
            {
                TransformGroup transformGroup = new TransformGroup();
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = (double)width / sourceImage.PixelWidth;
                scaleTransform.ScaleY = (double)height / sourceImage.PixelHeight;
                transformGroup.Children.Add(scaleTransform);

                DrawingVisual vis = new DrawingVisual();
                DrawingContext cont = vis.RenderOpen();
                cont.PushTransform(transformGroup);
                cont.DrawImage(sourceImage, new Rect(new Size(sourceImage.PixelWidth, sourceImage.PixelHeight)));
                cont.Close();

                RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
                rtb.Render(vis);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(stream);
                    stream.Close();
                }
            }
            catch { }
        }

        private async Task iniciarComportamientoTabla(string pNombre, string pTag)
        {
            string[] datos = pTag.Split('|');
            int x = 0;
            int tamano;
            while (animaciones && maximizado && !editar)
            {
                try
                {
                    List<DataTable> LisTablas = CargarListaTablas(pNombre, pTag);
                    tamano = LisTablas.Count;
                    if (x < tamano)
                        CambiarContenidoTabla(pNombre, pTag, LisTablas, x++);
                    else
                    {
                        x = 0;
                        CambiarContenidoTabla(pNombre, pTag, LisTablas, x++);
                    }
                    Task.Delay(int.Parse(datos[3]) * 1000).Wait();
                }
                catch (Exception) { }
            }
        }

        private async Task iniciarVideoFullScream(string pNombre, string pTag)
        {
            string[] datos = pTag.Split('|');
            if (datos.Length > 2)
                while (animaciones && maximizado && !editar)
                {
                    try
                    {
                        Task.Delay(int.Parse(datos[2]) * 1000).Wait();
                        if (animaciones && maximizado && !editar)
                        {
                            int duracion = await Task.Run(() => MostrarVideoFullScream(pNombre, pTag));
                            Task.Delay(duracion).Wait();
                            await Task.Run(() => EliminarVideoFullScream(pNombre, pTag));
                        }

                    }
                    catch (Exception) { }
                }
        }

        private async Task ComportamientoObjetos()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var itemObjets in Principal.Children)
                {
                    switch (itemObjets.GetType().Name.ToString())
                    {
                        case "DataGrid":
                            string nombreGrid = ((DataGrid)itemObjets).Name;
                            string tagGrid = ((DataGrid)itemObjets).Tag.ToString();
                            Task.Run(() => iniciarComportamientoTabla(nombreGrid, tagGrid));
                            break;
                        case "MediaElement":
                            string nombreVideo = ((MediaElement)itemObjets).Name;
                            string tagVideo = ((MediaElement)itemObjets).Tag.ToString();
                            if (tagVideo.Length > 0)
                                Task.Run(() => iniciarVideoFullScream(nombreVideo, tagVideo));
                            break;
                        default:
                            break;
                    }
                }
            }));
        }

        private async Task EscuhcarTurnos()
        {
            if (activarTurnero)
            {
                try
                {
                    AsynchronousSocketListener.StartListening();
                }
                catch (Exception) { }
            }
        }

        public void CambiarContenidoTabla(string pNombre, string pTag, List<DataTable> pLisTablas, int x)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    DataGrid control = (DataGrid)FindName(pNombre);
                    if (!pLisTablas[x].Rows[0][0].ToString().Equals("Sin datos"))
                        control.ItemsSource = pLisTablas[x].DefaultView;
                    else
                        control.ItemsSource = null;

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    if (config.AppSettings.Settings[pNombre] != null)
                    {
                        Seguridad vSeguridad = new Seguridad();
                        string[] datos = vSeguridad.DecryptString(nombreApp, config.AppSettings.Settings[pNombre].Value).Split('|');
                        
                            if (datos.Length > 2)
                            {
                            Label item = (Label)FindName(datos[2]);
                            if(item!=null)
                                item.Content = pLisTablas[x].TableName;
                            }
                        }


                        foreach (DataGridColumn column in control.Columns)
                        column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);

                    //control.UpdateLayout();
                    ColorFuenteFondoTabla(pNombre, pTag);
                }
                catch (Exception) { }

            }));
        }

        public async Task<int> MostrarVideoFullScream(string pNombre, string pTag)
        {
            int duracion = 0;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    SilenciarVideos(true);
                    MediaElement control = (MediaElement)FindName(pNombre);
                    string[] datos = pTag.Split('|');
                    MediaElement obj = new MediaElement();
                    obj.Name = "FullScreamVideo";
                    obj.Source = new Uri(control.Source.ToString());
                    obj.Position = control.Position;
                    obj.LoadedBehavior = MediaState.Play;
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.Stretch = Stretch.Uniform;
                    obj.Height = this.ActualHeight;
                    obj.Volume = 1;

                    duracion = Convert.ToInt32(Math.Round(control.NaturalDuration.TimeSpan.TotalMilliseconds));
                    if (int.Parse(datos[3]) != 0)
                        duracion = int.Parse(datos[3]) * 1000;

                    MediaElement videoFull = (MediaElement)FindName("FullScreamVideo");
                    if (videoFull != null)
                    {
                        Video.Children.Remove(videoFull);
                        NameScope.GetNameScope(this).UnregisterName(videoFull.Name);
                        //Video.UpdateLayout();
                    }
                    Video.Background = new SolidColorBrush(Colors.Black);
                    NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                    Video.Children.Add(obj);
                    //Video.UpdateLayout();
                }
                catch (Exception) { }
            }));
            return duracion;
        }

        public async Task EliminarVideoFullScream(string pNombre, string pTag)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    MediaElement control = (MediaElement)FindName(pNombre);
                    MediaElement video = (MediaElement)FindName("FullScreamVideo");
                    if (video != null)
                    {
                        Video.Children.Remove(video);
                        NameScope.GetNameScope(this).UnregisterName(video.Name);
                        Video.Background = null;
                        //Video.UpdateLayout();
                        SilenciarVideos(false);
                    }
                }
                catch (Exception) { }
            }));
        }

        private void PausarVideos(bool pPlay)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    foreach (var itemObjets in Principal.Children)
                    {
                        switch (itemObjets.GetType().Name.ToString())
                        {
                            case "MediaElement":
                                if (!pPlay)
                                {
                                    ((MediaElement)itemObjets).LoadedBehavior = MediaState.Manual;
                                    ((MediaElement)itemObjets).Pause();
                                }
                                else
                                {
                                    ((MediaElement)itemObjets).Play();
                                    ((MediaElement)itemObjets).LoadedBehavior = MediaState.Play;
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception) { }
            }));
        }

        private void SilenciarVideos(bool pSilencio)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    foreach (var itemObjets in Principal.Children)
                    {
                        switch (itemObjets.GetType().Name.ToString())
                        {
                            case "MediaElement":
                                if (pSilencio)
                                {
                                    ((MediaElement)itemObjets).Volume = 0;
                                }
                                else
                                {
                                    string[] datos = ((MediaElement)itemObjets).Tag.ToString().Split('|');
                                    if (datos[0].Equals("S"))
                                        ((MediaElement)itemObjets).Volume = 1;
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception) { }
            }));
        }

        private void Turnero_Click(object sender, RoutedEventArgs e)
        {
            Turnero turnero = new Turnero(this);
            turnero.ShowDialog();
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
                    foreach (var itemObjets in Principal.Children)
                    {
                        string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                        if (SeModificaControl(nombreControl))
                        {
                            BorarObjeto(nombreControl, false);
                        }
                        else if (nombreControl.Equals("Fondo"))
                        {
                            Fondo.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }
                catch { }
            }
        }

        private void QuitarAnimaciones()
        {
            foreach (UIElement item in Principal.Children)
            {

                if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                {
                    bool seModifico = false;
                    try
                    {
                        item.RenderTransform = (TranslateTransform)((TransformGroup)item.RenderTransform).Children[((TransformGroup)item.RenderTransform).Children.Count - 1];
                        seModifico = true;
                    }
                    catch
                    {
                        if (item.RenderTransform != null)
                            seModifico = true;
                    }
                    if (!seModifico)
                        item.RenderTransform = null;
                }
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

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            TransformGroup myTransformGroup = new TransformGroup();

            if (config.AppSettings.Settings[pNombreControl] != null)
            {
                string[] animaciones = new Seguridad().DecryptString(MainWindow.nombreApp, config.AppSettings.Settings[pNombreControl].Value).Split('-');
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
            }

            if (_currentTTEscalar != null)
                myTransformGroup.Children.Add(new TranslateTransform(_currentTTEscalar.X, _currentTTEscalar.Y));
            item.RenderTransformOrigin = new Point(.5, .5);
            item.RenderTransform = myTransformGroup;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}