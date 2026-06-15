using System;
using System.Windows;

namespace Priceio
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ¿Se lanzó con argumento --kiosko IP:PUERTO?
            if (e.Args.Length >= 2 && e.Args[0] == "--kiosko")
            {
                // Modo tablet: solo pantalla de turnos, sin BD local
                string[] partes = e.Args[1].Split(':');
                string ip = partes[0];
                int puerto = partes.Length > 1 ? int.Parse(partes[1]) : 8080;

                VentanaTurnos ventana = new VentanaTurnos(ip, puerto);
                ventana.Show();
            }
            else
            {
                // Modo normal: PC principal con todo el sistema
                MainWindow ventana = new MainWindow();
                ventana.Show();
            }
        }
    }
}