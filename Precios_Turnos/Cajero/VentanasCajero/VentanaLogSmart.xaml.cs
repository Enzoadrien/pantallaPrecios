using Priceio.Cajero.Payout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Priceio.Cajero.VentanasCajero
{
    /// <summary>
    /// Lógica de interacción para Log.xaml
    /// </summary>
    public partial class VentanaLogSmart : Window
    {
        public VentanaLogSmart()
        {
            InitializeComponent();
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        internal void recargarlog(string pStrLogHopper, string pStrLogPayout)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (!lblLogHopper.Text.Equals(pStrLogHopper))
                {
                    lblLogHopper.Text = pStrLogHopper;
                    MyScrollViewerHopper.ScrollToBottom();
                }
                if (!lblLogPayout.Text.Equals(pStrLogPayout))
                {
                    lblLogPayout.Text = pStrLogPayout;
                    MyScrollViewerPayout.ScrollToBottom();
                }
            }));
        }

    }
}
