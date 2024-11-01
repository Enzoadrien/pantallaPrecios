using DataGate;
using Precios_Turnos;
using Priceio;
using Priceio.Cajero.Hopper;
using Priceio.Cajero.Payout;
using Priceio.Cajero.VentanasCajero;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using static Priceio.Cajero.StateObjectCajero;

namespace Priceio.Cajero
{
    class ProcesarPagoCajero
    {
        private SMARTPayout smartPayout;
        private SMARTHopper smartHopper;

        internal async Task<string> ProcesarComando(string pvSrtComando, StateObjectCajero pvStateObject, SMARTPayout pSmartPayout, SMARTHopper pSmartHopper)
        {
            try
            {
                Pago? pago = JsonSerializer.Deserialize<Pago>(pvSrtComando);
                smartPayout = pSmartPayout;
                smartHopper = pSmartHopper;
                switch (pago.TipoPago)
                {
                    case Pago.Tipo.PAGO:
                        pvStateObject.SetEstadoActual(EstadoCajero.NUEVO_PAGO);
                        pvStateObject.SetPago(NuevoPago(pago));
                        if (pvStateObject.GetPago() != null)
                        {
                            if (pvStateObject.GetPago().EstadoPago == Pago.Estado.OK)
                                pvStateObject.SetEstadoActual(EstadoCajero.OK);
                            else if(pvStateObject.GetPago().EstadoPago == Pago.Estado.CANCELADO)
                                pvStateObject.SetEstadoActual(EstadoCajero.CANCELADO);
                            else if(pvStateObject.GetPago().EstadoPago == Pago.Estado.SIN_EFECTIVO)
                                pvStateObject.SetEstadoActual(EstadoCajero.SIN_EFECTIVO);
                            else
                                pvStateObject.SetEstadoActual(EstadoCajero.ERROR);
                        }
                        else
                        {
                            pvStateObject.SetError("No se puede procesar el pago.");
                            pvStateObject.SetEstadoActual(EstadoCajero.ERROR);
                        }
                        break;
                    case Pago.Tipo.RETIRO:
                        pvStateObject.SetEstadoActual(EstadoCajero.NUEVO_PAGO);
                        pvStateObject.SetPago(NuevoRetiro(pago));
                        if (pvStateObject.GetPago() != null)
                        {
                            if (pvStateObject.GetPago().EstadoPago == Pago.Estado.OK)
                                pvStateObject.SetEstadoActual(EstadoCajero.OK);
                            else if (pvStateObject.GetPago().EstadoPago == Pago.Estado.CANCELADO)
                                pvStateObject.SetEstadoActual(EstadoCajero.CANCELADO);
                            else if (pvStateObject.GetPago().EstadoPago == Pago.Estado.SIN_EFECTIVO)
                                pvStateObject.SetEstadoActual(EstadoCajero.SIN_EFECTIVO);
                            else
                                pvStateObject.SetEstadoActual(EstadoCajero.ERROR);
                        }
                        else
                        {
                            pvStateObject.SetError("No se puede procesar el pago.");
                            pvStateObject.SetEstadoActual(EstadoCajero.ERROR);
                        }
                        break;
    
                    case Pago.Tipo.IMPRESION:
                        pvStateObject.SetPago(pago);
                        PrintDocument pdoc = new PrintDocument();
                        pdoc.DocumentName = pago.NumPago.ToString();
                        pdoc.PrinterSettings.PrinterName = "POS58";
                        pdoc.PrintPage += (sender, e) => Document_PrintText(e, pago.Impresion.Replace("<br>", "\n"));
                        pdoc.Print();
                        pvStateObject.SetEstadoActual(EstadoCajero.OK);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                pvStateObject.SetError(e.Message);
                pvStateObject.SetEstadoActual(EstadoCajero.ERROR);
            }
            return Respuesta(pvStateObject);
        }

        private void Document_PrintText(PrintPageEventArgs e, string inputString)
        {
            Image img = Image.FromFile(@".\Recursos\logo.png");
            e.Graphics.DrawImage(img, 0, 0, 70, 70);
            e.Graphics.DrawString(inputString, new Font("Courier New", 8, System.Drawing.FontStyle.Bold), Brushes.Black, 0, 0);
        }

        internal Pago? NuevoRetiro(Pago? pvPago)
        {
            try
            {
                pvPago.CantidadIngresada = 0;
                pvPago.CantidadFaltante = 0;
                pvPago.Cambio = Convert.ToInt32(pvPago.CantidadTotal);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    pvPago.Pagado = true;
                    MostrarVentanaSplash dialog = new MostrarVentanaSplash(false, null, "", pvPago, smartPayout, smartHopper);
                    dialog.IniciarProcesosCajero();
                    dialog.ShowDialog();
                    pvPago = dialog.recuperaPago();
                }));
            }
            catch { pvPago = null; }
            return pvPago;
        }

        internal Pago? NuevoPago(Pago? pvPago)
        {
            try
            {
                pvPago.CantidadFaltante = Convert.ToInt32(pvPago.CantidadTotal);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MostrarVentanaSplash dialog = new MostrarVentanaSplash(false, null, "", pvPago, smartPayout, smartHopper);
                    dialog.IniciarProcesosCajero();
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
            string respuesta = string.Empty;

            switch (pvStateObject.GetEstadoActual())
            {
                case EstadoCajero.ERROR:
                    respuesta = "ERROR|"+ pvStateObject.GetPago().CantidadIngresada + "|0|" + pvStateObject.GetError()+"|";
                    break;
                case EstadoCajero.CANCELADO:
                    respuesta = "CANCEL|"+ pvStateObject.GetPago().CantidadIngresada + "|" + pvStateObject.GetPago().Cambio +"|";
                    break;
                case EstadoCajero.OK:
                    respuesta = "OK|" + pvStateObject.GetPago().CantidadIngresada + "|" + pvStateObject.GetPago().Cambio+"|";
                    break;
                case EstadoCajero.SIN_EFECTIVO:
                    respuesta = "SIN_EFECTIVO|0|0|0|";
                    break;
                default:
                    respuesta = "ERROR|0|0|NO SE COMPRETO EL PAGO|";
                    break;
            }
            return respuesta;
        }
    }
}
