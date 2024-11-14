using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademiEnroll.Models
{
 
     public class MantenimientoEstudiante
    {
        private readonly AcademiEnrollContext _context;

        public MantenimientoEstudiante(AcademiEnrollContext context)
        {
            _context = context;
        }

        public async Task<int> Ingresar(Estudiante estudiante)
        {
            _context.Estudiantes.Add(estudiante);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Estudiante>> ListarTodos()
        {
            return await _context.Estudiantes.ToListAsync();
        }

        public async Task<Estudiante> Consultar(int id)
        {
            return await _context.Estudiantes.FindAsync(id);
        }

        public async Task<int> Modificar(Estudiante estudiante)
        {
            _context.Estudiantes.Update(estudiante);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Borrar(int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante != null)
            {
                _context.Estudiantes.Remove(estudiante);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }
    }
}

