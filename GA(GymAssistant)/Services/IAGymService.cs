using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GA_GymAssistant_.Models;

namespace GA_GymAssistant.Services
{
    public class IAGymService
    {
        string apiKeyDirecta = "AIzaSyDQkzOGEg93AKwLAadTPA5ugtAzhpNYJLs";
        string modelo = "gemini-1.5-flash";
        private readonly HttpClient _httpClient;
        private readonly string  _apiKey;

        // El "Prompt del Sistema" define la personalidad de tu IA
        private const string PROMPT_SISTEMA = @"
            Eres 'GymPass Trainer', un entrenador personal experto, motivador y directo.
            Tu objetivo es ayudar a los usuarios a mejorar su salud física.
            REGLAS:
            1. Responde de forma breve (máximo 1 o 2 párrafos).
            2. Ten en cuenta SIEMPRE las lesiones del usuario si las menciona el contexto.
            3. Si te preguntan algo fuera de fitness/salud, responde amablemente que solo hablas de entrenamiento.
        ";

        public IAGymService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Busca la clave en tu appsettings.json
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<string> ObtenerRespuesta(string preguntaUsuario, string contextoUsuario)
        {
           
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelo}:generateContent?key={apiKeyDirecta}";

            var promptCompleto = $@"
                {PROMPT_SISTEMA}
                CONTEXTO DEL USUARIO: {contextoUsuario}
                PREGUNTA: {preguntaUsuario}
            ";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = promptCompleto } } } }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jsonNode = JsonNode.Parse(result);
                    // Navegamos por el JSON de respuesta de Google
                    return jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
                           ?? "La IA no devolvió texto.";
                }
                return $"Error de conexión con IA: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"Error técnico: {ex.Message}";
            }
        }
        // ... (código anterior de la clase) ...

        public async Task<string> GenerateRoutineFromAI(Usuario usuario, List<Ejercicio> ejerciciosDisponibles, string comentariosAdicionales = "")
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-pro-preview:generateContent?key={_apiKey}";

            // PASO CLAVE: Convertir la lista de objetos a un STRING legible para la IA
            // Esto creará un texto tipo:
            // - Press de Banca (Pecho)
            // - Sentadilla (Piernas)
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

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jsonNode = JsonNode.Parse(result);
                    return jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
                           ?? "La IA no generó respuesta.";
                }
                return "Error en la API de IA.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}