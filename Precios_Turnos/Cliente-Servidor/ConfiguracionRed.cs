using System;
using System.IO;
using System.Text.Json;

namespace Priceio.Turnero
{
    public class ConfiguracionRed
    {
        private static readonly string RutaArchivo =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_red.json");

        // ... propiedades existentes ...
        public string Ip { get; set; } = "192.168.1.64";
        public int Puerto { get; set; } = 8080;
        public string ApiKey { get; set; } = GenerarApiKey();
        public string NombreLogo { get; set; } = "";
        public string NombreFondo { get; set; } = "";
        public bool ColorPersonalizado { get; set; } = false;
        public byte ColorBotonR { get; set; } = 14;
        public byte ColorBotonG { get; set; } = 165;
        public byte ColorBotonB { get; set; } = 233;

        // ── AGREGAR AQUÍ ──────────────────────────────────────
        public byte ColorTextoR { get; set; } = 255;
        public byte ColorTextoG { get; set; } = 255;
        public byte ColorTextoB { get; set; } = 255;
        public bool ColorTextoPersonalizado { get; set; } = false;

        public byte ColorSombraR { get; set; } = 14;
        public byte ColorSombraG { get; set; } = 165;
        public byte ColorSombraB { get; set; } = 233;
        public bool ColorSombraPersonalizado { get; set; } = false;
        // ─────────────────────────────────────────────────────
        public string ContrasenaHash { get; set; } = HashContrasena("1234");


        // ── Carpeta interna donde se guardan las imágenes ─────
        public static string CarpetaTurnero =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "turnero");

        public string RutaLogoCompleta =>
            string.IsNullOrEmpty(NombreLogo) ? "" :
            Path.Combine(CarpetaTurnero, NombreLogo);

        public string RutaFondoCompleta =>
            string.IsNullOrEmpty(NombreFondo) ? "" :
            Path.Combine(CarpetaTurnero, NombreFondo);

        public static string HashContrasena(string contrasena)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(contrasena));
            return Convert.ToBase64String(bytes);
        }

        public bool VerificarContrasena(string contrasena)
        {
            return ContrasenaHash == HashContrasena(contrasena);
        }

        public void CambiarContrasena(string nuevaContrasena)
        {
            ContrasenaHash = HashContrasena(nuevaContrasena);
            Guardar();
        }

        // ── ApiKey ────────────────────────────────────────────
        private static string GenerarApiKey()
        {
            var bytes = new byte[24];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        // ── Cargar ────────────────────────────────────────────
        public static ConfiguracionRed Cargar()
        {
            try
            {
                if (File.Exists(RutaArchivo))
                {
                    string json = File.ReadAllText(RutaArchivo);
                    return JsonSerializer.Deserialize<ConfiguracionRed>(json)
                           ?? new ConfiguracionRed();
                }
            }
            catch { }

            var nueva = new ConfiguracionRed();
            nueva.Guardar();
            return nueva;
        }

        // ── Guardar ───────────────────────────────────────────
        public void Guardar()
        {
            try
            {
                string json = JsonSerializer.Serialize(this,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(RutaArchivo, json);
            }
            catch { }
        }

        // ── Copiar imagen a carpeta interna ───────────────────
        public static string CopiarImagen(string rutaOrigen, string nombre)
        {
            try
            {
                Directory.CreateDirectory(CarpetaTurnero);
                string extension = Path.GetExtension(rutaOrigen);
                string nombreArchivo = nombre + extension;
                string destino = Path.Combine(CarpetaTurnero, nombreArchivo);
                File.Copy(rutaOrigen, destino, true);
                return nombreArchivo; // solo guardamos el nombre
            }
            catch { return ""; }
        }
    }
}