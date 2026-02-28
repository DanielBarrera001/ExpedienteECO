using ExpedienteECO.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpedienteECO
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablas principales del sistema
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Medicamento> Medicamentos { get; set; }
        public DbSet<DetalleTratamiento> DetalleTratamientos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Es vital mantener el base.OnModelCreating para las tablas de Identity (Usuarios)
            base.OnModelCreating(builder);

        }
    }
}