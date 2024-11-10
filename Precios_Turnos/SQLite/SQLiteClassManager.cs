using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using System;
using System.Collections.Generic;
using System.Data;

namespace Priceio.SQLite
{
    internal class SQLiteClassManager
    {
        private ManagerSQLite managerSQLite = new ManagerSQLite();
        private Seguridad vSeguridad = new Seguridad();

        internal ConfiguracionODBC? GetConfiguracionODBC()
        {
            ConfiguracionODBC SQLiteClass = new ConfiguracionODBC();
            try
            {
                managerSQLite.ConectarBD();
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.ODBC = row.Field<string>("ODBC");
                SQLiteClass.UsuarioODBC = row.Field<string>("UsuarioODBC");
                SQLiteClass.ContrasenaODBC = vSeguridad.DecryptString(MainWindow.nombreApp, row.Field<string>("ContrasenaODBC"));
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionODBC(ConfiguracionODBC SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionVentanaSplash? GetConfiguracionVentanaSplash()
        {
            ConfiguracionVentanaSplash SQLiteClass = new ConfiguracionVentanaSplash();
            try
            {
                managerSQLite.ConectarBD();
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.TipoSplash = row.Field<string>("TipoSplash");
                SQLiteClass.Audio = row.Field<string>("Audio");
                SQLiteClass.Duracion = (int)row.Field<long>("Duracion");
                SQLiteClass.Ancho = (int)row.Field<long>("Ancho");
                SQLiteClass.Alto = (int)row.Field<long>("Alto");
                SQLiteClass.Voz = Convert.ToBoolean(row.Field<long>("Voz"));
                SQLiteClass.ActivarSplash = Convert.ToBoolean(row.Field<long>("ActivarSplash"));
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionVentanaSplash(ConfiguracionVentanaSplash SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionTurnero? GetConfiguracionTurnero()
        {
            ConfiguracionTurnero SQLiteClass = new ConfiguracionTurnero();
            try
            {
                managerSQLite.ConectarBD();
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.TipoTurnero = row.Field<string>("TipoTurnero");
                SQLiteClass.ProtocoloTurnero = row.Field<string>("ProtocoloTurnero");
                SQLiteClass.PuertoTCP = (int)row.Field<long>("PuertoTCP");
                SQLiteClass.TurnosAnteriores = (int)row.Field<long>("TurnosAnteriores");
                SQLiteClass.MostrarNombres = Convert.ToBoolean(row.Field<long>("MostrarNombres"));
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }
        
        internal bool SetConfiguracionTurnero(ConfiguracionTurnero SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal VozSplash? GetVozSplash()
        {
            VozSplash SQLiteClass = new VozSplash();
            try
            {
                managerSQLite.ConectarBD();
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.TipoVoz = row.Field<string>("TipoVoz");
                SQLiteClass.TextoVoz = row.Field<string>("TextoVoz");
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetVozSplash(VozSplash SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal List<ClientesTurnero>? GetClientesTurnero()
        {
            List<ClientesTurnero> SQLiteClass = new List<ClientesTurnero>();
            try
            {
                managerSQLite.ConectarBD();
                foreach (DataRow dr in managerSQLite.GetAllTableData(new ClientesTurnero().GetType().Name).Rows)
                    SQLiteClass.Add(new ClientesTurnero() { Cliente = dr.Field<string>("Cliente") });

                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetClientesTurnero(List<ClientesTurnero> SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(new ClientesTurnero().GetType().Name);
                foreach (ClientesTurnero clientesTurnero in SQLiteClass)
                    managerSQLite.SaveTableData(new TableClass(clientesTurnero.GetType(), clientesTurnero));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal List<NombresClientesTurnero>? GetNombresClientesTurnero()
        {
            List<NombresClientesTurnero> SQLiteClass = new List<NombresClientesTurnero>();
            try
            {
                managerSQLite.ConectarBD();
                foreach (DataRow dr in managerSQLite.GetAllTableData(new NombresClientesTurnero().GetType().Name).Rows)
                    SQLiteClass.Add(new NombresClientesTurnero()
                    {
                        Identificador = dr.Field<string>("Identificador"),
                        Nombre = dr.Field<string>("Nombre")
                    });
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetNombresClientesTurnero(List<NombresClientesTurnero> SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(new NombresClientesTurnero().GetType().Name);
                foreach (NombresClientesTurnero nombresClientesTurnero in SQLiteClass)
                    managerSQLite.SaveTableData(new TableClass(nombresClientesTurnero.GetType(), nombresClientesTurnero));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionVerificador? GetConfiguracionVerificador()
        {
            ConfiguracionVerificador SQLiteClass = new ConfiguracionVerificador();
            try
            {
                managerSQLite.ConectarBD();
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionVerificador(ConfiguracionVerificador SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionCajero? GetConfiguracionCajero()
        {
            ConfiguracionCajero SQLiteClass = new ConfiguracionCajero();
            try
            {
                managerSQLite.ConectarBD();
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.PuertoTCP = (int)row.Field<long>("PuertoTCP");
                SQLiteClass.COMPayout = row.Field<string>("COMPayout");
                SQLiteClass.SSPPayout = (int)row.Field<long>("SSPPayout");
                SQLiteClass.COMHopper = row.Field<string>("COMHopper");
                SQLiteClass.SSPHopper = (int)row.Field<long>("SSPHopper");
                SQLiteClass.LogPagos = Convert.ToBoolean(row.Field<long>("LogPagos"));
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionCajero(ConfiguracionCajero SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }
        
        internal ConfiguracionImpresora? GetConfiguracionImpresora()
        {
            ConfiguracionImpresora SQLiteClass = new ConfiguracionImpresora();
            try
            {
                managerSQLite.ConectarBD();
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.Nombre = row.Field<string>("Nombre");
                SQLiteClass.TipoLetra = row.Field<string>("TipoLetra");
                SQLiteClass.TamanoLetra = (int)row.Field<long>("TamanoLetra");
                SQLiteClass.Negrita = Convert.ToBoolean(row.Field<long>("Negrita"));
                SQLiteClass.Cursiva = Convert.ToBoolean(row.Field<long>("Cursiva"));
                SQLiteClass.Logo = Convert.ToBoolean(row.Field<long>("Logo"));
                SQLiteClass.RutaLogo = row.Field<string>("RutaLogo");
                SQLiteClass.TamanoLogo = (int)row.Field<long>("TamanoLogo");
                SQLiteClass.CordenadaXLogo = (int)row.Field<long>("CordenadaXLogo");
                SQLiteClass.CordenadaYLogo = (int)row.Field<long>("CordenadaYLogo");
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionImpresora(ConfiguracionImpresora SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionLector? GetConfiguracionLector()
        {
            ConfiguracionLector SQLiteClass = new ConfiguracionLector();
            try
            {
                managerSQLite.ConectarBD();
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];

                SQLiteClass.Activo = Convert.ToBoolean(row.Field<long>("Activo"));
                SQLiteClass.FormatoCodigo = row.Field<string>("FormatoCodigo");
                SQLiteClass.Imprmir = Convert.ToBoolean(row.Field<long>("Imprmir"));
                SQLiteClass.FormatoImpresora = row.Field<string>("FormatoImpresora");
                managerSQLite.DesconectarBD();
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionLector(ConfiguracionLector SQLiteClass)
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }

        internal bool ResetConfigSplash()
        {
            try
            {
                managerSQLite.ConectarBD();
                managerSQLite.TruncateTableData(new ConfiguracionVentanaSplash().GetType().Name);
                managerSQLite.TruncateTableData(new ConfiguracionTurnero().GetType().Name);
                managerSQLite.TruncateTableData(new VozSplash().GetType().Name);
                managerSQLite.TruncateTableData(new ClientesTurnero().GetType().Name);
                managerSQLite.TruncateTableData(new ConfiguracionVerificador().GetType().Name);
                managerSQLite.TruncateTableData(new ConfiguracionCajero().GetType().Name);
                managerSQLite.DesconectarBD();
            }
            catch { return false; }
            return true;
        }
    }
}
