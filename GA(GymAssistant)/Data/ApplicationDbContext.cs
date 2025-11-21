using Microsoft.EntityFrameworkCore;
using GA_GymAssistant_.Models; // Asegúrate de que este namespace coincida con el de tus modelos

namespace GA_GymAssistant.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor: Recibe la configuración de la base de datos desde Program.cs
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =======================================================
        // Propiedades DbSet<> - Mapeo de Tablas SQL
        // =======================================================

        // Tablas de Perfil y Seguimiento
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<HistorialProgreso> HistorialProgreso { get; set; }
        public DbSet<IAConsulta> IAConsultas { get; set; }

        // Tablas de Catálogo
        public DbSet<Ejercicio> Ejercicios { get; set; }
        public DbSet<Maquinas> Maquinas { get; set; }

        // Tablas de Rutinas
        public DbSet<Rutina> Rutinas { get; set; }
        public DbSet<RutinaDetalle> RutinaDetalle { get; set; }


        // --------------------------------------------------------------------------------
        // Configuración para Relaciones (Opcional, si no usaste convenciones perfectas)
        // --------------------------------------------------------------------------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Nota: Aquí se pueden configurar claves primarias compuestas,
            // relaciones muchos a muchos, o nombres de tablas específicos
            // si difieren de las convenciones de C#.

            base.OnModelCreating(modelBuilder);
        }
    }
}