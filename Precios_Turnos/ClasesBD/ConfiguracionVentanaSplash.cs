using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.ClasesBD
{
    internal class ConfiguracionVentanaSplash
    {
        public string? TipoSplash
        {
            get;
            set;
        }
        public bool ActivarSplash
        {
            get;
            set;
        }
        public int Ancho
        {
            get;
            set;
        }
        public int Alto
        {
            get;
            set;
        }
        public string? Audio
        {
            get;
            set;
        }
        
        public int Duracion
        {
            get;
            set;
        }
    }
}
