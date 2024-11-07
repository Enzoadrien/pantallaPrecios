using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Priceio.Turnero.StateObject;
using System.Windows;
using System.Numerics;
using MySql.Data.MySqlClient;
using Precios_Turnos;

namespace Priceio.ClasesGenericas
{
    internal class ValidarLicencia
    {
        private Seguridad vSeguridad = new Seguridad();
        internal string cargarLicenciaApp(string Correo, string Codigo)
        {
            MySqlConnection connection = new ServerConfig().connection();
            try
            {
                string llave = "";
                using (MySqlCommand cmd = connection.CreateCommand())
                {    //watch out for this SQL injection vulnerability below
                    cmd.CommandText = string.Format("SELECT llave FROM licencias WHERE correo='" + Correo + "' AND codigo='" + Codigo + "' AND nombreApp='" + MainWindow.nombreApp + "' AND activo=1;");
                    connection.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        llave = reader.GetString(0);
                    }
                }
                connection.Close();

                if (llave.Length == 0)
                {
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

        internal bool validarLicenciaApp(string Correo, string Codigo)
        {
            MySqlConnection connection = new ServerConfig().connection();
            try
            {

                string llave = "";
                using (MySqlCommand cmd = connection.CreateCommand())
                {    //watch out for this SQL injection vulnerability below
                    cmd.CommandText = string.Format("SELECT llave FROM licencias WHERE correo='" + Correo + "' AND codigo='" + Codigo + "' AND nombreApp='" + MainWindow.nombreApp + "' AND activo=1;");
                    connection.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        llave = reader.GetString(0);
                    }
                }
                connection.Close();

                if (llave.Length == 0)
                {
                    Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialogError.lblNombre.Content = "¡Error!";
                    dialogError.lblTexto.Text = "No se encuentra la llave de activacion en el servidor, consulte al administrador.";
                    dialogError.ShowDialog();
                    return false;
                }

                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialogError.lblNombre.Content = "¡Error!";
                dialogError.lblTexto.Text = "No se puede conectar con el servidor, consulte al administrador.";
                dialogError.ShowDialog();
                return false;
            }
        }

        internal string recuperaLicenciaApp(string Correo, string Codigo)
        {
            MySqlConnection connection = new ServerConfig().connection();
            try
            {

                string llave = "";
                using (MySqlCommand cmd = connection.CreateCommand())
                {    //watch out for this SQL injection vulnerability below
                    cmd.CommandText = string.Format("SELECT llave FROM licencias WHERE correo='" + Correo + "' AND codigo='" + Codigo + "' AND nombreApp='" + MainWindow.nombreApp + "' AND activo=1;");
                    connection.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        llave = reader.GetString(0);
                    }
                }
                connection.Close();

                if (llave.Length == 0)
                {
                    Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                    dialogError.lblNombre.Content = "¡Error!";
                    dialogError.lblTexto.Text = "No se encuentra la llave de activacion en el servidor, consulte al administrador.";
                    dialogError.ShowDialog();
                    return string.Empty;
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
                return string.Empty;
            }
        }

        internal void actualizaUltimaConexion(string Correo, string Codigo, bool LicenciaValida)
        {
            MySqlConnection connection = new ServerConfig().connection();
            try
            {

                using (MySqlCommand cmd = connection.CreateCommand())
                {    //watch out for this SQL injection vulnerability below
                    cmd.CommandText = string.Format("UPDATE  licencias set ultimaConexion='" + DateTime.Parse(DateTime.Now.ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "' " + (LicenciaValida == true ? ", licenciaValida=1 " : ", licenciaValida=0 ") + " WHERE correo='" + Correo + "' AND codigo='" + Codigo + "';");
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                Mensajes dialogError = new Mensajes(Recursos.TipoMensaje.ERROR);
                dialogError.lblNombre.Content = "¡Error!";
                dialogError.lblTexto.Text = "Ocurrio un error al sincronizar con el servidor, consulte al administrador.";
                dialogError.ShowDialog();
            }
        }

    }
}
