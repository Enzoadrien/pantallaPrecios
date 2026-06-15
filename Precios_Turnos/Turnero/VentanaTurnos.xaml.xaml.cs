using Microsoft.Win32;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using Priceio.Turnero;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Priceio
{
    public partial class VentanaTurnos : Window
    {
        private MainWindow? _mainWindow;
        private int _ultimoTurno = 0;
        private bool _modoRed = false;
        private string _ipServidor = "";
        private int _puertoServidor = 8080;
        private string _apiKey = "";

        // HttpClient estático — una sola instancia para toda la app
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10) // subido de 5 a 10 para redes lentas
        };

        // ── Constructor MODO LOCAL ────────────────────────────
        public VentanaTurnos(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _modoRed = false;
            InicializarContadorLocal();
            CargarDiseñoGuardado();
        }

        // ── Constructor MODO RED ──────────────────────────────
        public VentanaTurnos(string ipServidor, int puerto)
        {
            InitializeComponent();
            _mainWindow = null;
            _modoRed = true;
            _ipServidor = ipServidor;
            _puertoServidor = puerto;
            CargarDiseñoGuardado();
            _ = InicializarModoRed();
        }

        // ── Inicialización modo red ───────────────────────────
        private async Task InicializarModoRed()
        {
            await ObtenerApiKeyDelServidor();
            await InicializarContadorRed();
        }

        private void ActualizarIndicadorConexion(bool conectado)
        {
            Dispatcher.Invoke(() =>
            {
                if (conectado)
                {
                    IndConexion.Fill = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Colors.LimeGreen);
                    TxtConexion.Text = $"Conectado · {_ipServidor}:{_puertoServidor}";
                }
                else
                {
                    IndConexion.Fill = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Colors.OrangeRed);
                    TxtConexion.Text = "Sin conexión";
                }
            });
        }

        private async Task ObtenerApiKeyDelServidor()
        {
            try
            {
                string url = $"http://{_ipServidor}:{_puertoServidor}/turno/auth";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ActualizarIndicadorConexion(false);
                    return;
                }
                string json = await response.Content.ReadAsStringAsync();
                RespuestaAuth? resp = JsonSerializer.Deserialize<RespuestaAuth>(json);
                if (resp != null && resp.Exito && !string.IsNullOrEmpty(resp.ApiKey))
                {
                    _apiKey = resp.ApiKey;
                    ActualizarIndicadorConexion(true); // ✓ conectado
                }
                else
                    ActualizarIndicadorConexion(false);
            }
            catch
            {
                ActualizarIndicadorConexion(false);
            }
        }

        private void InicializarContadorLocal()
        {
            try
            {
                var db = new SQLiteClassManager();
                int turnoActivo = db.GetTurno()?.NumeroTurno ?? 0;
                List<TurnosAnteriores>? anteriores = db.GetTurnosAnteriores();
                int maxAnterior = anteriores?.Count > 0 ? anteriores.Max(t => t.NumeroTurno) : 0;
                _ultimoTurno = Math.Max(turnoActivo, maxAnterior);
            }
            catch { _ultimoTurno = 0; }
        }

        private async Task InicializarContadorRed()
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                    await ObtenerApiKeyDelServidor();

                if (string.IsNullOrEmpty(_apiKey)) return;

                string url = $"http://{_ipServidor}:{_puertoServidor}/turno/ultimo";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-Api-Key", _apiKey);
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return;

                string json = await response.Content.ReadAsStringAsync();
                RespuestaTurno? resp = JsonSerializer.Deserialize<RespuestaTurno>(json);
                if (resp != null && resp.Exito)
                    _ultimoTurno = resp.NumeroTurno;
            }
            catch { _ultimoTurno = 0; }
        }

        // ── Obtener turno ─────────────────────────────────────
        private async void BtnObtenerTurno_Click(object sender, RoutedEventArgs e)
        {
            BtnObtenerTurno.IsEnabled = false;
            TxtMensaje.Text = "Generando turno...";
            ContenedorNumero.Visibility = Visibility.Collapsed;

            try
            {
                int nuevoTurno = _modoRed
                    ? await ObtenerTurnoRed()
                    : ObtenerTurnoLocal();

                if (nuevoTurno > 0)
                {
                    _ultimoTurno = nuevoTurno;
                    TxtNumeroTurno.Text = nuevoTurno.ToString();
                    TxtMensaje.Text = "¡Tu turno ha sido registrado!";
                    ContenedorNumero.Visibility = Visibility.Visible;
                    ImprimirTicket(nuevoTurno);
                }
                else
                {
                    TxtMensaje.Text = "No se pudo conectar con el servidor.";
                }
            }
            catch
            {
                TxtMensaje.Text = "Error al generar el turno.";
            }
            finally
            {
                BtnObtenerTurno.IsEnabled = true;
            }
        }

        private int ObtenerTurnoLocal()
        {
            var db = new SQLiteClassManager();
            int turnoActivo = db.GetTurno()?.NumeroTurno ?? 0;
            List<TurnosAnteriores>? anteriores = db.GetTurnosAnteriores();
            int maxAnterior = anteriores?.Count > 0 ? anteriores.Max(t => t.NumeroTurno) : 0;
            int maxActual = Math.Max(turnoActivo, maxAnterior);

            // ── Si la BD tiene 0 después de un reset, respetar el 0 ──
            if (maxActual > _ultimoTurno)
                _ultimoTurno = maxActual;

            return _ultimoTurno + 1;
        }

        private async Task<int> ObtenerTurnoRed()
        {
            const int maxIntentos = 3;
            const int esperaEntreIntentos = 2000; // 2 segundos

            for (int intento = 1; intento <= maxIntentos; intento++)
            {
                try
                {
                    if (string.IsNullOrEmpty(_apiKey))
                        await ObtenerApiKeyDelServidor();

                    if (string.IsNullOrEmpty(_apiKey))
                    {
                        if (intento < maxIntentos)
                        {
                            TxtMensaje.Text = $"Reintentando conexión ({intento}/{maxIntentos})...";
                            await Task.Delay(esperaEntreIntentos);
                            continue;
                        }
                        return 0;
                    }

                    string url = $"http://{_ipServidor}:{_puertoServidor}/turno/ultimo";
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("X-Api-Key", _apiKey);
                    var response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (intento < maxIntentos)
                        {
                            TxtMensaje.Text = $"Reintentando conexión ({intento}/{maxIntentos})...";
                            await Task.Delay(esperaEntreIntentos);
                            continue;
                        }
                        return 0;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    RespuestaTurno? resp = JsonSerializer.Deserialize<RespuestaTurno>(json);
                    int ultimoEnServidor = resp?.NumeroTurno ?? 0;

                    if (ultimoEnServidor >= _ultimoTurno)
                        _ultimoTurno = ultimoEnServidor;

                    return _ultimoTurno + 1; // ✓ éxito
                }
                catch
                {
                    if (intento < maxIntentos)
                    {
                        TxtMensaje.Text = $"Reintentando conexión ({intento}/{maxIntentos})...";
                        await Task.Delay(esperaEntreIntentos);
                    }
                }
            }
            return 0;
        }

        // ── Impresión ─────────────────────────────────────────
        private void ImprimirTicket(int numeroTurno)
        {
            try
            {
                ConfiguracionImpresora? cfg = new SQLiteClassManager().GetConfiguracionImpresora();
                PrintDocument pdoc = new PrintDocument();
                pdoc.DocumentName = $"Turno_{numeroTurno}";
                pdoc.PrinterSettings.PrinterName = cfg?.Nombre ?? "POS58";
                pdoc.PrintPage += (s, e) => Document_PrintText(e, numeroTurno, cfg);
                pdoc.Print();
            }
            catch { }
        }

        private void Document_PrintText(PrintPageEventArgs e, int numeroTurno,
            ConfiguracionImpresora? cfg)
        {
            var g = e.Graphics;
            float yActual = 0f;
            int anchoPage = (int)e.PageSettings.PrintableArea.Width;

            using var sfCentrado = new StringFormat(StringFormat.GenericTypographic)
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };
            using var sfIzquierda = new StringFormat(StringFormat.GenericTypographic)
            {
                Alignment = StringAlignment.Near
            };

            string familia = cfg?.TipoLetra ?? "Courier New";
            float tamBase = cfg?.TamanoLetra ?? 8f;

            System.Drawing.FontStyle estilo = System.Drawing.FontStyle.Regular;
            if (cfg?.Negrita == true) estilo |= System.Drawing.FontStyle.Bold;
            if (cfg?.Cursiva == true) estilo |= System.Drawing.FontStyle.Italic;

            using var fuenteNormal = new System.Drawing.Font(familia, tamBase, estilo);
            using var fuenteTitulo = new System.Drawing.Font(familia, tamBase, System.Drawing.FontStyle.Bold);

            int tamanoLogo = cfg?.TamanoLogo > 0 ? cfg.TamanoLogo : 70;
            float tamanoNumero = tamanoLogo * 0.45f;
            using var fuenteNumero = new System.Drawing.Font(familia, tamanoNumero, System.Drawing.FontStyle.Bold);

            var rectAncho = new System.Drawing.RectangleF(0, 0, anchoPage, 9999);

            // Logo
            if (cfg?.Logo == true)
            {
                try
                {
                    string ruta = !string.IsNullOrEmpty(cfg.RutaLogo)
                        ? @".\data\impresora\" + cfg.RutaLogo
                        : @".\Recursos\logo.png";
                    using var img = System.Drawing.Image.FromFile(ruta);
                    float xLogo = (anchoPage - tamanoLogo) / 2f;
                    g.DrawImage(img, xLogo, yActual, tamanoLogo, tamanoLogo);
                    yActual += tamanoLogo + 8f;
                }
                catch { }
            }

            string sep = "================================";
            void DibujarCentrado(string texto, System.Drawing.Font fuente)
            {
                rectAncho.Y = yActual;
                g.DrawString(texto, fuente, System.Drawing.Brushes.Black, rectAncho, sfCentrado);
                yActual += g.MeasureString(texto, fuente, anchoPage, sfCentrado).Height;
            }

            DibujarCentrado(sep, fuenteNormal);
            DibujarCentrado("SISTEMA DE TURNOS", fuenteTitulo);
            DibujarCentrado(sep, fuenteNormal);
            yActual += 4f;

            // Fecha izquierda
            string fecha = $"Fecha: {DateTime.Now:dd/MM/yyyy  HH:mm}";
            rectAncho.Y = yActual;
            g.DrawString(fecha, fuenteNormal, System.Drawing.Brushes.Black, rectAncho, sfIzquierda);
            yActual += g.MeasureString(fecha, fuenteNormal, anchoPage, sfIzquierda).Height + 4f;

            DibujarCentrado("Tu número de turno es:", fuenteNormal);
            yActual += 6f;
            DibujarCentrado(numeroTurno.ToString(), fuenteNumero);
            yActual += 8f;
            DibujarCentrado(sep, fuenteNormal);
            yActual += 2f;
            DibujarCentrado("Por favor espere su turno.", fuenteNormal);
            yActual += g.MeasureString("Por favor espere su turno.", fuenteNormal, anchoPage, sfCentrado).Height;
            DibujarCentrado(sep, fuenteNormal);
        }

        // ── Contraseña ────────────────────────────────────────
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            if (!PedirContrasena()) return;
            Close();
        }

        private bool PedirContrasena()
        {
            var cfg = ConfiguracionRed.Cargar(); // ← leer config

            var ventana = new Window
            {
                Title = "Acceso restringido",
                Width = 320,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#1E293B"))
            };

            var panel = new StackPanel { Margin = new Thickness(20) };
            var lbl = new TextBlock
            {
                Text = "Ingresa la contraseña para continuar:",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var txt = new PasswordBox
            {
                FontSize = 14,
                Height = 32,
                Margin = new Thickness(0, 0, 0, 12),
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#0F172A")),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#334155")),
                BorderThickness = new Thickness(1)
            };
            var btn = new Button
            {
                Content = "Aceptar",
                Height = 32,
                FontSize = 13,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#0EA5E9")),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0)
            };

            bool resultado = false;
            btn.Click += (s, ev) =>
            {
                // ── Verificar contra el hash guardado ─────────────────
                if (cfg.VerificarContrasena(txt.Password))
                {
                    resultado = true;
                    ventana.Close();
                }
                else
                {
                    lbl.Text = "❌ Contraseña incorrecta. Intenta de nuevo:";
                    lbl.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    txt.Clear();
                    txt.Focus();
                }
            };
            txt.KeyDown += (s, ev) =>
            {
                if (ev.Key == Key.Enter)
                    btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            };

            panel.Children.Add(lbl);
            panel.Children.Add(txt);
            panel.Children.Add(btn);
            ventana.Content = panel;
            ventana.Loaded += (s, ev) => txt.Focus();
            ventana.ShowDialog();
            return resultado;
        }

        // ── Diseño guardado ───────────────────────────────────
        private void CargarDiseñoGuardado()
        {
            try
            {
                var cfg = ConfiguracionRed.Cargar();

                if (!string.IsNullOrEmpty(cfg.RutaLogoCompleta) &&
                    System.IO.File.Exists(cfg.RutaLogoCompleta))
                {
                    ImgLogo.Source = CargarImagenSegura(cfg.RutaLogoCompleta);
                    ContenedorLogo.Visibility = Visibility.Visible;
                }

                if (!string.IsNullOrEmpty(cfg.RutaFondoCompleta) &&
                    System.IO.File.Exists(cfg.RutaFondoCompleta))
                {
                    ImgFondo.Source = CargarImagenSegura(cfg.RutaFondoCompleta);
                    ImgFondo.Visibility = Visibility.Visible;
                }

                if (cfg.ColorPersonalizado || cfg.ColorTextoPersonalizado || cfg.ColorSombraPersonalizado)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        BtnObtenerTurno.ApplyTemplate();
                        var borde = BtnObtenerTurno.Template
                            .FindName("BordeBTN", BtnObtenerTurno) as Border;

                        if (cfg.ColorPersonalizado && borde != null)
                            borde.Background = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(
                                    cfg.ColorBotonR, cfg.ColorBotonG, cfg.ColorBotonB));

                        if (cfg.ColorTextoPersonalizado)
                            ActualizarTextBlocks(BtnObtenerTurno,
                                new System.Windows.Media.SolidColorBrush(
                                    System.Windows.Media.Color.FromRgb(
                                        cfg.ColorTextoR, cfg.ColorTextoG, cfg.ColorTextoB)));

                        if (cfg.ColorSombraPersonalizado && borde != null)
                            borde.Effect = new System.Windows.Media.Effects.DropShadowEffect
                            {
                                Color = System.Windows.Media.Color.FromRgb(
                                    cfg.ColorSombraR, cfg.ColorSombraG, cfg.ColorSombraB),
                                BlurRadius = 28,
                                ShadowDepth = 0,
                                Opacity = 0.55
                            };

                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
            catch { }
        }

        private BitmapImage? CargarImagenSegura(string ruta)
        {
            try
            {
                var imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.UriSource = new Uri(ruta);
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.EndInit();
                return imagen;
            }
            catch { return null; }
        }

        private void ActualizarTextBlocks(DependencyObject parent,
            System.Windows.Media.Brush brush)
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is TextBlock tb)
                    tb.Foreground = brush;
                else
                    ActualizarTextBlocks(child, brush);
            }
        }

        private async void BtnResetTurno_Click(object sender, RoutedEventArgs e)
        {
            if (!PedirContrasena()) return;

            var confirmacion = MessageBox.Show(
                "¿Deseas reiniciar el contador de turnos al número 1?\nEsta acción no se puede deshacer.",
                "Confirmar reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            if (_modoRed)
            {
                // ── Modo red: resetear vía servidor ───────────────────
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post,
                        $"http://{_ipServidor}:{_puertoServidor}/turno/reset");
                    request.Headers.Add("X-Api-Key", _apiKey);
                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                        _ultimoTurno = 0;
                    else
                    {
                        // Si el servidor no tiene el endpoint aún, resetear solo local
                        _ultimoTurno = 0;
                    }
                }
                catch
                {
                    _ultimoTurno = 0;
                }
            }
            else
            {
                // ── Modo local: resetear en BD ────────────────────────
                try
                {
                    var db = new SQLiteClassManager();

                    // Resetear turno a 0 para que el siguiente sea 1
                    db.SetTurno(new Turno { NumeroTurno = 0, NumeroEquipo = "PC" });

                    // Limpiar turnos anteriores
                    db.EliminarDatosTabla("TurnosAnteriores");
                }
                catch { }
                finally
                {
                    // Siempre resetear el contador local
                    _ultimoTurno = 0;
                }
            }

            // ── Actualizar UI ─────────────────────────────────────────
            TxtNumeroTurno.Text = "";
            TxtMensaje.Text = "Contador reiniciado. El próximo turno será el 1.";
            ContenedorNumero.Visibility = Visibility.Collapsed;
        }

    }

    internal class RespuestaTurno
    {
        public bool Exito { get; set; }
        public int NumeroTurno { get; set; }
        public string Mensaje { get; set; } = "";
    }

    internal class RespuestaAuth
    {
        public bool Exito { get; set; }
        public string ApiKey { get; set; } = "";
    }
}