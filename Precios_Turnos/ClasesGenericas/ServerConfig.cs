using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Priceio.ClasesGenericas
{
    internal class ServerConfig
    {
        private readonly string serverKey = "Aplicaciones_3K_Mto_Prof";
        private Seguridad vSeguridad = new Seguridad();

        internal MySqlConnection connection()
        {

            MySqlConnection connection;
            try
            {
                var builder = new MySqlConnectionStringBuilder
                {
                    Server = vSeguridad.DecryptString(serverKey, recuperarValorServer("server")),
                    Port = uint.Parse(vSeguridad.DecryptString(serverKey, recuperarValorServer("port"))),
                    Database = vSeguridad.DecryptString(serverKey, recuperarValorServer("database")),
                    UserID = vSeguridad.DecryptString(serverKey, recuperarValorServer("user")),
                    Password = vSeguridad.DecryptString(serverKey, recuperarValorServer("password")),
                };

                connection = new MySqlConnection(builder.ConnectionString);
            }
            catch (Exception ex)
            {
                connection = new MySqlConnection();
            }
            return connection;
        }
        private string recuperarValorServer(string dato)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@".\Recursos\server.3k");

                XmlNode node = doc.DocumentElement.SelectSingleNode("/serverConfiguration");

                foreach (XmlNode nodeChild in node.ChildNodes)
                {
                    if (nodeChild.Name.Equals(dato))
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
