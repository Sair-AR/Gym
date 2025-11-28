using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GA_GymAssistant_.Models;

namespace GA_GymAssistant.Services
{
    public class IAGymService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // ✅ CORRECCIÓN 1: Usamos el modelo que confirmamos que funciona
        private const string MODELO = "gemini-2.5-flash";

        // ⚠️ PEGA TU CLAVE AQUÍ SI NO QUIERES USAR APPSETTINGS (Pero cuidado al compartir el código)
        private const string API_KEY_RESPALDO = "AIzaSyDQkzOGEg93AKwLAadTPA5ugtAzhpNYJLs";

        public IAGymService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            // Intenta leer del archivo json, si no encuentra nada, usa la de respaldo
            var keyConfig = configuration["Gemini:ApiKey"];
            _apiKey = !string.IsNullOrEmpty(keyConfig) ? keyConfig : API_KEY_RESPALDO;
        }

        public async Task<string> ObtenerRespuesta(string preguntaUsuario, string contextoUsuario)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{MODELO}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[] {
                    new { parts = new[] { new { text = $"Contexto: {contextoUsuario}. Pregunta: {preguntaUsuario}" } } }
                }
            };

            return await EnviarSolicitud(url, requestBody);
        }

        public async Task<string> GenerateRoutineFromAI(Usuario usuario, List<Ejercicio> ejerciciosDisponibles, string comentariosAdicionales = "")
        {
            // ✅ CORRECCIÓN 2: Ahora este método usa también 'gemini-1.5-flash' (antes tenía el gemini-3 que fallaba)
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{MODELO}:generateContent?key={_apiKey}";

            var listadoEjerciciosTexto = string.Join("\n",
                ejerciciosDisponibles.Select(e => $"- {e.Nombre} (Zona: {e.Zona}, Nivel: {e.Nivel})"));

            var promptRutina = $@"
                Actúa como un entrenador personal experto. Genera una rutina para este usuario:
                
                PERFIL:
                - Nombre: {usuario.Nombre}
                - Objetivo: {usuario.Objetivo}
                - Nivel: {usuario.Nivel}
                - Lesiones: {usuario.Lesiones}
                
                PREFERENCIAS EXTRA: {comentariosAdicionales}

                INSTRUCCIÓN IMPORTANTE:
                La rutina DEBE crearse basándose PRINCIPALMENTE en la siguiente lista de ejercicios disponibles en nuestro gimnasio.
                Si necesitas agregar un ejercicio muy común que no esté en la lista, puedes hacerlo, pero prioriza estos:

                LISTA DE EJERCICIOS DISPONIBLES:
                {listadoEjerciciosTexto}

                FORMATO DE RESPUESTA:
                Día X: [Grupo Muscular]
                - Ejercicio: [Nombre] | Series: [X] | Reps: [X]
            ";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = promptRutina } } } }
            };

            return await EnviarSolicitud(url, requestBody);
        }

        // ✅ MEJORA: Método auxiliar para no repetir código y manejar errores en un solo lugar
        private async Task<string> EnviarSolicitud(string url, object requestBody)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    return $"FALLO DE CONEXIÓN (Error {response.StatusCode}). \nVerifica tu API Key o el modelo.";
                }

                var result = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(result);
                return jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
                       ?? "La IA no generó respuesta.";
            }
            catch (Exception ex)
            {
                return $"Error técnico: {ex.Message}";
            }
        }
    }
}