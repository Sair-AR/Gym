using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GA_GymAssistant_.Models
{
    public class Ejercicio
    {
        [Key]
        public int IdEjercicio { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Zona { get; set; }

        public string Nivel { get; set; }

        [Required]
        public bool RequiereMaquina { get; set; }

        [ForeignKey("Maquinas")]
        public int? IdMaquina { get; set; } // Permite null si RequiereMaquina es false

        // Propiedad de navegación
        public Maquinas Maquina { get; set; }
    }
}
