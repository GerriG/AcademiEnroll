using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace AcademiEnroll.Controllers
{
    public class InscripcionController : Controller
    {
        private readonly AcademiEnrollContext _context;

        public InscripcionController(AcademiEnrollContext context)
        {
            _context = context;
        }

        // GET: Inscripcion/Index
        public async Task<IActionResult> Index()
        {
            
            
            // Obtener el Id del Estudiante desde los claims
            var usuarioIdEstudiante = User.Claims.FirstOrDefault(c => c.Type == "IdEstudiante")?.Value;

            if (usuarioIdEstudiante == null)
            {
                return Unauthorized(); // El usuario no es un estudiante
            }

            var inscripciones = await _context.Inscripciones
                .Include(i => i.Materia)
                .Where(i => i.IdEstudiante == int.Parse(usuarioIdEstudiante))
                .ToListAsync();

            var materias = await _context.Materias
                .Include(m => m.Docente)
                .ToListAsync();

            // Pasar a la vista las materias y las inscripciones actuales
            ViewBag.Inscripciones = inscripciones;
            return View(materias);
        }

        // GET: Inscripcion/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Obtener la materia de la base de datos
            var materia = await _context.Materias
                .Include(m => m.Docente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materia == null)
            {
                return NotFound(); // Si no se encuentra la materia, retorna NotFound
            }

            // Obtener el Id del Estudiante desde los claims
            var usuarioIdEstudiante = User.Claims.FirstOrDefault(c => c.Type == "IdEstudiante")?.Value;

            if (usuarioIdEstudiante == null)
            {
                return Unauthorized(); // El usuario no es un estudiante
            }

            var estudianteId = int.Parse(usuarioIdEstudiante);

            // Verificar si el estudiante ya está inscrito en esta materia
            var inscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.IdEstudiante == estudianteId && i.IdMateria == id);

            if (inscripcion != null)
            {
                // Si ya está inscrito, agregar detalles de la inscripción a la vista
                ViewBag.CodInscripcion = inscripcion.CodInscripcion;
                ViewBag.FechaInscripcion = inscripcion.FechaInscripcion; // Asegúrate de tener esta columna
            }

            return View(materia); // Solo muestra la materia (no la inscripción si no está inscrito)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retirar(int id)
        {
            var usuarioIdEstudiante = User.Claims.FirstOrDefault(c => c.Type == "IdEstudiante")?.Value;

            if (usuarioIdEstudiante == null)
            {
                return Unauthorized();
            }

            var estudianteId = int.Parse(usuarioIdEstudiante);

            // Buscar la inscripción correspondiente
            var inscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.IdEstudiante == estudianteId && i.IdMateria == id);

            if (inscripcion == null)
            {
                TempData["Error"] = "No estás inscrito en esta materia.";
                return RedirectToAction(nameof(Index));
            }

            // Eliminar la inscripción
            _context.Inscripciones.Remove(inscripcion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Materia retirada con éxito.";
            return RedirectToAction(nameof(Index));
        }


        // POST: Inscripcion/Inscribir/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(int id)
        {
            var usuarioIdEstudiante = User.Claims.FirstOrDefault(c => c.Type == "IdEstudiante")?.Value;

            if (usuarioIdEstudiante == null)
            {
                return Unauthorized(); // El usuario no es un estudiante
            }

            var estudianteId = int.Parse(usuarioIdEstudiante);

            // Verificar cuántas materias tiene inscritas el estudiante
            var cantidadInscritas = await _context.Inscripciones
                .CountAsync(i => i.IdEstudiante == estudianteId);

            // Limitar a 5 materias
            if (cantidadInscritas >= 5)
            {
                TempData["Error"] = "Ya has alcanzado el máximo de 5 materias inscritas.";
                return RedirectToAction(nameof(Index)); // Redirige a la página de inscripción con mensaje
            }

            // Verificar si el estudiante ya está inscrito en esta materia
            var existenciaInscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.IdEstudiante == estudianteId && i.IdMateria == id);

            if (existenciaInscripcion != null)
            {
                TempData["Error"] = "Ya estás inscrito en esta materia.";
                return RedirectToAction(nameof(Details), new { id = id }); // Si ya está inscrito, redirige
            }

            // Crear la nueva inscripción
            var inscripcion = new Inscripciones
            {
                IdEstudiante = estudianteId,
                IdMateria = id
            };

            _context.Inscripciones.Add(inscripcion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Te has inscrito exitosamente en la materia.";
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}