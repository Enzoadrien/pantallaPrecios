using Priceio.ClasesSQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.Turnero
{
    internal class StateObject
    {
        internal const int BufferSize = 1024;

        // Receive buffer.  
        internal byte[] buffer = new byte[BufferSize];

        // Received data string.
        internal StringBuilder sb = new StringBuilder();

        // Client socket.
        internal Socket workSocket = null;

        internal Turnero Turno;

        internal EstadoTurno EstadoActual;

        internal enum EstadoTurno
        {
            ESTADOINICIAL, REPLICA_TURNO, NUEVO_TURNO, ANTERIOR_TURNO, SETEAR_TURNO, SOLO_MOSTRAR_TURNO, ERROR
        }

        internal void SetEstadoActual(EstadoTurno pvEstadoActual)
        {
            EstadoActual = pvEstadoActual;
        }

        internal EstadoTurno GetEstadoActual()
        {
            return EstadoActual;
        }

        internal void SetTurno(Turnero turno)
        {
            Turno = turno;
        }
        internal Turnero GetTurno()
        {
            return Turno;
        }
    }
}
