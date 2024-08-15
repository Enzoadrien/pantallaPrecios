using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Precios_Turnos.StateObject;
using System.Windows;
using MySqlX.XDevAPI;
using System.Numerics;
using Microsoft.Data.SqlClient;

namespace Precios_Turnos
{
    internal class ValidarLicencia
    {
        private Seguridad vSeguridad = new Seguridad();
        internal string cargarLicenciaApp(string Correo, string Codigo)
        {
            SqlConnection connection = new ServerConfig().connection();
            try
            {
                connection.Open();
                string llave = "";
                using var command = new SqlCommand("SELECT llave FROM licenciasApp WHERE correo='"+ Correo + "' AND codigo='"+ Codigo + "' AND nombreApp='"+MainWindow.nombreApp+"' AND activo=0;", connection);
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
            catch (SqlException e)
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

            SqlConnection connection = new ServerConfig().connection();
            try
            {
                connection.Open();
                using var command = new SqlCommand("UPDATE licenciasApp SET fechaActivacion ='"+ vSeguridad.GetNetworkTime().Date.ToString("yyyy-MM-dd") + "', activo=1 WHERE nombreApp='" + MainWindow.nombreApp + "'AND correo ='" + Correo + "'AND codigo='" + Codigo + "';", connection);
                using var reader = command.ExecuteReader();
                connection.Close();
                return true;
            }
            catch (SqlException e)
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
            SqlConnection connection = new ServerConfig().connection();
            try
            {
                connection.Open();
                string llave = "";
                using var command = new SqlCommand("SELECT llave FROM licenciasDiseno WHERE correo='" + Correo + "' AND codigo='" + Codigo + "' AND nombreApp='" + MainWindow.nombreApp + "';", connection);
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
            catch (SqlException e)
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

            SqlConnection connection = new ServerConfig().connection();
            try
            {
                connection.Open();
                string fechaActivacion = "";
                using var command = new SqlCommand("SELECT fechaActivacion, activo FROM licenciasDiseno WHERE correo='" + Correo + "' AND codigo='" + Codigo + "' AND llave='" + Llave + "' AND nombreApp='" + MainWindow.nombreApp + "';", connection);
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
            catch (SqlException e)
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

            SqlConnection connection = new ServerConfig().connection();
            try
            {
                connection.Open();
                using var command = new SqlCommand("UPDATE licenciasDiseno SET fechaActivacion ='" + vSeguridad.GetNetworkTime().Date.ToString("yyyy-MM-dd") + "', activo=1 WHERE nombreApp='" + MainWindow.nombreApp + "'AND correo ='" + Correo + "'AND codigo='" + Codigo + "' AND llave='"+ Llave + "';", connection);
                using var reader = command.ExecuteReader();
                connection.Close();
                return true;
            }
            catch (SqlException e)
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
