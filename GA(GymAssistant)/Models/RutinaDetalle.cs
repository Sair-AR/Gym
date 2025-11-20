using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GA_GymAssistant_.Models
{
    public class RutinaDetalle
    {
        [Key]
        public int IdRutinaDetalle { get; set; }

        [Required]
        [ForeignKey("Rutina")]
        public int IdRutina { get; set; }

        [Required]
        [ForeignKey("Ejercicio")]
        public int IdEjercicio { get; set; }

        [Required]
        public int Series { get; set; }

        [Required]
        public int Repeticiones { get; set; }

        public string ParametrosIA { get; set; }

        // Propiedades de navegación
        public Rutina Rutina { get; set; }
        public Ejercicio Ejercicio { get; set; }
    }
}
