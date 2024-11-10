
namespace Priceio.ClasesSQLite
{
    internal class ConfiguracionImpresora
    {
        public string? Nombre
        {
            get;
            set;
        }
        public string? TipoLetra
        {
            get;
            set;
        }
        public int TamanoLetra
        {
            get;
            set;
        }
        public bool? Negrita
        {
            get;
            set;
        }
        public bool? Cursiva
        {
            get;
            set;
        }

        public bool? Logo
        {
            get;
            set;
        }
        public string? RutaLogo
        {
            get;
            set;
        }
        public int TamanoLogo
        {
            get;
            set;
        }
        public int CordenadaXLogo
        {
            get;
            set;
        }
        public int CordenadaYLogo
        {
            get;
            set;
        }
    }
}
