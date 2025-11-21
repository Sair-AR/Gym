
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using GA_GymAssistant_.Models;

namespace GA_GymAssistant.Services
{
    public class IAGymService
    {
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        // Usamos el modelo Flash por su velocidad y eficiencia en tareas estructuradas
        private const string GEMINI_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        // Inyección de HttpClient (desde Program.cs) e IConfiguration (para la clave API)
        public IAGymService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Lee la clave API de la sección "Gemini:ApiKey" en appsettings.json
            _geminiApiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured.");
        }

        /// <summary>
        /// Genera una rutina de ejercicios llamando a la API de Google Gemini.
        /// </summary>
        public async Task<string> GenerateRoutineFromAI(Usuario user, List<Ejercicio> availableExercises)
        {
            // 1. Crear el System Instruction (Reglas para la IA)
            string systemInstruction = @"
                Eres un asistente de entrenamiento experto. Tu única tarea es generar una rutina semanal de ejercicios
                en formato JSON ESTRICTO. La salida debe ser SOLO el objeto JSON que cumpla con el siguiente schema:
                {
                    ""rutina_base"": ""[string]"",
                    ""rutina_ia"": ""[string]"",
                    ""notas"": ""[string]"",
                    ""ejercicios_detalle"": [
                        { ""id_ejercicio"": [int], ""series"": [int], ""repeticiones"": [int], ""parametros_ia"": ""[string: Ej. Peso sugerido o tiempo de descanso]"" }
                    ]
                }
                Los valores 'id_ejercicio' DEBEN ser tomados EXCLUSIVAMENTE de la lista de ejercicios disponibles proporcionada en el mensaje del usuario. 
                Asegúrate de que la rutina siga el Objetivo, Nivel y Lesiones del usuario.
            ";

            // 2. Crear el User Message (Datos y Contexto)
            StringBuilder userMessageBuilder = new StringBuilder();
            userMessageBuilder.AppendLine($"Usuario: {user.Nombre}, Peso: {user.Peso}kg, Altura: {user.Altura}m.");
            userMessageBuilder.AppendLine($"Objetivo: {user.Objetivo}");
            userMessageBuilder.AppendLine($"Nivel: {user.Nivel}");
            userMessageBuilder.AppendLine($"Lesiones/Restricciones: {user.Lesiones}");
            userMessageBuilder.AppendLine("Ejercicios disponibles (ID: Nombre | Zona):");

            foreach (var exercise in availableExercises)
            {
                userMessageBuilder.AppendLine($"- ID {exercise.IdEjercicio}: {exercise.Nombre} | Zona: {exercise.Zona} | Requiere Máquina: {exercise.RequiereMaquina}");
            }
            userMessageBuilder.AppendLine("Genera la rutina semanal en el JSON estricto.");

            string userMessage = userMessageBuilder.ToString();

            // 3. Crear el Cuerpo de la Solicitud (Request Body)
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userMessage } }
                    }
                },
                config = new
                {
                    systemInstruction = systemInstruction,
                    // ESTO ES CLAVE: Pide la respuesta en formato JSON
                    responseMimeType = "application/json"
                }
            };

            // 4. Enviar la Solicitud HTTP a Gemini
            string jsonPayload = JsonSerializer.Serialize(requestBody);

            // La clave API se envía en la URL como parámetro 'key'
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{GEMINI_ENDPOINT}?key={_geminiApiKey}");
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Lanza excepción si hay error HTTP

            string responseContent = await response.Content.ReadAsStringAsync();

            // 5. Extraer el JSON de la respuesta de Gemini
            // Gemini envuelve el JSON que pedimos dentro de su propia estructura JSON.
            using JsonDocument doc = JsonDocument.Parse(responseContent);

            // Navegación para obtener el JSON de rutina que está dentro del campo 'text'
            var textElement = doc.RootElement
                                 .GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text");

            // Devuelve la cadena JSON limpia que será deserializada por el controlador.
            return textElement.GetString() ?? throw new Exception("Gemini returned empty routine content.");
        }
    }
}