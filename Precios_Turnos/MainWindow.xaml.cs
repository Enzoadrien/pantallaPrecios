using Microsoft.Identity.Client.NativeInterop;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using Priceio;
using Priceio.Cajero;
using Priceio.Cajero.Hopper;
using Priceio.Cajero.Payout;
using Priceio.Cajero.VentanasCajero;
using Priceio.ClasesGenericas;
using Priceio.SQLite;
using Priceio.Turnero;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Resources;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using static Priceio.Cajero.ChannelData;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Priceio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string nombreApp = "App_Priceio";
        private Seguridad seguridad = new Seguridad();
        private Licencia licencia = new Licencia();
        private Point _positionInBlock;
        private TranslateTransform? _currentTT;
        private bool esAplicacion = false;
        private bool editar = false;
        private bool animaciones = false;
        private bool maximizado = false;
        private bool activarSplash = false;
        private string? tipoSplash;
        private bool estaSaliendo = false;
        public Color ultimoColorLetra;
        public Color ultimoColorFondo;
        private string? controlClickName;
        internal string? controlSelectedName;
        public Dictionary<long, Dictionary<bool, string>> listaTurnosTeclas = new Dictionary<long, Dictionary<bool, string>>();
        internal static List<StateObject> listaTurnosKretz = new List<StateObject>();
        private SolidColorBrush? ultimoColor;
        internal double ultimaOpacidad;
        private string? datosVerificador;

        private SMARTPayout smartPayout = new SMARTPayout();
        private SMARTHopper smartHopper = new SMARTHopper();

        private DispatcherTimer timer = new DispatcherTimer();
        private bool runLog = false;
        private VentanaLogSmart? ventanaLogSmart;
        private bool seMuestraLogSmart = false;

        public MainWindow()
        {
            InitializeComponent();
            //new TableGenerator().GenenarBD();

            crearDirectorios();
            Coordenadas.Visibility = Visibility.Hidden;
            CargarVistaPrevia();

            MaxHeight = SystemParameters.VirtualScreenHeight;
            MinHeight = Height = MaxHeight / 1.5;

            MaxWidth = SystemParameters.VirtualScreenWidth;
            MinWidth = Width = MaxWidth / 1.5;

            try
            {
                if (seguridad.CheckInternetConnecition())
                    actualizaLlave();
            }
            catch { }
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

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private bool ValidarActivar()
        {
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

                string cadena = seguridad.DecryptString(licencia.Codigo, licencia.Key);
                string[] subs = cadena.Split('|');
                if (subs.Length > 0)
                {
                    string correo = seguridad.DecryptString(licencia.Codigo, licencia.Correo);
                    string codigo = seguridad.EncryptString(correo, seguridad.numeroSerieHD() + "|" +
                                                                   seguridad.numeroSeriePlacaBase() + "|" +
                                                                   MainWindow.nombreApp);

                    if (subs[0].Equals(correo)
                        && subs[3].Equals(seguridad.numeroSerieHD()) && subs[4].Equals(seguridad.numeroSeriePlacaBase())
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
                                    if (seguridad.GetNetworkTime().Date <= Convert.ToDateTime(subs[6]).Date)
                                    {
                                        ActivarControlesMenu();
                                        return true;
                                    }
                                    else
                                    {
                                        dialog.lblTexto.Text = "La licencia ha caducado.";
                                        dialog.ShowDialog();
                                    }
                                }
                                else
                                {
                                    ActivarControlesMenu();
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

            BloquearControlesMenu();
            return false;
        }

        private void actualizaLlave()
        {

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

                string correo = seguridad.DecryptString(licencia.Codigo, licencia.Correo);
                string codigo = seguridad.EncryptString(correo, seguridad.numeroSerieHD() + "|" +
                                                                   seguridad.numeroSeriePlacaBase() + "|" +
                                                                   MainWindow.nombreApp);


                string llave = new ValidarLicencia().recuperaLicenciaApp(correo, codigo);

                if (!licencia.Llave.Equals(llave))
                {
                    new ValidarLicencia().actualizaUltimaConexion(correo, codigo, false);

                    string cadena = seguridad.DecryptString(codigo, llave);

                    string[] subs = cadena.Split('|');
                    if (subs.Length > 1)
                    {
                        string strKey = seguridad.EncryptString(codigo, cadena + '|' + DateTime.Now.Date.AddDays(int.Parse(subs[1])).ToShortDateString());
                        GuardarLicencia(correo, codigo, llave, strKey);

                    }
                    else
                    {
                        GuardarLicencia(correo, codigo, llave, string.Empty);
                    }
                }
                else
                    new ValidarLicencia().actualizaUltimaConexion(correo, codigo, true);

            }
        }

        private void GuardarLicencia(string correo, string codigo, string llave, string pStrKey)
        {
            var Licencia = new Licencia
            {
                Correo = seguridad.EncryptString(codigo, correo),
                Codigo = codigo,
                Llave = llave,
                Key = pStrKey
            };

            string jsonString = JsonSerializer.Serialize(Licencia);

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Lista de precios 3K", true);
            key.SetValue("Key", jsonString);

            using (Stream stream = new FileStream(@".\Llave.key", FileMode.Create))
            {
                stream.SetLength(0);
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

        }

        private void BloquearControlesMenu()
        {
            Conexion.IsEnabled = false;
            ImportarDiseno.IsEnabled = false;
            ExportarDiseno.IsEnabled = false;
            EditarDiseno.IsEnabled = false;
            ResizeMode = ResizeMode.NoResize;
        }

        private void ActivarControlesMenu()
        {
            Conexion.IsEnabled = true;
            ImportarDiseno.IsEnabled = true;
            ExportarDiseno.IsEnabled = true;
            EditarDiseno.IsEnabled = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized && !maximizado && !editar)
            {
                //Topmost = true;
                maximizado = true;
                esAplicacion = true;
                //Principal.IsHitTestVisible = false;
                ModoEdicion.Visibility = Visibility.Hidden;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Visibility = Visibility.Collapsed;
                Menu.Visibility = Visibility.Hidden;
                LogoPrincipal.Opacity = .5;

                //// re-show the window after changing style
                Visibility = Visibility.Visible;
                LimpiarVistaPrevia();
                PausarVideos(true);
                animaciones = true;
                CargarWEB(false);
                CargarAnimaciones();
                Task.Run(() => ComportamientoObjetos());

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                activarSplash = config.AppSettings.Settings["ActivarSplash"].Value.Equals("true") ? true : false;
                tipoSplash = config.AppSettings.Settings["TipoSplash"].Value;
                seMuestraLogSmart = config.AppSettings.Settings["LogPago"].Value.Equals("true") ? true : false;
                if (activarSplash)
                {
                    switch (tipoSplash)
                    {
                        case "T":
                            EscucharTurnos();
                            break;
                        case "V":
                            break;
                        case "C":
                            EscucharPagos();
                            try
                            {
                                if (seMuestraLogSmart)
                                {
                                    timer.Tick += new EventHandler(TimerTickLog);
                                    MostrarLogPagos();
                                }
                            }
                            catch
                            {
                            }
                            break;
                    }
                    
                }
            }

            else if (WindowState == WindowState.Maximized && editar)
            {
                //Topmost = false;
                maximizado = true;
                animaciones = false;
                //Principal.IsHitTestVisible = true;
                ModoEdicion.Visibility = Visibility.Visible;
                Coordenadas.Content = "Tamaño ventana: " + Width.ToString() + "X, " + Height.ToString() + "Y";
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Visibility = Visibility.Collapsed;
                Menu.Visibility = Visibility.Hidden;
                LogoPrincipal.Opacity = .5;

                //// re-show the window after changing style
                Visibility = Visibility.Visible;
                LimpiarVistaPrevia();
                PausarVideos(true);
                CargarWEB(true);

            }
            else if (WindowState != WindowState.Maximized && !editar && maximizado)
            {
                //Topmost = false;
                maximizado = false;
                animaciones = false;
                //Principal.IsHitTestVisible = true;

                Menu.Visibility = Visibility.Visible;
                ResizeMode = ResizeMode.CanResize;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                CenterWindowOnScreen();
                LogoPrincipal.Opacity = 1;

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                activarSplash = config.AppSettings.Settings["ActivarSplash"].Value.Equals("true") ? true : false;
                tipoSplash = config.AppSettings.Settings["TipoSplash"].Value;
                seMuestraLogSmart = config.AppSettings.Settings["LogPago"].Value.Equals("true") ? true : false;
                if (activarSplash)
                {
                    switch (tipoSplash)
                    {
                        case "T":
                            AsynchronousSocketListener.StopListening();
                            break;
                        case "V":
                            break;
                        case "C":
                            DetenerPagos();
                            if (ventanaLogSmart != null)
                                ventanaLogSmart.Close();
                            break;
                    }
                    ConfigurarSplash.Visibility = Visibility.Visible;
                }
                else
                {
                    ConfigurarSplash.Visibility = Visibility.Hidden;
                }



                if (esAplicacion)
                {
                    QuitarAnimaciones();
                    esAplicacion = false;
                }

                CargarVistaPrevia();
                QuitarWEB(true);
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
            else if (maximizado && activarSplash)
            {

                switch (tipoSplash)
                {
                    case "T":
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        if (config.AppSettings.Settings["ProtocoloTurnero"].Value.Equals("T"))
                            Task.Run(() => ProcesarTecla(e));
                        break;
                    case "V":
                        switch (e.Key)
                        {
                            case Key.Enter:
                                string datoEnviar = datosVerificador;
                                Task.Run(() => ProcesarVerificador(datoEnviar));
                                datosVerificador = string.Empty;
                                break;
                            default:
                                datosVerificador += (char)KeyInterop.VirtualKeyFromKey(e.Key);
                                break;
                        }
                        break;
                    case "C":
                        break;

                }
            }

        }

        internal void ProcesarVerificador(string pvStrDatosVerificador)
        {
            Application.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                MostrarVentanaSplash mostrarTurno = new MostrarVentanaSplash(false, this, pvStrDatosVerificador);
                mostrarTurno.ShowInTaskbar = false;
                mostrarTurno.ShowDialog();
            }));
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
                            listaTurnosTeclas.Add(new Random().NextInt64(), new Dictionary<bool, string>() { { false, "00" } });
                            //ProcesarTurnoTeclado(false, "00");
                            break;
                        case Key.Right:
                            listaTurnosTeclas.Add(new Random().NextInt64(), new Dictionary<bool, string>() { { true, "00" } });
                            //ProcesarTurnoTeclado(true, "00");
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
                                var numeroTurnoAnt = FindName("NumeroTurnoAnt") as UIElement;
                                if (numeroTurnoAnt != null)
                                    numeroTurnoAnt.SetValue(ContentProperty, "");
                                var numeroEquipoAnt = FindName("NumeroEquipoAnt") as UIElement;
                                if (numeroEquipoAnt != null)
                                    numeroEquipoAnt.SetValue(ContentProperty, "");
                            }
                            break;
                        case Key.Up:
                            CapturaTexto dialog2 = new CapturaTexto(this);
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
                                listaTurnosTeclas.Add(new Random().NextInt64(), new Dictionary<bool, string>() { { true, "0" + new Recursos().NumericKeys[e.Key] } });
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

        private async Task ProcesarTurno(bool siguiente, string pvSrtrEquipo)
        {
            int turno = new Recursos().CargarNumeroTurno();
            string equipo = pvSrtrEquipo;
            List<string>? turnosAnteriores = new List<string>();

            while (true)
            {
                try
                {
                    using (Stream stream = new FileStream(@".\Recursos\turnoAnt.3k", FileMode.Open))
                    {
                        var sr = new StreamReader(stream);
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            turnosAnteriores.Add(line);
                        }
                        stream.Close();
                        break;
                    }
                }
                catch
                {
                }
            }
            if (turno >= 0)
            {
                if (siguiente)
                    ++turno;
                else
                {
                    if (turno > 1)
                        --turno;
                }
            }

            new Recursos().GuardarNumeroTurno(turno);
            new Recursos().GuardarTurnoAnt(turno, equipo);
            await new Recursos().MostrarTurno(turno, equipo, turnosAnteriores, this);
            MostrarVentanaSplash.synthesizer.SpeakAsyncCancelAll();
            CargarTurnosPrincipal();
        }

        private async void ProcesarListadoTurnosTeclado()
        {
            while (animaciones)
            {
                try
                {
                    if (listaTurnosTeclas.Count > 0)
                    {
                        var first = listaTurnosTeclas.First();
                        await ProcesarTurno(first.Value.First().Key, first.Value.First().Value);
                        listaTurnosTeclas.Remove(first.Key);
                    }
                    await Task.Delay(1000);

                }
                catch (Exception) { }
            }
        }

        private async void ProcesarListadoTurnosKretz()
        {
            while (animaciones)
            {
                try
                {
                    if (listaTurnosKretz.Count > 0)
                    {
                        var first = listaTurnosKretz.First();
                        await new Recursos().MostrarTurno(first.GetTurno().GetNumTurno(), first.GetTurno().GetNumEquipo(), first.GetTurno().GetTurnosAnt(), this);
                        MostrarVentanaSplash.synthesizer.SpeakAsyncCancelAll();
                        CargarTurnosPrincipal();
                        listaTurnosKretz.Remove(first);
                    }
                    await Task.Delay(1000);

                }
                catch (Exception) { }
            }
        }

        public void CargarTurnosPrincipal()
        {
            List<string>? turnosAnteriores = new List<string>();

            while (true)
            {
                try
                {
                    using (Stream stream = new FileStream(@".\Recursos\turnoAnt.3k", FileMode.Open))
                    {
                        var sr = new StreamReader(stream);
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            turnosAnteriores.Add(line);
                        }
                        stream.Close();
                        break;
                    }
                }
                catch
                { }
            }
            if (turnosAnteriores != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var numeroTurnoAnt = FindName("NumeroTurnoAnt") as UIElement;
                    var numeroEquipoAnt = FindName("NumeroEquipoAnt") as UIElement;
                    if (numeroTurnoAnt != null)
                        numeroTurnoAnt.SetValue(ContentProperty, "");
                    if (numeroEquipoAnt != null)
                        numeroEquipoAnt.SetValue(ContentProperty, "");

                    foreach (string text in turnosAnteriores)
                    {
                        string[] anteriores = text.Split('|');
                        if (numeroTurnoAnt != null)
                            numeroTurnoAnt.SetValue(ContentProperty, numeroTurnoAnt.GetValue(ContentProperty) + anteriores[0] + "\n");
                        if (numeroEquipoAnt != null)
                            numeroEquipoAnt.SetValue(ContentProperty, numeroEquipoAnt.GetValue(ContentProperty) + anteriores[1] + "\n");
                    }
                    if (numeroTurnoAnt != null)
                        numeroTurnoAnt.SetValue(ContentProperty, numeroTurnoAnt.GetValue(ContentProperty).ToString().Substring(0, numeroTurnoAnt.GetValue(ContentProperty).ToString().Length - 1));
                    if (numeroEquipoAnt != null)
                        numeroEquipoAnt.SetValue(ContentProperty, numeroEquipoAnt.GetValue(ContentProperty).ToString().Substring(0, numeroEquipoAnt.GetValue(ContentProperty).ToString().Length - 1));

                }));
            }
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditarDiseno_Click(object sender, RoutedEventArgs e)
        {
            if (new ValidarLicencia().validarLicenciaApp(seguridad.DecryptString(licencia.Codigo, licencia.Correo), licencia.Codigo))
            {
                actualizaLlave();
                if (ValidarActivar())
                {
                    editar = true;
                    WindowState = WindowState.Maximized;
                    Coordenadas.Visibility = Visibility.Visible;
                    ModoEdicion.Content = "Modo edición";
                    ModoEdicion.FontSize = 24;
                }
                else
                {
                    Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialogError.lblNombre.Content = "¡Error!";
                    dialogError.lblTexto.Text = "Licencia no válida. La aplicación se desactivo, consulte al administrador.";
                    dialogError.ShowDialog();
                }
            }
            else
            {
                Mensajes dialog3 = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA);
                dialog3.lblNombre.Content = "¡Advertencia!";
                dialog3.lblTexto.Text = "Se requiere de una conexión con el servidor para entrar a modo edición, consulte al administrador.";
                dialog3.ShowDialog();
            }
        }

        private void ImportarDiseno_Click(object sender, RoutedEventArgs e)
        {
            if (new ValidarLicencia().validarLicenciaApp(seguridad.DecryptString(licencia.Codigo, licencia.Correo), licencia.Codigo))
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Archivos de diseño  (.3kzip)|*.3kzip";
                bool? checarOK = openFileDialog.ShowDialog();
                if (checarOK == true)
                {

                    List<string> objEliminar = new List<string>();
                    Mensajes dialog2 = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
                    dialog2.lblNombre.Content = "¡Advertencia!";
                    dialog2.lblTexto.Text = "Se reemplazará el diseño actual, ¿Está seguro que desea continuar?. ¡Esta accion no se puede revertir!";
                    if (dialog2.ShowDialog() == true)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        try
                        {
                            foreach (var itemObjets in Principal.Children)
                            {
                                string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                                if (SeModificaControl(nombreControl))
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
                            }

                            foreach (string ob in objEliminar)
                            {
                                BorarObjeto(ob, false);
                            }


                            using (var archive = ZipFile.Open(openFileDialog.FileName, ZipArchiveMode.Read))
                            {
                                if (Directory.Exists(@".\objetos"))
                                    Directory.Delete(@".\objetos", true);

                                if (Directory.Exists(@".\objetosSplash"))
                                    Directory.Delete(@".\objetosSplash", true);

                                if (File.Exists(@"./Principal.png"))
                                    File.Delete(@"./Principal.png");

                                archive.ExtractToDirectory(@".\");
                            }
                            CargarControles();
                            LimpiarVistaPrevia();
                            CargarVistaPrevia();
                        }
                        catch (Exception ex)
                        {
                            Mensajes dialog3 = new Mensajes(Recursos.TipoMensaje.ERROR, true);
                            dialog3.lblNombre.Content = "¡Error!";
                            dialog3.lblTexto.Text = "Error al importar el diseño: \n" + ex.Message;
                            dialog3.ShowDialog();
                        }
                        Mouse.OverrideCursor = Cursors.Arrow;
                    }
                }
            }
            else
            {
                Mensajes dialog3 = new Mensajes(Recursos.TipoMensaje.ERROR, true);
                dialog3.lblNombre.Content = "¡Error!";
                dialog3.lblTexto.Text = "Error en el servidor, consulte al administrador. ";
                dialog3.ShowDialog();
            }
        }

        private void ExportarDiseno_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = "Diseño 3K (" + MaxWidth + "x" + MaxHeight + ")"; // Default file name
                dlg.DefaultExt = ".zip"; // Default file extension
                dlg.Filter = "Archivos de diseño  (.3kzip)|*.3kzip"; // Filter files by extension

                // Show save file dialog box
                bool? result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    //Carpeta multimedia principal
                    if (Directory.Exists(@".\objetos"))
                    {
                        Directory.CreateDirectory(@".\temp\objetos");
                        CopyDirectory(@".\objetos", @".\temp\objetos", true);
                    }
                    if (Directory.Exists(@".\objetosSplash"))
                    {
                        Directory.CreateDirectory(@".\temp\objetosSplash");
                        CopyDirectory(@".\objetosSplash", @".\temp\objetosSplash", true);
                    }


                    FileInfo fi = new FileInfo(@"./Principal.png");
                    if (fi.Exists)
                        fi.CopyTo(@".\temp\Principal.png", true);

                    if (File.Exists(dlg.FileName))
                        File.Delete(dlg.FileName);


                    ZipFile.CreateFromDirectory(@".\temp", dlg.FileName, CompressionLevel.Fastest, false);

                    if (Directory.Exists(@".\temp"))
                        Directory.Delete(@".\temp", true);
                }
            }
            catch (Exception ex)
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, true);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Error al exportar el diseño: \n" + ex.Message; ;
                dialog.ShowDialog();
                if (Directory.Exists(@".\temp"))
                    Directory.Delete(@".\temp", true);
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private void Principal_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = e.Source as UIElement;

                if (!editar && item.GetType().Name.Equals("DockPanel"))
                {
                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        var container = VisualTreeHelper.GetParent(item) as UIElement;
                        _positionInBlock = e.GetPosition(container);
                        _currentTT = item.RenderTransform as TranslateTransform;
                        item.CaptureMouse();
                    }
                }
                else if (editar)
                {
                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        var container = VisualTreeHelper.GetParent(item) as UIElement;
                        _positionInBlock = e.GetPosition(container);
                        _currentTT = item.RenderTransform as TranslateTransform;
                        item.CaptureMouse();
                    }
                }
            }
            catch (Exception) { }

        }

        private void Principal_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = e.Source as UIElement;

                if (!editar && item.GetType().Name.Equals("DockPanel"))
                {
                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        _currentTT = item.RenderTransform as TranslateTransform;
                        // release this control.
                        item.ReleaseMouseCapture();

                    }
                }
                else if (editar)
                {
                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        _currentTT = item.RenderTransform as TranslateTransform;
                        // release this control.
                        item.ReleaseMouseCapture();

                    }
                }
            }
            catch (Exception) { }

        }

        private void Principal_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var item = e.Source as UIElement;

                if (!editar && item.GetType().Name.Equals("DockPanel"))
                {
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
                else if (editar)
                {
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
            }
            catch (Exception) { }
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
                case "Menu":
                case "LogoPrincipal":
                case "Fondo":
                case "VistaPrevia":
                case "PantallaCompleta":
                    seModifica = false;
                    break;
                default:
                    seModifica = true;
                    break;
            }
            return seModifica;
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
                case "Menu":
                case "LogoPrincipal":
                case "Fondo":
                case "VistaPrevia":
                case "PantallaCompleta":
                    seElimina = false;
                    break;
                default:
                    if (name.Contains("Img_"))
                        seElimina = false;
                    else
                        seElimina = true;
                    break;
            }
            return seElimina;
        }

        private void Principal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = e.Source as UIElement;
                controlClickName = item.GetValue(NameProperty).ToString();

                if (e.ClickCount == 2)
                {
                    if (!editar && item.GetType().Name.Equals("DockPanel"))
                    {
                        DockPanel dockPanel = (DockPanel)FindName(item.GetValue(NameProperty).ToString());
                        foreach (UIElement item2 in dockPanel.Children)
                        {
                            if (item2.Visibility == Visibility.Visible)
                                item2.Visibility = Visibility.Hidden;
                            else
                                item2.Visibility = Visibility.Visible;
                        }
                    }
                    else if (editar)
                    {

                        if (SeModificaControl(controlClickName))
                        {
                            switch (item.GetType().Name.ToString())
                            {
                                case "Label":
                                    item.SetValue(BackgroundProperty, ultimoColor);
                                    break;
                                case "DataGrid":
                                case "MediaElement":
                                case "WebView2":
                                case "Image":
                                    item.SetValue(OpacityProperty, ultimaOpacidad);
                                    break;
                            }

                            mostarPropiedadesObjetos(e.GetPosition(Principal));
                        }
                    }
                }

                if (!SeModificaControl(controlClickName))
                    Coordenadas.Content = "Tamaño ventana: " + Width.ToString() + "X, " + Height.ToString() + "Y";
            }
            catch (Exception) { }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var item = e.Source as UIElement;
            controlClickName = item.GetValue(NameProperty).ToString();

            mostarPropiedadesObjetos(item.PointToScreen(new Point(((Button)item).ActualWidth, ((Button)item).ActualHeight)));
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

                        Label control = (Label)FindName("NumeroTurnoAnt");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[10]).Items[0]).Header = "Eliminar turnos anteriores";
                        else
                            ((MenuItem)((MenuItem)cm.Items[10]).Items[0]).Header = "Agregar turnos anteriores";

                        control = (Label)FindName("NumeroEquipoAnt");
                        if (control != null)
                            ((MenuItem)((MenuItem)cm.Items[10]).Items[1]).Header = "Eliminar equipos anteriores";
                        else
                            ((MenuItem)((MenuItem)cm.Items[10]).Items[1]).Header = "Agregar equipos anteriores";

                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        tipoSplash = config.AppSettings.Settings["TipoSplash"].Value;
                        activarSplash = config.AppSettings.Settings["ActivarSplash"].Value.Equals("true") ? true : false;
                        if (activarSplash)
                        {
                            ((MenuItem)cm.Items[9]).IsEnabled = true;
                            if (tipoSplash.Equals("T"))
                            {
                                ((MenuItem)cm.Items[10]).IsEnabled = true;
                            }
                            else if (tipoSplash.Equals("V"))
                            {
                                ((MenuItem)cm.Items[10]).IsEnabled = false;
                            }
                            else if (tipoSplash.Equals("C"))
                            {
                                ((MenuItem)cm.Items[10]).IsEnabled = false;
                            }
                        }
                        else
                        {
                            ((MenuItem)cm.Items[9]).IsEnabled = false;
                            ((MenuItem)cm.Items[10]).IsEnabled = false;
                        }



                        MenuItem itemCm = (MenuItem)cm.Items[12];
                        itemCm.Items.Clear();
                        foreach (var itemObjets in Principal.Children)
                        {
                            //if ((itemObjets as UIElement).Visibility == Visibility.Visible)
                            //{
                            string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                            if (SeModificaControl(nombreControl))
                            {
                                MenuItem itemControl = new MenuItem();
                                itemControl.Header = nombreControl + "-(" + (itemObjets as UIElement).GetType().Name + ")";
                                itemControl.Tag = nombreControl;
                                itemControl.PreviewMouseLeftButtonDown += MenuListaObjetos_PreviewMouseLeftButtonDown;
                                itemCm.Items.Add(itemControl);
                            }
                            //}
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
                PantallaCompleta.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                PantallaCompleta.Background = null;
                //Video.UpdateLayout();
            }


            Coordenadas.Visibility = Visibility.Hidden;
            ModoEdicion.Visibility = Visibility.Hidden;
            LogoPrincipal.Visibility = Visibility.Hidden;
            if (editar)
            {
                try
                {
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
                SaveFrameworkElementToPng(Principal, 900, 557, @".\Principal.png");
                GuardarControles();
                editar = false;
            }

            WindowState = WindowState.Normal;
            LogoPrincipal.Visibility = Visibility.Visible;
            LogoPrincipal.IsHitTestVisible = false;
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
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.UriSource = new Uri(@"./Principal.png", UriKind.RelativeOrAbsolute);
                image.EndInit();
                myBrush.ImageSource = image;
                myBrush.Stretch = Stretch.Fill;
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
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
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
            dialog.ContenidoTextBox.IsReadOnly = true;
            if (dialog.ShowDialog() == true)
            {
                FileInfo fi = new FileInfo(dialog.ContenidoText);
                string extension = fi.Extension;
                try
                {
                    FileInfo fileImg = new FileInfo(@".\objetos\multimedia\" + fi.Name);
                    if (File.Exists(@".\objetos\multimedia\" + fi.Name) && !fi.FullName.Equals(fileImg.FullName))
                    {
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, "Remplazar", "Mantener");
                        dialogMsg.lblNombre.Content = "¡Advertencia!";
                        dialogMsg.lblTexto.Text = "Ya existe un archivo con el mismo nombre y extension en la aplicación, ¿Desea remplazarlo o mantener la actual?. ¡Esta accion no se puede revertir!";
                        if (dialogMsg.ShowDialog() == true)
                        {
                            fi.CopyTo(@".\objetos\multimedia\" + fi.Name, true);
                            foreach (var itemObjets in Principal.Children)
                            {
                                string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                                if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo"))
                                {
                                    switch (itemObjets.GetType().Name.ToString())
                                    {
                                        case "Image":
                                            try
                                            {
                                                if (((BitmapImage)((Image)itemObjets).Source).UriSource.Equals(@".\objetos\multimedia\" + fi.Name))
                                                {
                                                    BitmapImage bitmapImage = new BitmapImage();
                                                    bitmapImage.BeginInit();
                                                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                                    bitmapImage.UriSource = new Uri(@".\objetos\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                                                    bitmapImage.EndInit();

                                                    ((Image)itemObjets).Source = bitmapImage;
                                                }
                                            }
                                            catch { }
                                            break;
                                        case "MediaElement":
                                            try
                                            {
                                                if (((MediaElement)itemObjets).Source.Equals(@".\objetos\multimedia\" + fi.Name))
                                                {
                                                    Application.Current.Dispatcher.Invoke(new Action(async () =>
                                                    {
                                                        ((MediaElement)itemObjets).Source = null;
                                                        await Task.Delay(100);
                                                        ((MediaElement)itemObjets).Source = new Uri(@".\objetos\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                                                    }));
                                                }
                                            }
                                            catch { }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    else
                        fi.CopyTo(@".\objetos\multimedia\" + fi.Name, true);
                }
                catch
                {
                }

                if (extension.Equals(".gif"))
                {
                    MediaElement obj = new MediaElement();
                    obj.Name = dialog.NombreText.ToUpper();
                    obj.ToolTip = dialog.NombreText.ToUpper();
                    obj.Source = new Uri(@".\objetos\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
                    bitmapImage.UriSource = new Uri(@".\objetos\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
                    bitmapImage.UriSource = new Uri(@".\objetos\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                    bitmapImage.EndInit();

                    obj.Source = bitmapImage;
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.Stretch = Stretch.Uniform;
                    obj.MaxHeight = MaxHeight;
                    obj.MaxWidth = MaxWidth;
                    obj.Height = bitmapImage.Height;
                    obj.Tag = "";
                    obj.MouseLeave += objetoMedia_MouseLeave;
                    obj.MouseEnter += objetoMedia_MouseEnter;
                    NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                    Principal.Children.Add(obj);

                }
            }
        }

        private void MenuAgregarReloj_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            dialog.Titulo.Content = "Agregar hora";
            dialog.ContenidoText = DateTime.Now.ToLongTimeString();
            dialog.ContenidoTextBox.IsReadOnly = true;
            if (dialog.ShowDialog() == true)
            {
                Button obj = new Button();
                obj.Name = dialog.NombreText.ToUpper();
                obj.ToolTip = dialog.NombreText.ToUpper();
                obj.Content = dialog.ContenidoText;
                obj.Tag = "R|L";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.BorderThickness = new Thickness(0);
                obj.Background = new SolidColorBrush(Colors.Transparent);
                obj.MouseDoubleClick += Button_Click;

                DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
                timer.Interval = TimeSpan.FromMilliseconds(1);
                timer.IsEnabled = true;
                timer.Tick += (s, e) =>
                {
                    UpdateTime(obj);
                };

                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
            }
        }

        private void MenuAgregarFecha_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            dialog.Titulo.Content = "Agregar fecha";
            dialog.ContenidoText = DateTime.Now.ToLongDateString();
            dialog.ContenidoTextBox.IsReadOnly = true;
            if (dialog.ShowDialog() == true)
            {
                Button obj = new Button();
                obj.Name = dialog.NombreText.ToUpper();
                obj.ToolTip = dialog.NombreText.ToUpper();
                obj.Content = dialog.ContenidoText;
                obj.Tag = "F|L";
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.BorderThickness = new Thickness(0);
                obj.Background = new SolidColorBrush(Colors.Transparent);
                obj.MouseDoubleClick += Button_Click;

                DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.IsEnabled = true;
                timer.Tick += (s, e) =>
                {
                    UpdateTime(obj);
                };

                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
            }
        }

        private void UpdateTime(Button reloj)
        {
            try
            {
                string[] datos = reloj.Tag.ToString().Split('|');
                if (datos[0].Equals("R"))
                    if (datos[1].Equals("L"))
                        reloj.Content = DateTime.Now.ToLongTimeString();
                    else
                        reloj.Content = DateTime.Now.ToShortTimeString();

                else
                    if (datos[1].Equals("L"))
                    reloj.Content = DateTime.Now.ToLongDateString();
                else
                    reloj.Content = DateTime.Now.ToShortDateString();
            }
            catch { }
        }

        private void MenuAgregarWEB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            dialog.Titulo.Content = "Agregar web";
            dialog.ContenidoText = "https://www.google.com.mx/";
            dialog.btnAbrir.Visibility = Visibility.Hidden;
            if (dialog.ShowDialog() == true)
            {
                WebView2 web = new WebView2();
                try
                {
                    web.Source = new Uri(dialog.ContenidoText);
                    web.Margin = new Thickness(0, 20, 0, 0);
                    web.Tag = dialog.ContenidoText;

                    DockPanel obj = new DockPanel();
                    obj.Name = dialog.NombreText.ToUpper();
                    obj.ToolTip = dialog.NombreText.ToUpper();
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.MaxHeight = MaxHeight;
                    obj.MaxWidth = MaxWidth;
                    obj.Height = 400;
                    obj.Width = 600;
                    obj.Tag = "";
                    obj.Background = new SolidColorBrush(Colors.Transparent);

                    obj.Children.Add(web);

                    obj.MouseLeave += objeto_MouseLeave;
                    obj.MouseEnter += objeto_MouseEnter;

                    NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                    Principal.Children.Add(obj);
                }
                catch
                {
                    Mensajes dialogE = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialogE.lblNombre.Content = "¡Error!";
                    dialogE.lblTexto.Text = "La url no es válida, no se puede agregar el objeto web.";
                    dialogE.ShowDialog();
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
            if (editar)
                try
                {
                    mostarPropiedadesObjetos(e.GetPosition(Principal));
                }
                catch (Exception) { }
        }

        private void MenuMostrarOcultarTurnoAnt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroTurnoAnt");
            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Agregar turno anterior";
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
                obj.HorizontalContentAlignment = HorizontalAlignment.Center;
                obj.VerticalContentAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);

                itemCm.Header = "Eliminar turno anterior";
            }
        }

        private void MenuMostrarOcultarEquipoAnt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuItem itemCm = (MenuItem)sender;
            Label control = (Label)FindName("NumeroEquipoAnt");
            if (control != null)
            {
                Principal.Children.Remove(control);
                NameScope.GetNameScope(this).UnregisterName(control.Name);
                itemCm.Header = "Agregar equipo anterior";
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
                obj.HorizontalContentAlignment = HorizontalAlignment.Center;
                obj.VerticalContentAlignment = VerticalAlignment.Center;
                obj.FontSize = 24;
                obj.FontFamily = new FontFamily("Arial");
                obj.MouseLeave += objeto_MouseLeave;
                obj.MouseEnter += objeto_MouseEnter;
                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);
                itemCm.Header = "Eliminar equipo anterior";
            }
        }

        private void mostarPropiedadesObjetos(Point point)
        {
            var item = FindName(controlClickName) as UIElement;
            Point pointItem = item.TransformToAncestor(this).Transform(new Point(0, 0));

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
                    propiedadesLabel.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();
                    propiedadesLabel.TipoControl.Text = item.GetType().Name;
                    propiedadesLabel.Contenido.Text = ((Label)item).Content.ToString();
                    propiedadesLabel.cbxFuente.SelectedItem = item.GetValue(FontFamilyProperty);
                    propiedadesLabel.cbxTamano.SelectedValue = item.GetValue(FontSizeProperty);
                    propiedadesLabel.chkNegrita.IsChecked = item.GetValue(FontWeightProperty).ToString().CompareTo("Bold") == 0 ? true : false;
                    propiedadesLabel.chkCursiva.IsChecked = item.GetValue(FontStyleProperty).ToString().CompareTo("Italic") == 0 ? true : false;
                    propiedadesLabel.btnColorFuente.Fill = new SolidColorBrush((((Label)item).Foreground as SolidColorBrush).Color);
                    propiedadesLabel.btnColorFondo.Fill = new SolidColorBrush((((Label)item).Background as SolidColorBrush).Color);
                    propiedadesLabel.Opacidad.Value = item.Opacity;

                    propiedadesLabel.CoordenadaX.Text = Math.Round(pointItem.X).ToString();
                    propiedadesLabel.CoordenadaY.Text = Math.Round(pointItem.Y).ToString();

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
                    propiedadesImagen.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();
                    propiedadesImagen.TipoControl.Text = item.GetType().Name;
                    propiedadesImagen.Ruta.Text = Path.GetFileName(((Image)item).Source.ToString());
                    propiedadesImagen.Ruta.ToolTip = Path.GetFileName(((Image)item).Source.ToString());

                    propiedadesImagen.Alto.Text = Math.Round(((Image)item).ActualHeight).ToString();
                    propiedadesImagen.Ancho.Text = Math.Round(((Image)item).ActualWidth).ToString();
                    propiedadesImagen.Opacidad.Value = item.Opacity;

                    propiedadesImagen.CoordenadaX.Text = Math.Round(pointItem.X).ToString();
                    propiedadesImagen.CoordenadaY.Text = Math.Round(pointItem.Y).ToString();

                    propiedadesImagen.chkSonido.IsEnabled = false;
                    propiedadesImagen.Cada.IsEnabled = false;
                    propiedadesImagen.Durar.IsEnabled = false;
                    propiedadesImagen.chkMaximizar.IsEnabled = false;
                    propiedadesImagen.chkRelacion.IsChecked = true;
                    propiedadesImagen.esInicio = false;
                    propiedadesImagen.ShowDialog();
                    break;
                case "MediaElement":
                    PropiedadesMultimedia propiedadesMultimedia = new PropiedadesMultimedia(this);
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
                    propiedadesMultimedia.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();
                    propiedadesMultimedia.TipoControl.Text = item.GetType().Name;
                    propiedadesMultimedia.Ruta.Text = Path.GetFileName(((MediaElement)item).Source.ToString());
                    propiedadesMultimedia.Ruta.ToolTip = Path.GetFileName(((MediaElement)item).Source.ToString());

                    string extension = Path.GetExtension(((MediaElement)item).Source.ToString()).Replace(".", "").ToLower();
                    if (extension.Equals("wav") || extension.Equals("mp3"))
                    {
                        propiedadesMultimedia.chkRelacion.IsEnabled = false;
                        propiedadesMultimedia.Alto.IsEnabled = false;
                        propiedadesMultimedia.Ancho.IsEnabled = false;
                        propiedadesMultimedia.Opacidad.IsEnabled = false;
                        propiedadesMultimedia.chkSonido.IsEnabled = false;
                        propiedadesMultimedia.chkMaximizar.IsEnabled = false;
                        propiedadesMultimedia.chkOcultar.IsEnabled = false;
                    }
                    else if (extension.Equals("gif"))
                    {
                        propiedadesMultimedia.chkSonido.IsEnabled = false;
                        propiedadesMultimedia.chkMaximizar.IsEnabled = false;
                    }


                    propiedadesMultimedia.chkRelacion.IsChecked = true;
                    propiedadesMultimedia.Alto.Text = Convert.ToInt32(((MediaElement)item).ActualHeight).ToString();
                    propiedadesMultimedia.Ancho.Text = Convert.ToInt32(((MediaElement)item).ActualWidth).ToString();
                    propiedadesMultimedia.Opacidad.Value = item.Opacity;

                    propiedadesMultimedia.chkSonido.IsChecked = ((MediaElement)item).Volume == .5 ? true : false;

                    propiedadesMultimedia.CoordenadaX.Text = Math.Round(pointItem.X).ToString();
                    propiedadesMultimedia.CoordenadaY.Text = Math.Round(pointItem.Y).ToString();

                    string[] datosTag = ((MediaElement)item).Tag.ToString().Split('|');
                    if (datosTag.Length > 2)
                    {
                        propiedadesMultimedia.chkMaximizar.IsChecked = true;
                        propiedadesMultimedia.Cada.Text = datosTag[2];
                        propiedadesMultimedia.Durar.Text = datosTag[3];
                    }
                    else
                    {
                        propiedadesMultimedia.chkMaximizar.IsChecked = false;
                        propiedadesMultimedia.Cada.IsEnabled = false;
                        propiedadesMultimedia.Durar.IsEnabled = false;
                    }
                    propiedadesMultimedia.chkOcultar.IsChecked = ((MediaElement)item).Visibility == Visibility.Hidden;

                    propiedadesMultimedia.esInicio = false;
                    propiedadesMultimedia.ShowDialog();
                    break;
                case "DockPanel":
                    PropiedadesWEB propiedadesWebView2 = new PropiedadesWEB(this);
                    propiedadesWebView2.WindowStartupLocation = WindowStartupLocation.Manual;

                    if (point.X + propiedadesWebView2.Width >= MaxWidth)
                        propiedadesWebView2.Left = point.X - propiedadesWebView2.Width;
                    else
                        propiedadesWebView2.Left = point.X;

                    if (point.Y + propiedadesWebView2.Height >= MaxHeight)
                        propiedadesWebView2.Top = point.Y - propiedadesWebView2.Height;
                    else
                        propiedadesWebView2.Top = point.Y;

                    propiedadesWebView2.Titulo.Content = "Propiedades \"" + item.GetValue(NameProperty).ToString() + "\"";
                    propiedadesWebView2.NombreControl.Text = item.GetValue(NameProperty).ToString();
                    propiedadesWebView2.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();
                    propiedadesWebView2.TipoControl.Text = "WebView2";


                    propiedadesWebView2.Alto.Text = Math.Round(((DockPanel)item).ActualHeight).ToString();
                    propiedadesWebView2.Ancho.Text = Math.Round(((DockPanel)item).ActualWidth).ToString();

                    propiedadesWebView2.CoordenadaX.Text = Math.Round(pointItem.X).ToString();
                    propiedadesWebView2.CoordenadaY.Text = Math.Round(pointItem.Y).ToString();

                    propiedadesWebView2.esInicio = false;
                    propiedadesWebView2.ShowDialog();
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
                    propiedadesTabla.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();
                    propiedadesTabla.TipoControl.Text = item.GetType().Name;

                    propiedadesTabla.cbxFuente.SelectedItem = item.GetValue(FontFamilyProperty);
                    propiedadesTabla.cbxTamano.SelectedValue = item.GetValue(FontSizeProperty);
                    propiedadesTabla.chkNegrita.IsChecked = item.GetValue(FontWeightProperty).ToString().CompareTo("Bold") == 0 ? true : false;
                    propiedadesTabla.chkCursiva.IsChecked = item.GetValue(FontStyleProperty).ToString().CompareTo("Italic") == 0 ? true : false;
                    propiedadesTabla.cbxCRegistros.SelectedValue = ((DataGrid)item).Items.Count * 2;
                    propiedadesTabla.chkLineas.IsChecked = ((DataGrid)item).GridLinesVisibility == DataGridGridLinesVisibility.All ? true : false;

                    var itemImg = FindName("Img_" + item.GetValue(NameProperty).ToString()) as UIElement;
                    if (itemImg != null)
                    {
                        propiedadesTabla.cbxCBloques.IsEnabled = false;
                        propiedadesTabla.cbxCRegistros.IsEnabled = false;
                    }

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

                    propiedadesTabla.CoordenadaX.Text = Math.Round(pointItem.X).ToString();
                    propiedadesTabla.CoordenadaY.Text = Math.Round(pointItem.Y).ToString();

                    propiedadesTabla.ShowDialog();
                    break;
                case "Button":
                    PropiedadesReloj propiedadesReloj = new PropiedadesReloj(this);
                    propiedadesReloj.WindowStartupLocation = WindowStartupLocation.Manual;

                    if (point.X + propiedadesReloj.Width >= MaxWidth)
                        propiedadesReloj.Left = point.X - propiedadesReloj.Width;
                    else
                        propiedadesReloj.Left = point.X;

                    if (point.Y + propiedadesReloj.Height >= MaxHeight)
                        propiedadesReloj.Top = point.Y - propiedadesReloj.Height;
                    else
                        propiedadesReloj.Top = point.Y;

                    propiedadesReloj.NombreControl.Text = item.GetValue(NameProperty).ToString();
                    propiedadesReloj.NombreControl.ToolTip = item.GetValue(NameProperty).ToString();

                    string[] datos2 = ((Button)item).Tag.ToString().Split('|');
                    if (datos2[0].Equals("R"))
                    {
                        propiedadesReloj.TipoControl.Text = "Hora";
                        propiedadesReloj.Titulo.Content = "Propiedades hora";
                    }
                    else
                    {
                        propiedadesReloj.TipoControl.Text = "Fecha";
                        propiedadesReloj.Titulo.Content = "Propiedades fecha";
                    }


                    propiedadesReloj.cbxFormato.SelectedValue = datos2[1];
                    propiedadesReloj.cbxFuente.SelectedItem = item.GetValue(FontFamilyProperty);
                    propiedadesReloj.cbxTamano.SelectedValue = item.GetValue(FontSizeProperty);
                    propiedadesReloj.chkNegrita.IsChecked = item.GetValue(FontWeightProperty).ToString().CompareTo("Bold") == 0 ? true : false;
                    propiedadesReloj.chkCursiva.IsChecked = item.GetValue(FontStyleProperty).ToString().CompareTo("Italic") == 0 ? true : false;
                    propiedadesReloj.btnColorFuente.Fill = new SolidColorBrush((((Button)item).Foreground as SolidColorBrush).Color);
                    propiedadesReloj.btnColorFondo.Fill = new SolidColorBrush((((Button)item).Background as SolidColorBrush).Color);
                    propiedadesReloj.Opacidad.Value = item.Opacity;

                    propiedadesReloj.CoordenadaX.Text = Math.Round(pointItem.X).ToString();
                    propiedadesReloj.CoordenadaY.Text = Math.Round(pointItem.Y).ToString();

                    propiedadesReloj.ShowDialog();
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
                        DirectoryInfo info = new DirectoryInfo(@"objetos\");


                        foreach (var file in info.GetFiles())
                        {
                            string[] nombre = file.Name.Split('-');
                            if (nombre[1].Equals(pNombre + ".xaml"))
                                File.Delete(file.FullName);
                        }

                        info = new DirectoryInfo(@"objetos\animaciones");


                        foreach (var file in info.GetFiles())
                        {
                            if (file.Name.Equals(pNombre + ".anim"))
                                File.Delete(file.FullName);
                        }

                        info = new DirectoryInfo(@"objetos\consultasSQL");


                        foreach (var file in info.GetFiles())
                        {
                            if (file.Name.Equals(pNombre + ".sql"))
                                File.Delete(file.FullName);
                        }

                        switch (item.GetType().Name.ToString())
                        {
                            case "DataGrid":
                                var itemImg = FindName("Img_" + pNombre) as UIElement;

                                if (itemImg != null)
                                {
                                    Principal.Children.Remove(itemImg);
                                    NameScope.GetNameScope(this).UnregisterName("Img_" + pNombre);
                                    if (Directory.Exists(@".\objetos\" + pNombre))
                                    {
                                        Directory.Delete(@".\objetos\" + pNombre, true);
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
            dialog.ContenidoTextBox.IsReadOnly = true;
            dialog.esVideo = true;
            if (dialog.ShowDialog() == true)
            {
                FileInfo fi = new FileInfo(dialog.ContenidoText);
                try
                {
                    FileInfo fileVid = new FileInfo(@".\objetos\multimedia\" + fi.Name);
                    if (File.Exists(@".\objetos\multimedia\" + fi.Name) && !fi.FullName.Equals(fileVid.FullName))
                    {
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, "Remplazar", "Mantener");
                        dialogMsg.lblNombre.Content = "¡Advertencia!";
                        dialogMsg.lblTexto.Text = "Ya existe un archivo con el mismo nombre y extension en la aplicación, ¿Desea remplazarlo o mantener la actual?. ¡Esta accion no se puede revertir!";
                        if (dialogMsg.ShowDialog() == true)
                        {
                            fi.CopyTo(@".\objetos\multimedia\" + fi.Name, true);
                            foreach (var itemObjets in Principal.Children)
                            {
                                string nombreControl = (itemObjets as UIElement).GetValue(NameProperty).ToString();
                                if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo"))
                                {
                                    switch (itemObjets.GetType().Name.ToString())
                                    {

                                        case "MediaElement":
                                            if (((MediaElement)itemObjets).Source.Equals(@".\objetos\multimedia\" + fi.Name))
                                            {
                                                Application.Current.Dispatcher.Invoke(new Action(async () =>
                                                {
                                                    ((MediaElement)itemObjets).Stop();
                                                    ((MediaElement)itemObjets).Source = null;
                                                    await Task.Delay(100);
                                                    ((MediaElement)itemObjets).Source = new Uri(@".\objetos\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
                        fi.CopyTo(@".\objetos\multimedia\" + fi.Name, true);
                }
                catch
                {
                }
                MediaElement obj = new MediaElement();
                obj.Name = dialog.NombreText.ToUpper();
                obj.ToolTip = dialog.NombreText.ToUpper();
                obj.Source = new Uri(@".\objetos\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
                obj.LoadedBehavior = MediaState.Play;
                obj.MediaEnded += MediaElement_MediaEnded;
                obj.HorizontalAlignment = HorizontalAlignment.Center;
                obj.VerticalAlignment = VerticalAlignment.Center;
                obj.Stretch = Stretch.Uniform;
                obj.MaxHeight = MaxHeight;
                obj.MaxWidth = MaxWidth;
                obj.Width = MaxWidth / 3;
                obj.Height = MaxHeight / 3;
                obj.Volume = .5;
                obj.Tag = "";
                obj.MouseLeave += objetoMedia_MouseLeave;
                obj.MouseEnter += objetoMedia_MouseEnter;

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
                obj.ItemsSource = CargarListaTablas(dialog.NombreText, obj.Tag.ToString(), "")[0].DefaultView;
                obj.MouseLeave += objetoMedia_MouseLeave;
                obj.MouseEnter += objetoMedia_MouseEnter;

                NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                Principal.Children.Add(obj);

            }
        }

        public List<DataTable> LlenarListaTablas(int bloques, int cantFilas, DataTable dt, string orientacion, string pNombreControl, string pCampoImagen, string pNombreIndex = "")
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
            {
                if (pCampoImagen.Length > 0)
                    dtFinal.Columns.Add(dt.Columns[z].ColumnName);
                else
                    dtFinal.Columns.Add();
            }


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
                            ListTablas.Add(PivotTable(dtFinal, pCampoImagen));
                        }
                        else
                        {

                            ListTablas.Add(dtFinal);
                        }
                        dtFinal = new DataTable();
                        for (int z = 0; z < (dt.Columns.Count * bloques) + (bloques - 1); z++)
                        {
                            if (pCampoImagen.Length > 0)
                                dtFinal.Columns.Add(dt.Columns[z].ColumnName);
                            else
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

        private DataTable PivotTable(DataTable origTable, string pCampoImagen)
        {

            DataTable newTable = new DataTable();
            DataRow dr = null;

            //Add Columns to new Table
            for (int i = 0; i < origTable.Rows.Count; i++)
            {

                if (pCampoImagen.Length > 0)
                    newTable.Columns.Add(origTable.Rows[0][pCampoImagen].ToString());
                else
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
            mostarPropiedadesObjetos(e.GetPosition(Principal));
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
                    if (SeModificaControl(nombreControl) || nombreControl.Equals("Fondo") || nombreControl.Equals("Base"))
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
                            ((DataGrid)itemObjets).ItemsSource = CargarListaTablas(nombreControl, ((DataGrid)itemObjets).Tag.ToString(), "")[0].DefaultView;
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
                foreach (var file in info.GetFiles().OrderBy(x => int.Parse(x.Name.Substring(0, x.Name.IndexOf('-')))).ToArray())
                {
                    try
                    {
                        StreamReader sR = new StreamReader(@file.FullName);
                        string text = sR.ReadToEnd();
                        sR.Close();
                        Seguridad vSeguridad = new Seguridad();
                        StringReader stringReader = new StringReader(vSeguridad.DecryptString(nombreApp, text));
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
                        else
                        {
                            NameScope.GetNameScope(this).RegisterName(item.GetValue(NameProperty).ToString(), item);
                            Principal.Children.Add(item);

                            switch (item.GetType().Name.ToString())
                            {
                                case "Label":
                                    item.MouseLeave += objeto_MouseLeave;
                                    item.MouseEnter += objeto_MouseEnter;
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
                                    List<DataTable> list = CargarListaTablas(control.Name, control.Tag.ToString(), "");
                                    if (!list[0].Rows[0][0].ToString().Equals("Sin datos"))
                                        control.ItemsSource = list[0].DefaultView;

                                    //control.UpdateLayout();
                                    //ColorFuenteFondoTabla(control.Name, control.Tag.ToString());
                                    item.MouseLeave += objetoMedia_MouseLeave;
                                    item.MouseEnter += objetoMedia_MouseEnter;

                                    break;
                                case "MediaElement":
                                    MediaElement media = (MediaElement)FindName(item.GetValue(NameProperty).ToString());
                                    media.MediaEnded += MediaElement_MediaEnded;
                                    item.MouseLeave += objetoMedia_MouseLeave;
                                    item.MouseEnter += objetoMedia_MouseEnter;
                                    break;
                                case "DockPanel":
                                    item.MouseLeave += objeto_MouseLeave;
                                    item.MouseEnter += objeto_MouseEnter;
                                    break;
                                case "Image":
                                    item.MouseLeave += objetoMedia_MouseLeave;
                                    item.MouseEnter += objetoMedia_MouseEnter;
                                    break;
                                case "Button":
                                    Button reloj = (Button)FindName(item.GetValue(NameProperty).ToString());
                                    reloj.MouseDoubleClick += Button_Click;
                                    DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
                                    timer.Interval = TimeSpan.FromSeconds(1);
                                    timer.IsEnabled = true;
                                    timer.Tick += (s, e) =>
                                    {
                                        UpdateTime(reloj);
                                    };
                                    break;
                            }

                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
        }

        public List<DataTable> CargarListaTablas(string pNombre, string pTag, string pCampoImagen, bool pMostrarMensaje = false)
        {
            string[] datos = pTag.Split('|');
            List<DataTable> ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2], pNombre, pCampoImagen);
            try
            {
                Seguridad vSeguridad = new Seguridad();
                //Create the object
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string odbc = config.AppSettings.Settings["ODBC"].Value;
                string usuario = config.AppSettings.Settings["UsuarioODBC"].Value;
                string contrasena = vSeguridad.DecryptString(nombreApp, config.AppSettings.Settings["ContrasenaODBC"].Value);
                string consulta = string.Empty;
                string nombreIndex = string.Empty;

                DirectoryInfo info = new DirectoryInfo(@".\objetos\consultasSQL");

                foreach (var file in info.GetFiles())
                {
                    if (@file.Name.Equals(pNombre + ".sql"))
                    {
                        StreamReader sR = new StreamReader(@file.FullName);
                        string lectura = sR.ReadToEnd();
                        sR.Close();
                        string[] datosC = vSeguridad.DecryptString(nombreApp, lectura).Split('|');
                        consulta = datosC[0];
                        if (datosC.Length > 3)
                            nombreIndex = datosC[3];

                        OdbcConnection connection = new OdbcConnection("DSN=" + odbc + ";uid=" + usuario + ";pwd=" + contrasena);

                        connection.Open();
                        OdbcCommand MyCommand = new OdbcCommand(consulta, connection);
                        OdbcDataReader MyDataReader = MyCommand.ExecuteReader();
                        if (MyDataReader.HasRows)
                        {
                            DataTable dt = new DataTable();
                            dt.Load(MyDataReader);
                            ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), dt, datos[2], pNombre, datosC[2], nombreIndex);

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
                ListaTablas = LlenarListaTablas(int.Parse(datos[0]), int.Parse(datos[1]), new DataTable(), datos[2], pNombre, "");
            }
            return ListaTablas;
        }

        public void ColorFuenteFondoTabla(string pNombre, string pTag)
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
                    {
                        var array2 = (((DataRowView)row.Item).Row).ItemArray;

                        if (!array2.All(i => i is DBNull))
                        {
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

        private async void iniciarComportamientoTabla(string pNombre, string pTag)
        {
            try
            {
                string[] datos = pTag.Split('|');
                int x = 0;
                int tamano;
                while (animaciones && maximizado && !editar)
                {
                    List<DataTable> LisTablas = CargarListaTablas(pNombre, pTag, "");
                    tamano = LisTablas.Count;
                    if (x < tamano)
                        CambiarContenidoTabla(pNombre, pTag, LisTablas, x++);
                    else
                    {
                        x = 0;
                        CambiarContenidoTabla(pNombre, pTag, LisTablas, x++);
                    }
                    await Task.Delay(int.Parse(datos[3]) * 1000);
                }

            }
            catch { }
        }

        private async void iniciarVideoFullScream(string pNombre, string pTag)
        {
            try
            {
                string[] datos = pTag.Split('|');
                if (datos.Length > 2)
                    while (animaciones && maximizado && !editar)
                    {
                        await Task.Delay(int.Parse(datos[2]) * 1000);
                        if (animaciones && maximizado && !editar)
                        {
                            await Task.Delay(MostrarVideoFullScream(pNombre, pTag));
                            EliminarVideoFullScream(pNombre);
                        }
                    }
            }
            catch { }
        }

        private void ComportamientoObjetos()
        {
            try
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
                    Focus();
                }));
            }
            catch { }
        }

        private void EscucharTurnos()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string Protocolo = config.AppSettings.Settings["ProtocoloTurnero"].Value;
                switch (Protocolo)
                {
                    case "T":
                        Task.Run(() => ProcesarListadoTurnosTeclado());
                        break;
                    case "K":
                        Task.Run(() => AsynchronousSocketListener.StartListening());
                        Task.Run(() => ProcesarListadoTurnosKretz());
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        private void EscucharPagos()
        {
            try
            {

                Task.Run(() => smartHopper.RunHopper());
                Task.Run(() => smartPayout.RunPayout());
                Task.Run(() => AsynchronousSocketListenerCajero.StartListening(smartPayout,smartHopper));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR");
            }
        }

        private void DetenerPagos()
        {
            try
            {

                smartPayout.detenerPayout();
                smartHopper.detenerHopper();
                AsynchronousSocketListenerCajero.StopListening();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "ERROR");
            }
        }

        private void MostrarLogPagos()
        {
            ventanaLogSmart = new VentanaLogSmart();
            ventanaLogSmart.Show();
            Task.Run(() => recargarlog(ventanaLogSmart));
        }

        internal async Task recargarlog(VentanaLogSmart dialog)
        {
            runLog = true;
            while (runLog)
            {
                dialog.recargarlog(smartHopper.logPagoHopper, smartPayout.logPagoPayout);

                timer.Start();
                while (timer.IsEnabled)
                {
                    await Task.Delay(1); // Yield to free up CPU
                }
            }
        }
        private void TimerTickLog(object sender, EventArgs e)
        {
            timer.Stop();
        }

        public void CambiarContenidoTabla(string pNombre, string pTag, List<DataTable> pLisTablas, int x)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                DataGrid control = (DataGrid)FindName(pNombre);
                if (!pLisTablas[x].Rows[0][0].ToString().Equals("Sin datos"))
                    control.ItemsSource = pLisTablas[x].DefaultView;
                else
                    control.ItemsSource = null;


                DirectoryInfo info = new DirectoryInfo(@".\objetos\consultasSQL");

                foreach (var file in info.GetFiles())
                {
                    if (@file.Name.Equals(pNombre + ".sql"))
                    {
                        StreamReader sR = new StreamReader(@file.FullName);
                        string lectura = sR.ReadToEnd();
                        sR.Close();
                        string[] datos = new Seguridad().DecryptString(nombreApp, lectura).Split('|');
                        if (datos.Length > 4)
                        {
                            Label item = (Label)FindName(datos[4]);
                            if (item != null)
                                item.Content = pLisTablas[x].TableName;
                        }

                        string pNombreImg = "Img_" + pNombre;
                        var item2 = FindName(pNombreImg) as UIElement;

                        if (item2 != null)
                        {
                            try
                            {
                                string[] datosTag = pTag.ToString().Split('|');

                                if (datosTag[2].Equals("V"))
                                {
                                    String imagenBuscar = pLisTablas[x].Columns[0].ColumnName;
                                    BitmapImage bitmapImage = new BitmapImage();
                                    bitmapImage.BeginInit();
                                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmapImage.UriSource = new Uri(@".\Objetos\" + pNombre + "\\" + imagenBuscar + ".png", UriKind.RelativeOrAbsolute);
                                    bitmapImage.EndInit();

                                    ((Image)item2).Source = bitmapImage;
                                }
                                else
                                {

                                    String imagenBuscar = pLisTablas[x].Rows[0][datos[2]].ToString();
                                    BitmapImage bitmapImage = new BitmapImage();
                                    bitmapImage.BeginInit();
                                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmapImage.UriSource = new Uri(@".\Objetos\" + pNombre + "\\" + imagenBuscar + ".png", UriKind.RelativeOrAbsolute);
                                    bitmapImage.EndInit();

                                    ((Image)item2).Source = bitmapImage;
                                }
                            }
                            catch
                            {
                                ((Image)item2).Source = null;
                            }
                        }


                        break;
                    }
                }
                foreach (DataGridColumn column in control.Columns)
                    column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);

                ColorFuenteFondoTabla(pNombre, pTag);
            }));

            }
            catch { }
        }

        public int MostrarVideoFullScream(string pNombre, string pTag)
        {
            int duracion = 0;
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {

                    SilenciarVideos(true);
                    MediaElement control = (MediaElement)FindName(pNombre);
                    string[] datos = pTag.Split('|');
                    MediaElement obj = new MediaElement();
                    obj.Name = "FullScreamVideo";
                    obj.Source = new Uri(@control.Source.ToString(), UriKind.RelativeOrAbsolute);
                    obj.Position = control.Position;
                    obj.LoadedBehavior = MediaState.Play;
                    obj.MediaEnded += MediaElement_MediaEnded;
                    obj.HorizontalAlignment = HorizontalAlignment.Center;
                    obj.VerticalAlignment = VerticalAlignment.Center;
                    obj.Stretch = Stretch.Uniform;
                    obj.Height = this.ActualHeight;
                    obj.Volume = .5;

                    if (int.Parse(datos[3]) != 0)
                        duracion = int.Parse(datos[3]) * 1000;

                    MediaElement videoFull = (MediaElement)FindName("FullScreamVideo");
                    if (videoFull != null)
                    {
                        PantallaCompleta.Children.Remove(videoFull);
                        NameScope.GetNameScope(this).UnregisterName(videoFull.Name);
                        //Video.UpdateLayout();
                    }
                    PantallaCompleta.Background = new SolidColorBrush(Colors.Black);
                    NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
                    PantallaCompleta.Children.Add(obj);


                }));
            }
            catch { }
            return duracion;
        }

        public void EliminarVideoFullScream(string pNombre)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                MediaElement control = (MediaElement)FindName(pNombre);
                MediaElement video = (MediaElement)FindName("FullScreamVideo");
                if (video != null)
                {
                    if (control.Visibility == Visibility.Hidden)
                        control.Position = video.Position;

                    PantallaCompleta.Children.Remove(video);
                    NameScope.GetNameScope(this).UnregisterName(video.Name);
                    PantallaCompleta.Background = null;
                    SilenciarVideos(false);
                }
            }));

            }
            catch (Exception) { }
        }

        private void PausarVideos(bool pPlay)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
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
                                if (((MediaElement)itemObjets).Visibility == Visibility.Visible)
                                {
                                    ((MediaElement)itemObjets).Play();
                                    ((MediaElement)itemObjets).LoadedBehavior = MediaState.Play;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }));
            }
            catch { }
        }

        private void SilenciarVideos(bool pSilencio)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
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
                                    ((MediaElement)itemObjets).Volume = .5;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }));

            }
            catch { }
        }

        private void VentanaSplash_Click(object sender, RoutedEventArgs e)
        {
            ConfigurarVentanaSplash dialog = new ConfigurarVentanaSplash(this, smartPayout, smartHopper);
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

        private void MenuReiniciarDiseno_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            List<string> objEliminar = new List<string>();
            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
            dialog.lblNombre.Content = "¡Advertencia!";
            dialog.lblTexto.Text = "Se eliminará todo el diseño de forma permanente. ¿Está seguro que desea continuar?";
            if (dialog.ShowDialog() == true)
            {
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
                    }

                    foreach (string ob in objEliminar)
                    {
                        BorarObjeto(ob, false);
                    }
                    foreach (var item in Directory.GetFiles(@".\objetos\multimedia", "*.*"))
                    {
                        File.SetAttributes(item, FileAttributes.Normal);
                        File.Delete(item);
                    }

                    Directory.Delete(@".\objetosSplash", true);
                }
                catch { }
            }
        }

        private void QuitarAnimaciones()
        {
            try
            {
                foreach (UIElement item in Principal.Children)
                {

                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                    {
                        bool seModifico = false;
                        try
                        {
                            if (item.RenderTransform != null)
                            {
                                item.RenderTransform = (TranslateTransform)((TransformGroup)item.RenderTransform).Children[((TransformGroup)item.RenderTransform).Children.Count - 1];
                                seModifico = true;
                            }
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
            catch { }
        }

        private void CargarAnimaciones()
        {
            try
            {
                foreach (UIElement item in Principal.Children)
                {
                    if (SeModificaControl(item.GetValue(NameProperty).ToString()))
                        Animacion(item.GetValue(NameProperty).ToString());
                }
            }
            catch { }
        }

        private void QuitarWEB(bool esDiseno)
        {
            try
            {
                foreach (UIElement item in Principal.Children)
                {
                    switch (item.GetType().Name.ToString())
                    {
                        case "DockPanel":
                            item.IsHitTestVisible = true;
                            DockPanel dockPanel = (DockPanel)FindName(item.GetValue(NameProperty).ToString());
                            foreach (UIElement item2 in dockPanel.Children)
                            {
                                item2.Visibility = Visibility.Hidden;
                                ((WebView2)item2).Tag = ((WebView2)item2).Source;
                                ((WebView2)item2).Source = new Uri("https://www.google.com.mx/");
                            }
                            break;
                        default:
                            if (!esDiseno)
                                item.IsHitTestVisible = false;
                            else
                                item.IsHitTestVisible = true;
                            break;
                    }

                }
            }
            catch { }
        }

        private void CargarWEB(bool esDiseno)
        {
            try
            {
                foreach (UIElement item in Principal.Children)
                {
                    switch (item.GetType().Name.ToString())
                    {
                        case "DockPanel":
                            item.IsHitTestVisible = true;
                            DockPanel dockPanel = (DockPanel)FindName(item.GetValue(NameProperty).ToString());
                            dockPanel.IsHitTestVisible = true;
                            foreach (UIElement item2 in dockPanel.Children)
                            {
                                item2.Visibility = Visibility.Visible;
                                ((WebView2)item2).Source = new Uri(((WebView2)item2).Tag.ToString());
                            }
                            break;
                        case "Button":
                            if (((Button)item).Name.Equals("LogoPrincipal"))
                                item.IsHitTestVisible = true;
                            else
                                item.IsHitTestVisible = false;
                            break;
                        default:
                            if (!esDiseno)
                                item.IsHitTestVisible = false;
                            else
                                item.IsHitTestVisible = true;
                            break;
                    }
                }
            }
            catch { }
        }

        private void Animacion(string pNombreControl)
        {
            var item = FindName(pNombreControl) as UIElement;
            if (item != null && !item.GetType().Name.Equals("DockPanel-"))
            {
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
                DirectoryInfo info = new DirectoryInfo(@"objetos\animaciones");
                foreach (var file in info.GetFiles())
                {
                    if (@file.Name.Equals(pNombreControl + ".anim"))
                    {
                        StreamReader sR = new StreamReader(@file.FullName);
                        string lectura = sR.ReadToEnd();
                        sR.Close();
                        string[] animaciones = new Seguridad().DecryptString(nombreApp, lectura).Split('-');
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

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        public static void crearDirectorios()
        {
            //Carpetas de animaciones
            if (!Directory.Exists(@".\objetos\animaciones"))
            {
                Directory.CreateDirectory(@".\objetos\animaciones");
            }

            //Carpeta de consultas sql
            if (!Directory.Exists(@".\objetos\consultasSQL"))
            {
                Directory.CreateDirectory(@".\objetos\consultasSQL");
            }

            //Carpeta multimedia principal
            if (!Directory.Exists(@".\objetos\multimedia"))
            {
                Directory.CreateDirectory(@".\objetos\multimedia");
            }

        }

        private void VentanaPrincipal_Activated(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized && !editar)
            {
                FocusManager.SetFocusedElement(this, VentanaPrincipal);
            }
        }

        private void LogoPrincipal_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true);
            dialog.lblNombre.Content = "¡Advertencia!";
            if (!editar)
                dialog.lblTexto.Text = "Esta a punto salir del modo presentación, ¿Está seguro que desea continuar?.";
            else
                dialog.lblTexto.Text = "Esta a punto salir del modo edición, ¿Está seguro que desea continuar?.";
            dialog.btnCancelar.Visibility = Visibility.Visible;
            new Recursos().ventanaMensajesGrande800x600(dialog);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (dialog.ShowDialog() == true)
            {
                SalirMaximizar();
            }
            Principal.Focus();
        }

        private void Sobre_Click(object sender, RoutedEventArgs e)
        {
            Sobre dialog = new Sobre();
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

        private void LogoPrincipal_Click(object sender, RoutedEventArgs e)
        {
            Principal.Focus();
        }

        private void EcribirErroresLog(string error)
        {
            DateTime dt = DateTime.Now;
            if (!File.Exists(@".\Log_" + dt.ToString("dd-MM-yyyy") + ".3k"))
            {
                using (File.Create(@".\Log_" + dt.ToString("dd-MM-yyyy") + ".3k")) { }
            }
            using (StreamWriter stream = new StreamWriter(@".\Log_" + dt.ToString("dd-MM-yyyy") + ".3k", true))
            {
                stream.WriteLine(this.Name + "_Error: " + error + " - " + dt.ToShortTimeString());
                stream.Close();
            }
        }

        private void MenuVentanaSplash_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MostrarVentanaSplash dialog = new MostrarVentanaSplash(true);
            dialog.ShowDialog();
        }

        private void ConfigurarSplash_Click(object sender, RoutedEventArgs e)
        {
            ConfigurarVentanaSplash dialog = new ConfigurarVentanaSplash(this, smartPayout, smartHopper);
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
}