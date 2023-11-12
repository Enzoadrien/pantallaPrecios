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
                try
                {
                    List<string>? turnosAnteriores = new List<string>();
                    using (Stream stream = new FileStream(@".\turnoAnt.3k", FileMode.Open))
                    {
                        var sr = new StreamReader(stream);
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            turnosAnteriores.Add(line);
                        }
                        stream.Close();
                    }

                    if (turnosAnteriores.Count() < 3)
                        turnosAnteriores.Add(numeroTurno.ToString() + '|' + numeroEquipo);
                    else
                    {
                        turnosAnteriores.RemoveAt(0);
                        turnosAnteriores.Add(numeroTurno.ToString() + '|' + numeroEquipo);
                    }
                    
                        File.WriteAllLines(@".\turnoAnt.3k", turnosAnteriores.ToArray());
                }
                catch
                {
                }
        }

        internal void MostrarTurno(int numeroTurno, string numeroEquipo, List<string>? turnosAnteriores = null, Window? parentWindow = null)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(async () =>
                {
                    MostrarTurno mostrarTurno = new MostrarTurno(false,parentWindow);
                    mostrarTurno.WindowStyle = WindowStyle.None;
                    mostrarTurno.ShowInTaskbar = false;
                    mostrarTurno.NumeroTurno.Content = numeroTurno;
                    mostrarTurno.NumeroEquipo.Content = numeroEquipo;
                    mostrarTurno.NumeroTurnoAnt.Content = "";
                    mostrarTurno.NumeroEquipoAnt.Content = "";
                    if (turnosAnteriores != null)
                    {

                        foreach (string text in turnosAnteriores)
                        {
                            string[] anteriores = text.Split('|');
                            mostrarTurno.NumeroTurnoAnt.Content = mostrarTurno.NumeroTurnoAnt.Content + anteriores[0] + "\n";
                            mostrarTurno.NumeroEquipoAnt.Content = mostrarTurno.NumeroEquipoAnt.Content + anteriores[1] + "\n";
                        }
                        
                    }
                    


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

                                    }

                                }
                                stream.Close();
                            }
                        }
                        catch { }
                    }
                    else
                        mostrarTurno.NombreEquipo.Content = "";

                     mostrarTurno.Show();
                }));
            }
            catch (Exception e){
            
            }
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
            pVentana.btnCancelar.Margin = new Thickness(5,5,5,5);
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
