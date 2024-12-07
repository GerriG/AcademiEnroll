using AcademiEnroll.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AcademiEnroll.Data
{
    public class AcademiEnrollContext : DbContext
    {
        public AcademiEnrollContext(DbContextOptions<AcademiEnrollContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Models.Administrador> Administrador { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Nota> Notas { get; set; }
        public DbSet<Inscripciones> Inscripciones { get; set; }
        public DbSet<PeriodoGlobal> PeriodoGlobal { get; set; }
        public DbSet<MateriasAprobadas> MateriasAprobadas { get; set; } // Agregado

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Docente>().HasKey(d => d.IdDocente);
            modelBuilder.Entity<Estudiante>().HasKey(e => e.IdEstudiante);
            modelBuilder.Entity<Models.Administrador>().HasKey(e => e.IdAdministrador);

            // Clave primaria para Usuario
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);

            // Relación Usuario -> Docente (uno a uno, sin propiedad de navegación)
            modelBuilder.Entity<Docente>()
                .HasOne<Usuario>() // Relación con Usuario
                .WithOne() // Sin propiedad de navegación
                .HasForeignKey<Docente>(d => d.IdUsuario) // FK en Docente
                .OnDelete(DeleteBehavior.Cascade); // Eliminación en cascada

            // Relación Usuario -> Estudiante (uno a uno, sin propiedad de navegación)
            modelBuilder.Entity<Estudiante>()
                .HasOne<Usuario>()
                .WithOne()
                .HasForeignKey<Estudiante>(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Usuario -> Administrador (uno a uno, sin propiedad de navegación)
            modelBuilder.Entity<Models.Administrador>()
                .HasOne<Usuario>()
                .WithOne()
                .HasForeignKey<Models.Administrador>(a => a.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // Claves primarias para Docente, Estudiante, Administrador
            modelBuilder.Entity<Docente>().HasKey(d => d.IdDocente);
            modelBuilder.Entity<Estudiante>().HasKey(e => e.IdEstudiante);
            modelBuilder.Entity<Models.Administrador>().HasKey(a => a.IdAdministrador);

            // Relación Materia -> Docente (muchos a uno)
            modelBuilder.Entity<Materia>()
                .HasOne(m => m.Docente)
                .WithMany()
                .HasForeignKey(m => m.IdDocente)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Inscripciones -> Estudiante
            modelBuilder.Entity<Inscripciones>()
                .HasOne(i => i.Estudiante)
                .WithMany()
                .HasForeignKey(i => i.IdEstudiante)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Inscripciones -> Materia
            modelBuilder.Entity<Inscripciones>()
                .HasOne(i => i.Materia)
                .WithMany()
                .HasForeignKey(i => i.IdMateria)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Materia -> Usuario (Docente)
            modelBuilder.Entity<Materia>()
                .HasOne(m => m.Docente)
                .WithMany()
                .HasForeignKey(m => m.IdDocente)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación entre Inscripcion y Estudiante
            modelBuilder.Entity<Inscripciones>()
                .HasOne(i => i.Estudiante)
                .WithMany()  // Relación de uno a muchos, un estudiante puede tener muchas inscripciones
                .HasForeignKey(i => i.IdEstudiante)
                .OnDelete(DeleteBehavior.Cascade);  // Eliminación en cascada cuando se elimina un estudiante

            // Relación entre Inscripcion y Materia
            modelBuilder.Entity<Inscripciones>()
                .HasOne(i => i.Materia)
                .WithMany()  // Relación de uno a muchos, una materia puede tener muchas inscripciones
                .HasForeignKey(i => i.IdMateria)
                .OnDelete(DeleteBehavior.Cascade);  // Eliminación en cascada cuando se elimina una materia
			

			// Relación Materia -> Docente (opcional si es relevante para otras consultas)
			modelBuilder.Entity<Materia>()
				.HasOne(m => m.Docente)
				.WithMany()
				.HasForeignKey(m => m.IdDocente);

            // Configuración para MateriasAprobadas
            modelBuilder.Entity<MateriasAprobadas>().HasKey(ma => ma.Id);

            // Relación MateriasAprobadas -> Estudiante
            modelBuilder.Entity<MateriasAprobadas>()
                .HasOne<Estudiante>() // Sin propiedad de navegación explícita
                .WithMany()
                .HasForeignKey(ma => ma.IdEstudiante)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación MateriasAprobadas -> Materia
            modelBuilder.Entity<MateriasAprobadas>()
                .HasOne<Materia>() // Sin propiedad de navegación explícita
                .WithMany()
                .HasForeignKey(ma => ma.IdMateria)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // Método para ejecutar un procedimiento almacenado para el Dashboard usando EF Core
        public async Task<DashBoard> ObtenerDashboardDataAsync()
        {
            var result = await this.Set<DashBoard>()
                .FromSqlRaw("EXEC sp_ReporteDashboard")
                .AsNoTracking()
                .ToListAsync();

            return result.FirstOrDefault() ?? new DashBoard();
        }
    }
}
