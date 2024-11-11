using Priceio.ClasesSQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.Turnero
{
    internal class Turnero
    {
        private List<TurnosAnteriores>? TurnosAnt = new List<TurnosAnteriores>();
        public string? CodigoTurnero
        {
            get;
            set;
        }
        public string? NumeroTurnero
        {
            get;
            set;
        }
        public string? CodigoEquipo
        {
            get;
            set;
        }
        public string? NumeroEquipo
        {
            get;
            set;
        }
        public int NumeroTurno
        {
            get;
            set;
        }

        internal void SetTurnosAnt(TurnosAnteriores pvStrTurnosAnt)
        {
            TurnosAnt.Add(pvStrTurnosAnt);
        }
        internal List<TurnosAnteriores>? GetTurnosAnt()
        {
            return TurnosAnt;
        }
    }
}
