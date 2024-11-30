﻿using AcademiEnroll.Models;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Docente>().HasKey(d => d.IdDocente);
            modelBuilder.Entity<Estudiante>().HasKey(e => e.IdEstudiante);
            modelBuilder.Entity<Models.Administrador>().HasKey(e => e.IdAdministrador);

            // Relación Materia -> Usuario (Docente)
            modelBuilder.Entity<Materia>()
                .HasOne(m => m.Docente)
                .WithMany()
                .HasForeignKey(m => m.IdDocente)
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
