using System.ComponentModel.DataAnnotations;

namespace GA_GymAssistant_.Models
{
    public class Maquinas
    {
        [Key]
        public int IdMaquina { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Zona { get; set; }
    }
}
