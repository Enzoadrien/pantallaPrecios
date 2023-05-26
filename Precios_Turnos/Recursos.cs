using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Precios_Turnos
{
    public class Recursos
    {
        public enum TipoMensaje
        {
            ERROR, ADVERTENCIA, ACEPTAR
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

        internal void GuardarTurnoAnt(int numeroTurno, string numeroEquipo)
        {
            while (true)
            {
                try
                {

                    using (Stream stream = new FileStream(@".\turnoAnt.3k", FileMode.Open))
                    {
                        stream.SetLength(0);
                        byte[] bytes = Encoding.UTF8.GetBytes(numeroTurno.ToString() + '|' + numeroEquipo);
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

        internal void MostrarTurno(int numeroTurno, string numeroEquipo, int numeroTurnoAnt, string numeroEquipoAnt)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MostrarTurno mostrarTurno = new MostrarTurno();
                    mostrarTurno.WindowStyle = WindowStyle.None;
                    mostrarTurno.ShowInTaskbar = false;
                    mostrarTurno.NumeroTurno.Content = numeroTurno;
                    mostrarTurno.NumeroEquipo.Content = numeroEquipo;
                    mostrarTurno.NumeroTurnoAnt.Content = numeroTurnoAnt == 0 ? "" : numeroTurnoAnt;
                    mostrarTurno.NumeroEquipoAnt.Content = numeroEquipoAnt.Equals(string.Empty) ? "" : numeroEquipoAnt;


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
                                    if (equipo.Length == 2)
                                    {
                                        if (equipo[0].Equals(numeroEquipo))
                                            mostrarTurno.NombreEquipo.Content = equipo[1];
                                        if (equipo[0].Equals(numeroEquipo))
                                        {
                                            if (!numeroEquipoAnt.Equals(string.Empty))
                                                mostrarTurno.NombreEquipoAnt.Content = equipo[1];
                                            else
                                                mostrarTurno.NombreEquipoAnt.Content = string.Empty;
                                        }
                                            
                                    }

                                }
                                stream.Close();
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        mostrarTurno.NombreEquipo.Content = "";
                        mostrarTurno.NombreEquipoAnt.Content = "";
                    }
                    mostrarTurno.Show();


                }));
            }
            catch { }
        }
    }
}
