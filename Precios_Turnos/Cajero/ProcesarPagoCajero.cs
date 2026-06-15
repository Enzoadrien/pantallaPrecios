using Priceio.Cajero.Hopper;
using Priceio.Cajero.Payout;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Priceio.Cajero.StateObjectCajero;

namespace Priceio.Cajero
{
    class ProcesarPagoCajero
    {
        private SMARTPayout? smartPayout;
        private SMARTHopper? smartHopper;

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
                        ConfiguracionImpresora? SQLiteClass = new SQLiteClassManager().GetConfiguracionImpresora();
                        pvStateObject.SetPago(pago);
                        PrintDocument pdoc = new PrintDocument();
                        pdoc.DocumentName = pago.NumPago.ToString();
                        if (SQLiteClass != null)
                        {
                            pdoc.PrinterSettings.PrinterName = SQLiteClass.Nombre;
                            pdoc.PrintPage += (sender, e) => Document_PrintText(e, pago.Impresion.Replace("<br>", "\n"), SQLiteClass);
                        }
                        else
                        {
                            pdoc.PrinterSettings.PrinterName = "POS58";
                            pdoc.PrintPage += (sender, e) => Document_PrintText(e, pago.Impresion.Replace("<br>", "\n"), null);
                        }
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

        private void Document_PrintText(PrintPageEventArgs e, string inputString, ConfiguracionImpresora? configuracionImpresora)
        {
            if (configuracionImpresora != null)
            {
                if (configuracionImpresora.Logo == true)
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(@".\Recursos\logo.png");
                    if (configuracionImpresora.RutaLogo.Length > 0)
                        img = System.Drawing.Image.FromFile(@".\data\impresora\"+configuracionImpresora.RutaLogo);
                    e.Graphics.DrawImage(img, configuracionImpresora.CordenadaXLogo, configuracionImpresora.CordenadaYLogo, configuracionImpresora.TamanoLogo, configuracionImpresora.TamanoLogo);
                }

                if (configuracionImpresora.Negrita == true && configuracionImpresora.Cursiva == true)
                    e.Graphics.DrawString(inputString, new Font(configuracionImpresora.TipoLetra, configuracionImpresora.TamanoLetra, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic), System.Drawing.Brushes.Black, 0, 0);
                else if (configuracionImpresora.Negrita == true && configuracionImpresora.Cursiva == false)
                    e.Graphics.DrawString(inputString, new Font(configuracionImpresora.TipoLetra, configuracionImpresora.TamanoLetra, System.Drawing.FontStyle.Bold), System.Drawing.Brushes.Black, 0, 0);
                else if (configuracionImpresora.Negrita == false && configuracionImpresora.Cursiva == true)
                    e.Graphics.DrawString(inputString, new Font(configuracionImpresora.TipoLetra, configuracionImpresora.TamanoLetra, System.Drawing.FontStyle.Italic), System.Drawing.Brushes.Black, 0, 0);
                else
                    e.Graphics.DrawString(inputString, new Font(configuracionImpresora.TipoLetra, configuracionImpresora.TamanoLetra), System.Drawing.Brushes.Black, 0, 0);

            }
            else
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(@".\Recursos\logo.png");
                e.Graphics.DrawImage(img, 0, 0, 70, 70);
                e.Graphics.DrawString(inputString, new Font("Courier New", 8, System.Drawing.FontStyle.Bold), Brushes.Black, 0, 0);
            }
            
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
            pvStateObject.GetPago().Fecha = DateOnly.FromDateTime(DateTime.Now);
            pvStateObject.GetPago().Hora = TimeOnly.FromDateTime(DateTime.Now);
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
            if(pvStateObject.GetPago().TipoPago == Pago.Tipo.PAGO || pvStateObject.GetPago().TipoPago == Pago.Tipo.RETIRO)
                if (!new SQLiteClassManager().SetPago(pvStateObject.GetPago()))
                {
                    Mensajes dialog = new Mensajes(Recursos.TipoMensaje.ERROR, false);
                    dialog.lblNombre.Content = "¡Error!";
                    dialog.lblTexto.Text = "Ocurrio un error al guardar la información del pago, consulte al administrador";
                    dialog.btnCancelar.Visibility = Visibility.Visible;
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.ShowDialog();
                }
            return respuesta;
        }
    }
}
