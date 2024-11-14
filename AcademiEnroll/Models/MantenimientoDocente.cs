using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademiEnroll.Models
{
    public class MantenimientoDocente
    {
        private readonly AcademiEnrollContext _context;

        public MantenimientoDocente(AcademiEnrollContext context)
        {
            _context = context;
        }

        public async Task<int> Ingresar(Docente docente)
        {
            _context.Docentes.Add(docente);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Docente>> ListarTodos()
        {
            return await _context.Docentes.ToListAsync();
        }

        public async Task<Docente> Consultar(int id)
        {
            return await _context.Docentes.FindAsync(id);
        }

        public async Task<int> Modificar(Docente docente)
        {
            _context.Docentes.Update(docente);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Borrar(int id)
        {
            var docente = await _context.Docentes.FindAsync(id);
            if (docente != null)
            {
                _context.Docentes.Remove(docente);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }
    }
}