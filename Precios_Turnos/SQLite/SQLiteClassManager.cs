using Priceio.ClasesGenericas;
using Priceio.ClasesSQLite;
using System;
using System.Collections.Generic;
using System.Data;
using static Priceio.ClasesSQLite.Pago;

namespace Priceio.SQLite
{
    internal class SQLiteClassManager
    {
        private ManagerSQLite managerSQLite = new ManagerSQLite();
        private Seguridad vSeguridad = new Seguridad();

        internal ConfiguracionGeneral? GetConfiguracionGeneral()
        {
            ConfiguracionGeneral SQLiteClass = new ConfiguracionGeneral();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.TamanoMensaje = row.Field<string>("TamanoMensaje");
                SQLiteClass.CerradoAutomatico = Convert.ToBoolean(row.Field<long>("CerradoAutomatico"));
                SQLiteClass.TiempoMensaje = (int)row.Field<long>("TiempoMensaje");
                SQLiteClass.ForzarResolucion = Convert.ToBoolean(row.Field<long>("ForzarResolucion"));
                SQLiteClass.AnchoResolucion = (int)row.Field<long>("AnchoResolucion");
                SQLiteClass.AltoResolucion = (int)row.Field<long>("AltoResolucion");
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionGeneral(ConfiguracionGeneral SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }
        
        internal ConfiguracionODBC? GetConfiguracionODBC()
        {
            ConfiguracionODBC SQLiteClass = new ConfiguracionODBC();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.ODBC = row.Field<string>("ODBC");
                SQLiteClass.UsuarioODBC = row.Field<string>("UsuarioODBC");
                SQLiteClass.ContrasenaODBC = vSeguridad.DecryptString(MainWindow.nombreApp, row.Field<string>("ContrasenaODBC"));
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionODBC(ConfiguracionODBC SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionVentanaSplash? GetConfiguracionVentanaSplash()
        {
            ConfiguracionVentanaSplash SQLiteClass = new ConfiguracionVentanaSplash();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.TipoSplash = row.Field<string>("TipoSplash");
                SQLiteClass.Audio = row.Field<string>("Audio");
                SQLiteClass.Duracion = (int)row.Field<long>("Duracion");
                SQLiteClass.Ancho = (int)row.Field<long>("Ancho");
                SQLiteClass.Alto = (int)row.Field<long>("Alto");
                SQLiteClass.Voz = Convert.ToBoolean(row.Field<long>("Voz"));
                SQLiteClass.ActivarSplash = Convert.ToBoolean(row.Field<long>("ActivarSplash"));
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionVentanaSplash(ConfiguracionVentanaSplash SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionTurnero? GetConfiguracionTurnero()
        {
            ConfiguracionTurnero SQLiteClass = new ConfiguracionTurnero();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.TipoTurnero = row.Field<string>("TipoTurnero");
                SQLiteClass.ProtocoloTurnero = row.Field<string>("ProtocoloTurnero");
                SQLiteClass.PuertoTCP = (int)row.Field<long>("PuertoTCP");
                SQLiteClass.TurnosAnteriores = (int)row.Field<long>("TurnosAnteriores");
                SQLiteClass.MostrarNombres = Convert.ToBoolean(row.Field<long>("MostrarNombres"));
            }
            catch { return null; }
            return SQLiteClass;
        }
        
        internal bool SetConfiguracionTurnero(ConfiguracionTurnero SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal VozSplash? GetVozSplash()
        {
            VozSplash SQLiteClass = new VozSplash();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.TipoVoz = row.Field<string>("TipoVoz");
                SQLiteClass.TextoVoz1 = row.Field<string>("TextoVoz1");
                SQLiteClass.TextoVoz2 = row.Field<string>("TextoVoz2");
                SQLiteClass.TextoVoz3 = row.Field<string>("TextoVoz3");
                SQLiteClass.VozIngresoEfectivo = Convert.ToBoolean(row.Field<long>("VozIngresoEfectivo"));
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetVozSplash(VozSplash SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal List<ClientesTurnero>? GetClientesTurnero()
        {
            List<ClientesTurnero> SQLiteClass = new List<ClientesTurnero>();
            try
            {
                foreach (DataRow dr in managerSQLite.GetAllTableData(new ClientesTurnero().GetType().Name).Rows)
                    SQLiteClass.Add(new ClientesTurnero() { Cliente = dr.Field<string>("Cliente") });
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetClientesTurnero(List<ClientesTurnero> SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(new ClientesTurnero().GetType().Name);
                foreach (ClientesTurnero clientesTurnero in SQLiteClass)
                    managerSQLite.SaveTableData(new TableClass(clientesTurnero.GetType(), clientesTurnero));
            }
            catch { return false; }
            return true;
        }

        internal List<NombresClientesTurnero>? GetNombresClientesTurnero()
        {
            List<NombresClientesTurnero> SQLiteClass = new List<NombresClientesTurnero>();
            try
            {
                foreach (DataRow dr in managerSQLite.GetAllTableData(new NombresClientesTurnero().GetType().Name).Rows)
                    SQLiteClass.Add(new NombresClientesTurnero()
                    {
                        Identificador = dr.Field<string>("Identificador"),
                        Nombre = dr.Field<string>("Nombre")
                    });
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetNombresClientesTurnero(List<NombresClientesTurnero> SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(new NombresClientesTurnero().GetType().Name);
                foreach (NombresClientesTurnero nombresClientesTurnero in SQLiteClass)
                    managerSQLite.SaveTableData(new TableClass(nombresClientesTurnero.GetType(), nombresClientesTurnero));
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionVerificador? GetConfiguracionVerificador()
        {
            ConfiguracionVerificador SQLiteClass = new ConfiguracionVerificador();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionVerificador(ConfiguracionVerificador SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionCajero? GetConfiguracionCajero()
        {
            ConfiguracionCajero SQLiteClass = new ConfiguracionCajero();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.PuertoTCP = (int)row.Field<long>("PuertoTCP");
                SQLiteClass.COMPayout = row.Field<string>("COMPayout");
                SQLiteClass.SSPPayout = (int)row.Field<long>("SSPPayout");
                SQLiteClass.COMHopper = row.Field<string>("COMHopper");
                SQLiteClass.SSPHopper = (int)row.Field<long>("SSPHopper");
                SQLiteClass.LogPagos = Convert.ToBoolean(row.Field<long>("LogPagos"));
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionCajero(ConfiguracionCajero SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }
        
        internal ConfiguracionImpresora? GetConfiguracionImpresora()
        {
            ConfiguracionImpresora SQLiteClass = new ConfiguracionImpresora();
            try
            {
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
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionImpresora(ConfiguracionImpresora SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionLector? GetConfiguracionLector()
        {
            ConfiguracionLector SQLiteClass = new ConfiguracionLector();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];

                SQLiteClass.Activo = Convert.ToBoolean(row.Field<long>("Activo"));
                SQLiteClass.FormatoCodigo = row.Field<string>("FormatoCodigo");
                SQLiteClass.CantidadDecimales = (int)row.Field<long>("CantidadDecimales");
                SQLiteClass.Imprmir = Convert.ToBoolean(row.Field<long>("Imprmir"));
                SQLiteClass.FormatoImpresora = row.Field<string>("FormatoImpresora");
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionLector(ConfiguracionLector SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal Turno? GetTurno()
        {
            Turno SQLiteClass = new Turno();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.NumeroEquipo = row.Field<string>("NumeroEquipo");
                SQLiteClass.NumeroTurno = (int)row.Field<long>("NumeroTurno");
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetTurno(Turno SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal List<TurnosAnteriores>? GetTurnosAnteriores()
        {
            List<TurnosAnteriores> SQLiteClass = new List<TurnosAnteriores>();
            try
            {
                foreach (DataRow dr in managerSQLite.GetAllTableData(new TurnosAnteriores().GetType().Name).Rows)
                    SQLiteClass.Add(new TurnosAnteriores()
                    {
                        NumeroEquipo = dr.Field<string>("NumeroEquipo"),
                        NumeroTurno = (int)dr.Field<long>("NumeroTurno")
                    });
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetTurnosAnteriores(List<TurnosAnteriores> SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData("TurnosAnteriores");
                foreach (TurnosAnteriores turnosAnteriores in SQLiteClass)
                    managerSQLite.SaveTableData(new TableClass(turnosAnteriores.GetType(), turnosAnteriores));
            }
            catch { return false; }
            return true;
        }

        internal bool EliminarDatosTabla(string nombre)
        {
            try
            {
                managerSQLite.TruncateTableData(nombre);
            }
            catch { return false; }
            return true;
        }

        internal Pago? GetPago()
        {
            Pago SQLiteClass = new Pago();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.TipoPago = (Tipo)row.Field<long>("TipoPago");
                SQLiteClass.EstadoPago = (Estado)row.Field<long>("EstadoPago");
                SQLiteClass.NumPago = (int)row.Field<long>("NumPago");
                SQLiteClass.Equipo  = row.Field<string>("Equipo");
                SQLiteClass.CantidadTotal = (int)row.Field<long>("CantidadTotal");
                SQLiteClass.CantidadIngresada = (int)row.Field<long>("CantidadIngresada");
                SQLiteClass.CantidadFaltante = (int)row.Field<long>("CantidadFaltante");
                SQLiteClass.Cambio = (int)row.Field<long>("Cambio");
                SQLiteClass.CambioCancelado = (int)row.Field<long>("CambioCancelado");
                SQLiteClass.Pagado = Convert.ToBoolean(row.Field<long>("Pagado"));
                SQLiteClass.CantidadBilletesIngresados = (int)row.Field<long>("CantidadBilletesIngresados");
                SQLiteClass.CantidadMonedasIngresadas = (int)row.Field<long>("CantidadMonedasIngresadas");
                SQLiteClass.Impresion = row.Field<string>("Impresion");
                SQLiteClass.BilletesCambio = (int)row.Field<long>("BilletesCambio");
                SQLiteClass.MonedasCambio = (int)row.Field<long>("MonedasCambio");
                SQLiteClass.Canal = (int)row.Field<long>("Canal");
                SQLiteClass.Fecha = DateOnly.Parse(row.Field<string>("Fecha"));
                SQLiteClass.Hora = TimeOnly.Parse(row.Field<string>("Hora"));
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetPago(Pago SQLiteClass)
        {
            try
            {
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionCanalesHopper? GetConfiguracionCanalesHopper()
        {
            ConfiguracionCanalesHopper SQLiteClass = new ConfiguracionCanalesHopper();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.MinCh1 = (int)row.Field<long>("MinCh1");
                SQLiteClass.MinCh2 = (int)row.Field<long>("MinCh2");
                SQLiteClass.MinCh3 = (int)row.Field<long>("MinCh3");
                SQLiteClass.MinCh4 = (int)row.Field<long>("MinCh4");
                SQLiteClass.MinCh5 = (int)row.Field<long>("MinCh5");
                SQLiteClass.MinCh6 = (int)row.Field<long>("MinCh6");
                SQLiteClass.MinCh7 = (int)row.Field<long>("MinCh7");
                SQLiteClass.MinCh8 = (int)row.Field<long>("MinCh8");
                SQLiteClass.MaxCh1 = (int)row.Field<long>("MaxCh1");
                SQLiteClass.MaxCh2 = (int)row.Field<long>("MaxCh2");
                SQLiteClass.MaxCh3 = (int)row.Field<long>("MaxCh3");
                SQLiteClass.MaxCh4 = (int)row.Field<long>("MaxCh4");
                SQLiteClass.MaxCh5 = (int)row.Field<long>("MaxCh5");
                SQLiteClass.MaxCh6 = (int)row.Field<long>("MaxCh6");
                SQLiteClass.MaxCh7 = (int)row.Field<long>("MaxCh7");
                SQLiteClass.MaxCh8 = (int)row.Field<long>("MaxCh8");
                SQLiteClass.ActivoCh1 = Convert.ToBoolean(row.Field<long>("ActivoCh1"));
                SQLiteClass.ActivoCh2 = Convert.ToBoolean(row.Field<long>("ActivoCh2"));
                SQLiteClass.ActivoCh3 = Convert.ToBoolean(row.Field<long>("ActivoCh3"));
                SQLiteClass.ActivoCh4 = Convert.ToBoolean(row.Field<long>("ActivoCh4"));
                SQLiteClass.ActivoCh5 = Convert.ToBoolean(row.Field<long>("ActivoCh5"));
                SQLiteClass.ActivoCh6 = Convert.ToBoolean(row.Field<long>("ActivoCh6"));
                SQLiteClass.ActivoCh7 = Convert.ToBoolean(row.Field<long>("ActivoCh7"));
                SQLiteClass.ActivoCh8 = Convert.ToBoolean(row.Field<long>("ActivoCh8"));
                SQLiteClass.PagoMax = (int)row.Field<long>("PagoMax");
                SQLiteClass.MostrarLog = Convert.ToBoolean(row.Field<long>("MostrarLog"));
            }
            catch { return null; }
            return SQLiteClass;
        }

        internal bool SetConfiguracionCanalesHopper(ConfiguracionCanalesHopper SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }

        internal ConfiguracionCanalesPayout? GetConfiguracionCanalesPayout()
        {
            ConfiguracionCanalesPayout SQLiteClass = new ConfiguracionCanalesPayout();
            try
            {
                DataRow row = managerSQLite.GetAllTableData(SQLiteClass.GetType().Name).Rows[0];
                SQLiteClass.MinCh1 = (int)row.Field<long>("MinCh1");
                SQLiteClass.MinCh2 = (int)row.Field<long>("MinCh2");
                SQLiteClass.MinCh3 = (int)row.Field<long>("MinCh3");
                SQLiteClass.MinCh4 = (int)row.Field<long>("MinCh4");
                SQLiteClass.MinCh5 = (int)row.Field<long>("MinCh5");
                SQLiteClass.MinCh6 = (int)row.Field<long>("MinCh6");
                SQLiteClass.MinCh7 = (int)row.Field<long>("MinCh7");
                SQLiteClass.MinCh8 = (int)row.Field<long>("MinCh8");
                SQLiteClass.MaxCh1 = (int)row.Field<long>("MaxCh1");
                SQLiteClass.MaxCh2 = (int)row.Field<long>("MaxCh2");
                SQLiteClass.MaxCh3 = (int)row.Field<long>("MaxCh3");
                SQLiteClass.MaxCh4 = (int)row.Field<long>("MaxCh4");
                SQLiteClass.MaxCh5 = (int)row.Field<long>("MaxCh5");
                SQLiteClass.MaxCh6 = (int)row.Field<long>("MaxCh6");
                SQLiteClass.MaxCh7 = (int)row.Field<long>("MaxCh7");
                SQLiteClass.MaxCh8 = (int)row.Field<long>("MaxCh8");
                SQLiteClass.ActivoCh1 = Convert.ToBoolean(row.Field<long>("ActivoCh1"));
                SQLiteClass.ActivoCh2 = Convert.ToBoolean(row.Field<long>("ActivoCh2"));
                SQLiteClass.ActivoCh3 = Convert.ToBoolean(row.Field<long>("ActivoCh3"));
                SQLiteClass.ActivoCh4 = Convert.ToBoolean(row.Field<long>("ActivoCh4"));
                SQLiteClass.ActivoCh5 = Convert.ToBoolean(row.Field<long>("ActivoCh5"));
                SQLiteClass.ActivoCh6 = Convert.ToBoolean(row.Field<long>("ActivoCh6"));
                SQLiteClass.ActivoCh7 = Convert.ToBoolean(row.Field<long>("ActivoCh7"));
                SQLiteClass.ActivoCh8 = Convert.ToBoolean(row.Field<long>("ActivoCh8"));
                SQLiteClass.PagoMax = (int)row.Field<long>("PagoMax");
                SQLiteClass.MostrarLog = Convert.ToBoolean(row.Field<long>("MostrarLog"));
            }
            catch { return null; }
            return SQLiteClass;
        }


        internal bool SetConfiguracionCanalesPayout(ConfiguracionCanalesPayout SQLiteClass)
        {
            try
            {
                managerSQLite.TruncateTableData(SQLiteClass.GetType().Name);
                managerSQLite.SaveTableData(new TableClass(SQLiteClass.GetType(), SQLiteClass));
            }
            catch { return false; }
            return true;
        }
        
        internal bool ResetConfigSplash()
        {
            try
            {
                managerSQLite.TruncateTableData(new ConfiguracionVentanaSplash().GetType().Name);
                managerSQLite.TruncateTableData(new ConfiguracionTurnero().GetType().Name);
                managerSQLite.TruncateTableData(new VozSplash().GetType().Name);
                managerSQLite.TruncateTableData(new ClientesTurnero().GetType().Name);
                managerSQLite.TruncateTableData(new ConfiguracionVerificador().GetType().Name);
                managerSQLite.TruncateTableData(new ConfiguracionCajero().GetType().Name);
                managerSQLite.TruncateTableData(new ConfiguracionCajero().GetType().Name);
            }
            catch { return false; }
            return true;
        }

    }
}
