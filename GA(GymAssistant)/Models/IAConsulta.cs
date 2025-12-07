using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GA_GymAssistant_.Models
{
    public class IAConsulta
    {
        [Key]
        public int IdConsulta { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        [Required]
        public string Pregunta { get; set; } // NVARCHAR(MAX)

        [Required]
        public string Respuesta { get; set; } // NVARCHAR(MAX)

        public DateTime Fecha { get; set; } = DateTime.Now; // DEFAULT GETDATE()

        // Propiedad de navegación
        public Usuario Usuario { get; set; }

        public int? IdConversacion { get; set; } // Nuevo campo

        public Conversacion? Conversacion { get; set; }
    }
}
