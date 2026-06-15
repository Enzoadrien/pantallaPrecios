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
using Priceio.Cajero.Payout;
using Priceio.Cajero.Hopper;
using Priceio.ClasesSQLite;
using Priceio.SQLite;

namespace Priceio.Cajero
{
    class AsynchronousSocketListenerCajero
    {
        internal static ManualResetEvent allDone = new ManualResetEvent(false);
        internal static Socket? listener;
        internal static bool start;
        private static SMARTPayout smartPayout;
        private static SMARTHopper smartHopper;

        public static void StartListening(SMARTPayout pSmartPayout, SMARTHopper pSmartHopper)
        {
            smartPayout = pSmartPayout;
            smartHopper = pSmartHopper;
            start = true;
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".
            //  //Create the object
            int Puerto = 0;
            ConfiguracionCajero ? configuracionCajero = new SQLiteClassManager().GetConfiguracionCajero();
            if (configuracionCajero != null)
            {
                Puerto = configuracionCajero.PuertoTCP;
            }
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Puerto);

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
                StateObjectCajero state = new StateObjectCajero
                {
                    workSocket = handler
                };
                handler.BeginReceive(state.buffer, 0, StateObjectCajero.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception) { }
        }

        internal static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObjectCajero state = (StateObjectCajero)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                bool datoCorrecto = false;
                if (state.EstadoActual != StateObjectCajero.EstadoCajero.OK)
                {
                    while (start)
                    {
                        string data = string.Empty;
                        // An incoming connection needs to be processed.  
                        while (start)
                        {
                            int bytesRec = handler.EndReceive(ar);
                            data += Encoding.ASCII.GetString(state.buffer, 0, bytesRec);
                            if (data.IndexOf('{') == 0)
                            {
                                datoCorrecto = true;
                                break;
                            }
                            else
                                break;
                        }
                        if (datoCorrecto)
                        {
                            ProcesarPagoCajero PT = new ProcesarPagoCajero();
                            data = PT.ProcesarComando(data, state, smartPayout, smartHopper).Result;

                            // Echo the data back to the client.
                            byte[] msg = Encoding.ASCII.GetBytes(data);
                            handler.Send(msg);
                        }
                        break;
                    }
                    return;
                }
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (SocketException)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception) { }
        }
    }
}
