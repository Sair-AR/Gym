using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GA_GymAssistant.Services
{
    public class IAGymService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string MODELO = "gemini-1.5-flash";

        // TU CLAVE API (Pégala aquí si no está en appsettings)
        private const string API_KEY_RESPALDO = "AIzaSyA2pg0dPzFkXpOeBpEL1b683-OTQB0HVPs";

        public IAGymService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var keyConfig = configuration["Gemini:ApiKey"];
            _apiKey = !string.IsNullOrEmpty(keyConfig) ? keyConfig : API_KEY_RESPALDO;
        }

        public async Task<string> ObtenerRespuesta(string preguntaUsuario, string contextoUsuario)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{MODELO}:generateContent?key={_apiKey}";

            // Le damos personalidad al chat
            var prompt = $"Actúa como un entrenador personal experto, motivador y conciso. Contexto del usuario: {contextoUsuario}. Pregunta: {preguntaUsuario}";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode) return "Lo siento, tuve un problema de conexión. Intenta de nuevo.";

                var result = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(result);

                // Extraer solo el texto de la respuesta
                return jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
                       ?? "No supe qué responder.";
            }
            catch (Exception)
            {
                return "Ocurrió un error al procesar tu consulta.";
            }
        }
    }
}