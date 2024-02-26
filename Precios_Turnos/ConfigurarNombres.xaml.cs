using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para Mensajes.xaml
    /// </summary>
    public partial class ConfigurarNombres : Window
    {
        public ConfigurarNombres()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CargarInfo();


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
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            GuardarContenido();
            Close();
        }


        private void GuardarContenido()
        {
            try
            {

                using (Stream stream = new FileStream(@".\Recursos\nombreEquipos.3k", FileMode.Open))
                {
                    stream.SetLength(0);
                    foreach (Item item in Nombres.ItemsSource)
                    {
                        if(item.ID != null && item.Nombre != null)
                        {
                            if (!item.ID.Equals(string.Empty) && !item.Nombre.Equals(string.Empty))
                            {
                                byte[] bytes = Encoding.UTF8.GetBytes(item.ID + "=" + item.Nombre + Environment.NewLine);
                                stream.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }
                    stream.Close();
                }
            }
            catch
            {
            }
        }
        private void CargarInfo()
        {
            try
            {
                List<Item> items = new List<Item>();
                using (Stream stream = new FileStream(@".\Recursos\nombreEquipos.3k", FileMode.Open))
                {
                    var sr = new StreamReader(stream);

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] datos = line.Split('=');
                        if (datos.Length == 2)
                        {
                            items.Add(new Item { ID = datos[0], Nombre = datos[1] });
                        }

                    }
                    stream.Close();
                }
                Nombres.ItemsSource = items;
       

            }
            catch
            {
            }
        }

    }
    public class Item
    {
        public string? ID { get; set; }
        public string? Nombre { get; set; }
    }
}
