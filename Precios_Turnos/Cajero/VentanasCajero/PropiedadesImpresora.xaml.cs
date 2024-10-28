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
using Priceio.Cajero;
using Priceio.ClasesGenericas;
using static System.Net.Mime.MediaTypeNames;

namespace Priceio
{
    /// <summary>
    /// Lógica de interacción para PropiedadesFondo.xaml
    /// </summary>
    public partial class PropiedadesImpresora : Window
    {
        
        internal string nombreControl;
        public PropiedadesImpresora()
        {
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
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cbxImpresora.SelectedValue = config.AppSettings.Settings["Impresora"].Value;
            lblLetraDemo.SetValue(FontFamilyProperty, new System.Windows.Media.FontFamily(config.AppSettings.Settings["TipoLetraImpresora"].Value));
            cbxFuente.SelectedItem = lblLetraDemo.GetValue(FontFamilyProperty);
            cbxTamano.SelectedValue = config.AppSettings.Settings["TamanoLetraImpresora"].Value;
            chkNegrita.IsChecked = config.AppSettings.Settings["NegritaLetraImpresora"].Value.Equals("true") ? true : false;
            chkCursiva.IsChecked = config.AppSettings.Settings["CursivaLetraImpresora"].Value.Equals("true") ? true : false;
            cbxTamanoLogo.SelectedValue = config.AppSettings.Settings["TamanoLogoImpresora"].Value;
            imgLogo.Tag = config.AppSettings.Settings["LogoImpresora"].Value;
            CoordenadaX.Text = config.AppSettings.Settings["CordenadaX"].Value;
            CoordenadaY.Text = config.AppSettings.Settings["CordenadaY"].Value;

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.CacheOption = BitmapCacheOption.OnLoad;

            if(File.Exists(@".\objetosSplash\multimedia\" + imgLogo.Tag))
                image.UriSource = new Uri(@".\objetosSplash\multimedia\" + imgLogo.Tag, UriKind.RelativeOrAbsolute);
            else
                image.UriSource = new Uri(@".\Recursos\logo.png", UriKind.RelativeOrAbsolute);
            image.EndInit();

            //FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            //newFormatedBitmapSource.BeginInit();
            //newFormatedBitmapSource.Source = image;
            //newFormatedBitmapSource.DestinationFormat = PixelFormats.Gray16;
            //newFormatedBitmapSource.EndInit();

            imgLogo.Source = image;
        }

        private void GuardarDatos()
        {
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Impresora"].Value = cbxImpresora.SelectedItem.ToString();
            config.AppSettings.Settings["TipoLetraImpresora"].Value = cbxFuente.SelectedItem.ToString();
            config.AppSettings.Settings["TamanoLetraImpresora"].Value = ((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString();
            config.AppSettings.Settings["NegritaLetraImpresora"].Value = chkNegrita.IsChecked == true ? "true" : "false";
            config.AppSettings.Settings["CursivaLetraImpresora"].Value = chkCursiva.IsChecked == true ? "true" : "false";
            config.AppSettings.Settings["TamanoLogoImpresora"].Value = ((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString();
            config.AppSettings.Settings["LogoImpresora"].Value = imgLogo.Tag.ToString();
            config.AppSettings.Settings["CordenadaX"].Value = CoordenadaX.Text;
            config.AppSettings.Settings["CordenadaY"].Value = CoordenadaY.Text;


            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

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
                    FileInfo fileImg = new FileInfo(@".\objetosSplash\multimedia\" + fi.Name);
                    if (File.Exists(@".\objetosSplash\multimedia\" + fi.Name) && !fi.FullName.Equals(fileImg.FullName))
                    {
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, "Remplazar", "Mantener");
                        dialogMsg.lblNombre.Content = "¡Advertencia!";
                        dialogMsg.lblTexto.Text = "Ya existe un archivo con el mismo nombre y extension en la aplicación, ¿Desea remplazarlo o mantener la actual?. ¡Esta accion no se puede revertir!";
                        if (dialogMsg.ShowDialog() == true)
                        {
                            fi.CopyTo(@".\objetosSplash\multimedia\" + fi.Name, true);
                        }
                    }
                    else
                        fi.CopyTo(@".\objetosSplash\multimedia\" + fi.Name, true);
                }
                catch
                {
                }
                FileInfo Img = new FileInfo(@".\objetosSplash\multimedia\" + fi.Name);
                // Open document 
                imgLogo.Tag = Img.Name;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@".\objetosSplash\multimedia\" + fi.Name, UriKind.RelativeOrAbsolute);
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
            try
            {
                PrintDocument pdoc = new PrintDocument();
                pdoc.DocumentName = "PBA" + new Random();
                pdoc.PrinterSettings.PrinterName = cbxImpresora.SelectedItem.ToString();
                pdoc.PrintPage += (sender, e) => Document_PrintText(e, "          3K MANTENIMIENTO<br>          PROFESIONAL<br>          HERNANDO MARTELL<br>          No. 96-A<br>          COLONIAL LA LOMA<br>          33-3390-5151<br><br>ESTACION: ESTACION01<br>USUARIO: GRUPO<br>FECHA: 13-11-2022<br>HORA: 00:17:55<br>FOLIO: 15<br>CLIENTE: (SYS)<br> Cliente de mostrador<br><br>CANT.     PRECIO     TOTAL<br>--------------------------<br>   CARE DOG CACHORRO<br>5.00       22.00    110.00<br>   CARE DOG CACHORRO<br>10.00      22.00    220.00<br>   CROQUETA PERRON<br>10.00      25.00    250.00<br>   DOG CHOU  ADULTO<br>20.00      47.00    940.00<br>   DOG CHOU CACHORRO<br>10.00      50.00    500.00<br><br><br> Importe:       $ 2,020.00<br> Impuesto:          $ 0.00<br> Total:         $ 2,020.00<br><br>DOS MIL  VEINTE PESOS<br>00/100 PESOS MEXICANOS<br><br><br>-----------Pago-----------<br> Pago en EFE:   $ 5,000.00<br> Cambio:        $ 2,980.00<br><br><br>* GRACIAS POR SU COMPRA *<br>".Replace("<br>", "\n"));
                pdoc.Print();
            }
            catch(Exception ex) {

                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error durante la impresión, consulte al administrador. Error: " + ex.Message;
                new Recursos().ventanaMensajesGrande800x600(dialog);
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
            }
        }
        private void Document_PrintText(PrintPageEventArgs e, string inputString)
        {
            if (CoordenadaX.Text.Length == 0)
                CoordenadaX.Text = "0";
            if (CoordenadaY.Text.Length == 0)
                CoordenadaY.Text = "0";

            System.Drawing.Image img = System.Drawing.Image.FromFile(imgLogo.Source.ToString());
            e.Graphics.DrawImage(img, float.Parse(CoordenadaX.Text), float.Parse(CoordenadaY.Text), int.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString()), int.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString()));

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
