using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using MySqlX.XDevAPI;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
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

namespace Priceio
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
            List<NombresClientesTurnero> ListNombresClientesTurnero = new List<NombresClientesTurnero>();
            foreach (Item item in Nombres.ItemsSource)
            {
                if (item.ID != null && item.Nombre != null)
                {
                    if (!item.ID.Equals(string.Empty) && !item.Nombre.Equals(string.Empty))
                    {
                        ListNombresClientesTurnero.Add(new NombresClientesTurnero { Identificador = item.ID, Nombre = item.Nombre });
                    }
                }
            }
            if(!new SQLiteClassManager().SetNombresClientesTurnero(ListNombresClientesTurnero))
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();    
            }
        }
        private void CargarInfo()
        {
            List<NombresClientesTurnero>? SQLiteClass = new SQLiteClassManager().GetNombresClientesTurnero();
            if (SQLiteClass != null)
            {
                List<Item> items = new List<Item>();
                foreach (NombresClientesTurnero clientes in SQLiteClass)
                {
                    items.Add(new Item { ID = clientes.Identificador, Nombre = clientes.Nombre });
                }

                Nombres.ItemsSource = items;
            }    
        }
    }
    public class Item
    {
        public string? ID { get; set; }
        public string? Nombre { get; set; }
    }
}
