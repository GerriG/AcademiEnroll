﻿using AcademiEnroll.Models;
using Microsoft.EntityFrameworkCore;


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

        public DbSet<Nota> Notas { get; set; }

        public DbSet<Inscripcion> Inscripciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Docente>().HasKey(d => d.IdDocente);
            modelBuilder.Entity<Estudiante>().HasKey(e => e.IdEstudiante);
            modelBuilder.Entity<Models.Administrador>().HasKey(e => e.IdAdministrador);
            // Configura otros modelos según sea necesario


        }
    }
}
