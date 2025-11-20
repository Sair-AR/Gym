using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GA_GymAssistant_.Models
{
    public class Rutina
    {
        [Key]
        public int IdRutina { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string RutinaBase { get; set; } // NVARCHAR(MAX)

        public string RutinaIA { get; set; } // NVARCHAR(MAX)

        public string Notas { get; set; } // NVARCHAR(MAX)

        // Propiedad de navegación
        public Usuario Usuario { get; set; }
    }
}
