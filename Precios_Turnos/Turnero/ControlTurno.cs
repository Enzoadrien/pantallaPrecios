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
        internal int CargarNumeroTurno()
        {
            while (true)
            {
                try
                {
                    using (Stream stream = new FileStream(@".\Recursos\controlTurno.3k", FileMode.Open))
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

                    using (Stream stream = new FileStream(@".\Recursos\controlTurno.3k", FileMode.Open))
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

        internal void GuardarTurnoAnt(int numeroTurno, string numeroEquipo)
        {
            try
            {
                Dictionary<string, int>? turnosAnteriores = new Dictionary<string, int>();
                using (Stream stream = new FileStream(@".\Recursos\turnoAnt.3k", FileMode.Open))
                {
                    var sr = new StreamReader(stream);
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] turnos = line.Split('|');
                        turnosAnteriores.Add(turnos[1], int.Parse(turnos[0]));
                    }
                    stream.Close();
                }
                ConfiguracionTurnero? configuracionTurnero = new SQLiteClassManager().GetConfiguracionTurnero();
                if (configuracionTurnero != null)
                {
                    int TurnosAnt = configuracionTurnero.TurnosAnteriores;
                    if (turnosAnteriores.ContainsKey(numeroEquipo))
                        turnosAnteriores.Remove(numeroEquipo);

                    if (turnosAnteriores.Count() < TurnosAnt)
                        turnosAnteriores.Add(numeroEquipo, numeroTurno);

                    else
                    {
                        turnosAnteriores.Remove(turnosAnteriores.First().Key);
                        turnosAnteriores.Add(numeroEquipo, numeroTurno);
                    }
                    Dictionary<string, int>? turnosAnterioresOrdenados = turnosAnteriores.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                    using (Stream stream = new FileStream(@".\Recursos\turnoAnt.3k", FileMode.Open))
                    {
                        stream.SetLength(0);
                        foreach (KeyValuePair<string, int> entry in turnosAnterioresOrdenados)
                        {
                            byte[] bytes = Encoding.UTF8.GetBytes(entry.Value + "|" + entry.Key + "\n");
                            stream.Write(bytes, 0, bytes.Length);
                        }
                        stream.Close();
                    }
                }
            }
            catch
            {
            }
        }

        internal async Task MostrarTurno(int numeroTurno, string numeroEquipo, List<string>? turnosAnteriores = null, MainWindow? parentWindow = null)
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
                    if (turnosAnteriores != null)
                    {

                        foreach (string text in turnosAnteriores)
                        {
                            string[] anteriores = text.Split('|');
                            if (NumeroTurnoAnt != null)
                                NumeroTurnoAnt.Content = NumeroTurnoAnt.Content + anteriores[0] + "\n";
                            if (NumeroEquipoAnt != null)
                                NumeroEquipoAnt.Content = NumeroEquipoAnt.Content + anteriores[1] + "\n";
                        }
                    }
                    ConfiguracionTurnero? configuracionTurnero = new SQLiteClassManager().GetConfiguracionTurnero();
                    if (configuracionTurnero != null)
                    {
                        if (configuracionTurnero.MostrarNombres == true)
                        {
                            try
                            {
                                using (Stream stream = new FileStream(@".\Recursos\nombreEquipos.3k", FileMode.Open))
                                {
                                    var sr = new StreamReader(stream);

                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        string[] equipo = line.Split('=');
                                        if (equipo.Length == 2)
                                        {
                                            if (equipo[0].Equals(numeroEquipo))
                                            {
                                                Label NombreEquipo = (Label)mostrarTurno.FindName("NombreEquipo");
                                                if (NombreEquipo != null)
                                                    NombreEquipo.Content = equipo[1];
                                            }
                                        }

                                    }
                                    stream.Close();
                                }
                            }
                            catch { }
                        }
                    }
                    mostrarTurno.ShowDialog();
                }));
            }
            catch { }
        }
    }
}
