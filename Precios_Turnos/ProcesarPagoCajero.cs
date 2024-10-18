using DataGate;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Precios_Turnos.StateObjectCajero;

namespace Precios_Turnos
{
    class ProcesarPagoCajero
    {
        internal string ProcesarComando(string pvSrtComando, StateObjectCajero pvStateObject)
        {
            try
            {

                Pago? pago = JsonSerializer.Deserialize<Pago>(pvSrtComando);

                pvStateObject.SetEstadoActual(EstadoCajero.NUEVO_PAGO);
                pvStateObject.SetPago(NuevoPago(pago));
                if(pvStateObject.GetPago() != null)
                {
                    if(pvStateObject.GetPago().CantidadTotal == 0)
                        pvStateObject.SetEstadoActual(EstadoCajero.CANCELADO);
                    else
                        pvStateObject.SetEstadoActual(EstadoCajero.ESTADOFINAL);
                }
                else
                {
                    pvStateObject.SetError("No se puede procesar el pago.");
                    pvStateObject.SetEstadoActual(EstadoCajero.ERROR);
                }

            }
            catch (Exception e){
                pvStateObject.SetError(e.Message);
                pvStateObject.SetEstadoActual(EstadoCajero.ERROR);
            }
            return Respuesta(pvStateObject);
        }

        internal Pago? NuevoPago(Pago? pvPago)
        {
            try
            {

                pvPago.CantidadFaltante = Convert.ToInt32(pvPago.CantidadTotal);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MostrarVentanaSplash dialog = new MostrarVentanaSplash(false, null, "", pvPago);
                    dialog.ShowDialog();
                    pvPago = dialog.recuperaPago();
                }));
            }
            catch { pvPago = null; }
            return pvPago;
        }


        internal string Respuesta(StateObjectCajero pvStateObject)  
        {
            // Arma respuesta determinado el estado actual.
            string respuesta=string.Empty;

            switch (pvStateObject.GetEstadoActual())
            {
                case EstadoCajero.ERROR:
                    respuesta = "Error:" + pvStateObject.GetError();
                    break;
                case EstadoCajero.CANCELADO:
                    respuesta = "CANCEL";
                    break;
                case EstadoCajero.ESTADOFINAL:
                    respuesta = "OK";
                    break;
                default:
                    respuesta = "No se pudo completar el pago.";
                    break;
            }
            return respuesta;
        }
    }
}
