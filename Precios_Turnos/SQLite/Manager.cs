using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.SQLite
{
    internal class Manager
    {
        internal SqliteConnection? Connection;
        internal bool ConectarBD(){
            try
            {
                Connection = new SqliteConnection(@"Data Source=C:\db\pub.db");
                Connection.Open();
                return true;

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        internal bool DesconectarBD()
        {
            try
            {
                if(Connection != null) 
                    Connection.Close();
                return true;

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        internal DataTable GetDataQuerry(string query)
        {
            DataTable dt = new DataTable();
            SqliteCommand command;
            try
            {
                command = new SqliteCommand(query, Connection);
                dt.Load(command.ExecuteReader());
            }
            catch{}
            return dt;
        }

        internal int ExecuteQuery(string query)
        {
            SqliteCommand command;
            try
            {
                command = new SqliteCommand(query, Connection);
                return command.ExecuteNonQuery();
            }
            catch { }
            return 0;
        }

        internal void crearBD()
        {

        }

    }
}
