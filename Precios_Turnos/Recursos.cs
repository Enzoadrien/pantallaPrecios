using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Precios_Turnos
{
    public class Recursos
    {
        public enum TipoMensaje
        {
            ERROR, ADVERTENCIA, ACEPTAR
        }

        public readonly IDictionary<Key, int> NumericKeys = new Dictionary<Key, int> {
        { Key.D0, 0 },
        { Key.D1, 1 },
        { Key.D2, 2 },
        { Key.D3, 3 },
        { Key.D4, 4 },
        { Key.D5, 5 },
        { Key.D6, 6 },
        { Key.D7, 7 },
        { Key.D8, 8 },
        { Key.D9, 9 },
        { Key.NumPad0, 0 },
        { Key.NumPad1, 1 },
        { Key.NumPad2, 2 },
        { Key.NumPad3, 3 },
        { Key.NumPad4, 4 },
        { Key.NumPad5, 5 },
        { Key.NumPad6, 6 },
        { Key.NumPad7, 7 },
        { Key.NumPad8, 8 },
        { Key.NumPad9, 9 }};

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
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                int TurnosAnt = int.Parse(config.AppSettings.Settings["TurnosAnteriores"].Value);
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
                  MostrarTurno mostrarTurno = new MostrarTurno(false, parentWindow);
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

                  Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                  if (config.AppSettings.Settings["MostrarNombres"].Value.Equals("true"))
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

                  mostrarTurno.ShowDialog();
              }));
            }
            catch { }
        }

        internal void ventanaMensajesGrande800x600(Mensajes pVentana)
        {
            pVentana.Width = 800;
            pVentana.Height = 600;
            pVentana.Salir.Width = 100;
            pVentana.Salir.Height = 100;
            pVentana.Salir.FontSize = 80;
            pVentana.lblNombre.FontSize = 100;
            pVentana.lblTexto.FontSize = 60;
            pVentana.btnOK.FontSize = 60;
            pVentana.btnOK.Width = 300;
            pVentana.btnOK.Height = 100;
            pVentana.btnCancelar.FontSize = 60;
            pVentana.btnCancelar.Width = 300;
            pVentana.btnCancelar.Height = 100;
            pVentana.btnCancelar.HorizontalAlignment = HorizontalAlignment.Left;
            pVentana.btnCancelar.Margin = new Thickness(5, 5, 5, 5);
        }

        internal void ventanaCapturaTextoGrande800x600(CapturaTexto pVentana)
        {
            pVentana.Width = 800;
            pVentana.Height = 600;
            pVentana.Salir.Width = 100;
            pVentana.Salir.Height = 100;
            pVentana.Salir.FontSize = 80;
            pVentana.lblNombre.FontSize = 100;
            pVentana.Texto.FontSize = 60;
            pVentana.btnOK.FontSize = 60;
            pVentana.btnOK.Width = 300;
            pVentana.btnOK.Height = 100;
            pVentana.btnCancelar.FontSize = 60;
            pVentana.btnCancelar.Width = 300;
            pVentana.btnCancelar.Height = 100;
            pVentana.btnCancelar.HorizontalAlignment = HorizontalAlignment.Left;
            pVentana.btnCancelar.Margin = new Thickness(5, 5, 5, 5);
        }
    }
}
