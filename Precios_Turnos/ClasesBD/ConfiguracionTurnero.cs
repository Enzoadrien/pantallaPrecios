using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.ClasesBD
{
    internal class ConfiguracionTurnero
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
        public string? TipoTurnero
        {
            get;
            set;
        }
        public string? ProtocoloTurnero
        {
            get;
            set;
        }
        public int PuertoTCP
        {
            get;
            set;
        }
        public int TurnosAnteriores
        {
            get;
            set;
        }
        public string? Clientes
        {
            get;
            set;
        }
        public string? MostrarNombres
        {
            get;
            set;
        }
    }
}
