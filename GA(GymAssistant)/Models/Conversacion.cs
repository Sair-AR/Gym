using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GA_GymAssistant_.Models
{
    public class Conversacion
    {
        [Key]
        public int IdConversacion { get; set; }

        public int IdUsuario { get; set; }
        public string Titulo { get; set; } = "Nueva Conversación";
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        // Relación con los mensajes
        public List<IAConsulta> Mensajes { get; set; } = new List<IAConsulta>();
    }
}

