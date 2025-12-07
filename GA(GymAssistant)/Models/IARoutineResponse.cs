using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace GA_GymAssistant.Models
{
    // Modelo auxiliar para el detalle de ejercicios (se usa en la lista)
    public class RoutineDetailModel
    {
        [JsonPropertyName("id_ejercicio")]
        public int id_ejercicio { get; set; }

        [JsonPropertyName("series")]
        public int series { get; set; }

        [JsonPropertyName("repeticiones")]
        public int repeticiones { get; set; }

        [JsonPropertyName("parametros_ia")]
        public string? parametros_ia { get; set; }
    }

    // Modelo principal que representa la estructura JSON de la rutina generada por la IA
    public class IARoutineResponse
    {

        [JsonPropertyName("saludo")]
        public string? saludo { get; set; }

        [JsonPropertyName("rutina_base")]
        public string? rutina_base { get; set; }

        [JsonPropertyName("rutina_ia")]
        public string? rutina_ia { get; set; }

        [JsonPropertyName("notas")]
        public string? notas { get; set; }

        [JsonPropertyName("nutricion")]
        public string? nutricion { get; set; }

        // Colección de los ejercicios y sus parámetros
        [JsonPropertyName("ejercicios_detalle")]
        public List<RoutineDetailModel>? ejercicios_detalle { get; set; }
    }
}