using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.ClasesSQLite
{
    internal class ConfiguracionLector
    {
        public bool? Activo
        {
            get;
            set;
        }
        public string? FormatoCodigo
        {
            get;
            set;
        }
        public bool? Imprmir
        {
            get;
            set;
        }
        public string? FormatoImpresora
        {
            get;
            set;
        }
    }
}
