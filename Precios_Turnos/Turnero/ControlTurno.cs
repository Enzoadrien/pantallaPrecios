using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Priceio.Turnero
{
    internal class ControlTurno
    {
        internal Turno? CargarNumeroTurno()
        {
            Turno turno = new SQLiteClassManager().GetTurno();
            if (turno != null)
                return turno;
            return null ;
        }

        internal void GuardarNumeroTurno(Turno turno)
        {
            if (turno != null) 
                new SQLiteClassManager().SetTurno(turno);
           
        }

        internal List<TurnosAnteriores>? CargarTurnosAnteriores()
        {
            List<TurnosAnteriores>? turnosAnteriores = new SQLiteClassManager().GetTurnosAnteriores();
            if (turnosAnteriores != null)
                return turnosAnteriores;
            return null;
        }

        internal void GuardarTurnoAnt(int numeroTurno, string numeroEquipo)
        {
            try
            {
                List<TurnosAnteriores>? turnosAnteriores = new SQLiteClassManager().GetTurnosAnteriores();
                if (turnosAnteriores == null)
                    turnosAnteriores =  new List<TurnosAnteriores>(); 
                ConfiguracionTurnero? configuracionTurnero = new SQLiteClassManager().GetConfiguracionTurnero();
                if (configuracionTurnero != null)
                {
                    int TurnosAnt = configuracionTurnero.TurnosAnteriores;
                    TurnosAnteriores turnoAnterior = turnosAnteriores.Where(nc => nc.NumeroEquipo == numeroEquipo).FirstOrDefault();
                    if (turnoAnterior != null)
                        turnosAnteriores.Remove(turnoAnterior);

                    if (turnosAnteriores.Count() < TurnosAnt)
                        turnosAnteriores.Add(new TurnosAnteriores() { NumeroTurno=numeroTurno, NumeroEquipo = numeroEquipo});
                    else
                    {
                        turnosAnteriores.Remove(turnosAnteriores.First());
                        turnosAnteriores.Add(new TurnosAnteriores() { NumeroTurno = numeroTurno, NumeroEquipo = numeroEquipo });
                    }
                    new SQLiteClassManager().SetTurnosAnteriores(turnosAnteriores);
                }
            }
            catch
            {
            }
        }

        internal async Task MostrarTurno(int numeroTurno, string numeroEquipo, List<TurnosAnteriores>? turnosAnteriores = null, MainWindow? parentWindow = null)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                {
                    MostrarVentanaSplash mostrarTurno = new MostrarVentanaSplash(false, parentWindow);
                    mostrarTurno.WindowStyle = WindowStyle.None;
                    mostrarTurno.ShowInTaskbar = false;
                    mostrarTurno.CargarControles();

                    Label NumeroTurno = (Label)mostrarTurno.FindName("NumeroTurno");
                    if (NumeroTurno != null)
                        NumeroTurno.Content = numeroTurno;
                    Label NumeroEquipo = (Label)mostrarTurno.FindName("NumeroEquipo");
                    if (NumeroEquipo != null)
                        NumeroEquipo.Content = numeroEquipo;
                    Label NumeroTurnoAnt = (Label)mostrarTurno.FindName("NumeroTurnoAnt");
                    if (NumeroTurnoAnt != null)
                        NumeroTurnoAnt.Content = "";
                    Label NumeroEquipoAnt = (Label)mostrarTurno.FindName("NumeroEquipoAnt");
                    if (NumeroEquipoAnt != null)
                        NumeroEquipoAnt.Content = "";
                    Label NombreEquipoAnt = (Label)mostrarTurno.FindName("NombreEquipoAnt");
                    if (NombreEquipoAnt != null)
                        NombreEquipoAnt.Content = "";

                    if (turnosAnteriores != null)
                    {

                        foreach (TurnosAnteriores turnoAnterior in turnosAnteriores)
                        {
                            if (NumeroTurnoAnt != null)
                                NumeroTurnoAnt.Content = NumeroTurnoAnt.Content + turnoAnterior.NumeroTurno.ToString() + "\n";
                            if (NumeroEquipoAnt != null)
                                NumeroEquipoAnt.Content = NumeroEquipoAnt.Content + turnoAnterior.NumeroEquipo + "\n";
                            if (NombreEquipoAnt != null)
                            {
                                List<NombresClientesTurnero>? nombresClientesTurnero = new SQLiteClassManager().GetNombresClientesTurnero();
                                if (nombresClientesTurnero != null)
                                {
                                    if (NombreEquipoAnt != null)
                                        NombreEquipoAnt.Content = NombreEquipoAnt.Content +  nombresClientesTurnero.Where(nc => nc.Identificador == turnoAnterior.NumeroEquipo).Select(i => i.Nombre).First() + "\n";
                                }
                            }
                        }
                    }
                    ConfiguracionTurnero? configuracionTurnero = new SQLiteClassManager().GetConfiguracionTurnero();
                    if (configuracionTurnero != null)
                    {
                        if (configuracionTurnero.MostrarNombres == true)
                        {
                            List<NombresClientesTurnero>? nombresClientesTurnero = new SQLiteClassManager().GetNombresClientesTurnero();
                            if (nombresClientesTurnero != null)
                            {
                                Label NombreEquipo = (Label)mostrarTurno.FindName("NombreEquipo");
                                if (NombreEquipo != null)
                                    NombreEquipo.Content = nombresClientesTurnero.Where(nc => nc.Identificador == numeroEquipo).Select(i => i.Nombre).First();
                            }
                        }
                    }
                    mostrarTurno.ShowDialog();
                }));
            }
            catch { }
        }
    }
}
