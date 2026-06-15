using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.ClasesSQLite
{
    internal class ConfiguracionGeneral
    {
        public string? TamanoMensaje
        {
            get;
            set;
        }
        public bool? CerradoAutomatico
        {
            get;
            set;
        }
        public int TiempoMensaje
        {
            get;
            set;
        }

        public bool? ForzarResolucion
        {
            get;
            set;
        }

        public int AnchoResolucion
        {
            get;
            set;
        }
        public int AltoResolucion
        {
            get;
            set;
        }
    }
}
