using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections;
using System.IO;
using Precios_Turnos;
using Priceio.Turnero.Kretz;
using Priceio.ClasesGenericas;

namespace Priceio.Turnero
{
    class AsynchronousSocketListener
    {
        internal static ManualResetEvent allDone = new ManualResetEvent(false);
        internal static Socket? listener;
        internal static bool start;
        internal static string? Protocolo;

        public static void StartListening()
        {

            start = true;
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".
            //  //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Protocolo = config.AppSettings.Settings["ProtocoloTurnero"].Value;
            int PuertoTurnero = int.Parse(config.AppSettings.Settings["PuertoTCP"].Value);
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PuertoTurnero);

            // Create a TCP/IP socket.  
            listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);


                Console.WriteLine("Esperando conexiones...");

                while (start)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections. 
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }
        public static void StopListening()
        {
            start = false;
            if (listener != null)
                listener.Close();
        }

        internal static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.  
                allDone.Set();

                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);


                Console.WriteLine("Conexion entrante de " + handler.RemoteEndPoint);
                // Create the state object.  
                StateObject state = new StateObject
                {
                    workSocket = handler
                };
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception) { }
        }

        internal static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                byte[] msg = null;
                while (start)
                {
                    string data = null;
                    // An incoming connection needs to be processed.  
                    while (start)
                    {
                        int bytesRec = handler.EndReceive(ar);
                        data += Encoding.ASCII.GetString(state.buffer, 0, bytesRec);
                        if (data.IndexOf(new Seguridad().ConvertirHEXToASCII("4")) > -1)
                        {
                            break;
                        }
                    }
                    switch (Protocolo)
                    {
                        case "K":
                            ProcesarTurnoKretz PT = new ProcesarTurnoKretz();
                            data = PT.ProcesarComando(data.Substring(1, data.Length - 2), state);
                            Task.Run(async () =>
                            {
                                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                                if (config.AppSettings.Settings["TipoTurnero"].Value.Equals("S"))
                                {
                                    int puerto = int.Parse(config.AppSettings.Settings["PuertoTCP"].Value);
                                    string[] clientes = config.AppSettings.Settings["Clientes"].Value.Split('|');
                                    if (clientes[0].Length > 0)
                                        foreach (string cliente in clientes)
                                            await new AsynchronousClient().StartClient(cliente, puerto, new Comunicacion().CrearComandoBascula(data));
                                }
                            });
                            break;
                        default: break;
                    }
                    // Echo the data back to the client.
                    string[] dataSend = data.Split('-');
                    msg = Encoding.ASCII.GetBytes(new Comunicacion().CrearComandoBascula(dataSend[0]));
                    handler.Send(msg);
                    break;
                }
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (SocketException)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex) { }
        }
    }
}
