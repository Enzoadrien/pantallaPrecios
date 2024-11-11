using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Drawing;
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
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using Priceio.ClasesGenericas;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace Priceio.Cajero.VentanasCajero
{
    /// <summary>
    /// Lógica de interacción para FormatoPagoEscaner.xaml
    /// </summary>
    public partial class FormatoPagoEscaner : Window
    {
        internal string formatoImpresora = string.Empty;
        private bool esCambio = true;
        private double escala = 1.50;
        public FormatoPagoEscaner(string formato)
        {
            InitializeComponent();
            CargarDatos();
            lblTexto.Text = formatoImpresora = formato;
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

        private void cbxFuente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblTexto.SetValue(FontFamilyProperty, new System.Windows.Media.FontFamily(cbxFuente.SelectedItem.ToString()));
        }

        private void cbxTamano_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblTexto.SetValue(FontSizeProperty, Double.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()));
        }

        private void chkNegrita_Checked(object sender, RoutedEventArgs e)
        {
            lblTexto.SetValue(FontWeightProperty, FontWeights.Bold);
        }

        private void chkNegrita_Unchecked(object sender, RoutedEventArgs e)
        {
            lblTexto.SetValue(FontWeightProperty, FontWeights.Normal);
        }

        private void chkCursiva_Checked(object sender, RoutedEventArgs e)
        {
            lblTexto.SetValue(FontStyleProperty, FontStyles.Italic);

        }

        private void chkCursiva_Unchecked(object sender, RoutedEventArgs e)
        {
            lblTexto.SetValue(FontStyleProperty, FontStyles.Normal);
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

        private void ResponseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;

            if (e.Key == Key.Space && cajaTexto.IsFocused == true)
                e.Handled = true;
        }

        private void Coordenada_TextChanged(object sender, TextChangedEventArgs e)
        {
                if (esCambio)
                {
                    var item = gridTicket.FindName("LogoImpresora") as UIElement;
                    try
                    {
                        if (CoordenadaX.Text.Length > 0 && CoordenadaY.Text.Length > 0)
                        {

                            System.Windows.Point point = item.TransformToAncestor(gridTicket).Transform(new System.Windows.Point(0, 0));
                            double pX = Math.Round(point.X);
                            double pY = Math.Round(point.Y);

                            double currentX = 0;
                            double currentY = 0;
                            try
                            {
                                TranslateTransform _currentTT = item.RenderTransform as TranslateTransform;
                            if (_currentTT != null)
                            {

                                currentX = Math.Round(_currentTT.X);
                                currentY = Math.Round(_currentTT.Y);
                            }
                            }
                            catch { }

                            double x = double.Parse(CoordenadaX.Text);
                            double y = double.Parse(CoordenadaY.Text);
                            double movX = 0;
                            double movY = 0;

                            if (pX != x && currentX == 0)
                                movX = -pX + x;
                            else if (pX == x)
                                movX = currentX;
                            else
                                movX = currentX - pX + x;

                            if (pY != y && currentY == 0)
                                movY = -pY + y;
                            else if (pY == y)
                                movY = currentY;
                            else
                                movY = currentY - pY + y;

                            item.RenderTransform = new TranslateTransform(movX/escala, movY/escala);
                        }

                    }
                    catch (Exception) { }
                    esCambio = false;
                    if (CoordenadaX.Text.Length > 0)
                        CoordenadaX.Text = Convert.ToInt32(CoordenadaX.Text).ToString();
                    if (CoordenadaY.Text.Length > 0)
                        CoordenadaY.Text = Convert.ToInt32(CoordenadaY.Text).ToString();
                    esCambio = true;
                }
        }
        
        private void Coordenada_LostFocus(object sender, RoutedEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if (cajaTexto.Text.Length == 0)
            {
                item = gridTicket.FindName("LogoImpresora") as UIElement;
                if (item != null)
                {
                    System.Windows.Point point = item.TransformToAncestor(gridTicket).Transform(new System.Windows.Point(0, 0));
                    double pX = Math.Round(point.X);
                    double pY = Math.Round(point.Y);

                    if (cajaTexto.Name.Equals("CoordenadaX"))
                        cajaTexto.Text = Convert.ToInt32(pX).ToString();
                    else
                        cajaTexto.Text = Convert.ToInt32(pY).ToString();
                }
            }
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

            System.Windows.Controls.Image obj = new System.Windows.Controls.Image();
            obj.Name = "LogoImpresora";
            obj.Stretch = Stretch.Uniform;
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.CacheOption = BitmapCacheOption.OnLoad;
            if (File.Exists(@".\data\impresora\" + imgLogo.Tag))
                image.UriSource = new Uri(@".\data\impresora\" + imgLogo.Tag, UriKind.RelativeOrAbsolute);
            else
                image.UriSource = new Uri(@".\Recursos\logo.png", UriKind.RelativeOrAbsolute);
            image.EndInit();
            obj.Source = image;
            obj.HorizontalAlignment = HorizontalAlignment.Left;
            obj.VerticalAlignment = VerticalAlignment.Top;
            NameScope.GetNameScope(this).RegisterName(obj.Name, obj);
            gridTicket.Children.Add(obj);
            try
            {
                obj.Height = double.Parse(cbxTamanoLogo.SelectedValue.ToString()) / escala;
                obj.Width = double.Parse(cbxTamanoLogo.SelectedValue.ToString()) / escala;
                obj.RenderTransform = new TranslateTransform(int.Parse(CoordenadaX.Text), int.Parse(CoordenadaY.Text));
            }
            catch { }
        }

        private void chkLogo_Unchecked(object sender, RoutedEventArgs e)
        {
            btnAbrir.IsEnabled = false;
            cbxTamanoLogo.IsEnabled = false;
            CoordenadaX.IsEnabled = false;
            CoordenadaY.IsEnabled = false;
            imgLogo.Visibility = Visibility.Hidden;

            var item = gridTicket.FindName("LogoImpresora") as UIElement;
            if(item != null)
            {
                gridTicket.Children.Remove(item);
                NameScope.GetNameScope(this).UnregisterName("LogoImpresora");
            }
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
            var item = gridTicket.FindName("LogoImpresora") as UIElement;
            if (item != null)
            {
                ((System.Windows.Controls.Image)item).Width = tamano / escala;
                ((System.Windows.Controls.Image)item).Height = tamano / escala;
            }
        }

        private void Document_PrintText(PrintPageEventArgs e, string inputString)
        {
            if (CoordenadaX.Text.Length == 0)
                CoordenadaX.Text = "0";
            if (CoordenadaY.Text.Length == 0)
                CoordenadaY.Text = "0";
            if (chkLogo.IsChecked == true)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(imgLogo.Source.ToString());
                e.Graphics.DrawImage(img, float.Parse(CoordenadaX.Text), float.Parse(CoordenadaY.Text), int.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString()), int.Parse(((ComboBoxItem)cbxTamanoLogo.SelectedItem).Tag.ToString()));
            }

            if (chkNegrita.IsChecked == true && chkCursiva.IsChecked == true)
                e.Graphics.DrawString(inputString, new Font(cbxFuente.SelectedItem.ToString(), int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()), System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic), System.Drawing.Brushes.Black, 0, 0);
            else if (chkNegrita.IsChecked == true && chkCursiva.IsChecked == false)
                e.Graphics.DrawString(inputString, new Font(cbxFuente.SelectedItem.ToString(), int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()), System.Drawing.FontStyle.Bold), System.Drawing.Brushes.Black, 0, 0);
            else if (chkNegrita.IsChecked == false && chkCursiva.IsChecked == true)
                e.Graphics.DrawString(inputString, new Font(cbxFuente.SelectedItem.ToString(), int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString()), System.Drawing.FontStyle.Italic), System.Drawing.Brushes.Black, 0, 0);
            else
                e.Graphics.DrawString(inputString, new Font(cbxFuente.SelectedItem.ToString(), int.Parse(((ComboBoxItem)cbxTamano.SelectedItem).Tag.ToString())), System.Drawing.Brushes.Black, 0, 0);
        }

        private void CargarDatos()
        {
            ConfiguracionImpresora? SQLiteClass = new SQLiteClassManager().GetConfiguracionImpresora();
            if (SQLiteClass != null)
            {
                cbxImpresora.SelectedValue = SQLiteClass.Nombre;
                lblTexto.SetValue(FontFamilyProperty, new System.Windows.Media.FontFamily(SQLiteClass.TipoLetra));
                cbxFuente.SelectedItem = lblTexto.GetValue(FontFamilyProperty);
                cbxTamano.SelectedValue = SQLiteClass.TamanoLetra;
                chkNegrita.IsChecked = SQLiteClass.Negrita;
                chkCursiva.IsChecked = SQLiteClass.Cursiva;
                cbxTamanoLogo.SelectedValue = SQLiteClass.TamanoLogo;
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
                chkLogo.IsChecked = true;
                CoordenadaX.Text = SQLiteClass.CordenadaXLogo.ToString();
                CoordenadaY.Text = SQLiteClass.CordenadaYLogo.ToString();
                chkLogo.IsChecked = SQLiteClass.Logo;

            }
            else
            {
                cbxImpresora.SelectedValue = "POS58";
                lblTexto.SetValue(FontFamilyProperty, new System.Windows.Media.FontFamily("Courier New"));
                cbxFuente.SelectedItem = lblTexto.GetValue(FontFamilyProperty);
                cbxTamano.SelectedValue = 8;
                chkNegrita.IsChecked = true;
                chkCursiva.IsChecked = false;
                cbxTamanoLogo.SelectedValue = 70;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@".\Recursos\logo.png", UriKind.RelativeOrAbsolute);
                image.EndInit();
                imgLogo.Source = image;
                imgLogo.Tag = "";
                chkLogo.IsChecked = true;
                CoordenadaX.Text = "0";
                CoordenadaY.Text = "0";
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
                        Mensajes dialogMsg = new Mensajes(Recursos.TipoMensaje.ADVERTENCIA, true, "Remplazar", "Mantener");
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

                var item = gridTicket.FindName("LogoImpresora") as UIElement;
                if (item != null)
                {
                    ((System.Windows.Controls.Image)item).Source = image;
                }
            }

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            GuardarDatos();
            formatoImpresora = lblTexto.Text;
            Close();
        }

        private void btnPrueba_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDocument pdoc = new PrintDocument();
                pdoc.DocumentName = "PBA" + new Random();
                pdoc.PrinterSettings.PrinterName = cbxImpresora.SelectedItem.ToString();
                pdoc.PrintPage += (sender, e) => Document_PrintText(e, lblTexto.Text);
                pdoc.Print();
            }
            catch (Exception ex)
            {

                Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialog.lblNombre.Content = "¡Error!";
                dialog.lblTexto.Text = "Ocurrio un error durante la impresión, consulte al administrador. Error: " + ex.Message;
                new Recursos().ventanaMensajesGrande800x600(dialog);
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
            }
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
