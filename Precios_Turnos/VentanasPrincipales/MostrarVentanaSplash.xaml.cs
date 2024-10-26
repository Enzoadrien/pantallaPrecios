using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using ITLlib;
using Org.BouncyCastle.Asn1.X509;
using Priceio.Cajero;
using Priceio.Cajero.Hopper;
using Priceio.Cajero.Payout;
using Priceio.ClasesGenericas;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para MostrarTurno.xaml
    /// </summary>
    public partial class MostrarVentanaSplash : Window
    {
        private Point _positionInBlock;
        private TranslateTransform? _currentTT;
        private string? controlClickName;
        internal Color ultimoColorLetra;
        internal Color ultimoColorFondo;
        private bool estaSaliendo = false;
        private bool esDiseno;
        private SolidColorBrush? ultimoColor;
        internal double ultimaOpacidad;
        internal string? controlSelectedName;
        private MainWindow? mainWindow;
        internal static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private string datoVerificador;
        private string tipoVentana = string.Empty;
        private double anchoAnt = 0;
        private double altoAnt = 0;
        private Pago? pago;

        private SMARTPayout? smartPayout;
        private SMARTHopper? smartHopper;
        private string moneda = "MXN";
        //

        internal MostrarVentanaSplash(bool pEsDiseno = false, MainWindow? parentWindow = null, string pvSrtDatoVerificador = "", Pago? pvPago = null, SMARTPayout? payout = null, SMARTHopper? hopper = null)
        {
            smartPayout = payout;
            smartHopper = hopper;

            if(pvPago != null)
            {
                if (pvPago.TipoPago == Pago.Tipo.PAGO)
                {
                    smartPayout.Payout.EnableValidator(smartPayout.logPagoPayout);
                    smartHopper.Hopper.EnableCoinMech(smartHopper.logPagoHopper);
                }
                smartPayout.actualizaPago(ref pvPago);
                smartHopper.actualizaPago(ref pvPago);
            }
                

            Owner = parentWindow;
            mainWindow = parentWindow;
            esDiseno = pEsDiseno;
            datoVerificador = pvSrtDatoVerificador;

            InitializeComponent();
            crearDirectorios();
            pago = pvPago;

            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                double Ancho = double.Parse(config.AppSettings.Settings["Ancho"].Value);
                double Alto = double.Parse(config.AppSettings.Settings["Alto"].Value);
                if (Ancho == 0 & Alto == 0)
                {
                    Width = SystemParameters.VirtualScreenWidth / 2;
                    Height = SystemParameters.VirtualScreenHeight / 2;

                    config.AppSettings.Settings["Ancho"].Value = Width.ToString();
                    config.AppSettings.Settings["Alto"].Value = Height.ToString();
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                else
                {
                    Width = double.Parse(config.AppSettings.Settings["Ancho"].Value);
                    Height = double.Parse(config.AppSettings.Settings["Alto"].Value);
                }

                MinWidth = 300;
                MaxWidth = SystemParameters.VirtualScreenWidth;
                MinHeight = 300;
                MaxHeight = SystemParameters.VirtualScreenHeight;

                tipoVentana = config.AppSettings.Settings["TipoSplash"].Value;
            }
            catch { }
            if (!esDiseno)
            {
                BarraMenus.Visibility = Visibility.Hidden;
                ModoEdicion.Visibility = Visibility.Hidden;
                Coordenadas.Visibility = Visibility.Hidden;

                if (!tipoVentana.Equals("C"))
                {
                    IsHitTestVisible = false;
                    StartCloseTimer();
                }

            }

            CenterWindowOnScreen();
        }

        internal Pago? recuperaPago()
        {
            return pago;
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }


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

        private void ActivarVoz()
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
                    //Datos para voz de turnero 
                    Label NumeroTurno = (Label)FindName("NumeroTurno");
                    if (NumeroTurno != null)
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

                    //Datos para Verificador de precios
                    DataGrid control = (DataGrid)FindName("TablaDatos");
                    if (control != null)
                    {
                        int i = 1;
                        foreach (DataRowView o in (control).Items)
                            line = line.Replace(@"TextoVoz" + (i++), o[0].ToString());
                    }
                }));

                synthesizer.SpeakAsync(line);
            }
            catch { }
        }

        private void Principal_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (esDiseno)
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

        }

        private void Principal_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (esDiseno)
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

        }

        private void Principal_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (esDiseno)
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
        }

        private void Principal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (esDiseno)
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
                            mostarPropiedadesObjetos(e.GetPosition(Principal));
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        private void Principal_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (esDiseno)
            {
                try
                {
                    var item = e.Source as UIElement;

                    if (!SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        ContextMenu cm = this.FindResource("cmdPrincipalContexMenu") as ContextMenu;

                        if (tipoVentana.Equals("T"))
                        {
                            ((MenuItem)cm.Items[3]).IsEnabled = false;
                            ((MenuItem)cm.Items[4]).IsEnabled = false;

                        }
                        else if (tipoVentana.Equals("V"))
                        {
                            ((MenuItem)cm.Items[2]).IsEnabled = false;
                            ((MenuItem)cm.Items[4]).IsEnabled = false;
                        }
                        else if (tipoVentana.Equals("C"))
                        {
                            ((MenuItem)cm.Items[2]).IsEnabled = false;
                            ((MenuItem)cm.Items[3]).IsEnabled = false;
                        }

                        Label control = (Label)FindName("NumeroTurno");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[0]).Header = "Eliminar turno";
                        else
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[0]).Header = "Agregar turno";

                        control = (Label)FindName("NumeroEquipo");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[1]).Header = "Eliminar equipo";
                        else
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[1]).Header = "Agregar equipo";

                        control = (Label)FindName("NombreEquipo");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[2]).Header = "Eliminar nombre equipo";
                        else
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[2]).Header = "Agregar nombre equipo";


                        control = (Label)FindName("NumeroTurnoAnt");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[4]).Header = "Eliminar turno anterior";
                        else
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[4]).Header = "Agregar turno anterior";

                        control = (Label)FindName("NumeroEquipoAnt");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[5]).Header = "Eliminar equipo anterior";
                        else
                            ((MenuItem)((MenuItem)cm.Items[2]).Items[5]).Header = "Agregar equipo anterior";

                        DataGrid control2 = (DataGrid)FindName("TablaDatos");
                        if (control2 != null)
                            ((MenuItem)((MenuItem)cm.Items[3]).Items[0]).Header = "Eliminar tabla de datos";
                        else
                            ((MenuItem)((MenuItem)cm.Items[3]).Items[0]).Header = "Agregar tabla de datos";

                        control = (Label)FindName("CantidadTotal");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[0]).Header = "Eliminar cantidad total";
                        else
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[0]).Header = "Agregar cantidad total";
                        control = (Label)FindName("CantidadIngresada");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[1]).Header = "Eliminar cantidad ingresada";
                        else
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[1]).Header = "Agregar cantidad ingresada";
                        control = (Label)FindName("CantidadFaltante");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[2]).Header = "Eliminar cantidad faltante";
                        else
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[2]).Header = "Agregar cantidad faltante";
                        control = (Label)FindName("Cambio");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[3]).Header = "Eliminar cambio";
                        else
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[3]).Header = "Agregar cambio";
                        Button controlBtn = (Button)FindName("BtnCancelar");
                        if (controlBtn != null)
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[4]).Header = "Eliminar botón cancelar";
                        else
                            ((MenuItem)((MenuItem)cm.Items[4]).Items[4]).Header = "Agregar botón cancelar";

                        MenuItem itemCm = (MenuItem)cm.Items[6];
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
        }

        private void MenuListaObjetos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem item = (MenuItem)e.Source;
            controlClickName = item.Tag.ToString();
            mostarPropiedadesObjetos(item.PointToScreen(new Point(((MenuItem)item).ActualWidth, ((MenuItem)item).ActualHeight)));
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
                case "ImgTablaDatos":
                case "BarraMenus":
                case "TituloMenu":
                case "Salir":
                case "Maximizar":
                case "VentanaSplash":
                    seElimina = false;
                    break;
                default:
                    seElimina = true;
                    break;
            }
            return seElimina;
        }

        internal bool SeModificaControl(string name)
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
                case "BarraMenus":
                case "TituloMenu":
                case "Salir":
                case "Maximizar":
                case "VentanaSplash":
                    seModifica = false;
                    break;
                default:
                    seModifica = true;
                    break;
            }
            return seModifica;
        }

        private void mostarPropiedadesObjetos(Point point)
        {
            if (esDiseno)
            {
                var item = FindName(controlClickName) as UIElement;
                Point pointItem = item.TransformToAncestor(this).Transform(new Point(0, 0));

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
                    case "DataGrid":
                        PropiedadesTablaVerificador propiedadesTabla = new PropiedadesTablaVerificador(this);
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
                        propiedadesTabla.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();
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

                        propiedadesTabla.CoordenadaX.Text = Math.Round(pointItem.X).ToString();
                        propiedadesTabla.CoordenadaY.Text = Math.Round(pointItem.Y).ToString();

                        propiedadesTabla.ShowDialog();
                        break;
                    default:

                        break;
                }
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

                if (tipoVentana.Equals("C"))
                    if (pago != null)
                    {
                        actualizaPagoControles();
                    }
            }

        }

        internal void IniciarProcesosCajero()
        {
            Task.Run(() => verificaPago());
        }

        internal void verificaPago()
        {
            bool finalizarPagoHilo = true;
            while (finalizarPagoHilo)
            {

                try
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (pago.TipoPago == Pago.Tipo.RETIRO)
                        {

                            Button controlBtn = (Button)FindName("BtnCancelar");
                            if (controlBtn != null)
                                controlBtn.IsEnabled = false;
                        }
                        if (pago.Pagado)
                        {
                            while (finalizarPagoHilo)
                            {
                                if(smartHopper.Hopper.UnitDataList.Count == 4 && smartPayout.Payout.UnitDataList.Count == 6)
                                {

                                    smartPayout.Payout.DisableValidator(smartPayout.logPagoPayout);
                                    smartHopper.Hopper.DisableCoinMech(smartHopper.logPagoHopper);

                                    if (pago.Cambio > 0)
                                    {
                                        bool cambio = calcularCambios();
                                        if(cambio && pago.EstadoPago != Pago.Estado.CANCELADO)
                                            pago.EstadoPago = Pago.Estado.OK;
                                        else if(cambio && pago.EstadoPago == Pago.Estado.CANCELADO)
                                            pago.EstadoPago = Pago.Estado.CANCELADO;
                                        else
                                            pago.EstadoPago = Pago.Estado.SIN_EFECTIVO;
                                    }
                                    else
                                        if (pago.EstadoPago != Pago.Estado.CANCELADO)
                                            pago.EstadoPago = Pago.Estado.OK;
                                        

                                    finalizarPagoHilo = false;
                                    StartCloseTimer();
                                    break;
                                }
                            }
                        }
                    }));
                }
                catch{}
                actualizaPagoControles();
                Thread.Sleep(250);
            }
        }

        private void actualizaPagoControles()
        {
            Application.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                Label control = (Label)FindName("CantidadTotal");
                if (control != null)
                    control.Content = "$ " + pago.CantidadTotal;
                control = (Label)FindName("CantidadFaltante");
                if (control != null)
                    control.Content = "$ " + pago.CantidadFaltante;
                control = (Label)FindName("CantidadIngresada");
                if (control != null)
                    control.Content = "$ " + pago.CantidadIngresada;
                control = (Label)FindName("Cambio");
                if (control != null)
                    control.Content = "$ " + pago.Cambio;
            }));
        }

        private bool calcularCambios()
        {
            ValidarCambio validarCambio = new ValidarCambio();
            bool cambioCompleto = validarCambio.calculaCambio(pago.Cambio, smartHopper.Hopper.UnitDataList, smartPayout.Payout.UnitDataList);
            if (cambioCompleto)
            {

                if (validarCambio.monedasCambio > 0)
                {
                    if (!smartHopper.CalculatePayoutHopper(validarCambio.monedasCambio.ToString(), moneda.ToCharArray()))
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, false);
                            dialog.lblNombre.Content = "¡Advertencia!";
                            dialog.lblTexto.Text = "El cajero no cuenta con cambio suficiente, se reembolsará el total de dinero ingresado, consulte al administrador.";
                            new Recursos().ventanaMensajesGrande800x600(dialog);
                            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            dialog.ShowDialog();

                        }));

                    }
                }

                if (validarCambio.billetesCambio > 0)
                {
                    if (!smartPayout.CalculatePayout(validarCambio.billetesCambio.ToString(), moneda.ToCharArray()))
                    {

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, false);
                            dialog.lblNombre.Content = "¡Advertencia!";
                            dialog.lblTexto.Text = "El cajero no cuenta con cambio suficiente, se reembolsará el total de dinero ingresado, consulte al administrador.";
                            new Recursos().ventanaMensajesGrande800x600(dialog);
                            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            dialog.ShowDialog();

                        }));
                    }

                }
            }
            else
            {
                if (pago.TipoPago == Pago.Tipo.PAGO)
                {
                    if (pago.CantidadMonedasIngresadas > 0)
                    {
                        if (!smartHopper.CalculatePayoutHopper(pago.CantidadMonedasIngresadas.ToString(), moneda.ToCharArray()))
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, false);
                                dialog.lblNombre.Content = "¡Advertencia!";
                                dialog.lblTexto.Text = "El cajero no cuenta con cambio suficiente, se reembolsará el total de dinero ingresado, consulte al administrador.";
                                new Recursos().ventanaMensajesGrande800x600(dialog);
                                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                dialog.ShowDialog();

                            }));
                        }
                    }

                    if (pago.CantidadBilletesIngresados > 0)
                    {
                        if (!smartPayout.CalculatePayout(pago.CantidadBilletesIngresados.ToString(), moneda.ToCharArray()))
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, false);
                                dialog.lblNombre.Content = "¡Advertencia!";
                                dialog.lblTexto.Text = "El cajero no cuenta con cambio suficiente, se reembolsará el total de dinero ingresado, consulte al administrador.";
                                new Recursos().ventanaMensajesGrande800x600(dialog);
                                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                dialog.ShowDialog();

                            }));
                        }

                    }
                }

            }


            return cambioCompleto;
        }

        internal bool BorarObjeto(string pNombre, bool pMuestraMensaje = true)
        {
            if (SeEliminaControl(pNombre))
            {
                var item = FindName(pNombre) as UIElement;

                bool seBorra = false;
                if (pMuestraMensaje)
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
                    dialog.lblNombre.Content = "¡Advertencia!";
                    dialog.lblTexto.Text = "Se eliminará de forma permanente el objeto " + pNombre + "(" + item.GetType().Name + ").";
                    if (dialog.ShowDialog() == true)
                        seBorra = true;
                }
                if (seBorra || !pMuestraMensaje)
                {
                    Principal.Children.Remove(item);
                    NameScope.GetNameScope(this).UnregisterName(pNombre);
                    try
                    {
                        DirectoryInfo info = new DirectoryInfo(@"objetosSplash\");
                        foreach (var file in info.GetFiles())
                        {
                            string[] nombre = file.Name.Split('-');
                            if (nombre[1].Equals(pNombre + ".xaml"))
                                File.Delete(file.FullName);
                        }

                        info = new DirectoryInfo(@"objetosSplash\animaciones");

                        foreach (var file in info.GetFiles())
                        {
                            if (file.Name.Equals(pNombre + ".anim"))
                                File.Delete(file.FullName);
                        }

                        info = new DirectoryInfo(@"objetosSplash\consultasSQL");

                        foreach (var file in info.GetFiles())
                        {
                            if (file.Name.Equals(pNombre + ".sql"))
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
                            case "DataGrid":
                                var itemImg = FindName("ImgTablaDatos") as UIElement;

                                if (itemImg != null)
                                {
                                    Principal.Children.Remove(itemImg);
                                    NameScope.GetNameScope(this).UnregisterName("ImgTablaDatos");
                                    if (Directory.Exists(@".\objetosSplash\TablaDatos"))
                                    {
                                        Directory.Delete(@".\objetosSplash\TablaDatos", true);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch { }
                    return true;
                }
            }
            else
            {
                if (pMuestraMensaje)
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Este objeto no puede ser borrado.";
                    dialog.ShowDialog();
                }
            }
            return false;
        }

        private void objeto_MouseLeave(object sender, MouseEventArgs e)
        {
            if (esDiseno)
            {
                var control = e.Source as UIElement;
                control.SetValue(BackgroundProperty, ultimoColor);
            }
        }

        private void objeto_MouseEnter(object sender, MouseEventArgs e)
        {
            if (esDiseno)
            {
                var control = e.Source as UIElement;
                ultimoColor = new SolidColorBrush((control.GetValue(BackgroundProperty) as SolidColorBrush).Color);
                control.SetValue(BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffbee6fd")));
                controlSelectedName = control.GetValue(NameProperty).ToString();
            }
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

        private void PropiedadesVentana_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // get the position within the container
            var mousePosition = e.GetPosition(this);
            PropiedadesVentanaSplash propiedadesVentanaTurnero = new PropiedadesVentanaSplash(this);
            propiedadesVentanaTurnero.WindowStartupLocation = WindowStartupLocation.Manual;

            if (mousePosition.X + propiedadesVentanaTurnero.Width >= MaxWidth)
                propiedadesVentanaTurnero.Left = mousePosition.X - propiedadesVentanaTurnero.Width;
            else
                propiedadesVentanaTurnero.Left = mousePosition.X;

            if (mousePosition.Y + propiedadesVentanaTurnero.Height >= MaxHeight)
                propiedadesVentanaTurnero.Top = mousePosition.Y - propiedadesVentanaTurnero.Height;
            else
                propiedadesVentanaTurnero.Top = mousePosition.Y;

            var item = FindName("Borde") as UIElement;

            propiedadesVentanaTurnero.nombreControl = item.GetValue(NameProperty).ToString();
            propiedadesVentanaTurnero.btnColorBorde.Fill = new SolidColorBrush((((Border)item).BorderBrush as SolidColorBrush).Color);
            propiedadesVentanaTurnero.cbxGrosor.SelectedValue = ((Border)item).BorderThickness.Left;


            try
            {
                propiedadesVentanaTurnero.btnColorFondo.Fill = new SolidColorBrush(((SolidColorBrush)Base.Background).Color);
            }
            catch (Exception)
            {
                propiedadesVentanaTurnero.btnColorFondo.Fill = new SolidColorBrush(Colors.White);
            }
            try
            {
                propiedadesVentanaTurnero.ContenidoTextBox.Text = Path.GetFileName(Fondo.Source.ToString());
                propiedadesVentanaTurnero.ContenidoTextBox.ToolTip = Path.GetFileName(Fondo.Source.ToString());
                propiedadesVentanaTurnero.Opacidad.IsEnabled = true;
                propiedadesVentanaTurnero.Opacidad.Value = Fondo.Opacity;
            }
            catch { }

            propiedadesVentanaTurnero.ShowDialog();
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
                    FileInfo fileImg = new FileInfo(@".\objetosSplash\multimedia\" + fi.Name);
                    if (File.Exists(@".\objetosSplash\multimedia\" + fi.Name) && !fi.FullName.Equals(fileImg.FullName))
                    {
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, "Remplazar", "Mantener");
                        dialogMsg.lblNombre.Content = "¡Advertencia!";
                        dialogMsg.lblTexto.Text = "Ya existe un archivo con el mismo nombre y extension en la aplicación, ¿Desea remplazarlo o mantener la actual?. ¡Esta accion no se puede revertir!";
                        if (dialogMsg.ShowDialog() == true)
                        {
                            fi.CopyTo(@".\objetosSplash\multimedia\" + fi.Name, true);
                            foreach (var itemObjets in Principal.Children)
                            {
                                string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                                if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo"))
                                {
                                    switch (itemObjets.GetType().Name.ToString())
                                    {
                                        case "Image":
                                            if (((BitmapImage)((Image)itemObjets).Source).UriSource.Equals(@".\objetosSplash\multimedia\" + fi.Name))
                                            {
                                                BitmapImage bitmapImage = new BitmapImage();
                                                bitmapImage.BeginInit();
                                                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                                bitmapImage.UriSource = new Uri(@".\objetosSplash\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                                                bitmapImage.EndInit();

                                                ((Image)itemObjets).Source = bitmapImage;
                                            }
                                            break;
                                        case "MediaElement":
                                            if (((MediaElement)itemObjets).Source.Equals(@".\objetosSplash\multimedia\" + fi.Name))
                                            {
                                                Application.Current.Dispatcher.Invoke(new Action(async () =>
                                                {
                                                    ((MediaElement)itemObjets).Source = null;
                                                    await Task.Delay(100);
                                                    ((MediaElement)itemObjets).Source = new Uri(@".\objetosSplash\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
                        fi.CopyTo(@".\objetosSplash\multimedia\" + fi.Name, true);
                }
                catch
                {
                }
                if (extension.Equals("gif"))
                {
                    MediaElement obj = new MediaElement();
                    obj.Name = dialog.NombreText.ToUpper();
                    obj.ToolTip = dialog.NombreText.ToUpper();
                    obj.Source = new Uri(@".\objetosSplash\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
                    bitmapImage.UriSource = new Uri(@".\objetosSplash\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
                    bitmapImage.UriSource = new Uri(@".\objetosSplash\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
                mostarPropiedadesObjetos(e.GetPosition(Principal));
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
            dialog.lblTexto.Text = "Se eliminará todo el diseño de forma permanente. ¿Está seguro que desea continuar?";
            if (dialog.ShowDialog() == true)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["Ancho"].Value = (SystemParameters.VirtualScreenWidth / 2).ToString();
                config.AppSettings.Settings["Alto"].Value = (SystemParameters.VirtualScreenHeight / 2).ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                Width = SystemParameters.VirtualScreenWidth / 2;
                Height = SystemParameters.VirtualScreenHeight / 2;

                List<string> objEliminar = new List<string>();
                try
                {
                    foreach (var itemObjets in Principal.Children)
                    {
                        string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                        if (SeEliminaControl(nombreControl))
                        {
                            objEliminar.Add(nombreControl);
                        }
                        else if (nombreControl.Equals("Fondo"))
                        {
                            Fondo.Source = null;
                        }
                        else if (nombreControl.Equals("Base"))
                        {
                            Base.Background = new SolidColorBrush(Colors.White);
                        }
                        else if (nombreControl.Equals("Borde"))
                        {
                            Border control = (Border)FindName(nombreControl);
                            control.BorderBrush = new SolidColorBrush(Colors.Black);
                            control.BorderThickness = new Thickness(3);
                        }
                    }

                    foreach (string ob in objEliminar)
                    {
                        BorarObjeto(ob, false);
                    }
                    foreach (var item in Directory.GetFiles(@".\objetosSplash\multimedia", "*.*"))
                    {
                        File.SetAttributes(item, FileAttributes.Normal);
                        File.Delete(item);
                    }
                }
                catch { }
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
                GuardarDatos();
                var controlSelected = FindName(controlSelectedName) as UIElement;
                if (controlSelected != null)
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
                if (!Directory.Exists(@".\objetosSplash"))
                {
                    Directory.CreateDirectory(@".\objetosSplash");
                }
                int x = 0;
                DirectoryInfo di = new DirectoryInfo(@".\objetosSplash");
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (var itemObjets in Principal.Children)
                {
                    bool esTabla;
                    string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                    if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo") || nombreControl.Equals("Base") || nombreControl.Equals("Borde"))
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
                        File.WriteAllText(@"objetosSplash\" + ++x + "-" + nombreControl + ".xaml", vSeguridad.EncryptString(MainWindow.nombreApp, savedControls));
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

        private void GuardarDatos()
        {
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Ancho"].Value = Width.ToString();
            config.AppSettings.Settings["Alto"].Value = Height.ToString();

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        internal void CargarControles()
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(@"objetosSplash\");
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
                                        List<DataTable> list = CargarListaTablas(control.Name, control.Tag.ToString(), false, datoVerificador);
                                        control.ItemsSource = list[0].DefaultView;

                                        item.MouseLeave += objetoMedia_MouseLeave;
                                        item.MouseEnter += objetoMedia_MouseEnter;

                                        //ColorFuenteFondoTabla(control.Name, control.Tag.ToString());

                                        break;
                                    case "Button":
                                        Button button = (Button)FindName(item.GetValue(NameProperty).ToString());
                                        button.Click += Button_Click;
                                        button.MouseDoubleClick += Button_DobleClick;
                                        break;
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception) { }
                }
                string pNombre = "ImgTablaDatos";
                var itemImg = FindName(pNombre) as UIElement;
                if (itemImg != null)
                {
                    try
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.UriSource = new Uri(@".\objetosSplash\TablaDatos\" + datoVerificador + ".png", UriKind.RelativeOrAbsolute);
                        bitmapImage.EndInit();
                        ((Image)itemImg).Source = bitmapImage;

                        ((Image)itemImg).Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        if (datoVerificador.Length > 0)
                            ((Image)itemImg).Visibility = Visibility.Hidden;
                        else
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.UriSource = new Uri(@".\Recursos\pictureAdd.png", UriKind.RelativeOrAbsolute);
                            bitmapImage.EndInit();

                            ((Image)itemImg).Source = bitmapImage;
                            ((Image)itemImg).Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        private void SalirEdicion()
        {
            Close();
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
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
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["Ancho"].Value = Width.ToString();
                config.AppSettings.Settings["Alto"].Value = Height.ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                GuardarControles();
            }
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
            DirectoryInfo info = new DirectoryInfo(@"objetosSplash\animaciones");
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
            if (tipoVentana.Equals("T"))
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
                                if (numeroTurnoAnt != null)
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

        internal void SetearNumeroTurno(int numeroTurno)
        {
            new Recursos().GuardarNumeroTurno(numeroTurno);
        }

        private void AgregarTablaDatos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            DataGrid control = (DataGrid)FindName("TablaDatos");

            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar tabla de datos";
            }
            else
            {
                CapturaTextoDialogo dialog = new CapturaTextoDialogo(this, false);
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
                dialog.Titulo.Content = "Agregar tabla de datos";
                dialog.NombreText = "TablaDatos";
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
                    obj.Tag = "1|1|V|15";
                    obj.ItemsSource = CargarListaTablas(dialog.NombreText, obj.Tag.ToString())[0].DefaultView;
                    obj.MouseLeave += objetoMedia_MouseLeave;
                    obj.MouseEnter += objetoMedia_MouseEnter;

                    NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                    Principal.Children.Add(obj);

                    itemCm.Header = "Agregar tabla de datos";
                }
            }
        }

        internal List<DataTable> CargarListaTablas(string pNombre, string pTag, bool pMostrarMensaje = false, string pStrDatoBuscar = "")
        {
            string[] datos = pTag.Split('|');
            List<DataTable> ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2], pNombre);
            try
            {
                Seguridad vSeguridad = new Seguridad();
                //Create the object
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string odbc = config.AppSettings.Settings["ODBC"].Value;
                string usuario = config.AppSettings.Settings["UsuarioODBC"].Value;
                string contrasena = vSeguridad.DecryptString(MainWindow.nombreApp, config.AppSettings.Settings["ContrasenaODBC"].Value);
                string consulta = string.Empty;
                string nombreIndex = string.Empty;

                DirectoryInfo info = new DirectoryInfo(@".\objetosSplash\consultasSQL");

                foreach (var file in info.GetFiles())
                {
                    if (@file.Name.Equals(pNombre + ".sql"))
                    {
                        StreamReader sR = new StreamReader(@file.FullName);
                        string lectura = sR.ReadToEnd();
                        sR.Close();
                        string[] datosC = vSeguridad.DecryptString(MainWindow.nombreApp, lectura).Split('|');

                        if (datosC.Length > 1)
                        {
                            if (pStrDatoBuscar.Length == 0)
                                pStrDatoBuscar = datosC[2];

                            switch (datosC[1])
                            {
                                case "N":
                                    consulta = datosC[0].Replace("DatoBuscar_", pStrDatoBuscar);
                                    break;
                                case "T":
                                    consulta = datosC[0].Replace("DatoBuscar_", "'" + pStrDatoBuscar + "'");
                                    break;
                            }
                        }


                        OdbcConnection connection = new OdbcConnection("DSN=" + odbc + ";uid=" + usuario + ";pwd=" + contrasena);

                        connection.Open();
                        OdbcCommand MyCommand = new OdbcCommand(consulta, connection);
                        OdbcDataReader MyDataReader = MyCommand.ExecuteReader();
                        if (MyDataReader.HasRows)
                        {
                            DataTable dt = new DataTable();
                            dt.Load(MyDataReader);
                            ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), dt, datos[2], pNombre, nombreIndex);

                        }
                        else
                        {
                            ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2], pNombre);
                        }
                        connection.Close();
                        break;
                    }
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

        internal List<DataTable> LlenarListaTablas(int bloques, int cantFilas, DataTable dt, string orientacion, string pNombreControl, string pNombreIndex = "")
        {
            int index = 0;
            bool tieneCampoOrganizar = false;
            if (pNombreIndex.Length > 0)
            {
                index = dt.Columns.IndexOf(pNombreIndex);
                tieneCampoOrganizar = true;
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
                        while (x < cantFilas + 1)
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

        internal void ColorFuenteFondoTabla(string pNombre, string pTag)
        {
            string[] datos = pTag.Split('|');
            DataGrid control = (DataGrid)FindName(pNombre);
            if (datos.Length == 8)
            {
                foreach (var item in control.ItemsSource as IEnumerable)
                {
                    control.ScrollIntoView(item);
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

        internal static void crearDirectorios()
        {
            if (!Directory.Exists(@".\objetosSplash\animaciones"))
            {
                Directory.CreateDirectory(@".\objetosSplash\animaciones");
            }

            //Carpeta de consultas sql Splah
            if (!Directory.Exists(@".\objetosSplash\consultasSQL"))
            {
                Directory.CreateDirectory(@".\objetosSplash\consultasSQL");
            }

            //Carpeta multimedia Splah
            if (!Directory.Exists(@".\objetosSplash\multimedia"))
            {
                Directory.CreateDirectory(@".\objetosSplash\multimedia");
            }

        }

        private void Window_Activated(object sender, EventArgs e)
        {
            try
            {

                DataGrid control = (DataGrid)FindName("TablaDatos");
                if (control != null)
                    ColorFuenteFondoTabla(control.Name, control.Tag.ToString());
            }
            catch { }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Coordenadas.Content = "Tamaño ventana: " + Width.ToString() + "X, " + Height.ToString() + "Y";
        }

        private void Maximizar_Click(object sender, RoutedEventArgs e)
        {
            if (Width == SystemParameters.VirtualScreenWidth && Height == SystemParameters.VirtualScreenHeight)
            {
                Width = anchoAnt;
                Height = altoAnt;
                CenterWindowOnScreen();
            }
            else
            {
                anchoAnt = Width;
                altoAnt = Height;
                Width = SystemParameters.VirtualScreenWidth;
                Height = SystemParameters.VirtualScreenHeight;
                Left = 0;
                Top = 0;
            }

        }

        private void MenuMostrarOcultarCantidadTotal_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("CantidadTotal");

            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar cantidad total";
            }
            else
            {
                Label obj = new Label();
                obj.Name = "CantidadTotal";
                obj.ToolTip = "CantidadTotal";
                obj.Content = "CantidadTotal";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar cantidad total";
            }

        }

        private void MenuMostrarOcultarCantidadIngresada_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("CantidadIngresada");

            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar cantidad ingresada";
            }
            else
            {
                Label obj = new Label();
                obj.Name = "CantidadIngresada";
                obj.ToolTip = "CantidadIngresada";
                obj.Content = "CantidadIngresada";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar cantidad ingresada";
            }
        }

        private void MenuMostrarOcultarCantidadFaltante_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("CantidadFaltante");

            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar cantidad faltante";
            }
            else
            {
                Label obj = new Label();
                obj.Name = "CantidadFaltante";
                obj.ToolTip = "CantidadFaltante";
                obj.Content = "CantidadFaltante";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar cantidad faltante";
            }
        }

        private void MenuMostrarOcultarCambio_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("Cambio");

            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar cantidad cambio";
            }
            else
            {
                Label obj = new Label();
                obj.Name = "Cambio";
                obj.ToolTip = "Cambio";
                obj.Content = "Cambio";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar cantidad cambio";
            }
        }

        private void MenuMostrarOcultarBtnCancelar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Button control = (Button)FindName("BtnCancelar");

            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Eliminar botón cancelar";
            }
            else
            {
                Button obj = new Button();
                obj.Name = "BtnCancelar";
                obj.ToolTip = "BtnCancelar";
                obj.Content = "Cancelar";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.BorderThickness = new Thickness(1);
                obj.MouseDoubleClick += Button_DobleClick;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Agregar botón cancelar";
            }
        }

        private void Button_DobleClick(object sender, RoutedEventArgs e)
        {
            if (esDiseno)
            {
                var item = e.Source as UIElement;
                controlClickName = item.GetValue(NameProperty).ToString();

                mostarPropiedadesObjetos(item.PointToScreen(new Point(((Button)item).ActualWidth, ((Button)item).ActualHeight)));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
            dialog.lblNombre.Content = "¡Advertencia!";
            dialog.lblTexto.Text = "¿Está seguro que desea cancelar el pago?";
            new Recursos().ventanaMensajesGrande800x600(dialog);
            if (dialog.ShowDialog() == true)
            {
                pago.Cambio = pago.CantidadIngresada;
                pago.EstadoPago = Pago.Estado.CANCELADO;
                pago.Pagado = true;
                Button controlBtn = (Button)FindName("BtnCancelar");
                if (controlBtn != null)
                    controlBtn.IsEnabled = false;
            }
        }
    }
}
