using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Precios_Turnos
{
    internal class Turno
    {

        private string CodTurnero;
        private string NumTurnero;
        private static string CodEquipo = "C";
        private string NumEquipo;
        private int NumTurno;
        private List<string>? TurnosAnt = new List<string>();

        internal string GetCodTurnero()
        {
            return CodTurnero;
        }

        internal void SetCodTurnero(string pvStrCodTurnero)
        {
            CodTurnero = pvStrCodTurnero;
        }

        internal void SetNumTurnero(string pvStrNumTurnero)
        {
            NumTurnero = pvStrNumTurnero;
        }

        internal string GetNumTurnero()
        {
            return NumTurnero;
        }

        internal string GetCodEquipo()
        {
            return CodEquipo;
        }
        internal void SetNumEquipo(string pvStrNumEquipo)
        {
            NumEquipo = pvStrNumEquipo;
        }
        internal string GetNumEquipo()
        {
            return NumEquipo;
        }

        internal void SetNumTurno(int pvIntNumTurno)
        {
            NumTurno = pvIntNumTurno;
        }
        internal int GetNumTurno()
        {
            return NumTurno;
        }

        internal void SetTurnosAnt(string pvStrTurnosAnt)
        {
            TurnosAnt.Add(pvStrTurnosAnt);
        }
        internal List<string> GetTurnosAnt()
        {
            return TurnosAnt;
        }



    }
}
