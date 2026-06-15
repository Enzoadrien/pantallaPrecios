using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Priceio.ClasesSQLite;
using Priceio.SQLite;

namespace Priceio
{
    public partial class AtencionBasculas : Window
    {
        // Se dispara cuando la ventana se cierra, para que quien la abrió
        // (VentanaCajas) sepa que puede permitir abrir otra.
        public event Action? VentanaCerrada;

        // Se dispara cuando el usuario selecciona una báscula.
        // Entrega: número de báscula (1..N) y número de turno que esa
        // báscula está atendiendo (según TurnosAnteriores).
        public event Action<int, int>? BasculaSeleccionada;

        // Lista de turnos anteriores, en el mismo orden que se muestran
        // en NumeroTurnoAnt (posición 1 = Báscula 1, posición 2 = Báscula 2, etc.)
        private List<TurnosAnteriores> _turnosAnteriores = new List<TurnosAnteriores>();

        public AtencionBasculas()
        {
            InitializeComponent();
            CargarBasculas();
        }

        // ---------------------------------------------------------------
        //  Carga las básculas configuradas y genera un botón por cada una.
        //  La cantidad de básculas es la misma que "Turnos anteriores"
        //  configurado en: Configurar ventana splash > Turnero.
        //  Cada botón muestra el turno que esa báscula está atendiendo,
        //  buscando por NumeroEquipo ("01", "02", ... "0N"/"N").
        // ---------------------------------------------------------------
        private void CargarBasculas()
        {
            PanelBasculas.Children.Clear();

            int cantidadBasculas = ObtenerCantidadBasculas();
            _turnosAnteriores = ObtenerTurnosAnteriores();

            if (cantidadBasculas <= 0)
            {
                TxtSinBasculas.Visibility = Visibility.Visible;
                return;
            }

            TxtSinBasculas.Visibility = Visibility.Collapsed;

            for (int i = 1; i <= cantidadBasculas; i++)
            {
                int numeroTurno = ObtenerTurnoPorBascula(i);

                Button btnBascula = new Button
                {
                    Content = numeroTurno > 0
                        ? $"Báscula {i}\nTurno {numeroTurno}"
                        : $"Báscula {i}",
                    Tag = i,
                    Style = (Style)FindResource("BasculaButtonStyle")
                };
                btnBascula.Click += BasculaButton_Click;
                PanelBasculas.Children.Add(btnBascula);
            }
        }

        // Busca en _turnosAnteriores el turno cuyo NumeroEquipo corresponde
        // a la báscula indicada (1 -> "01", 2 -> "02", ..., 10 -> "10").
        private int ObtenerTurnoPorBascula(int numeroBascula)
        {
            string numeroEquipo = numeroBascula.ToString("00"); // "01", "02", ..., "10"

            TurnosAnteriores? turno = _turnosAnteriores.Find(t => t.NumeroEquipo == numeroEquipo);
            if (turno != null)
                return turno.NumeroTurno;

            return 0;
        }

        // Obtiene la cantidad de básculas desde ConfiguracionTurnero.TurnosAnteriores
        // (configurado en "Configurar ventana splash" > Turnero > Turnos anteriores).
        private int ObtenerCantidadBasculas()
        {
            try
            {
                ConfiguracionTurnero? configuracionTurnero = new SQLiteClassManager().GetConfiguracionTurnero();
                if (configuracionTurnero != null)
                    return configuracionTurnero.TurnosAnteriores;
            }
            catch { }
            return 0;
        }

        // Obtiene la lista de turnos anteriores.
        private List<TurnosAnteriores> ObtenerTurnosAnteriores()
        {
            try
            {
                List<TurnosAnteriores>? turnosAnteriores = new SQLiteClassManager().GetTurnosAnteriores();
                return turnosAnteriores ?? new List<TurnosAnteriores>();
            }
            catch
            {
                return new List<TurnosAnteriores>();
            }
        }

        // ---------------------------------------------------------------
        //  Click en una báscula
        // ---------------------------------------------------------------
        private void BasculaButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int numeroBascula)
            {
                int numeroTurno = ObtenerTurnoPorBascula(numeroBascula);

                BasculaSeleccionada?.Invoke(numeroBascula, numeroTurno);

                // Cerramos la ventana tras seleccionar la báscula,
                // así VentanaCajas vuelve a permitir abrir otra.
                Close();
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AtencionBasculasWindow_Closed(object sender, EventArgs e)
        {
            VentanaCerrada?.Invoke();
        }
    }
}