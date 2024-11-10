using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Priceio.ClasesGenericas
{
    public class Recursos
    {
        public enum TipoMensaje
        {
            ERROR, ADVERTENCIA, ACEPTAR
        }

        public readonly IDictionary<Key, int> NumericKeys = new Dictionary<Key, int> {
        { Key.D0, 0 },
        { Key.D1, 1 },
        { Key.D2, 2 },
        { Key.D3, 3 },
        { Key.D4, 4 },
        { Key.D5, 5 },
        { Key.D6, 6 },
        { Key.D7, 7 },
        { Key.D8, 8 },
        { Key.D9, 9 },
        { Key.NumPad0, 0 },
        { Key.NumPad1, 1 },
        { Key.NumPad2, 2 },
        { Key.NumPad3, 3 },
        { Key.NumPad4, 4 },
        { Key.NumPad5, 5 },
        { Key.NumPad6, 6 },
        { Key.NumPad7, 7 },
        { Key.NumPad8, 8 },
        { Key.NumPad9, 9 }};

        internal void ventanaMensajesGrande800x600(Mensajes pVentana)
        {
            pVentana.Width = 800;
            pVentana.Height = 600;
            pVentana.Salir.Width = 100;
            pVentana.Salir.Height = 100;
            pVentana.Salir.FontSize = 80;
            pVentana.lblNombre.FontSize = 100;
            pVentana.lblTexto.FontSize = 60;
            pVentana.btnOK.FontSize = 60;
            pVentana.btnOK.Width = 300;
            pVentana.btnOK.Height = 100;
            pVentana.btnCancelar.FontSize = 60;
            pVentana.btnCancelar.Width = 300;
            pVentana.btnCancelar.Height = 100;
            pVentana.btnCancelar.HorizontalAlignment = HorizontalAlignment.Left;
            pVentana.btnCancelar.Margin = new Thickness(5, 5, 5, 5);
        }

        internal void ventanaCapturaTextoGrande800x600(CapturaTexto pVentana)
        {
            pVentana.Width = 800;
            pVentana.Height = 600;
            pVentana.Salir.Width = 100;
            pVentana.Salir.Height = 100;
            pVentana.Salir.FontSize = 80;
            pVentana.lblNombre.FontSize = 100;
            pVentana.Texto.FontSize = 60;
            pVentana.btnOK.FontSize = 60;
            pVentana.btnOK.Width = 300;
            pVentana.btnOK.Height = 100;
            pVentana.btnCancelar.FontSize = 60;
            pVentana.btnCancelar.Width = 300;
            pVentana.btnCancelar.Height = 100;
            pVentana.btnCancelar.HorizontalAlignment = HorizontalAlignment.Left;
            pVentana.btnCancelar.Margin = new Thickness(5, 5, 5, 5);
        }
    }
}
