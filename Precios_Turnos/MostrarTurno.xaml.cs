using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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

        public MostrarTurno(bool pEsDiseno=false)
        {
            esDiseno = pEsDiseno;
            InitializeComponent();
            double height = SystemParameters.FullPrimaryScreenHeight;
            double width = SystemParameters.FullPrimaryScreenWidth;
            Height = height-(height*.10);
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
                        MenuItem itemCm = (MenuItem)cm.Items[6];
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

        private void MenuListaObjetos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem item = (MenuItem)e.Source;
            controlClickName = item.Header.ToString();
            mostarPropiedadesObjetos(e);
        }

        private void MenuEliminar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (controlClickName.Equals("Borde"))
            {
                Mensajes dialog = new Mensajes();
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Este objeto no puede ser borrado";
                dialog.lblTexto.Foreground = new SolidColorBrush(Colors.White);
                dialog.lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
                dialog.ShowDialog();
            }
            else
                BorarObjeto(controlClickName);
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

            switch (item.GetType().Name.ToString())
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
                    propiedadesLabel.TipoControl.Text = "Texto";
                    propiedadesLabel.Contenido.Text = ((Label)item).Content.ToString();
                    if (item.GetValue(NameProperty).ToString().Equals("NumeroTurno") || item.GetValue(NameProperty).ToString().Equals("NumeroEquipo"))
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

                case "Border":
                    PropiedadesBordeTurnero propiedadesBordeTurnero = new PropiedadesBordeTurnero(this);
                    propiedadesBordeTurnero.WindowStartupLocation = WindowStartupLocation.Manual;

                    if (point.X + propiedadesBordeTurnero.Width >= MaxWidth)
                        propiedadesBordeTurnero.Left = point.X - propiedadesBordeTurnero.Width;
                    else
                        propiedadesBordeTurnero.Left = point.X;

                    if (point.Y + propiedadesBordeTurnero.Height >= MaxHeight)
                        propiedadesBordeTurnero.Top = point.Y - propiedadesBordeTurnero.Height;
                    else
                        propiedadesBordeTurnero.Top = point.Y;

                    propiedadesBordeTurnero.NombreControl.Text = item.GetValue(NameProperty).ToString();
                    propiedadesBordeTurnero.TipoControl.Text = "Borde";
                    propiedadesBordeTurnero.btnColorBorde.Fill = new SolidColorBrush((((Border)item).BorderBrush as SolidColorBrush).Color);
                    propiedadesBordeTurnero.cbxGrosor.SelectedValue = ((Border)item).BorderThickness.Left;
                    propiedadesBordeTurnero.ShowDialog();
                    break;

                default:

                    break;
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
                    CargarControles();
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

            DirectoryInfo info = new DirectoryInfo(@"objetosTurno\");
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

        private void MenuAgregarTexto_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CapturaTextoDialogo dialog = new CapturaTextoDialogo(this);
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            var  point = e.GetPosition(Principal);
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

        private void GuardarControles()
        {
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
                if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo"))
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

        private void MenuMostrarOcultarTurno_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label control = (Label)FindName("NumeroTurno");
            if (control.Visibility==Visibility.Visible)
                control.Visibility=Visibility.Hidden;
            else
                control.Visibility = Visibility.Visible;


        }

        private void MenuMostrarOcultarEquipo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label control = (Label)FindName("NumeroEquipo");
            if (control.Visibility == Visibility.Visible)
                control.Visibility = Visibility.Hidden;
            else
                control.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (esDiseno)
                GuardarControles();
        }

        private void MenuMostrarOcultarNombreEquipo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label control = (Label)FindName("NombreEquipo");
            if (control.Visibility == Visibility.Visible)
                control.Visibility = Visibility.Hidden;
            else
                control.Visibility = Visibility.Visible;
        }
    }
}
