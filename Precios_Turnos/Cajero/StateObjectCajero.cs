using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Priceio.ClasesSQLite;

namespace Priceio.Cajero
{
    internal class StateObjectCajero
    {
        internal const int BufferSize = 1024;

        // Receive buffer.  
        internal byte[] buffer = new byte[BufferSize];

        // Received data string.
        internal StringBuilder sb = new StringBuilder();

        // Client socket.
        internal Socket? workSocket = null;

        internal EstadoCajero EstadoActual;

        internal Pago? Pago;

        internal string Error = string.Empty;

        internal enum EstadoCajero
        {
            ESTADOINICIAL, NUEVO_PAGO, OK, CANCELADO, SIN_EFECTIVO, ERROR
        }

        internal void SetEstadoActual(EstadoCajero pvEstadoActual)
        {
            EstadoActual = pvEstadoActual;
        }

        internal EstadoCajero GetEstadoActual()
        {
            return EstadoActual;
        }
        internal Pago? GetPago()
        {
            return Pago;
        }

        internal void SetPago(Pago? pago)
        {
            Pago = pago;
        }

        internal string GetError()
        {
            return Error;
        }

        internal void SetError(string error)
        {
            Error = error;
        }
    }
}
