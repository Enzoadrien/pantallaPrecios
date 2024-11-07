using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Priceio.SQLite
{
    internal class ManagerSQLite
    {
        internal SqliteConnection? Connection;

        internal bool ConectarBD()
        {
            try
            {
                Connection = new SqliteConnection(@"Data Source=.\data\priceio.3kdb");
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
                if (Connection != null)
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
            catch { }
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

        internal bool DeleteTableData(string tableName, string condition="")
        {
            StringBuilder sqlComand = new StringBuilder();
            sqlComand.Append("DELETE FROM " + tableName);
            if (condition.Length > 0)
            {
                sqlComand.Append(" WHERE " + condition);
            }
            sqlComand.AppendLine(";");

            if(ExecuteQuery(sqlComand.ToString())!=0)
                return true;
            else
                return  false;
        }

        internal bool TruncateTableData(string tableName)
        {
            StringBuilder sqlComand = new StringBuilder();

            sqlComand.AppendLine("DELETE FROM " + tableName + ";");
            sqlComand.AppendLine("DELETE FROM SQLITE_SEQUENCE WHERE name='" + tableName + "';");
            sqlComand.AppendLine("VACUUM;");

            if (ExecuteQuery(sqlComand.ToString()) != 0)
                return true;
            else
                return false;
        }

        internal bool SaveTableData(TableClass table)
        {
            StringBuilder sqlComand = new StringBuilder();
            StringBuilder values = new StringBuilder();
            sqlComand.AppendLine("INSERT OR REPLACE INTO " + table.ClassName);
            sqlComand.Append("(");
            values.Append("VALUES(");
            int countFields = 0;
            foreach (KeyValuePair<String, Type> field in table.Fields)
            {
                countFields++;

                sqlComand.Append(field.Key);
                if (esTexto(field.Value))
                    values.Append("'");
                values.Append(table.Class.GetType().GetProperty(field.Key).GetValue(table.Class, null));
                if (esTexto(field.Value))
                    values.Append("'");
                if (countFields < table.Fields.Count)
                {
                    sqlComand.Append(", ");
                    values.Append(", ");
                }
            }
            sqlComand.AppendLine(") ");
            values.Append(')');
            sqlComand.AppendLine(values.ToString());
            if (ExecuteQuery(sqlComand.ToString()) != 0)
                return true;
            else
                return false;
        }

        internal bool esTexto(Type field)
        {
            switch (TableClass.dataMapper[field])
            {
                case "TEXT":
                    return true;
                default:
                    return false;

            }
        }
    
        internal DataTable GetAllTableData(string tableName)
        {
            StringBuilder sqlComand = new StringBuilder();
            sqlComand.AppendLine("SELECT * FROM " + tableName + ";");
            return GetDataQuerry(sqlComand.ToString());
        }
    }
}
