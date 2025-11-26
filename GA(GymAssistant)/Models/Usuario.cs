using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GA_GymAssistant_.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool Estado { get; set; }
        public int? Edad { get; set; }

        public double? Peso { get; set; } // Puede ser nulo, se actualiza en el Historial

        public double? Altura { get; set; }

        public string? Nivel { get; set; } // Ej: Principiante, Intermedio

        public string? Objetivo { get; set; } // Ej: Ganar músculo, Pérdida de peso

        public string? ZonaObjetivo { get; set; }

        public string? Lesiones { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime? FechaRegistro { get; set; } = DateTime.Now; // DEFAULT GETDATE()

        public string? TipoUsuario { get; set; }
    }
}
