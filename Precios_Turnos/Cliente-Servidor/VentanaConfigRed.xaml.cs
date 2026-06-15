using Microsoft.Win32;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Priceio.Turnero
{
    public partial class VentanaConfigRed : Window
    {
        public string IpResultante { get; private set; } = "";
        public int PuertoResultante { get; private set; } = 8080;

        private VentanaTurnos? _ventanaTurnos;

        // Constructor sin referencia — solo IP/Puerto
        public VentanaConfigRed()
        {
            InitializeComponent();
            CargarConfigRed();
        }

        // Constructor con referencia — IP/Puerto + diseño
        public VentanaConfigRed(VentanaTurnos? ventanaTurnos)
        {
            _ventanaTurnos = ventanaTurnos;
            InitializeComponent();
            CargarConfigRed();
        }

        private void CargarConfigRed()
        {
            var cfg = ConfiguracionRed.Cargar();
            TxtPuerto.Text = cfg.Puerto.ToString();

            if (cfg.Ip == "192.168.1.64" || string.IsNullOrWhiteSpace(cfg.Ip))
                TxtIp.Text = ObtenerIpLocal();
            else
                TxtIp.Text = cfg.Ip;
        }

        private string ObtenerIpLocal()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }
            return "";
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string ip = TxtIp.Text.Trim();
            if (string.IsNullOrWhiteSpace(ip))
            {
                MostrarError("Escribe la dirección IP del servidor.");
                return;
            }

            if (!int.TryParse(TxtPuerto.Text.Trim(), out int puerto) || puerto < 1 || puerto > 65535)
            {
                MostrarError("El puerto debe ser un número entre 1 y 65535.");
                return;
            }

            var cfg = ConfiguracionRed.Cargar();
            cfg.Ip = ip;
            cfg.Puerto = puerto;
            cfg.Guardar();

            IpResultante = ip;
            PuertoResultante = puerto;

            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void MostrarError(string mensaje)
        {
            TxtError.Text = mensaje;
            TxtError.Visibility = Visibility.Visible;
        }

        // ── Logo ──────────────────────────────────────────────
        private void BtnSeleccionarLogo_Click(object sender, RoutedEventArgs e)
        {
            var vt = _ventanaTurnos ?? ObtenerVentanaTurnos();
            if (vt == null)
            {
                MessageBox.Show("La ventana de turnos no está abierta.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar logo",
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Copiar a carpeta interna
                    string nombreArchivo = ConfiguracionRed.CopiarImagen(dialog.FileName, "logo");
                    if (string.IsNullOrEmpty(nombreArchivo))
                        throw new Exception("No se pudo copiar el archivo.");

                    // Aplicar en VentanaTurnos
                    BitmapImage imagen = CargarImagen(dialog.FileName);
                    vt.ImgLogo.Source = imagen;
                    vt.ContenedorLogo.Visibility = Visibility.Visible;

                    // Guardar en config
                    var cfg = ConfiguracionRed.Cargar();
                    cfg.NombreLogo = nombreArchivo;
                    cfg.Guardar();
                }
                catch
                {
                    MessageBox.Show("No se pudo cargar el logo.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // ── Fondo ─────────────────────────────────────────────
        private void BtnSeleccionarFondo_Click(object sender, RoutedEventArgs e)
        {
            var vt = _ventanaTurnos ?? ObtenerVentanaTurnos();
            if (vt == null)
            {
                MessageBox.Show("La ventana de turnos no está abierta.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar fondo",
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Copiar a carpeta interna
                    string nombreArchivo = ConfiguracionRed.CopiarImagen(dialog.FileName, "fondo");
                    if (string.IsNullOrEmpty(nombreArchivo))
                        throw new Exception("No se pudo copiar el archivo.");

                    // Aplicar en VentanaTurnos
                    BitmapImage imagen = CargarImagen(dialog.FileName);
                    vt.ImgFondo.Source = imagen;
                    vt.ImgFondo.Visibility = Visibility.Visible;

                    // Guardar en config
                    var cfg = ConfiguracionRed.Cargar();
                    cfg.NombreFondo = nombreArchivo;
                    cfg.Guardar();
                }
                catch
                {
                    MessageBox.Show("No se pudo cargar el fondo.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // ── Fondo del botón ───────────────────────────────────────
        private void BtnColorFondo_Click(object sender, RoutedEventArgs e)
        {
            var vt = _ventanaTurnos ?? ObtenerVentanaTurnos();
            if (vt == null)
            {
                MessageBox.Show("La ventana de turnos no está abierta.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var colorDialog = new System.Windows.Forms.ColorDialog { FullOpen = true, AnyColor = true };
            if (colorDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var c = colorDialog.Color;
            var wpfColor = System.Windows.Media.Color.FromRgb(c.R, c.G, c.B);

            // Aplicar al Border del template
            vt.BtnObtenerTurno.ApplyTemplate();
            var borde = vt.BtnObtenerTurno.Template
                .FindName("BordeBTN", vt.BtnObtenerTurno) as Border;
            if (borde != null)
                borde.Background = new System.Windows.Media.SolidColorBrush(wpfColor);

            // Guardar
            var cfg = ConfiguracionRed.Cargar();
            cfg.ColorPersonalizado = true;
            cfg.ColorBotonR = c.R;
            cfg.ColorBotonG = c.G;
            cfg.ColorBotonB = c.B;
            cfg.Guardar();
        }

        // ── Texto del botón ───────────────────────────────────────
        private void BtnColorTexto_Click(object sender, RoutedEventArgs e)
        {
            var vt = _ventanaTurnos ?? ObtenerVentanaTurnos();
            if (vt == null)
            {
                MessageBox.Show("La ventana de turnos no está abierta.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var colorDialog = new System.Windows.Forms.ColorDialog { FullOpen = true, AnyColor = true };
            if (colorDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var c = colorDialog.Color;
            var wpfColor = System.Windows.Media.Color.FromRgb(c.R, c.G, c.B);

            // Aplicar al texto
            ActualizarTextBlocks(vt.BtnObtenerTurno,
                new System.Windows.Media.SolidColorBrush(wpfColor));

            // Guardar
            var cfg = ConfiguracionRed.Cargar();
            cfg.ColorTextoPersonalizado = true;
            cfg.ColorTextoR = c.R;
            cfg.ColorTextoG = c.G;
            cfg.ColorTextoB = c.B;
            cfg.Guardar();
        }

        // ── Sombra del botón ──────────────────────────────────────
        private void BtnColorSombra_Click(object sender, RoutedEventArgs e)
        {
            var vt = _ventanaTurnos ?? ObtenerVentanaTurnos();
            if (vt == null)
            {
                MessageBox.Show("La ventana de turnos no está abierta.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var colorDialog = new System.Windows.Forms.ColorDialog { FullOpen = true, AnyColor = true };
            if (colorDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var c = colorDialog.Color;
            var wpfColor = System.Windows.Media.Color.FromRgb(c.R, c.G, c.B);

            vt.BtnObtenerTurno.ApplyTemplate();
            var borde = vt.BtnObtenerTurno.Template
                .FindName("BordeBTN", vt.BtnObtenerTurno) as Border;

            if (borde != null)
            {
                // ── Crear nuevo efecto en lugar de modificar el existente ──
                borde.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = wpfColor,
                    BlurRadius = 28,
                    ShadowDepth = 0,
                    Opacity = 0.55
                };
            }

            // Guardar
            var cfg = ConfiguracionRed.Cargar();
            cfg.ColorSombraPersonalizado = true;
            cfg.ColorSombraR = c.R;
            cfg.ColorSombraG = c.G;
            cfg.ColorSombraB = c.B;
            cfg.Guardar();
        }

        // ── Helpers ───────────────────────────────────────────
        private VentanaTurnos? ObtenerVentanaTurnos()
        {
            foreach (Window w in Application.Current.Windows)
                if (w is VentanaTurnos vt)
                    return vt;
            return null;
        }

        private BitmapImage CargarImagen(string ruta)
        {
            var imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.UriSource = new Uri(ruta);
            imagen.CacheOption = BitmapCacheOption.OnLoad;
            imagen.EndInit();
            return imagen;
        }

        private void ActualizarTextBlocks(DependencyObject parent, Brush brush)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBlock tb)
                    tb.Foreground = brush;
                else
                    ActualizarTextBlocks(child, brush);
            }
        }

        private void BtnCambiarContrasena_Click(object sender, RoutedEventArgs e)
        {
            string nueva = TxtNuevaContrasena.Password.Trim();
            if (string.IsNullOrEmpty(nueva))
            {
                MessageBox.Show("Escribe una contraseña.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (nueva.Length < 4)
            {
                MessageBox.Show("La contraseña debe tener al menos 4 caracteres.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cfg = ConfiguracionRed.Cargar();
            cfg.CambiarContrasena(nueva);
            TxtNuevaContrasena.Clear();
            MessageBox.Show("Contraseña actualizada correctamente.", "Listo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}