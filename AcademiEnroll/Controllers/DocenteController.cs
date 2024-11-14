//Controlador 
using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Administrador.Controllers
{
    public class DocenteController : Controller
    {
        private readonly AcademiEnrollContext _context;

        public DocenteController(AcademiEnrollContext context)
        {
            _context = context;
        }

        // Acción para mostrar la lista de docentes
        public async Task<ActionResult> Index()
        {
            var docentes = await _context.Docentes.ToListAsync();
            return View(docentes);
        }

        // Acción para mostrar los detalles de un docente específico
        public async Task<ActionResult> Details(int id)
        {
            var docente = await _context.Docentes
                
                
                .FirstOrDefaultAsync(d => d.IdDocente == id);

            if (docente == null)
            {
                return NotFound();
            }
            return View(docente);
        }

        // Acción para mostrar el formulario de creación de un nuevo docente
        public ActionResult Create()
        {
            return RedirectToAction("Registro", "Cuenta");
        }

        // Acción para crear un nuevo docente en la base de datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("idUsuario,Nombre,Apellido,Correo")] Usuario docente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(docente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(docente);
        }

        // Acción para mostrar el formulario de edición de un docente
        public async Task<ActionResult> Edit(int id)
        {
            var docente = await _context.Docentes.FindAsync(id);
            if (docente == null)
            {
                return NotFound();
            }
            return View(docente);
        }

        // Acción para guardar los cambios realizados en la edición de un docente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("id,Nombre,Apellido,Correo,idUsuario")] Docente docente)
        {
            if (id != docente.IdDocente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(docente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocenteExists(docente.IdDocente))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(docente);
        }

        // Acción para mostrar el formulario de eliminación de un docente
        public async Task<ActionResult> Delete(int id)
        {
            var docente = await _context.Docentes.FindAsync(id);
            if (docente == null)
            {
                return NotFound();
            }
            return View(docente);
        }

        // Acción para eliminar un docente de la base de datos
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var docente = await _context.Docentes.FindAsync(id);
            _context.Docentes.Remove(docente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Acción para gestionar los estudiantes inscritos en un curso específico
        public async Task<ActionResult> GestionarEstudiantes(int idCurso)
        {
            var curso = await _context.Inscripciones
                .Where(c => c.CodInscripcion == idCurso)
                .Include(i => i.Estudiantes) // Incluye la lista de estudiantes para el curso seleccionado
                .FirstOrDefaultAsync();

            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        // Acción para agregar una nota a un estudiante en una asignatura específica
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AgregarNota(int idEstudiante, string nombreAsignatura, decimal calificacion)
        {
            // Validación de la calificación para que esté en el rango de 0 a 10
            if (calificacion < 0 || calificacion > 10)
            {
                ModelState.AddModelError("", "La calificación debe estar entre 0 y 10.");
                return BadRequest(ModelState);
            }

            // Buscar el estudiante para verificar que existe
            var estudiante = await _context.Estudiantes.FindAsync(idEstudiante);
            if (estudiante == null)
            {
                return NotFound();
            }

            // Crear y agregar la nueva nota en la base de datos
            var nota = new Nota
            {
                NombreEstudiante = estudiante.Nombre,
                NombreAsignatura = nombreAsignatura,
                Calificacion = calificacion
            };

            _context.Notas.Add(nota);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GestionarEstudiantes), new { idCurso = idEstudiante });
        }

        // Método privado para verificar si un docente existe en la base de datos
        private bool DocenteExists(int id)
        {
            return _context.Docentes.Any(e => e.IdDocente == id);
        }
    }
}
