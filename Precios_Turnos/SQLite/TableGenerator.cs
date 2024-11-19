using Priceio.ClasesSQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace Priceio.SQLite
{
    internal class TableGenerator
    {
        internal ManagerSQLite sqliteManager = new ManagerSQLite();
        internal bool GenenarBD()
        {
            List<TableClass> tables = new List<TableClass>();

            // Get Types in the assembly.
            tables.Add(new TableClass(new ConfiguracionODBC().GetType()));
            tables.Add(new TableClass(new ConfiguracionVentanaSplash().GetType()));
            tables.Add(new TableClass(new VozSplash().GetType()));
            tables.Add(new TableClass(new ConfiguracionTurnero().GetType()));
            tables.Add(new TableClass(new ClientesTurnero().GetType()));
            tables.Add(new TableClass(new NombresClientesTurnero().GetType()));
            tables.Add(new TableClass(new ConfiguracionVerificador().GetType()));
            tables.Add(new TableClass(new ConfiguracionCajero().GetType()));
            tables.Add(new TableClass(new ConfiguracionCanalesHopper().GetType()));
            tables.Add(new TableClass(new ConfiguracionCanalesPayout().GetType()));
            tables.Add(new TableClass(new ConfiguracionImpresora().GetType()));
            tables.Add(new TableClass(new ConfiguracionLector().GetType()));
            tables.Add(new TableClass(new Pago().GetType()));
            tables.Add(new TableClass(new Turno().GetType()));
            tables.Add(new TableClass(new TurnosAnteriores().GetType()));



            sqliteManager.ConectarBD();
            // Create SQL for each table
            foreach (TableClass table in tables)
            {
                if (sqliteManager.ExecuteQuery(table.CreateTableScript()) == 0)
                {
                    checkFields(table);
                }
            }

            // Total Hacked way to find FK relationships! Too lazy to fix right now
            foreach (TableClass table in tables)
            {
                foreach (KeyValuePair<String, Type> field in table.Fields)
                {
                    foreach (TableClass t2 in tables)
                    {
                        if (field.Value.Name == t2.ClassName)
                        {
                            // We have a FK Relationship!
                            Console.WriteLine("GO");
                            Console.WriteLine("ALTER TABLE " + table.ClassName + " WITH NOCHECK");
                            Console.WriteLine("ADD CONSTRAINT FK_" + field.Key + " FOREIGN KEY (" + field.Key + ") REFERENCES " + t2.ClassName + "(ID)");
                            Console.WriteLine("GO");

                        }
                    }
                }
            }
            sqliteManager.DesconectarBD();
            return true;
        }

        private void checkFields(TableClass table)
        {
            List<string> cols = new List<string>();

            foreach (DataRow row in sqliteManager.GetDataQuerry("PRAGMA table_info(" + table.ClassName + ")").Rows)
            {

                cols.Add(row[1].ToString());
            }
            foreach (KeyValuePair<String, Type> field in table.Fields)
            {
                if (!cols.Contains(field.Key))
                {
                    sqliteManager.ExecuteQuery("ALTER TABLE " + table.ClassName + " ADD COLUMN " + field.Key + " " + TableClass.dataMapper[field.Value]);
                }
            }
        }
    }


    public class TableClass
    {
        private List<KeyValuePair<String, Type>> _fieldInfo = new List<KeyValuePair<String, Type>>();
        private string _className = String.Empty;
        object _oClass;
        internal static Dictionary<Type, String> dataMapper
        {
            get
            {
                // Add the rest of your CLR Types to SQL Types mapping here
                Dictionary<Type, String> dataMapper = new Dictionary<Type, string>();
                dataMapper.Add(typeof(int), "INTEGER");
                dataMapper.Add(typeof(string), "TEXT");
                dataMapper.Add(typeof(Char), "TEXT");
                dataMapper.Add(typeof(bool), "INTEGER");
                dataMapper.Add(typeof(bool?), "INTEGER");
                dataMapper.Add(typeof(DateTime), "TEXT");
                dataMapper.Add(typeof(DateOnly), "TEXT");
                dataMapper.Add(typeof(TimeOnly), "TEXT");
                dataMapper.Add(typeof(float), "REAL");
                dataMapper.Add(typeof(decimal), "REAL");
                dataMapper.Add(typeof(double), "REAL");
                dataMapper.Add(typeof(Guid), "UNIQUEIDENTIFIER");
                dataMapper.Add(typeof(Pago.Tipo), "INTEGER");
                dataMapper.Add(typeof(Pago.Estado), "INTEGER");
                return dataMapper;
            }
        }

        public List<KeyValuePair<String, Type>> Fields
        {
            get { return this._fieldInfo; }
            set { this._fieldInfo = value; }
        }

        public string ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        public object Class
        {
            get { return _oClass; }
            set { _oClass = value; }
        }

        public TableClass(Type t, object oClass = null)
        {
            this._className = t.Name;
            _oClass = oClass;
            foreach (PropertyInfo p in t.GetProperties())
            {
                KeyValuePair<String, Type> field = new KeyValuePair<String, Type>(p.Name, p.PropertyType);

                this.Fields.Add(field);
            }
        }

        public string CreateTableScript()
        {
            StringBuilder script = new StringBuilder();

            script.AppendLine("CREATE TABLE " + ClassName);
            script.AppendLine("(");
            script.AppendLine("\t ID INTEGER PRIMARY KEY AUTOINCREMENT,");
            for (int i = 0; i < Fields.Count; i++)
            {
                KeyValuePair<String, Type> field = Fields[i];

                if (dataMapper.ContainsKey(field.Value))
                {
                    script.Append("\t " + field.Key + " " + dataMapper[field.Value]);

                    if(dataMapper[field.Value].Equals("TEXT"))
                        script.Append(" NOT NULL DEFAULT ''");
                    else
                        script.Append(" NOT NULL DEFAULT 0");
                }
                else
                {
                    // Complex Type? 
                    script.Append("\t " + field.Key + " BIGINT NOT NULL DEFAULT 0 ");
                }

                if (i != Fields.Count - 1)
                {
                    script.Append(",");
                }

                script.Append(Environment.NewLine);
            }
            script.AppendLine(")");
            return script.ToString();
        }
    }
}
