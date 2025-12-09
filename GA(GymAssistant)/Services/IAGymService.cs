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
        private const string MODELO = "gemini-2.5-flash";

        // TU CLAVE API (Pégala aquí si no está en appsettings)
        private const string API_KEY_RESPALDO = "AIzaSyAQkLfztZRFiFDXNprj1U-TP_ajH0u_9rg";

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
            var prompt = $"Actúa como un entrenador personal experto, motivador y conciso. Contexto del usuario: {contextoUsuario}. Pregunta: {preguntaUsuario}. Se Preciso y no des una espuesta muy extensa, da la rutina para 5 dias, y teniendo en cuenta las lesiones del usuario. MAXIMO 10 lineas por dia";


            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return $"ERROR DE API ({response.StatusCode}): {errorDetails}";
                }

                var result = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(result);

                return jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
                        ?? "La IA respondió, pero el texto vino vacío.";
            }
            catch (Exception ex)
            {
                return $"ERROR TÉCNICO CRÍTICO: {ex.Message}";
            }
        }
    }
}