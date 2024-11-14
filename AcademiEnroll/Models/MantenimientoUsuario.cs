using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademiEnroll.Models
{
    public class MantenimientoUsuario
    {
        private readonly AcademiEnrollContext _context;

        public MantenimientoUsuario(AcademiEnrollContext context)
        {
            _context = context;
        }

        public async Task<int> Ingresar(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Usuario>> ListarTodos()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario> Consultar(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<int> Modificar(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Borrar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }
    }
}

