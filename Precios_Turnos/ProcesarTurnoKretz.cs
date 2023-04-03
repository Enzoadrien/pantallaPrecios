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
                    pvStateObject.SetEstadoActual(StateObject.EstadoTurno.REPLICA_TURNO);
                    break;
                case 8501:
                    pvStateObject.SetTurno(NuevoTurno(pvSrtComando));
                    pvStateObject.SetEstadoActual(StateObject.EstadoTurno.NUEVO_TURNO);
                    break;
                case 8502:
                    pvStateObject.SetTurno(RegresarTurno(pvSrtComando));
                    pvStateObject.SetEstadoActual(StateObject.EstadoTurno.ANTERIOR_TURNO);
                    break;
                case 8503:
                    pvStateObject.SetTurno(SetearTurno(pvSrtComando));
                    pvStateObject.SetEstadoActual(StateObject.EstadoTurno.SETEAR_TURNO);
                    break;
                case 8550:
                    pvStateObject.SetTurno(CargarTurno(pvSrtComando));
                    pvStateObject.SetEstadoActual(StateObject.EstadoTurno.SOLO_MOSTRAR_TURNO);
                    break;
                default:
                    Console.WriteLine("***Comando enviado desconocido****");
                    pvStateObject.SetEstadoActual(StateObject.EstadoTurno.ERROR);
                    break;
            }
            if (pvStateObject.GetEstadoActual().Equals(StateObject.EstadoTurno.ERROR))
            {
                return (string.Empty);
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
            int numTurno = CargarNumeroTurno();
            nuevoTurno.SetNumTurno(++numTurno);

            return nuevoTurno;
        }

        internal Turno RegresarTurno(string cmd85)
        {
            ////Interpreta el nuevo item
            Turno nuevoTurno = new Turno();

            nuevoTurno.SetCodTurnero(cmd85.Substring(0, 1));
            nuevoTurno.SetNumTurnero(cmd85.Substring(1, 2));
            nuevoTurno.SetNumEquipo(cmd85.Substring(7, 2));
            int numTurno = CargarNumeroTurno();
            if(numTurno == 0)
                nuevoTurno.SetNumTurno(numTurno);
            else
                nuevoTurno.SetNumTurno(--numTurno);

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

        internal Turno CargarTurno(string cmd85)
        {
            ////Interpreta el nuevo item
            Turno nuevoTurno = new Turno();

            nuevoTurno.SetCodTurnero("D");
            nuevoTurno.SetNumTurnero(cmd85.Substring(7, 2));
            nuevoTurno.SetNumEquipo(cmd85.Substring(1, 2));
            nuevoTurno.SetNumTurno(int.Parse(cmd85.Substring(9, 6)));

            return nuevoTurno;
        }

        internal int CargarNumeroTurno()
        {
            while (true)
            {
                try
                {
                    using (Stream stream = new FileStream(@".\controlTurno.3k", FileMode.Open))
                    {
                        var sr = new StreamReader(stream);

                        string line;
                        int numeroTurno = 0;
                        while ((line = sr.ReadLine()) != null)
                        {
                            numeroTurno = int.Parse(line);
                        }
                        stream.Close();
                        return numeroTurno;
                    }
                }
                catch
                {
                }
            }
        }

        internal void GuardarNumeroTurno(int numeroTurno)
        {
            while (true)
            {
                try
                {

                    using (Stream stream = new FileStream(@".\controlTurno.3k", FileMode.Open))
                    {
                        stream.SetLength(0);
                        byte[] bytes = Encoding.UTF8.GetBytes(numeroTurno.ToString());
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Close();
                        break;
                    }
                }
                catch
                {
                }
            }
        }

        internal void MostrarTurno(int numeroTurno, int numeroEquipo)
        {
            try {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MostrarTurno mostrarTurno = new MostrarTurno();
                    mostrarTurno.WindowStyle = WindowStyle.None;
                    mostrarTurno.ShowInTaskbar = false;
                    mostrarTurno.NumeroTurno.Content = numeroTurno;
                    mostrarTurno.NumeroEquipo.Content = numeroEquipo;

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    if (config.AppSettings.Settings["MostrarNombres"].Value.Equals("true"))
                    {
                        try
                        {
                            using (Stream stream = new FileStream(@".\nombreEquipos.3k", FileMode.Open))
                            {
                                var sr = new StreamReader(stream);

                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    string[] equipo = line.Split('=');
                                    if(equipo.Length == 2)
                                        if (equipo[0].Equals(numeroEquipo.ToString()))
                                            mostrarTurno.NombreEquipo.Content = equipo[1];
                                }
                                stream.Close();
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        mostrarTurno.NombreEquipo.Content = "";
                    }
                    mostrarTurno.Show();

                    try
                    {
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(@".\audios\" + config.AppSettings.Settings["AudioTurnero"].Value);
                        player.Play();
                    }
                    catch { }
                }));
            }
            catch { }
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
                    MostrarTurno(pvStateObject.GetTurno().GetNumTurno(), int.Parse(pvStateObject.GetTurno().GetNumEquipo()));
                    break;

                case EstadoTurno.NUEVO_TURNO:
                   
                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().GetNumTurnero();
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().GetNumTurno().ToString(), 6);
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    GuardarNumeroTurno(pvStateObject.GetTurno().GetNumTurno());
                    MostrarTurno(pvStateObject.GetTurno().GetNumTurno(), int.Parse(pvStateObject.GetTurno().GetNumEquipo()));
                    break;
                case EstadoTurno.ANTERIOR_TURNO:

                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().GetNumTurnero();
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().GetNumTurno().ToString(),6);
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    GuardarNumeroTurno(pvStateObject.GetTurno().GetNumTurno());
                    MostrarTurno(pvStateObject.GetTurno().GetNumTurno(), int.Parse(pvStateObject.GetTurno().GetNumEquipo()));
                    break;
                case EstadoTurno.SETEAR_TURNO:

                    respuesta += TURNOSETEADO;
                    respuesta += pvStateObject.GetTurno().GetNumTurnero();
                    respuesta += new Seguridad().CadenaConCeros(pvStateObject.GetTurno().GetNumTurno().ToString(), 6);
                    pvStateObject.SetEstadoActual(EstadoTurno.ESTADOINICIAL);
                    GuardarNumeroTurno(pvStateObject.GetTurno().GetNumTurno());
                    MostrarTurno(pvStateObject.GetTurno().GetNumTurno(), int.Parse(pvStateObject.GetTurno().GetNumEquipo()));
                    break;
            }
            Comunicacion com = new Comunicacion();
            return com.CrearComandoBascula(respuesta);
        }
    }
}
