using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademiEnroll.Models
{
    public class MantenimientoNota
    {
        private readonly AcademiEnrollContext _context;

        public MantenimientoNota(AcademiEnrollContext context)
        {
            _context = context;
        }

        public async Task<int> Ingresar(Nota nota)
        {
            _context.Notas.Add(nota);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Nota>> ListarTodos()
        {
            return await _context.Notas.ToListAsync();
        }

        public async Task<Nota> Consultar(int id)
        {
            return await _context.Notas.FindAsync(id);
        }

        public async Task<int> Modificar(Nota nota)
        {
            _context.Notas.Update(nota);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Borrar(int id)
        {
            var nota = await _context.Notas.FindAsync(id);
            if (nota != null)
            {
                _context.Notas.Remove(nota);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }
    }
}
