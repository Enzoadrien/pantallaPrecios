using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Precios_Turnos
{
    public class Pago
    {

        public int NumPago{ get; set; }
        public double CantidadTotal { get; set; }
        public int CantidadIngresada { get; set; }
        public int CantidadFaltante { get; set; }
        public int Cambio { get; set; }
        public bool Pagado { get; set; }
        public int CantidadBilletesIngresados { get; set; }
        public int CantidadMonedasIngresadas { get; set; }


    }
}
