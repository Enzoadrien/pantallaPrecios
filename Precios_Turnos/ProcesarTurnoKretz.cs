using DataGate;
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
using static Precios_Turnos.StateObject;

namespace Precios_Turnos
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

        internal Turno NuevoTurno(string cmd85)
        {
            ////Interpreta el nuevo item
            Turno nuevoTurno = new Turno();

            nuevoTurno.SetCodTurnero(cmd85.Substring(0, 1));
            nuevoTurno.SetNumTurnero(cmd85.Substring(1, 2));
            nuevoTurno.SetNumEquipo(cmd85.Substring(7, 2));
            int numTurno = new Recursos().CargarNumeroTurno();
            nuevoTurno.SetNumTurno(++numTurno);

            while (true)
            {
                try
                {
                    using (Stream stream = new FileStream(@".\turnoAnt.3k", FileMode.Open))
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
            }
            return nuevoTurno;
        }

        internal Turno RegresarTurno(string cmd85)
        {
            ////Interpreta el nuevo item
            Turno nuevoTurno = new Turno();

            nuevoTurno.SetCodTurnero(cmd85.Substring(0, 1));
            nuevoTurno.SetNumTurnero(cmd85.Substring(1, 2));
            nuevoTurno.SetNumEquipo(cmd85.Substring(7, 2));
            int numTurno = new Recursos().CargarNumeroTurno();
            if(numTurno == 0)
                nuevoTurno.SetNumTurno(numTurno);
            else
                nuevoTurno.SetNumTurno(--numTurno);

            while (true)
            {
                try
                {
                    using (Stream stream = new FileStream(@".\turnoAnt.3k", FileMode.Open))
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
            }

            return nuevoTurno;
        }

        internal Turno SetearTurno(string cmd85)
        {
            ////Interpreta el nuevo item
            Turno nuevoTurno = new Turno();

            nuevoTurno.SetCodTurnero(cmd85.Substring(0, 1));
            nuevoTurno.SetNumTurnero(cmd85.Substring(1, 2));
            nuevoTurno.SetNumTurno(int.Parse(cmd85.Substring(7, 6)));
            nuevoTurno.SetNumEquipo(cmd85.Substring(13, 2));
            return nuevoTurno;
        }

        internal Turno CargarTurno(string cmd8550)
        {
            string[] cmd85 = cmd8550.Split('|');
            ////Interpreta el nuevo item
            Turno nuevoTurno = new Turno();

            nuevoTurno.SetCodTurnero("D");
            nuevoTurno.SetNumTurnero(cmd85[0].Substring(7, 2));
            nuevoTurno.SetNumEquipo(cmd85[0].Substring(1, 2));
            nuevoTurno.SetNumTurno(int.Parse(cmd85[0].Substring(9, 6)));
            /*if (cmd85.Length == 2)
            {
                string[] cmd = cmd85[1].Split('-');
                nuevoTurno.SetNumTurnoAnt(int.Parse(cmd[0]));
                nuevoTurno.SetNumEquipoAnt(cmd[1].Substring(0, 2));
            }*/

            return nuevoTurno;
        }

        internal string Respuesta(StateObject pvStateObject)  
        {
            // Arma respuesta determinado el estado actual.
            string respuesta=string.Empty;
            if (pvStateObject.GetTurno() != null) 
            { 
                respuesta = pvStateObject.GetTurno().GetCodEquipo();
                respuesta += pvStateObject.GetTurno().GetNumEquipo();
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
                    new Recursos().MostrarTurno(pvStateObject.GetTurno().GetNumTurno(), pvStateObject.GetTurno().GetNumEquipo(), pvStateObject.GetTurno().GetTurnosAnt());
                    break;

                case EstadoTurno.NUEVO_TURNO:
                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().GetNumTurnero();
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().GetNumTurno().ToString(), 6);
                    //respuesta += "|" + pvStateObject.GetTurno().GetNumTurnoAnt() + "-" + pvStateObject.GetTurno().GetNumEquipoAnt();
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    new Recursos().GuardarNumeroTurno(pvStateObject.GetTurno().GetNumTurno());
                    new Recursos().MostrarTurno(pvStateObject.GetTurno().GetNumTurno(), pvStateObject.GetTurno().GetNumEquipo(), pvStateObject.GetTurno().GetTurnosAnt());
                    new Recursos().GuardarTurnoAnt(pvStateObject.GetTurno().GetNumTurno(), pvStateObject.GetTurno().GetNumEquipo());
                    break;
                case EstadoTurno.ANTERIOR_TURNO:
                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().GetNumTurnero();
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().GetNumTurno().ToString(),6);
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    new Recursos().GuardarNumeroTurno(pvStateObject.GetTurno().GetNumTurno());
                    new Recursos().MostrarTurno(pvStateObject.GetTurno().GetNumTurno(), pvStateObject.GetTurno().GetNumEquipo(), pvStateObject.GetTurno().GetTurnosAnt());
                    new Recursos().GuardarTurnoAnt(pvStateObject.GetTurno().GetNumTurno(), pvStateObject.GetTurno().GetNumEquipo());
                    break;
                case EstadoTurno.SETEAR_TURNO:
                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().GetNumTurnero();
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().GetNumTurno().ToString(), 6);
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    new Recursos().GuardarNumeroTurno(pvStateObject.GetTurno().GetNumTurno());
                    new Recursos().MostrarTurno(pvStateObject.GetTurno().GetNumTurno(), pvStateObject.GetTurno().GetNumEquipo(), pvStateObject.GetTurno().GetTurnosAnt());
                    new Recursos().GuardarTurnoAnt(pvStateObject.GetTurno().GetNumTurno(), pvStateObject.GetTurno().GetNumEquipo());
                    break;
            }
            return respuesta;
        }
    }
}
