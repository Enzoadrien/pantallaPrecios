using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Priceio
{
    public partial class VentanaCajas : Window
    {
        // Evento que se dispara cuando el usuario presiona una caja.
        // Desde MainWindow puedes suscribirte: ventana.CajaPresionada += (numero) => { ... };
        public event Action<int>? CajaPresionada;

        // Tamaño mínimo y máximo (en px) que puede tener cada botón "Caja"
        private const double TamanoMin = 90;
        private const double TamanoMax = 180;
        private const double MargenBoton = 8;

        private int _ultimaCantidad = -1;

        public VentanaCajas()
        {
            InitializeComponent();
            Loaded += (s, e) => GenerarCajas();
        }

        // ---------------------------------------------------------------
        //  Validación del cuadro de texto: solo dígitos, máximo 0-100
        // ---------------------------------------------------------------
        private void TxtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]$");
        }

        private void TxtCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCantidad.Text))
                return;

            if (int.TryParse(TxtCantidad.Text, out int valor) && valor > 100)
            {
                TxtCantidad.Text = "100";
                TxtCantidad.CaretIndex = TxtCantidad.Text.Length;
            }
        }

        // ---------------------------------------------------------------
        //  Botones de encabezado
        // ---------------------------------------------------------------
        private void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            GenerarCajas();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Si el usuario cambia el tamaño de la ventana (o cambia de pantalla),
        // recalculamos el tamaño/distribución de las cajas ya generadas.
        private void VentanaCajasWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_ultimaCantidad >= 0)
                AcomodarCajas(_ultimaCantidad);
        }

        // ---------------------------------------------------------------
        //  Generación de cajas
        // ---------------------------------------------------------------
        private void GenerarCajas()
        {
            if (!int.TryParse(TxtCantidad.Text, out int cantidad))
            {
                MessageBox.Show("Ingresa una cantidad válida (0-100).", "Cajas",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cantidad < 0) cantidad = 0;
            if (cantidad > 100) cantidad = 100;

            AcomodarCajas(cantidad);
        }

        // Crea (o re-acomoda) los botones de caja, calculando el mejor
        // tamaño según el espacio disponible en pantalla.
        private void AcomodarCajas(int cantidad)
        {
            _ultimaCantidad = cantidad;

            PanelCajas.Children.Clear();

            if (cantidad == 0)
            {
                TxtInfo.Text = "No hay cajas para mostrar. Indica una cantidad mayor a 0 y presiona \"Generar cajas\".";
                return;
            }

            // Espacio disponible aproximado (área de scroll menos márgenes/scrollbar)
            double anchoDisponible = Math.Max(this.ActualWidth - 60, 300);
            double altoDisponible = Math.Max(this.ActualHeight - 160, 200);

            double tamanoFinal = CalcularTamanoOptimo(cantidad, anchoDisponible, altoDisponible);

            for (int i = 1; i <= cantidad; i++)
            {
                Button btnCaja = new Button
                {
                    Content = $"Caja {i}",
                    Width = tamanoFinal,
                    Height = tamanoFinal,
                    Margin = new Thickness(MargenBoton),
                    FontSize = Math.Max(14, tamanoFinal * 0.16),
                    Tag = i,
                    Style = (Style)FindResource("CajaButtonStyle")
                };
                btnCaja.Click += CajaButton_Click;
                PanelCajas.Children.Add(btnCaja);
            }

            TxtInfo.Text = $"{cantidad} caja(s) generada(s) — tamaño de botón: {tamanoFinal:0} px.";
        }

        // Calcula el tamaño (ancho/alto, son cuadrados) que debería tener
        // cada botón para acomodar "cantidad" cajas en el espacio disponible,
        // probando primero llenar el ancho y, si sobran filas, aceptando scroll.
        private double CalcularTamanoOptimo(int cantidad, double anchoDisponible, double altoDisponible)
        {
            double mejorTamano = TamanoMin;

            // Probamos distintos números de columnas y nos quedamos con el
            // que produzca el botón más grande posible dentro de los límites,
            // intentando que entren completas tantas filas como sea posible.
            for (int columnas = 1; columnas <= cantidad; columnas++)
            {
                double tamanoPorAncho = (anchoDisponible / columnas) - (MargenBoton * 2);
                if (tamanoPorAncho < TamanoMin)
                    break; // ya no entran más columnas con un tamaño usable

                int filas = (int)Math.Ceiling(cantidad / (double)columnas);
                double tamanoPorAlto = (altoDisponible / filas) - (MargenBoton * 2);

                // El tamaño real es el menor de los dos (para que quepa),
                // pero si por alto da muy chico, igual permitimos scroll
                // y usamos el tamaño por ancho (limitado al máximo).
                double candidato = Math.Min(tamanoPorAncho, Math.Max(tamanoPorAlto, TamanoMin));
                candidato = Math.Min(candidato, TamanoMax);
                candidato = Math.Max(candidato, TamanoMin);

                if (candidato > mejorTamano)
                    mejorTamano = candidato;
            }

            return Math.Round(mejorTamano);
        }

        // ---------------------------------------------------------------
        //  Click en una caja
        // ---------------------------------------------------------------
        private void CajaButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int numero)
            {
                // Notifica a quien esté escuchando (por ejemplo MainWindow)
                CajaPresionada?.Invoke(numero);

                // Feedback visual simple por defecto; puedes quitarlo si
                // quieres manejar la acción solo desde el evento CajaPresionada.
                // MessageBox.Show($"Presionaste la caja {numero}");
            }
        }
    }
}