using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using static Precios_Turnos.MainWindow;

namespace Precios_Turnos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal string nombreApp = "Precios_Turnos";
        private Point _positionInBlock;
        private TranslateTransform? _currentTT;
        private bool editar = false;
        public Color ultimoColorLetra;
        public Color ultimoColorFondo;
        private string controlClickName;
        private ObservableCollection<Articulo> Collection { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Coordenadas.Visibility = Visibility.Hidden;
            ValidarActivar();
        }

        private void ValidarActivar()
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
                    if (subs[4].Equals("0"))
                        Activar.IsEnabled = false;
                    else
                    {
                        if (vSeguridad.GetNetworkTime() <= Convert.ToDateTime(subs[4]))
                        {
                            Activar.IsEnabled = false;
                        }  
                        else
                        {
                            Conexion.IsEnabled = false;
                            Turnero.IsEnabled = false;
                            EditarDiseno.IsEnabled = false;
                        }
                    }  
                }
                else
                {
                    
                }

            }
            else
            {
                Conexion.IsEnabled = false;
                Turnero.IsEnabled = false;
                EditarDiseno.IsEnabled = false;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Topmost = true;
                if (editar)
                {
                    Topmost = false;
                    ModoEdicion.Visibility = Visibility.Visible;
                }
                else { 
                    ModoEdicion.Visibility = Visibility.Hidden;
                }

                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Visibility = Visibility.Collapsed;
                Menu.Visibility = Visibility.Hidden;

                //// re-show the window after changing style
                Visibility = Visibility.Visible;
                MaxHeight = SystemParameters.VirtualScreenHeight;
                MaxWidth = SystemParameters.VirtualScreenWidth;


            }

            else
            {
                Topmost = false;
                Menu.Visibility = Visibility.Visible;
                ResizeMode = ResizeMode.CanResize;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SalirEdicion();
            }
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditarDiseno_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            editar = true;
            Coordenadas.Visibility = Visibility.Visible;
            ModoEdicion.Content = "Modo edición";
            ModoEdicion.FontSize = 24;
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
            WindowState = WindowState.Normal;
            Coordenadas.Visibility = Visibility.Hidden;
            ModoEdicion.Content = "Vista previa";
            ModoEdicion.FontSize = 18;
            ModoEdicion.Visibility = Visibility.Visible;

            if (editar)
                editar = false;
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
                obj.HorizontalAlignment = HorizontalAlignment.Left;
                obj.VerticalAlignment = VerticalAlignment.Top;
                obj.Margin = new Thickness(dialog.Left, dialog.Top, 0, 0);
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial Rounded MT");
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
            var container = VisualTreeHelper.GetParent(item) as UIElement;

            Point point = item.TransformToAncestor(this).Transform(new Point(0, 0));

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
                    propiedadesImagen.chkSonido.Visibility = Visibility.Hidden;
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
                    propiedadesVideo.Largo.Text = Convert.ToInt32(((MediaElement)item).ActualWidth).ToString();
                    propiedadesVideo.Ancho.Text = Convert.ToInt32(((MediaElement)item).ActualHeight).ToString();
                    propiedadesVideo.Opacidad.Value = item.Opacity;
                    propiedadesVideo.CoordenadaX.Text = Convert.ToInt32(point.X).ToString();
                    propiedadesVideo.CoordenadaY.Text = Convert.ToInt32(point.Y).ToString();
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
                    propiedadesTabla.cbxCRegistros.SelectedValue = ((DataGrid)item).Items.Count*2;

                    SolidColorBrush brushLetra1 = new SolidColorBrush((((DataGrid)item).Foreground as SolidColorBrush).Color);
                    SolidColorBrush brushLetra2 = new SolidColorBrush((((DataGrid)item).Foreground as SolidColorBrush).Color);
                    SolidColorBrush brushFondo1 = new SolidColorBrush((((DataGrid)item).Background as SolidColorBrush).Color);
                    SolidColorBrush brushFondo2 = new SolidColorBrush((((DataGrid)item).Background as SolidColorBrush).Color);

                    for (int i = 0; i < 2; i++)
                    {
                        DataGridRow row = (DataGridRow)((DataGrid)item).ItemContainerGenerator.ContainerFromIndex(i);

                        if (row != null)
                        {
                            int index = row.GetIndex();
                            if (index % 2 == 0)
                            {
                                brushLetra1 = (SolidColorBrush)row.Foreground;
                                brushFondo1 = (SolidColorBrush)row.Background;
                            }
                            else
                            {
                                brushLetra2= (SolidColorBrush)row.Foreground;
                                brushFondo2 = (SolidColorBrush)row.Background;
                            }
                        }
                    }

                    if(brushLetra1 != brushLetra2)
                    {
                        propiedadesTabla.chkDoble.IsChecked = true;
                        propiedadesTabla.btnColorFuente.Fill = brushLetra1;
                        propiedadesTabla.btnColorFondo.Fill = brushFondo1;
                        propiedadesTabla.btnColorFuente2.Fill = brushLetra2;
                        propiedadesTabla.btnColorFondo2.Fill = brushFondo2;
                    }
                    else
                    {
                        propiedadesTabla.btnColorFuente.Fill = brushLetra1;
                        propiedadesTabla.btnColorFondo.Fill = brushFondo1;
                        propiedadesTabla.btnColorFuente2.Fill = brushLetra2;
                        propiedadesTabla.btnColorFondo2.Fill = brushFondo2;
                        propiedadesTabla.btnColorFuente2.Visibility = Visibility.Hidden;
                        propiedadesTabla.btnColorFondo2.Visibility = Visibility.Hidden;
                        propiedadesTabla.lblCFuenteUno.Visibility = Visibility.Hidden;
                        propiedadesTabla.lblCFuenteDos.Visibility = Visibility.Hidden;
                        propiedadesTabla.lblCFondoUno.Visibility = Visibility.Hidden;
                        propiedadesTabla.lblCFondoDos.Visibility = Visibility.Hidden;
                        Grid.SetColumnSpan(propiedadesTabla.btnColorFuente, 3);
                        Grid.SetColumnSpan(propiedadesTabla.btnColorFondo, 3);
                    }

                    propiedadesTabla.chkLineas.IsChecked = ((DataGrid)item).GridLinesVisibility == DataGridGridLinesVisibility.All ? true : false;
                    propiedadesTabla.chkCFijo.IsChecked = !((DataGrid)item).IsReadOnly;
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
            var item = FindName(controlClickName) as UIElement;
            Principal.Children.Remove(item);
            NameScope.GetNameScope(this).UnregisterName(controlClickName);
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
                obj.FontFamily = new FontFamily("Arial Rounded MT");
                obj.ItemsSource = collectionX2(30, true);
                obj.CellStyle = new Style(typeof(DataGridCell)) { 
                    Setters = { 
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right)
                    } };
                obj.HeadersVisibility = DataGridHeadersVisibility.None;
                obj.CanUserAddRows = false;
                obj.GridLinesVisibility = DataGridGridLinesVisibility.None;
                obj.BorderBrush = new SolidColorBrush(Colors.Transparent);
                obj.IsReadOnly = true;

                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
               
            }
        }

        public ObservableCollection<Articulo> collectionX2(int cantFilas, bool demo = false)
        {
            Collection = new ObservableCollection<Articulo>();

            for (int x = 1; x <= cantFilas; x += 2)
            {
                if (demo)
                {
                    Articulo articulo = new Articulo();
                    articulo.Nombre1 = "Articulo " + x;
                    articulo.Espacio1 = "        ";
                    articulo.Precio1 = "$"+Math.Round(new Random().NextDouble() * (300 - 30) + 30 , 2);
                    articulo.SeparacionDoble = "               ";
                    articulo.Nombre2 = "Articulo " + (x + 1);
                    articulo.Espacio2 = "        ";
                    articulo.Precio2 = "$"+Math.Round(new Random().NextDouble() * (300 - 30) + 30, 2);
                    Collection.Add(articulo);
                }
            }
            return Collection;
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
    }

    public class Articulo
    {
        public string Nombre1 { get; set; }

        public string Espacio1 { get; set; }

        public string Precio1 { get; set; }

        public string SeparacionDoble { get; set; }

        public string Nombre2 { get; set; }

        public string Espacio2 { get; set; }

        public string Precio2 { get; set; }
    }

}