using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Priceio.Turnero
{
    internal class ServidorTurnos
    {
        private HttpListener? _listener;
        private CancellationTokenSource? _cts;
        private int _puerto;
        private string _apiKey = ""; // ── NUEVO
        private bool _corriendo = false;

        public bool Corriendo => _corriendo;

        // ─────────────────────────────────────────
        //  INICIAR SERVIDOR
        // ─────────────────────────────────────────
        public void Iniciar(int puerto)
        {
            if (_corriendo) return;

            _puerto = puerto;

            // Lee la ApiKey directo de la config — nunca pasa por la UI
            _apiKey = ConfiguracionRed.Cargar().ApiKey;

            RegistrarUrlAcl(puerto);
            AbrirPuertoFirewall(puerto);

            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{_puerto}/turno/");
            _listener.Start();
            _corriendo = true;

            Task.Run(() => EscucharPeticiones(_cts.Token));
        }

       


        // ─────────────────────────────────────────
        //  REGISTRAR URL ACL
        //  Permite que la app use http://+:PUERTO/
        //  sin necesitar ser administrador cada vez.
        //  Solo se ejecuta si aún no está registrado.
        // ─────────────────────────────────────────
        private void RegistrarUrlAcl(int puerto)
        {
            try
            {
                // Verificar si ya está registrado para no repetirlo
                string check = EjecutarComando("netsh", $"http show urlacl url=http://+:{puerto}/turno/");
                if (check.Contains($"http://+:{puerto}/turno/"))
                    return; // Ya existe, no hacer nada

                // No está → registrar (pide UAC una sola vez)
                EjecutarComandoElevado("netsh",
                    $"http add urlacl url=http://+:{puerto}/turno/ user=Everyone");
            }
            catch { }
        }

        // ─────────────────────────────────────────
        //  ABRIR PUERTO EN FIREWALL
        //  Permite que la tablet alcance este puerto.
        //  Solo se ejecuta si la regla no existe.
        // ─────────────────────────────────────────
        private void AbrirPuertoFirewall(int puerto)
        {
            try
            {
                // Verificar si la regla ya existe
                string check = EjecutarComando("netsh",
                    $"advfirewall firewall show rule name=\"Turnos{puerto}\"");
                if (check.Contains($"Turnos{puerto}"))
                    return; // Ya existe

                // Crear la regla (pide UAC una sola vez)
                EjecutarComandoElevado("netsh",
                    $"advfirewall firewall add rule name=\"Turnos{puerto}\" " +
                    $"dir=in action=allow protocol=TCP localport={puerto}");
            }
            catch { }
        }

        // ─────────────────────────────────────────
        //  HELPERS: ejecutar comandos del sistema
        // ─────────────────────────────────────────

        /// <summary>
        /// Ejecuta un comando y devuelve su salida (sin elevar privilegios).
        /// Útil para verificar si algo ya existe antes de crearlo.
        /// </summary>
        private string EjecutarComando(string exe, string args)
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            string output = proc?.StandardOutput.ReadToEnd() ?? "";
            proc?.WaitForExit();
            return output;
        }

        /// <summary>
        /// Ejecuta un comando pidiendo elevación UAC.
        /// El usuario verá el prompt "¿Permitir cambios?" solo la primera vez;
        /// las siguientes ya estará registrado y no volverá a pedirlo.
        /// </summary>
        private void EjecutarComandoElevado(string exe, string args)
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                Verb = "runas",  // ← solicita UAC
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit();
        }

        // ─────────────────────────────────────────
        //  DETENER SERVIDOR
        // ─────────────────────────────────────────
        public void Detener()
        {
            try
            {
                _cts?.Cancel();
                _listener?.Stop();
                _listener?.Close();
            }
            catch { }
            finally { _corriendo = false; }
        }

        // ─────────────────────────────────────────
        //  LOOP DE PETICIONES
        // ─────────────────────────────────────────
        private async Task EscucharPeticiones(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext ctx = await _listener!.GetContextAsync();
                    _ = Task.Run(() => ProcesarPeticion(ctx));
                }
                catch { }
            }
        }

        // ─────────────────────────────────────────
        //  PROCESAR CADA PETICIÓN
        // ─────────────────────────────────────────

        private void ProcesarPeticion(HttpListenerContext ctx)
        {
            try
            {
                string path = ctx.Request.Url?.AbsolutePath.ToLower() ?? "";

                // ── /turno/auth va PRIMERO, sin validar ApiKey ────────
                if (path == "/turno/auth")
                {
                    string respuesta = JsonSerializer.Serialize(new RespuestaAuth
                    {
                        Exito = true,
                        ApiKey = _apiKey
                    });
                    byte[] buffer = Encoding.UTF8.GetBytes(respuesta);
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.ContentLength64 = buffer.Length;
                    ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    return; // ← sale aquí, nunca llega a validar ApiKey
                }

                // ── Validar ApiKey para todos los demás endpoints ─────
                string keyRecibida = ctx.Request.Headers["X-Api-Key"] ?? "";
                if (keyRecibida != _apiKey)
                {
                    ctx.Response.StatusCode = 401;
                    byte[] negado = Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize(new RespuestaTurno
                        {
                            Exito = false,
                            Mensaje = "No autorizado"
                        }));
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.ContentLength64 = negado.Length;
                    ctx.Response.OutputStream.Write(negado, 0, negado.Length);
                    return;
                }

                // ── Endpoints protegidos ──────────────────────────────
                string respuestaFinal;

                if (path == "/turno/siguiente")
                {
                    var db = new SQLiteClassManager();
                    int turnoActivo = db.GetTurno()?.NumeroTurno ?? 0;
                    List<TurnosAnteriores>? anteriores = db.GetTurnosAnteriores();
                    int maxAnterior = anteriores?.Count > 0 ? anteriores.Max(t => t.NumeroTurno) : 0;
                    int siguiente = Math.Max(turnoActivo, maxAnterior) + 1;

                    respuestaFinal = JsonSerializer.Serialize(new RespuestaTurno
                    {
                        Exito = true,
                        NumeroTurno = siguiente,
                        Mensaje = "Turno generado correctamente"
                    });
                }
                else if (path == "/turno/ultimo")
                {
                    var db = new SQLiteClassManager();
                    int turnoActivo = db.GetTurno()?.NumeroTurno ?? 0;
                    List<TurnosAnteriores>? anteriores = db.GetTurnosAnteriores();
                    int maxAnterior = anteriores?.Count > 0 ? anteriores.Max(t => t.NumeroTurno) : 0;
                    int ultimo = Math.Max(turnoActivo, maxAnterior);

                    respuestaFinal = JsonSerializer.Serialize(new RespuestaTurno
                    {
                        Exito = true,
                        NumeroTurno = ultimo,
                        Mensaje = "Último turno consultado"
                    });
                }
                else
                {
                    ctx.Response.StatusCode = 404;
                    respuestaFinal = JsonSerializer.Serialize(new RespuestaTurno
                    {
                        Exito = false,
                        Mensaje = "Endpoint no reconocido"
                    });
                }

                byte[] bufferFinal = Encoding.UTF8.GetBytes(respuestaFinal);
                ctx.Response.ContentType = "application/json";
                ctx.Response.ContentLength64 = bufferFinal.Length;
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.OutputStream.Write(bufferFinal, 0, bufferFinal.Length);
            }
            catch { }
            finally
            {
                try { ctx.Response.OutputStream.Close(); } catch { }
            }
        }

    }

    internal class RespuestaTurno
    {
        public bool Exito { get; set; }
        public int NumeroTurno { get; set; }
        public string Mensaje { get; set; } = "";
    }

    internal class RespuestaAuth
    {
        public bool Exito { get; set; }
        public string ApiKey { get; set; } = "";
    }
}