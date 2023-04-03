using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Precios_Turnos
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

        internal Turno Turno;

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

        internal void SetTurno(Turno turno)
        {
            Turno = turno;
        }
        internal Turno GetTurno()
        {
            return Turno;
        }
    }
}
