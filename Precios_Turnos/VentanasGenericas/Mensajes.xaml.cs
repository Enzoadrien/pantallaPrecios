using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para Mensajes.xaml
    /// </summary>
    public partial class Mensajes : Window
    {
        ConfiguracionGeneral? SQLiteClass = new SQLiteClassManager().GetConfiguracionGeneral();

        public Mensajes(Recursos.TipoMensaje tipoMensaje, bool esPregunta = false, bool tamanoEspecial = false, bool cerrarVentanaAuto = false, string textoBotonAceptar = "Aceptar", string textoBotonCancelar = "Cancelar")
        {
            InitializeComponent();
            if (tamanoEspecial)
            {
                if (SQLiteClass != null)
                {
                    switch (SQLiteClass.TamanoMensaje)
                    {
                        case "E":
                            ventanaMensajesExtraGrande();
                            break;
                        case "G":
                            ventanaMensajesGrande();
                            break;

                    }
                }
            }
            btnOK.Content = textoBotonAceptar;
            btnCancelar.Content = textoBotonCancelar;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (tipoMensaje == Recursos.TipoMensaje.ACEPTAR)
            {
                lblTexto.Foreground = new SolidColorBrush(Colors.White);
                lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E6D0E"));
            }
            else if (tipoMensaje == Recursos.TipoMensaje.ADVERTENCIA)
            {
                lblTexto.Foreground = new SolidColorBrush(Colors.White);
                lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF6B73C"));
            }
            else if (tipoMensaje == Recursos.TipoMensaje.ERROR)
            {
                lblTexto.Foreground = new SolidColorBrush(Colors.White);
                lblTexto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC42B1C"));
            }
            if (esPregunta)
                btnCancelar.Visibility = Visibility.Visible;
            if (cerrarVentanaAuto)
                if (SQLiteClass != null)
                    if(SQLiteClass.CerradoAutomatico == true)
                        StartCloseTimer();

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
            double ms;
            if (SQLiteClass != null)
                ms = SQLiteClass.TiempoMensaje * 1000;
            else
                ms = 5;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(ms);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        internal void ventanaMensajesExtraGrande()
        {
            Width = 800;
            Height = 600;
            Salir.Width = 100;
            Salir.Height = 100;
            Salir.FontSize = 80;
            lblNombre.FontSize = 100;
            lblTexto.FontSize = 60;
            btnOK.FontSize = 60;
            btnOK.Width = 300;
            btnOK.Height = 100;
            btnCancelar.FontSize = 60;
            btnCancelar.Width = 300;
            btnCancelar.Height = 100;
            btnCancelar.HorizontalAlignment = HorizontalAlignment.Left;
            btnCancelar.Margin = new Thickness(5, 5, 5, 5);
        }
        internal void ventanaMensajesGrande()
        {
            Width = 600;
            Height = 400;
            Salir.Width = 80;
            Salir.Height = 80;
            Salir.FontSize = 60;
            lblNombre.FontSize = 64;
            lblTexto.FontSize = 44;
            btnOK.FontSize = 40;
            btnOK.Width = 200;
            btnOK.Height = 70;
            btnCancelar.FontSize = 40;
            btnCancelar.Width = 200;
            btnCancelar.Height = 70;
            btnCancelar.HorizontalAlignment = HorizontalAlignment.Left;
            btnCancelar.Margin = new Thickness(5, 5, 5, 5);
        }
    }
}
