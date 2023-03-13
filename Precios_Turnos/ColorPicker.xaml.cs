using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Precios_Turnos
{
    /// <summary>
    /// Lógica de interacción para ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        #region Data

        private DrawingAttributes drawingAttributes = new DrawingAttributes();
        private Color selectedColor = Colors.Transparent;
        private Boolean IsMouseDown = false;
        private MainWindow mainWindow;
        private int numAnterior=0;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that initializes to ColorPicker to the specified color.
        /// </summary>
        /// <param name="initialColor"></param>
        public ColorPicker(MainWindow pMainWindow, Color initialColor)
        {
            InitializeComponent();
            SelectedColor = initialColor;
            mainWindow = pMainWindow;
            ultimoColorLetra.Background = new SolidColorBrush(mainWindow.ultimoColorLetra);
            ultimoColorFondo.Background = new SolidColorBrush(mainWindow.ultimoColorFondo);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or privately sets the Selected Color.
        /// </summary>
        public Color SelectedColor
        {
            get { return selectedColor; }
            private set
            {
                if (selectedColor != value)
                {
                    this.selectedColor = value;
                    AlphaSlider.Value = value.A;
                    CreateAlphaLinearBrush();
                    UpdateTextBoxes();
                    UpdateInk();
                }
            }
        }

        /// <summary>
        /// Sets the initial Selected Color.
        /// </summary>
        public Color InitialColor
        {
            set
            {
                SelectedColor = value;
                CreateAlphaLinearBrush();
                AlphaSlider.Value = value.A;
                UpdateCursorEllipse(value);
            }
        }

        #endregion

        #region Control Events

        /// <summary>
        /// 
        /// </summary>
        private void AlphaSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int change = e.Delta / Math.Abs(e.Delta);
            AlphaSlider.Value = AlphaSlider.Value + (double)change;
        }

        /// <summary>
        /// Update SelectedColor Alpha based on Slider value.
        /// </summary>
        private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SelectedColor = Color.FromArgb((byte)AlphaSlider.Value, SelectedColor.R, SelectedColor.G, SelectedColor.B);
        }

        /// <summary>
        /// Update the SelectedColor if moving the mouse with the left button down.
        /// </summary>
        private void CanvasImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown) UpdateColor();
        }

        /// <summary>
        /// Handle MouseDown event.
        /// </summary>
        private void CanvasImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = true;
            UpdateColor();
            AlphaSlider.Value = 255;
        }

        /// <summary>
        /// Handle MouseUp event.
        /// </summary>
        private void CanvasImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = false;
            //UpdateColor();
        }

        /// <summary>
        /// Apply the new Swatch image based on user requested swatch.
        /// </summary>
        private void Swatch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image img = (sender as Image);
            ColorImage.Source = img.Source;
            UpdateCursorEllipse(SelectedColor);
        }

        #endregion // Control Events

        #region Private Methods

        /// <summary>
        /// Creates a new LinearGradientBrush background for the Alpha area slider.  This is based on the current color.
        /// </summary>
        private void CreateAlphaLinearBrush()
        {
            Color startColor = Color.FromArgb((byte)0, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            Color endColor = Color.FromArgb((byte)255, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            LinearGradientBrush alphaBrush = new LinearGradientBrush(startColor, endColor, new Point(0, 0), new Point(1, 0));
            AlphaBorder.Background = alphaBrush;
        }

        /// <summary>
        /// Sets a new Selected Color based on the color of the pixel under the mouse pointer.
        /// </summary>
        private void UpdateColor()
        {
            // Test to ensure we do not get bad mouse positions along the edges
            int imageX = (int)Mouse.GetPosition(canvasImage).X;
            int imageY = (int)Mouse.GetPosition(canvasImage).Y;
            if ((imageX < 0) || (imageY < 0) || (imageX > ColorImage.Width - 1) || (imageY > ColorImage.Height - 1)) return;
            // Get the single pixel under the mouse into a bitmap and copy it to a byte array
            CroppedBitmap cb = new CroppedBitmap(ColorImage.Source as BitmapSource, new Int32Rect(imageX, imageY, 1, 1));
            byte[] pixels = new byte[4];
            cb.CopyPixels(pixels, 4, 0);
            // Update the mouse cursor position and the Selected Color
            ellipsePixel.SetValue(Canvas.LeftProperty, (double)(Mouse.GetPosition(canvasImage).X - (ellipsePixel.Width / 2.0)));
            ellipsePixel.SetValue(Canvas.TopProperty, (double)(Mouse.GetPosition(canvasImage).Y - (ellipsePixel.Width / 2.0)));
            canvasImage.InvalidateVisual();
            // Set the Selected Color based on the cursor pixel and Alpha Slider value
            SelectedColor = Color.FromArgb((byte)AlphaSlider.Value, pixels[2], pixels[1], pixels[0]);
        }

        /// <summary>
        /// Update the mouse cursor ellipse position.
        /// </summary>
        private void UpdateCursorEllipse(Color searchColor)
        {
            // Scan the canvas image for a color which matches the search color
            CroppedBitmap cb;
            Color tempColor = new Color();
            byte[] pixels = new byte[4];
            int searchY = 0;
            int searchX = 0;
            searchColor.A = 255;
            for (searchY = 0; searchY <= canvasImage.Width - 1; searchY++)
            {
                for (searchX = 0; searchX <= canvasImage.Height - 1; searchX++)
                {
                    cb = new CroppedBitmap(ColorImage.Source as BitmapSource, new Int32Rect(searchX, searchY, 1, 1));
                    cb.CopyPixels(pixels, 4, 0);
                    tempColor = Color.FromArgb(255, pixels[2], pixels[1], pixels[0]);
                    if (tempColor == searchColor) break;
                }
                if (tempColor == searchColor) break;
            }
            // Default to the top left if no match is found
            if (tempColor != searchColor)
            {
                searchX = 0;
                searchY = 0;
            }
            // Update the mouse cursor ellipse position
            ellipsePixel.SetValue(Canvas.LeftProperty, ((double)searchX - (ellipsePixel.Width / 2.0)));
            ellipsePixel.SetValue(Canvas.TopProperty, ((double)searchY - (ellipsePixel.Width / 2.0)));
        }

        /// <summary>
        /// Update text box values based on the Selected Color.
        /// </summary>
        private void UpdateTextBoxes()
        {
            txtAlpha.Text = SelectedColor.A.ToString();
            txtAlphaHex.Text = SelectedColor.A.ToString("X2");
            txtRed.Text = SelectedColor.R.ToString();
            txtRedHex.Text = SelectedColor.R.ToString("X2");
            txtGreen.Text = SelectedColor.G.ToString();
            txtGreenHex.Text = SelectedColor.G.ToString("X2");
            txtBlue.Text = SelectedColor.B.ToString();
            txtBlueHex.Text = SelectedColor.B.ToString("X2");
            txtAll.Text = String.Format("#{0}{1}{2}{3}", txtAlphaHex.Text, txtRedHex.Text, txtGreenHex.Text, txtBlueHex.Text);
        }

        private void UpdateColorTextBox() 
        {
            SelectedColor = Color.FromArgb(Convert.ToByte(int.Parse(txtAlpha.Text)), Convert.ToByte(int.Parse(txtRed.Text)), Convert.ToByte(int.Parse(txtGreen.Text)), Convert.ToByte(int.Parse(txtBlue.Text)));
        }

        /// <summary>
        /// Updates the Ink strokes based on the Selected Color.
        /// </summary>
        private void UpdateInk()
        {
            drawingAttributes.Color = SelectedColor;
            drawingAttributes.StylusTip = StylusTip.Ellipse;
            drawingAttributes.Width = 5;
            // Update drawing attributes on previewPresenter
            foreach (Stroke s in previewPresenter.Strokes)
            {
                s.DrawingAttributes = drawingAttributes;
            }
        }

        #endregion // Update Methods

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
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
                Close();
            }
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ultimoColorLetra_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = e.Source as UIElement;
            SelectedColor = (((Border)item).Background as SolidColorBrush).Color;
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

        private void ResponseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if(cajaTexto.Text.Length > 0 && cajaTexto.Text.Length<4)
                if(int.Parse(cajaTexto.Text) <= 255)
                    numAnterior = int.Parse(cajaTexto.Text);

            if (e.Key == Key.Space && cajaTexto.IsFocused == true)
                e.Handled = true;
        }

        private void txtAlpha_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if (cajaTexto.Text.Length > 0) {
                bool moverCursor = false;
                if (cajaTexto.Text.Substring(0, 1).CompareTo("0") == 0 && cajaTexto.Text.Length > 1)
                    moverCursor = true;
                cajaTexto.Text = int.Parse(cajaTexto.Text).ToString();
                if (moverCursor)
                    cajaTexto.CaretIndex = cajaTexto.Text.Length;
                if(int.Parse(cajaTexto.Text) > 255) {
                    cajaTexto.Text = numAnterior.ToString();
                    cajaTexto.CaretIndex = cajaTexto.Text.Length;
                }
                UpdateColorTextBox();
            }
            
        }

        private void txtBlue_LostFocus(object sender, RoutedEventArgs e)
        {
            var item = e.Source as UIElement;
            TextBox cajaTexto = (TextBox)item;
            if (cajaTexto.Text.Length == 0)
            {
                cajaTexto.Text = "0";
                UpdateColorTextBox();
            }
        }
    }
}
