using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.ClasesGenericas
{
    internal class ConfiguracionAnimaciones
    {
        public bool? Desplazar
        {
            get;
            set;
        }
        public bool? Horizontal
        {
            get;
            set;
        }
        public char? DirecionHorizontal
        {
            get;
            set;
        }
        public int VelocidadHorizontal
        {
            get;
            set;
        }
        public int CantidadHorizontal
        {
            get;
            set;
        }
        public bool? ReversaHorizontal
        {
            get;
            set;
        }
        public bool? Vertical
        {
            get;
            set;
        }
        public char? DirecionVertical
        {
            get;
            set;
        }
        public int VelocidadVertical
        {
            get;
            set;
        }
        public int CantidadVertical
        {
            get;
            set;
        }
        public bool? ReversaVertical
        {
            get;
            set;
        }
   
        public bool? Escalar
        {
            get;
            set;
        }
        public double TamanoEscalar
        {
            get;
            set;
        }
        public int VelocidadEscalar
        {
            get;
            set;
        }
        
        public bool? Girar
        {
            get;
            set;
        }
        public double AnguloGirar
        {
            get;
            set;
        }
        public bool? AutoGirar
        {
            get;
            set;
        }
        public double AnguloAutoGirar
        {
            get;
            set;
        }
        public int VelocidadGirar
        {
            get;
            set;
        }
        public bool? ReversaGirar
        {
            get;
            set;
        }

        public bool? Desvanecer
        {
            get;
            set;
        }
        public int VelocidadDesvanecer
        {
            get;
            set;
        }
        public bool? ReversaDesvanecer
        {
            get;
            set;
        }
    }
}
