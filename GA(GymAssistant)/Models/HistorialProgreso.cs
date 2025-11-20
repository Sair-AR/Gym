using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GA_GymAssistant_.Models
{
    public class HistorialProgreso
    {
        [Key]
        public int IdProgreso { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now; // DEFAULT GETDATE()

        public double? Peso { get; set; } // El ? indica que es nullable, como FLOAT en SQL

        public double? GrasaCorporal { get; set; }

        public double? MedidaPecho { get; set; }

        public double? MedidaCintura { get; set; }

        public double? MedidaPierna { get; set; }

        // Propiedad de navegación (opcional pero recomendada para EF Core)
        public Usuario Usuario { get; set; }
    }
}
