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

        // Método para ingresar un nuevo usuario
        public async Task<int> Ingresar(Usuario usuario)
        {
            try
            {
                _context.Usuarios.Add(usuario);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Manejo de error, puede registrar en log o algo similar
                // Log.Error(ex.Message); // Ejemplo de logging si tienes una librería para ello
                return 0; // Retornar 0 si falla
            }
        }

        // Método para listar todos los usuarios
        public async Task<List<Usuario>> ListarTodos()
        {
            try
            {
                return await _context.Usuarios.ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de error
                // Log.Error(ex.Message);
                return new List<Usuario>(); // Retornar una lista vacía si falla
            }
        }

        // Método para consultar un usuario por su ID
        public async Task<Usuario> Consultar(int id)
        {
            try
            {
                return await _context.Usuarios.FindAsync(id);
            }
            catch (Exception ex)
            {
                // Manejo de error
                // Log.Error(ex.Message);
                return null; // Retornar null si falla
            }
        }

        // Método para modificar un usuario
        public async Task<int> Modificar(Usuario usuario)
        {
            try
            {
                _context.Usuarios.Update(usuario);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Manejo de error
                // Log.Error(ex.Message);
                return 0; // Retornar 0 si falla
            }
        }

        // Método para borrar un usuario
        public async Task<int> Borrar(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario != null)
                {
                    _context.Usuarios.Remove(usuario);
                    int result = await _context.SaveChangesAsync();
                    return result;
                }
                return 0; // Retorna 0 si no se encuentra el usuario
            }
            catch (Exception ex)
            {
                // Manejo de error
                // Log.Error(ex.Message);
                throw new Exception("Error al intentar eliminar el usuario: " + ex.Message);
            }
        }
    }
}
