using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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


        // POST: Inscripcion/Inscribir/5
        // POST: Inscripcion/Inscribir/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(int id)
        {
            var usuarioIdEstudiante = User.Claims.FirstOrDefault(c => c.Type == "IdEstudiante")?.Value;

            if (usuarioIdEstudiante == null)
            {
                return Unauthorized();
            }

            var estudianteId = int.Parse(usuarioIdEstudiante);

            // Verificar si el estudiante ya está inscrito
            var existenciaInscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.IdEstudiante == estudianteId && i.IdMateria == id);

            if (existenciaInscripcion != null)
            {
                return RedirectToAction(nameof(Index)); // Si ya está inscrito, redirige
            }

            // Crear la nueva inscripción (no necesitas el campo Codigo)
            var inscripcion = new Inscripciones
            {
                IdEstudiante = estudianteId,
                IdMateria = id
            };

            _context.Inscripciones.Add(inscripcion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}


