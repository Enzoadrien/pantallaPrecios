using Priceio.Cajero;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
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
using static Org.BouncyCastle.Math.EC.ECCurve;
using static Priceio.ClasesSQLite.Pago;
using static Priceio.PropiedadesImpresora;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para PropiedadesFondo.xaml
    /// </summary>
    public partial class PropiedadesImpresora : Window
    {
        
        internal string? nombreControl;
        public enum TipoImpresion { Cajero, Turnero }
        private TipoImpresion _tipoImpresion = TipoImpresion.Cajero;


        public PropiedadesImpresora(TipoImpresion tipo = TipoImpresion.Cajero)
        {
            _tipoImpresion = tipo;
            InitializeComponent();
            CargarDatos();
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void CargarDatos()
        {
            ConfiguracionImpresora? SQLiteClass = new SQLiteClassManager().GetConfiguracionImpresora();
            if (SQLiteClass != null)
            {
                cbxImpresora.SelectedValue = SQLiteClass.Nombre;
                lblLetraDemo.SetValue(FontFamilyProperty, new System.Windows.Media.FontFamily(SQLiteClass.TipoLetra));
                cbxFuente.SelectedItem = lblLetraDemo.GetValue(FontFamilyProperty);
                cbxTamano.SelectedValue = SQLiteClass.TamanoLetra;
                chkNegrita.IsChecked = SQLiteClass.Negrita;
                chkCursiva.IsChecked = SQLiteClass.Cursiva;
                chkLogo.IsChecked = true;
                chkLogo.IsChecked = SQLiteClass.Logo;
                cbxTamanoLogo.SelectedValue = SQLiteClass.TamanoLogo;
                CoordenadaX.Text = SQLiteClass.CordenadaXLogo.ToString();
                CoordenadaY.Text = SQLiteClass.CordenadaYLogo.ToString();
                imgLogo.Tag = SQLiteClass.RutaLogo;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                if (File.Exists(@".\data\impresora\" + imgLogo.Tag))
                    image.UriSource = new Uri(@".\data\impresora\" + imgLogo.Tag, UriKind.RelativeOrAbsolute);
                else
                    image.UriSource = new Uri(@".\Recursos\logo.png", UriKind.RelativeOrAbsolute);
                image.EndInit();
                imgLogo.Source = image;
            }
            else
            {
                cbxImpresora.SelectedValue = "POS58";
                lblLetraDemo.SetValue(FontFamilyProperty, new System.Windows.Media.FontFamily("Courier New"));
                cbxFuente.SelectedItem = lblLetraDemo.GetValue(FontFamilyProperty);
                cbxTamano.SelectedValue = 8;
                chkNegrita.IsChecked = true;
                chkCursiva.IsChecked = false;
                chkLogo.IsChecked = true;
                cbxTamanoLogo.SelectedValue = 70;
                CoordenadaX.Text = "0";
                CoordenadaY.Text = "0";
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@".\Recursos\logo.png", UriKind.RelativeOrAbsolute);
                image.EndInit();
                imgLogo.Source = image;
                imgLogo.Tag = "";
            }


        }

        private void GuardarDatos()
        {
            ConfiguracionImpresora SQLiteClass = new ConfiguracionImpresora();
            SQLiteClass.Nombre = cbxImpresora.SelectedItem.ToString();
            SQLiteClass.TipoLetra = cbxFuente.SelectedItem.ToString();
            SQLiteClass.TamanoLetra = int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString());
            SQLiteClass.Negrita = chkNegrita.IsChecked;
            SQLiteClass.Cursiva = chkCursiva.IsChecked;
            SQLiteClass.Logo = chkLogo.IsChecked;
            SQLiteClass.RutaLogo = imgLogo.Tag.ToString();
            SQLiteClass.TamanoLogo = int.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString());
            SQLiteClass.CordenadaXLogo = int.Parse(CoordenadaX.Text);
            SQLiteClass.CordenadaYLogo = int.Parse(CoordenadaY.Text);

            if (!new SQLiteClassManager().SetConfiguracionImpresora(SQLiteClass))
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error al guardar la información, consulte al administrador";
                dialog.btnCancelar.Visibility = Visibility.Visible;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
            }

        }

        private void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            //dlg.DefaultExt = ".png";
            dlg.Filter = "Todos los archivos de imagen|*.jpeg;*.jpg;*.png|JPEG (*.jpeg;*.jpg)|*.jpeg;*.jpg|PNG (*.png)|*.png";


            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                FileInfo fi = new FileInfo(dlg.FileName);
                try
                {
                    FileInfo fileImg = new FileInfo(@".\data\impresora\" + fi.Name);
                    if (File.Exists(@".\data\impresora\" + fi.Name) && !fi.FullName.Equals(fileImg.FullName))
                    {
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, false,false, "Remplazar", "Mantener");
                        dialogMsg.lblNombre.Content = "¡Advertencia!";
                        dialogMsg.lblTexto.Text = "Ya existe un archivo con el mismo nombre y extension en la aplicación, ¿Desea remplazarlo o mantener la actual?. ¡Esta accion no se puede revertir!";
                        if (dialogMsg.ShowDialog() == true)
                        {
                            fi.CopyTo(@".\data\impresora\" + fi.Name, true);
                        }
                    }
                    else
                        fi.CopyTo(@".\data\impresora\" + fi.Name, true);
                }
                catch
                {
                }
                FileInfo Img = new FileInfo(@".\data\impresora\" + fi.Name);
                // Open document 
                imgLogo.Tag = Img.Name;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@".\data\impresora\" + fi.Name, UriKind.RelativeOrAbsolute);
                image.EndInit();

                imgLogo.Source = image;
            }

        }

        private void cbxFuente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblLetraDemo.SetValue(FontFamilyProperty, new System.Windows.Media.FontFamily(cbxFuente.SelectedItem.ToString()));
        }

        private void cbxTamano_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblLetraDemo.SetValue(FontSizeProperty, Double.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()));
        }

        private void chkNegrita_Checked(object sender, RoutedEventArgs e)
        {
            lblLetraDemo.SetValue(FontWeightProperty, FontWeights.Bold);
        }

        private void chkNegrita_Unchecked(object sender, RoutedEventArgs e)
        {
            lblLetraDemo.SetValue(FontWeightProperty, FontWeights.Normal);
        }

        private void chkCursiva_Checked(object sender, RoutedEventArgs e)
        {
            lblLetraDemo.SetValue(FontStyleProperty, FontStyles.Italic);

        }

        private void chkCursiva_Unchecked(object sender, RoutedEventArgs e)
        {
            lblLetraDemo.SetValue(FontStyleProperty, FontStyles.Normal);
        }

        private void cbxTamanoLogo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double tamano = 70;
            try
            {
                tamano = Double.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString());
            }
            catch { }

            TamanoImg.Width = tamano;
            TamanoImg.Height = tamano;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            Close();
        }

        private void btnPrueba_Click(object sender, RoutedEventArgs e)
        {
            if (cbxImpresora.SelectedItem == null) return;

            try
            {
                PrintDocument pdoc = new PrintDocument();
                pdoc.DocumentName = "Prueba_" + _tipoImpresion;
                pdoc.PrinterSettings.PrinterName = cbxImpresora.SelectedItem.ToString();

                if (_tipoImpresion == TipoImpresion.Turnero)
                {
                    // ── Imprime ticket de turno de prueba ──────────────
                    var cfg = new ConfiguracionImpresora
                    {
                        Nombre = cbxImpresora.SelectedItem.ToString(),
                        TipoLetra = cbxFuente.SelectedItem?.ToString() ?? "Courier New",
                        TamanoLetra = int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()),
                        Negrita = chkNegrita.IsChecked,
                        Cursiva = chkCursiva.IsChecked,
                        Logo = chkLogo.IsChecked,
                        RutaLogo = imgLogo.Tag?.ToString() ?? "",
                        TamanoLogo = int.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString()),
                        CordenadaXLogo = int.Parse(CoordenadaX.Text.Length > 0 ? CoordenadaX.Text : "0"),
                        CordenadaYLogo = int.Parse(CoordenadaY.Text.Length > 0 ? CoordenadaY.Text : "0")
                    };
                    pdoc.PrintPage += (s, ev) => ImprimirTicketTurno(ev, 42, cfg);
                }
                else
                {
                    // ── Imprime ticket de cajero de prueba ─────────────
                    pdoc.PrintPage += (s, ev) => Document_PrintText(ev,
                        "          3K MANTENIMIENTO\n          PROFESIONAL\n\n" +
                        "ESTACION: ESTACION01\nUSUARIO: GRUPO\n" +
                        "FECHA: 13-11-2022  HORA: 00:17:55\nFOLIO: 1\n\n" +
                        "CANT.     PRECIO     TOTAL\n--------------------------\n" +
                        "   CARE DOG CACHORRO\n5.00       22.00    110.00\n\n" +
                        " Importe:       $ 2,020.00\n Total:         $ 2,020.00\n\n" +
                        "* GRACIAS POR SU COMPRA *\n");
                }

                pdoc.Print();
            }
            catch (Exception ex)
            {
                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false, true);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Error durante la impresión: " + ex.Message;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
            }
        }

        private void ImprimirTicketTurno(PrintPageEventArgs e, int numeroTurno, ConfiguracionImpresora cfg)
        {
            var g = e.Graphics;
            float yActual = 0f;
            int anchoPage = e.PageBounds.Width;

            string familia = cfg?.TipoLetra ?? "Courier New";
            float tamBase = cfg?.TamanoLetra ?? 8f;

            System.Drawing.FontStyle estilo = System.Drawing.FontStyle.Regular;
            if (cfg?.Negrita == true) estilo |= System.Drawing.FontStyle.Bold;
            if (cfg?.Cursiva == true) estilo |= System.Drawing.FontStyle.Italic;

            var fuenteNormal = new System.Drawing.Font(familia, tamBase, estilo);
            int tamanoLogo = cfg?.TamanoLogo > 0 ? cfg.TamanoLogo : 70;
            float tamanoNumero = tamanoLogo * 0.45f;
            var fuenteNumero = new System.Drawing.Font(familia, tamanoNumero, System.Drawing.FontStyle.Bold);
            var fuenteTitulo = new System.Drawing.Font(familia, tamBase, System.Drawing.FontStyle.Bold);

            // Logo
            if (cfg?.Logo == true)
            {
                try
                {
                    string ruta = !string.IsNullOrEmpty(cfg.RutaLogo)
                        ? @".\data\impresora\" + cfg.RutaLogo
                        : @".\Recursos\logo.png";
                    System.Drawing.Image img = System.Drawing.Image.FromFile(ruta);
                    float xLogo = (anchoPage - tamanoLogo) / 2f;
                    g.DrawImage(img, xLogo, yActual, tamanoLogo, tamanoLogo);
                    yActual += tamanoLogo + 8f;
                }
                catch { }
            }

            string sep = "================================";
            g.DrawString(sep, fuenteNormal, System.Drawing.Brushes.Black, 0, yActual);
            yActual += g.MeasureString(sep, fuenteNormal).Height;

            string titulo = "SISTEMA DE TURNOS";
            float xTitulo = (anchoPage - g.MeasureString(titulo, fuenteTitulo).Width) / 2f;
            g.DrawString(titulo, fuenteTitulo, System.Drawing.Brushes.Black, xTitulo, yActual);
            yActual += g.MeasureString(titulo, fuenteTitulo).Height;

            g.DrawString(sep, fuenteNormal, System.Drawing.Brushes.Black, 0, yActual);
            yActual += g.MeasureString(sep, fuenteNormal).Height + 4f;

            string fecha = $"Fecha: {DateTime.Now:dd/MM/yyyy  HH:mm}";
            g.DrawString(fecha, fuenteNormal, System.Drawing.Brushes.Black, 0, yActual);
            yActual += g.MeasureString(fecha, fuenteNormal).Height + 4f;

            string etiqueta = "Tu número de turno es:";
            float xEtiq = (anchoPage - g.MeasureString(etiqueta, fuenteNormal).Width) / 2f;
            g.DrawString(etiqueta, fuenteNormal, System.Drawing.Brushes.Black, xEtiq, yActual);
            yActual += g.MeasureString(etiqueta, fuenteNormal).Height + 6f;

            string numStr = numeroTurno.ToString();
            float xNum = (anchoPage - g.MeasureString(numStr, fuenteNumero).Width) / 2f;
            g.DrawString(numStr, fuenteNumero, System.Drawing.Brushes.Black, xNum, yActual);
            yActual += g.MeasureString(numStr, fuenteNumero).Height + 8f;

            g.DrawString(sep, fuenteNormal, System.Drawing.Brushes.Black, 0, yActual);
            yActual += g.MeasureString(sep, fuenteNormal).Height + 2f;

            string pie = "Por favor espere su turno.";
            float xPie = (anchoPage - g.MeasureString(pie, fuenteNormal).Width) / 2f;
            g.DrawString(pie, fuenteNormal, System.Drawing.Brushes.Black, xPie, yActual);
            yActual += g.MeasureString(pie, fuenteNormal).Height;

            g.DrawString(sep, fuenteNormal, System.Drawing.Brushes.Black, 0, yActual);

            fuenteNormal.Dispose();
            fuenteNumero.Dispose();
            fuenteTitulo.Dispose();
        }

        private void Document_PrintText(PrintPageEventArgs e, string inputString)
        {
            if (CoordenadaX.Text.Length == 0)
                CoordenadaX.Text = "0";
            if (CoordenadaY.Text.Length == 0)
                CoordenadaY.Text = "0";
            if(chkLogo.IsChecked == true)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(imgLogo.Source.ToString());
                e.Graphics.DrawImage(img, float.Parse(CoordenadaX.Text), float.Parse(CoordenadaY.Text), int.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString()), int.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString()));
            }

            if (chkNegrita.IsChecked == true && chkCursiva.IsChecked == true)
                e.Graphics.DrawString(inputString, new Font(cbxFuente.SelectedItem.ToString(), int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()), System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic), System.Drawing.Brushes.Black, 0, 0);
            else if(chkNegrita.IsChecked == true && chkCursiva.IsChecked == false)
                e.Graphics.DrawString(inputString, new Font(cbxFuente.SelectedItem.ToString(), int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()), System.Drawing.FontStyle.Bold), System.Drawing.Brushes.Black, 0, 0);
            else if (chkNegrita.IsChecked == false && chkCursiva.IsChecked == true)
                e.Graphics.DrawString(inputString, new Font(cbxFuente.SelectedItem.ToString(), int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()), System.Drawing.FontStyle.Italic), System.Drawing.Brushes.Black, 0, 0);
            else
                e.Graphics.DrawString(inputString, new Font(cbxFuente.SelectedItem.ToString(), int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString())), System.Drawing.Brushes.Black, 0, 0);
        }

        private Boolean TextAllowed(String s)
        {
            foreach (Char c in s.ToCharArray())
            {
                if (Char.IsDigit(c)) continue;
                else return false;
            }
            return true;
        }

        private void ResponseTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextAllowed(e.Text);
        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            // more error handling would be needed here - this is asking for trouble!
            String s = (String)e.DataObject.GetData(typeof(String));
            if (!TextAllowed(s)) e.CancelCommand();
        }

        private void chkLogo_Checked(object sender, RoutedEventArgs e)
        {
            btnAbrir.IsEnabled = true;
            cbxTamanoLogo.IsEnabled = true;
            CoordenadaX.IsEnabled = true;
            CoordenadaY.IsEnabled = true;
            imgLogo.Visibility = Visibility.Visible;
        }

        private void chkLogo_Unchecked(object sender, RoutedEventArgs e)
        {
            btnAbrir.IsEnabled = false;
            cbxTamanoLogo.IsEnabled = false;
            CoordenadaX.IsEnabled = false;
            CoordenadaY.IsEnabled = false;
            imgLogo.Visibility = Visibility.Hidden;
        }
    }

    public class ViewModelImpresoras
    {
        public ObservableCollection<string> CmbContentImpresoras { get; private set; }

        public ViewModelImpresoras()
        {
            CmbContentImpresoras = new ObservableCollection<string>
            {
            };
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                CmbContentImpresoras.Add(printer);
            }
        }
    }
}
