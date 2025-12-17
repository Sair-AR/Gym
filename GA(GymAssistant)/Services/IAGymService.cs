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

        // Asegúrate que este modelo esté disponible para tu cuenta (suele ser gemini-1.5-flash)
        private const string MODELO = "gemini-2.5-flash";

        // ⚠️ He ocultado tu clave por seguridad. Regenerala y pégala aquí.
        private const string API_KEY_RESPALDO = "AIzaSyCv2IpVgs2qSqIfOpe_dS2W_i34vqmYqbc";

        public IAGymService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var keyConfig = configuration["Gemini:ApiKey"];
            _apiKey = !string.IsNullOrEmpty(keyConfig) ? keyConfig : API_KEY_RESPALDO;
        }

        public async Task<string> ObtenerRespuesta(string preguntaUsuario, string contextoUsuario)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{MODELO}:generateContent?key={_apiKey}";

            // --- PROMPT MEJORADO Y BLINDADO ---
            var prompt = $@"
                ROL: 
                Eres 'GymPass Trainer', un entrenador personal de élite experto en biomecánica y salud. NO eres un asistente general.

                REGLA DE BLOQUEO (IMPORTANTE):
                Si la 'PREGUNTA DEL USUARIO' no está relacionada con: ejercicio, rutinas, lesiones, nutrición o salud física, DEBES RESPONDER EXACTAMENTE:
                'Soy un entrenador personal y solo puedo ayudarte con temas de fitness y salud. ¿En qué te ayudo con tu entrenamiento?'
                (No respondas la pregunta fuera de contexto, ignórala totalmente).

                DATOS DEL CLIENTE (Contexto):
                {contextoUsuario}

                INSTRUCCIONES DE GENERACIÓN:
                1. Analiza las LESIONES del cliente en los datos de arriba. Si sugieres un ejercicio peligroso para su lesión, fallas tu misión.
                2. Si pide una rutina: Genera un plan de 5 DÍAS.
                3. Sé conciso: MÁXIMO 10 líneas por día de entrenamiento.
                4. Usa un tono motivador pero técnico.

                INSTRUCCIONES DE FORMATO OBLIGATORIAS:
                1. Si el usuario pide rutina, responde ÚNICAMENTE con una TABLA HTML.
                2. Columnas OBLIGATORIAS: 'Día', 'Músculo', 'Ejercicio', 'Series', 'Reps', 'Tips'.
                3. PROHIBIDO poner columnas con texto como 'Ver descripción' o enlaces.
                4. Si hay instrucciones técnicas (ej: 'bajada lenta'), ponlas en la columna 'Tips' o junto al nombre del ejercicio.
                5. No uses Markdown (```html), solo dame el código <table> puro.
                6. Agrega la clase 'w-full text-sm text-left rtl:text-right text-gray-500' a la tabla.

                PREGUNTA DEL USUARIO:
                {preguntaUsuario}
            ";
            // ----------------------------------

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