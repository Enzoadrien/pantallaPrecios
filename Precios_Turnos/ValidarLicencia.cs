using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;
using static Precios_Turnos.StateObject;
using System.Windows;
using MySqlX.XDevAPI;
using System.Numerics;

namespace Precios_Turnos
{
    internal class ValidarLicencia
    {
        private Seguridad vSeguridad = new Seguridad();
        private string server = "clERYmp7FWDroXEU1cTYAA==";
        private string database = "FVlhqRNJp0HJEUXXY43bOJBU6hikYygRUXMc/e9dObY=";
        private string user = "7Rl6MZ/paWHRlHou8wGlvw==";
        private string password = "3Ro78Q+3C9v7aCF5BoRy4A==";
        private string port = "BXg5UiO+Qm/gUKC+5Xltrg==";
        private MySqlSslMode sslMode = MySqlSslMode.Required;

        internal string cargarLicenciaApp(string Correo, string Codigo)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = vSeguridad.DecryptString(MainWindow.nombreApp, server),
                Database = vSeguridad.DecryptString(MainWindow.nombreApp, database),
                UserID = vSeguridad.DecryptString(MainWindow.nombreApp, user),
                Password = vSeguridad.DecryptString(MainWindow.nombreApp, password),
                Port = uint.Parse(vSeguridad.DecryptString(MainWindow.nombreApp, port)),
                SslMode = sslMode,
            };

            MySqlConnection connection = new MySqlConnection(builder.ConnectionString);
            try
            {
                connection.Open();
                string llave = "";
                using var command = new MySqlCommand("SELECT llave FROM licenciasApp WHERE correo='"+ Correo + "' AND codigo='"+ Codigo + "' AND nombreApp='"+MainWindow.nombreApp+"' AND activo=0;", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    llave = reader.GetString(0);
                }
                connection.Close();

                if (llave.Length == 0) { 
                    Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialogError.lblNombre.Content = "¡Error!";
                    dialogError.lblTexto.Text = "No se puede activar la aplicación, consulte al administrador.";
                    dialogError.ShowDialog();
                    return "";
                }

                return llave;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialogError.lblNombre.Content = "¡Error!";
                dialogError.lblTexto.Text = "No se puede conectar con el servidor, consulte al administrador.";
                dialogError.ShowDialog();
                return "";
            }
        }

        internal bool activarLicenciaApp(string Correo, string Codigo)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = vSeguridad.DecryptString(MainWindow.nombreApp, server),
                Database = vSeguridad.DecryptString(MainWindow.nombreApp, database),
                UserID = vSeguridad.DecryptString(MainWindow.nombreApp, user),
                Password = vSeguridad.DecryptString(MainWindow.nombreApp, password),
                Port = uint.Parse(vSeguridad.DecryptString(MainWindow.nombreApp, port)),
                SslMode = sslMode,
            };

            MySqlConnection connection = new MySqlConnection(builder.ConnectionString);
            try
            {
                connection.Open();
                using var command = new MySqlCommand("UPDATE licenciasApp SET fechaActivacion ='"+ vSeguridad.GetNetworkTime().Date.ToString("yyyy-MM-dd") + "', activo=1 WHERE nombreApp='" + MainWindow.nombreApp + "'AND correo ='" + Correo + "'AND codigo='" + Codigo + "';", connection);
                using var reader = command.ExecuteReader();
                connection.Close();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialogError.lblNombre.Content = "¡Error!";
                dialogError.lblTexto.Text = e.Message;
                dialogError.ShowDialog();
            }

            return false;
        }

        internal string cargarLicenciaDiseno(string Correo, string Codigo)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = vSeguridad.DecryptString(MainWindow.nombreApp, server),
                Database = vSeguridad.DecryptString(MainWindow.nombreApp, database),
                UserID = vSeguridad.DecryptString(MainWindow.nombreApp, user),
                Password = vSeguridad.DecryptString(MainWindow.nombreApp, password),
                Port = uint.Parse(vSeguridad.DecryptString(MainWindow.nombreApp, port)),
                SslMode = sslMode,
            };

            MySqlConnection connection = new MySqlConnection(builder.ConnectionString);
            try
            {
                connection.Open();
                string llave = "";
                using var command = new MySqlCommand("SELECT llave FROM licenciasDiseno WHERE correo='" + Correo + "' AND codigo='" + Codigo + "' AND nombreApp='" + MainWindow.nombreApp + "';", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    llave = reader.GetString(0);
                }
                connection.Close();

                if (llave.Length == 0)
                {
                    Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialogError.lblNombre.Content = "¡Error!";
                    dialogError.lblTexto.Text = "No se puede cargar la llave de edición, consulte al administrador.";
                    dialogError.ShowDialog();
                    return "";
                }

                return llave;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialogError.lblNombre.Content = "¡Error!";
                dialogError.lblTexto.Text = "No se puede conectar con el servidor, consulte al administrador.";
                dialogError.ShowDialog();
                return "";
            }
        }
        internal string cargarFechaActivacionLicenciaDiseno(string Correo, string Codigo, string Llave)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = vSeguridad.DecryptString(MainWindow.nombreApp, server),
                Database = vSeguridad.DecryptString(MainWindow.nombreApp, database),
                UserID = vSeguridad.DecryptString(MainWindow.nombreApp, user),
                Password = vSeguridad.DecryptString(MainWindow.nombreApp, password),
                Port = uint.Parse(vSeguridad.DecryptString(MainWindow.nombreApp, port)),
                SslMode = sslMode,
            };

            MySqlConnection connection = new MySqlConnection(builder.ConnectionString);
            try
            {
                connection.Open();
                string fechaActivacion = "";
                using var command = new MySqlCommand("SELECT fechaActivacion, activo FROM licenciasDiseno WHERE correo='" + Correo + "' AND codigo='" + Codigo + "' AND llave='" + Llave + "' AND nombreApp='" + MainWindow.nombreApp + "';", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetInt32(1) == 1)
                        fechaActivacion = reader.GetDateTime(0).ToShortDateString();
                    else
                        fechaActivacion = new DateTime(1900, 1, 1).ToShortDateString();
                }
                connection.Close();

                if (fechaActivacion.Length == 0)
                {
                    Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialogError.lblNombre.Content = "¡Error!";
                    dialogError.lblTexto.Text = "No se puede cargar la llave de edición, consulte al administrador.";
                    dialogError.ShowDialog();
                    return "";
                }

                return fechaActivacion;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialogError.lblNombre.Content = "¡Error!";
                dialogError.lblTexto.Text = "No se puede conectar con el servidor, consulte al administrador.";
                dialogError.ShowDialog();
                return "";
            }
        }

        internal bool activarLicenciaDiseno(string Correo, string Codigo, string Llave)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = vSeguridad.DecryptString(MainWindow.nombreApp, server),
                Database = vSeguridad.DecryptString(MainWindow.nombreApp, database),
                UserID = vSeguridad.DecryptString(MainWindow.nombreApp, user),
                Password = vSeguridad.DecryptString(MainWindow.nombreApp, password),
                Port = uint.Parse(vSeguridad.DecryptString(MainWindow.nombreApp, port)),
                SslMode = sslMode,
            };

            MySqlConnection connection = new MySqlConnection(builder.ConnectionString);
            try
            {
                connection.Open();
                using var command = new MySqlCommand("UPDATE licenciasDiseno SET fechaActivacion ='" + vSeguridad.GetNetworkTime().Date.ToString("yyyy-MM-dd") + "', activo=1 WHERE nombreApp='" + MainWindow.nombreApp + "'AND correo ='" + Correo + "'AND codigo='" + Codigo + "' AND llave='"+ Llave + "';", connection);
                using var reader = command.ExecuteReader();
                connection.Close();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialogError.lblNombre.Content = "¡Error!";
                dialogError.lblTexto.Text = e.Message;
                dialogError.ShowDialog();
            }

            return false;
        }

    }
}
