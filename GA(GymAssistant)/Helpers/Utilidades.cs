using System.Security.Cryptography;
using System.Text;

namespace GA_GymAssistant.Helpers
{
    public static class Utilidades
    {
        public static string EncriptarClave(string clave)
        {
            // Convierte "hola123" en una cadena segura tipo "a665a45920422f9d417..."
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(clave));

                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}

