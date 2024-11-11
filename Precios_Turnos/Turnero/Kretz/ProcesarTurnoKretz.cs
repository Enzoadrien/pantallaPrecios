using DataGate;
using Precios_Turnos;
using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Priceio.Turnero.StateObject;

namespace Priceio.Turnero.Kretz
{
    class ProcesarTurnoKretz
    {
        internal static string TURNOSETEADO = "8550";

        internal string ProcesarComando(string pvSrtComando, StateObject pvStateObject)
        {
            int numeroComando = int.Parse(pvSrtComando.Substring(3, 4));

            switch (numeroComando)
            {
                case 8500:
                    pvStateObject.SetEstadoActual(EstadoTurno.REPLICA_TURNO);
                    break;
                case 8501:
                    pvStateObject.SetTurno(NuevoTurno(pvSrtComando));
                    pvStateObject.SetEstadoActual(EstadoTurno.NUEVO_TURNO);
                    break;
                case 8502:
                    pvStateObject.SetTurno(RegresarTurno(pvSrtComando));
                    pvStateObject.SetEstadoActual(EstadoTurno.ANTERIOR_TURNO);
                    break;
                case 8503:
                    pvStateObject.SetTurno(SetearTurno(pvSrtComando));
                    pvStateObject.SetEstadoActual(EstadoTurno.SETEAR_TURNO);
                    break;
                case 8550:
                    pvStateObject.SetTurno(CargarTurno(pvSrtComando));
                    pvStateObject.SetEstadoActual(EstadoTurno.SOLO_MOSTRAR_TURNO);
                    break;
                default:
                    pvStateObject.SetEstadoActual(EstadoTurno.ERROR);
                    break;
            }
            if (pvStateObject.GetEstadoActual().Equals(EstadoTurno.ERROR))
            {
                return string.Empty;
            }
            else
            {
                return Respuesta(pvStateObject);
                // Echo the data back to the client.  
            }
        }

        internal Turnero NuevoTurno(string cmd85)
        {
            ////Interpreta el nuevo item
            Turnero nuevoTurno = new Turnero();

            nuevoTurno.CodigoTurnero = cmd85.Substring(0, 1);
            nuevoTurno.NumeroTurnero = cmd85.Substring(1, 2);
            nuevoTurno.NumeroEquipo = cmd85.Substring(7, 2);
            int numTurno = new ControlTurno().CargarNumeroTurno().NumeroTurno;
            nuevoTurno.NumeroTurno = ++numTurno;

            /*while (true)
            {
                try
                {
                    using (Stream stream = new FileStream(@".\Recursos\turnoAnt.3k", FileMode.Open))
                    {
                        var sr = new StreamReader(stream);

                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            nuevoTurno.SetTurnosAnt(line);
                        }
                        stream.Close();
                        break;
                    }
                }
                catch
                {
                }
            }*/
            return nuevoTurno;
        }

        internal Turnero RegresarTurno(string cmd85)
        {
            ////Interpreta el nuevo item
            Turnero nuevoTurno = new Turnero();

            nuevoTurno.CodigoTurnero = cmd85.Substring(0, 1);
            nuevoTurno.NumeroTurnero = cmd85.Substring(1, 2);
            nuevoTurno.NumeroEquipo = cmd85.Substring(7, 2);
            int numTurno = new ControlTurno().CargarNumeroTurno().NumeroTurno;
            if (numTurno == 0)
                nuevoTurno.NumeroTurno = numTurno;
            else
                nuevoTurno.NumeroTurno = --numTurno;

            /*while (true)
            {
                try
                {
                    using (Stream stream = new FileStream(@".\Recursos\turnoAnt.3k", FileMode.Open))
                    {
                        var sr = new StreamReader(stream);

                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            //nuevoTurno.SetTurnosAnt(line);
                        }
                        stream.Close();
                        break;
                    }
                }
                catch
                {
                }
            }*/

            return nuevoTurno;
        }

        internal Turnero SetearTurno(string cmd85)
        {
            ////Interpreta el nuevo item
            Turnero nuevoTurno = new Turnero();

            nuevoTurno.CodigoTurnero = cmd85.Substring(0, 1);
            nuevoTurno.NumeroTurnero = cmd85.Substring(1, 2);
            nuevoTurno.NumeroTurno = int.Parse(cmd85.Substring(7, 6));
            nuevoTurno.NumeroEquipo = cmd85.Substring(13, 2);
            return nuevoTurno;
        }

        internal Turnero CargarTurno(string cmd8550)
        {
            string[] cmd85 = cmd8550.Substring(0, cmd8550.Length - 2).Split('-');
            ////Interpreta el nuevo item
            Turnero nuevoTurno = new Turnero();

            nuevoTurno.CodigoTurnero = "D";
            nuevoTurno.NumeroTurnero = cmd85[0].Substring(7, 2);
            nuevoTurno.NumeroEquipo = cmd85[0].Substring(1, 2);
            nuevoTurno.NumeroTurno = int.Parse(cmd85[0].Substring(9, 6));
            if (cmd85.Length > 2)
            {
                for (int x = 1; x < cmd85.Length; x++)
                {
                    //nuevoTurno.SetTurnosAnt(cmd85[x]);
                }
            }

            return nuevoTurno;
        }

        internal string turnosAnt(List<string>? turnosAnteriores = null)
        {
            string cadena = string.Empty;
            foreach (string turno in turnosAnteriores)
                cadena += "-" + turno;
            return cadena;
        }

        internal string Respuesta(StateObject pvStateObject)
        {
            // Arma respuesta determinado el estado actual.
            string respuesta = string.Empty;
            if (pvStateObject.GetTurno() != null)
            {
                respuesta = pvStateObject.GetTurno().CodigoEquipo;
                respuesta += pvStateObject.GetTurno().NumeroEquipo;
            }

            switch (pvStateObject.GetEstadoActual())
            {
                case EstadoTurno.REPLICA_TURNO:
                    respuesta = "OK";
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    break;

                case EstadoTurno.SOLO_MOSTRAR_TURNO:
                    respuesta = "OK";
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    MainWindow.listaTurnosKretz.Add(pvStateObject);
                    break;

                case EstadoTurno.NUEVO_TURNO:
                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().NumeroTurnero;
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().NumeroTurno.ToString(), 6);
                    //respuesta += turnosAnt(pvStateObject.GetTurno().GetTurnosAnt());
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    new ControlTurno().GuardarNumeroTurno(new Turno() {  NumeroTurno = pvStateObject.GetTurno().NumeroTurno, NumeroEquipo = pvStateObject.GetTurno().NumeroEquipo });
                    new ControlTurno().GuardarTurnoAnt(pvStateObject.GetTurno().NumeroTurno, pvStateObject.GetTurno().NumeroEquipo);
                    MainWindow.listaTurnosKretz.Add(pvStateObject);
                    break;
                case EstadoTurno.ANTERIOR_TURNO:
                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().NumeroTurnero;
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().NumeroTurno.ToString(), 6);
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    new ControlTurno().GuardarNumeroTurno(new Turno() { NumeroTurno = pvStateObject.GetTurno().NumeroTurno, NumeroEquipo = pvStateObject.GetTurno().NumeroEquipo });
                    new ControlTurno().GuardarTurnoAnt(pvStateObject.GetTurno().NumeroTurno, pvStateObject.GetTurno().NumeroEquipo);
                    MainWindow.listaTurnosKretz.Add(pvStateObject);
                    break;
                case EstadoTurno.SETEAR_TURNO:
                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().NumeroTurnero;
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().NumeroTurno.ToString(), 6);
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    new ControlTurno().GuardarNumeroTurno(new Turno() { NumeroTurno = pvStateObject.GetTurno().NumeroTurno, NumeroEquipo = pvStateObject.GetTurno().NumeroEquipo });
                    new ControlTurno().GuardarTurnoAnt(pvStateObject.GetTurno().NumeroTurno, pvStateObject.GetTurno().NumeroEquipo);
                    MainWindow.listaTurnosKretz.Add(pvStateObject);
                    break;
            }
            return respuesta;
        }
    }
}
