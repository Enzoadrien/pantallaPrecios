
namespace Priceio.ClasesSQLite
{
    internal class ConfiguracionCajero
    {
        public int PuertoTCP
        {
            get;
            set;
        }
        public string? COMPayout
        {
            get;
            set;
        }
        public int SSPPayout
        {
            get;
            set;
        }
        public string? COMHopper
        {
            get;
            set;
        }
        public int SSPHopper
        {
            get;
            set;
        }
        public bool? LogPagos
        {
            get;
            set;
        }
    }
}
