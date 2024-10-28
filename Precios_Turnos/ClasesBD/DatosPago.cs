using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Priceio.Cajero.Pago;

namespace Priceio.ClasesBD
{
    internal class DatosPago
    {
        public Tipo TipoPago { get; set; }
        public Estado EstadoPago { get; set; }
        public int NumPago { get; set; }
        public double CantidadTotal { get; set; }
        public int CantidadIngresada { get; set; }
        public int CantidadFaltante { get; set; }
        public int Cambio { get; set; }
        public int CambioCancelado { get; set; }
        public bool Pagado { get; set; }
        public int CantidadBilletesIngresados { get; set; }
        public int CantidadMonedasIngresadas { get; set; }
        public string? Impresion { get; set; }
    }
}
