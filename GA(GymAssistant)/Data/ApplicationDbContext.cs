using Microsoft.EntityFrameworkCore;
using GA_GymAssistant_.Models; 
namespace GA_GymAssistant.Data 
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<HistorialProgreso> HistorialProgreso { get; set; }

        public DbSet<IAConsulta> IAConsultas { get; set; }

        public DbSet<Ejercicio> Ejercicios { get; set; }
        public DbSet<Maquinas> Maquinas { get; set; }
        public DbSet<Rutina> Rutinas { get; set; }
        public DbSet<RutinaDetalle> RutinaDetalle { get; set; }
        public DbSet<Conversacion> Conversaciones { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IAConsulta>().ToTable("IA_Consultas");
            modelBuilder.Entity<RutinaDetalle>().ToTable("RutinaDetalle");
            modelBuilder.Entity<Ejercicio>().ToTable("Ejercicios");
            modelBuilder.Entity<HistorialProgreso>().ToTable("HistorialProgreso");
            // --- CONFIGURACIÓN DE LA RELACIÓN CONVERSACIÓN <-> CONSULTAS ---
            modelBuilder.Entity<IAConsulta>()
                .HasOne(c => c.Conversacion)       // Una consulta tiene una conversación
                .WithMany(cv => cv.Mensajes)       // Una conversación tiene muchos mensajes
                .HasForeignKey(c => c.IdConversacion) // <--- ¡AQUÍ ESTÁ LA CLAVE!
                .OnDelete(DeleteBehavior.Cascade); // Opcional: Si borras el chat, se borran los mensajes

            // Asegúrate de que las tablas se llamen como en tu SQL
            modelBuilder.Entity<Conversacion>().ToTable("Conversaciones");
            modelBuilder.Entity<IAConsulta>().ToTable("IA_Consultas");
        }
    }
}