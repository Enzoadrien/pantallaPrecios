
namespace Priceio.ClasesSQLite
{
    internal class ConfiguracionTurnero
    {
        public string? TipoTurnero
        {
            get;
            set;
        }
        public string? ProtocoloTurnero
        {
            get;
            set;
        }
        public int PuertoTCP
        {
            get;
            set;
        }
        public int TurnosAnteriores
        {
            get;
            set;
        }
        public bool? MostrarNombres
        {
            get;
            set;
        }
    }
}
