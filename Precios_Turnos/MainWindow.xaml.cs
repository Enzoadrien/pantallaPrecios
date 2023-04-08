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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
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
            if (ValidarActivar())
                WindowState = WindowState.Maximized;
        }

        private bool ValidarActivar()
        {
            Seguridad vSeguridad = new Seguridad();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string cadena = vSeguridad.DecryptString(config.AppSettings.Settings["CodigoActivacion"].Value, vSeguridad.DecryptString(config.AppSettings.Settings["CodigoActivacion"].Value, config.AppSettings.Settings["Llave"].Value));
            string[] subs = cadena.Split('|');
            if (subs.Length >= 3)
            {
                if (subs[0].Equals(vSeguridad.DecryptString(config.AppSettings.Settings["CodigoActivacion"].Value, config.AppSettings.Settings["Correo"].Value))
                    && subs[1].Equals(vSeguridad.numeroSerieHD()) && subs[2].Equals(vSeguridad.numeroSeriePlacaBase())
                    && subs[3].Equals(nombreApp))
                {
                    if (!subs[4].Equals("0"))
                    {
                        if (vSeguridad.GetNetworkTime().Date < Convert.ToDateTime(subs[4]).Date)
                        {
                            Conexion.IsEnabled = true;
                            Turnero.IsEnabled = true;
                            EditarDiseno.IsEnabled = true;
                            return true;
                        }
                        else
                        {
                            Conexion.IsEnabled = false;
                            Turnero.IsEnabled = false;
                            EditarDiseno.IsEnabled = false;

                            Mensajes dialog = new Mensajes();
                            dialog.lblNombre.Content = "¡Error!";
                            dialog.lblTexto.Text = "Su licencia ha caducado. \n Fecha: " + Convert.ToDateTime(subs[4]).Date;
                            dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                            dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                            dialog.ShowDialog();
                        }
                    }
                    else
                    {

                        Conexion.IsEnabled = true;
                        Turnero.IsEnabled = true;
                        EditarDiseno.IsEnabled = true;
                        return true;
                    }
                }
                else
                {
                    Conexion.IsEnabled = false;
                    Turnero.IsEnabled = false;
                    EditarDiseno.IsEnabled = false;

                    Mensajes dialog = new Mensajes();
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "La licencia no es válida";
                    dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                    dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                    dialog.ShowDialog();
                }
            }
            else
            {
                Conexion.IsEnabled = false;
                Turnero.IsEnabled = false;
                EditarDiseno.IsEnabled = false;
            }
            ResizeMode = ResizeMode.NoResize;
            return false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ValidarActivar())
            {
                if (WindowState == WindowState.Maximized && !maximizado && !editar)
                {
                    maximizado = true;

                    //Topmost = true;

                    Principal.IsHitTestVisible = false;
                    ModoEdicion.Visibility = Visibility.Hidden;

                    WindowStyle = WindowStyle.None;
                    ResizeMode = ResizeMode.NoResize;
                    Visibility = Visibility.Collapsed;
                    Menu.Visibility = Visibility.Hidden;

                    //// re-show the window after changing style
                    Visibility = Visibility.Visible;
                    MaxHeight = SystemParameters.VirtualScreenHeight;
                    MaxWidth = SystemParameters.VirtualScreenWidth;
                    LimpiarVistaPrevia();
                    PausarVideos(true);
                    animaciones = true;
                    Task.Run(() => AnimacionesObjetos());

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    activarTurnero = config.AppSettings.Settings["Turnero"].Value.Equals("true") ? true : false;
                    Task.Run(() => EscuhcarTurnos());
                }

                else if (WindowState == WindowState.Maximized && editar)
                {
                    animaciones = false;
                    ModoEdicion.Visibility = Visibility.Visible;
                    Principal.IsHitTestVisible = true;

                    WindowStyle = WindowStyle.None;
                    ResizeMode = ResizeMode.NoResize;
                    Visibility = Visibility.Collapsed;
                    Menu.Visibility = Visibility.Hidden;

                    //// re-show the window after changing style
                    Visibility = Visibility.Visible;
                    MaxHeight = SystemParameters.VirtualScreenHeight;
                    MaxWidth = SystemParameters.VirtualScreenWidth;
                    LimpiarVistaPrevia();
                    PausarVideos(true);

                }
                else if (WindowState != WindowState.Maximized && !editar && maximizado)
                {
                    LimpiarVistaPrevia();
                    maximizado = false;
                    animaciones = false;
                    AsynchronousSocketListener.StopListening();
                    Topmost = false;
                    Menu.Visibility = Visibility.Visible;
                    ResizeMode = ResizeMode.CanResize;
                    WindowStyle = WindowStyle.ThreeDBorderWindow;
                    Principal.IsHitTestVisible = true;
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    CargarVistaPrevia();
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && maximizado && !estaSaliendo)
            {
                estaSaliendo = true;
                SalirEdicion();
                estaSaliendo = false;
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
                WindowState = WindowState.Maximized;
                editar = true;
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

        private bool SeModificaControl(string name)
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
                                itemControl.Header = nombreControl;
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
            SalirEdicion();
        }

        private void SalirEdicion()
        {
            PausarVideos(false);
            MediaElement control = (MediaElement)FindName("FullScreamVideo");
            if (control != null)
            {
                Video.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                Video.Background = null;
                Video.UpdateLayout();
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
                obj.Name = dialog.NombreText;
                obj.ToolTip = dialog.NombreText;
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
                obj.Name = dialog.NombreText;
                obj.ToolTip = dialog.NombreText;
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

            switch (item.GetType().Name.ToString())
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
                    propiedadesLabel.TipoControl.Text = "Texto";
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
                    propiedadesImagen.TipoControl.Text = "Imagen";
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
                    propiedadesVideo.TipoControl.Text = "Multimeda";
                    propiedadesVideo.Ruta.Text = ((MediaElement)item).Source.ToString();
                    propiedadesVideo.chkRelacion.IsChecked = true;
                    propiedadesVideo.Largo.Text = Convert.ToInt32(((MediaElement)item).ActualWidth).ToString();
                    propiedadesVideo.Ancho.Text = Convert.ToInt32(((MediaElement)item).ActualHeight).ToString();
                    propiedadesVideo.Opacidad.Value = item.Opacity;
                    propiedadesVideo.chkSonido.IsChecked = ((MediaElement)item).Volume == 1 ? true : false;
                    propiedadesVideo.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                    propiedadesVideo.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
                    string[] datosTag = ((MediaElement)item).Tag.ToString().Split('|');
                    if (datosTag.Length > 1)
                    {
                        propiedadesVideo.chkMaximizar.IsChecked = true;
                        propiedadesVideo.Cada.Text = datosTag[1];
                        propiedadesVideo.Durar.Text = datosTag[2];
                    }
                    else
                    {
                        propiedadesVideo.chkMaximizar.IsChecked = false;
                        propiedadesVideo.Cada.IsEnabled = false;
                        propiedadesVideo.Durar.IsEnabled = false;
                    }

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
                    propiedadesTabla.TipoControl.Text = "Tabla";

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

        public void BorarObjeto(string pNombre)
        {
            var item = FindName(pNombre) as UIElement;
            Principal.Children.Remove(item);
            NameScope.GetNameScope(this).UnregisterName(pNombre);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[pNombre] != null)
                config.AppSettings.Settings.Remove(pNombre);

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            DirectoryInfo info = new DirectoryInfo(@"objetos\");
            foreach (var file in info.GetFiles())
            {
                string[] nombre = file.Name.Split('-');
                if (nombre[1].Equals(pNombre + ".xaml"))
                    File.Delete(file.FullName);
            }
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
            dialog.Titulo.Content = "Agregar Video";
            dialog.ContenidoTextBox.IsEnabled = false;
            dialog.esVideo = true;
            if (dialog.ShowDialog() == true)
            {
                MediaElement obj = new MediaElement();
                obj.Name = dialog.NombreText;
                obj.Source = new Uri(dialog.ContenidoText);
                obj.LoadedBehavior = MediaState.Play;
                obj.MediaEnded += MediaElement_MediaEnded;
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.Stretch = Stretch.Uniform;
                obj.Height = 800;
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
                obj.Name = dialog.NombreText;
                obj.ToolTip = dialog.NombreText;
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

        public List<DataTable> LlenarListaTablas(int bloques, int cantFilas, DataTable dt, string orientacion)
        {
            List<DataTable> ListTablas = new List<DataTable>();
            int x = 1;
            int rowCont = 0;
            int rowContTotal = 0;

            DataTable dtFinal = new DataTable();
            for (int z = 0; z < (dt.Columns.Count * bloques) + (bloques - 1); z++)
                dtFinal.Columns.Add();

            object[] arrayTemp = new object[0];
            object[] arrayResult;
            foreach (DataRow row in dt.Rows)
            {
                rowCont++;
                rowContTotal++;
                if (x <= cantFilas)
                {
                    switch (bloques)
                    {
                        case 1:
                            dtFinal.Rows.Add(row.ItemArray);
                            x++;
                            break;
                        case 2:
                            arrayResult = arrayTemp.Concat(row.ItemArray).ToArray();
                            arrayTemp = arrayResult.Concat(new object[1] { "     " }.ToArray()).ToArray();
                            if (rowCont == 2 || rowContTotal == dt.Rows.Count)
                            {
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
                                dtFinal.Rows.Add(arrayResult);
                                arrayTemp = new object[0];
                                x++;
                                rowCont = 0;
                            }
                            break;
                        default:
                            break;
                    }

                    if (x == cantFilas + 1 || rowContTotal == dt.Rows.Count)
                    {
                        rowCont = 0;
                        if (orientacion.Equals("V"))
                            ListTablas.Add(PivotTable(dtFinal));
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
            controlClickName = item.Header.ToString();
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
                        ((DataGrid)itemObjets).UpdateLayout();
                        ColorFuenteFondoTabla(nombreControl, ((DataGrid)itemObjets).Tag.ToString());
                    }
                }
            }
        }

        private void CargarControles()
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
                                control.UpdateLayout();
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

        public List<DataTable> CargarListaTablas(string pNombre, string pTag)
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
                string contrasena = vSeguridad.DecryptString(config.AppSettings.Settings["CodigoActivacion"].Value, config.AppSettings.Settings["ContrasenaODBC"].Value);
                string Codigo = config.AppSettings.Settings["CodigoActivacion"].Value;
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
                        ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), dt, datos[2]);

                    }
                    else
                    {
                        ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2]);
                    }
                    connection.Close();
                }
                else
                {
                    ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2]);
                }
            }
            catch (Exception ex)
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Error al cargar la consulta: \n" + ex.Message;
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
                ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2]);
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
            control.UpdateLayout();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidarActivar())
                {
                    CargarControles();
                    if (WindowState != WindowState.Maximized)
                        CargarVistaPrevia();
                }
            }
            catch (Exception) { }

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
            if (datos.Length > 1)
                while (animaciones && maximizado && !editar)
                {
                    try
                    {
                        Task.Delay(int.Parse(datos[1]) * 1000).Wait();
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

        private async Task AnimacionesObjetos()
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
                    foreach (DataGridColumn column in control.Columns)
                        column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
                    control.UpdateLayout();
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
                    PausarVideos(false);
                    MediaElement control = (MediaElement)FindName(pNombre);
                    string[] datos = pTag.Split('|');
                    MediaElement obj = new MediaElement();
                    obj.Name = "FullScreamVideo";
                    obj.Source = new Uri(control.Source.ToString());
                    obj.LoadedBehavior = MediaState.Play;
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.Stretch = Stretch.Uniform;
                    obj.Height = this.ActualHeight;
                    obj.Volume = 1;

                    duracion = Convert.ToInt32(Math.Round(control.NaturalDuration.TimeSpan.TotalMilliseconds));
                    if (int.Parse(datos[2]) != 0)
                        duracion = int.Parse(datos[2]) * 1000;

                    MediaElement videoFull = (MediaElement)FindName("FullScreamVideo");
                    if (videoFull != null)
                    {
                        Video.Children.Remove(videoFull);
                        NameScope.GetNameScope(this).UnregisterName(videoFull.Name);
                        Video.UpdateLayout();
                    }
                    Video.Background = new SolidColorBrush(Colors.Black);
                    NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                    Video.Children.Add(obj);
                    Video.UpdateLayout();
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
                        Video.UpdateLayout();
                        PausarVideos(true);
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

        private void Turnero_Click(object sender, RoutedEventArgs e)
        {
            Turnero turnero = new Turnero(this);
            turnero.ShowDialog();
        }
    }
}