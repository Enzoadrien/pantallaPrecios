using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.ClasesBD
{
    internal class ConfiguracionVerificador
    {
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
        public bool Voz
        {
            get;
            set;
        }
        public int TipoVoz
        {
            get;
            set;
        }
        public int TextoVozInicio
        {
            get;
            set;
        }
        public int TextoVozFinal
        {
            get;
            set;
        }
    }
}
