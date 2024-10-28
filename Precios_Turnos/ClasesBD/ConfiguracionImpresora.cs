using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.ClasesBD
{
    internal class ConfiguracionImpresora
    {
        public string? Nombre
        {
            get;
            set;
        }
        public string? TipoLetra
        {
            get;
            set;
        }
        public int TamanoLetra
        {
            get;
            set;
        }
        public bool Negrita
        {
            get;
            set;
        }
        public bool Cursiva
        {
            get;
            set;
        }
        public string? Logo
        {
            get;
            set;
        }
        public int TamanoLogo
        {
            get;
            set;
        }
        public bool SinLogo
        {
            get;
            set;
        }
    }
}
