using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Precios_Turnos
{
    internal class ServerConfig
    {
        private readonly string serverKey = "Aplicaciones_3K_Mto_Prof";
        private Seguridad vSeguridad = new Seguridad();

        internal SqlConnection connection()
        {
            
            SqlConnection connection;
            try
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = vSeguridad.DecryptString(serverKey, recuperarValorServer("server")),
                    InitialCatalog = vSeguridad.DecryptString(serverKey, recuperarValorServer("database")),
                    UserID = vSeguridad.DecryptString(serverKey, recuperarValorServer("user")),
                    Password = vSeguridad.DecryptString(serverKey, recuperarValorServer("password")),
                };

                connection = new SqlConnection(builder.ConnectionString);
            } catch (Exception ex) {
                connection = new SqlConnection();
            }
            return connection;
        }
        private String recuperarValorServer(string dato)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@".\Recursos\server.3k");

                XmlNode node = doc.DocumentElement.SelectSingleNode("/serverConfiguration");

                foreach(XmlNode nodeChild in node.ChildNodes)
                {
                    if(nodeChild.Name.Equals(dato))
                    {
                        return nodeChild.InnerText;
                    }
                }
            }
            catch
            {
            }
            return string.Empty;
        }
    }
}
