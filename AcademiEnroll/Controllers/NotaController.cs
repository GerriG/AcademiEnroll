using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcademiEnroll.Controllers
{
    public class NotaController : Controller
    {
        private readonly AcademiEnrollContext _context;

        public NotaController(AcademiEnrollContext context)
        {
            _context = context;
        }

        // GET: NotaController
        public async Task<IActionResult> Index()
        {
            // Obtener el rol del usuario desde el claim
            var rol = User.FindFirst("Rol")?.Value;
            var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value; // Obtener el nombre del usuario (si es necesario)

            // Verificar si es Docente
            if (rol == "Docente")
            {
                var notasDocente = await _context.Notas
                    .Select(n => new Nota
                    {
                        Id = n.Id,
                        NombreEstudiante = n.NombreEstudiante,
                        NombreAsignatura = n.NombreAsignatura,
                        Calificacion = n.Calificacion
                    })
                    .ToListAsync();

                ViewBag.EsDocente = true;  // Indicar que es Docente
                ViewBag.EsAdministrador = false;  // No es Administrador
                return View(notasDocente);
            }

            // Verificar si es Estudiante
            if (rol == "Estudiante")
            {
                var notasEstudiante = await _context.Notas
                    .Where(n => n.NombreEstudiante == usuarioNombre)
                    .ToListAsync();

                ViewBag.EsDocente = false;  // No es Docente
                ViewBag.EsAdministrador = false;  // No es Administrador
                return View(notasEstudiante);
            }

            // Si es un Administrador (o cualquier otro rol), devolver todas las notas
            var todasLasNotas = await _context.Notas.ToListAsync();
            ViewBag.EsDocente = false;  // No es Docente
            ViewBag.EsAdministrador = true;  // Es Administrador
            return View(todasLasNotas);
        }

        // GET: NotaController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            // Buscar la nota por su id
            var nota = await _context.Notas.FindAsync(id);

            if (nota == null)
            {
                return NotFound();
            }

            // Obtener el rol del usuario
            var rol = User.FindFirst("Rol")?.Value;

            // Asignar a ViewBag.EsDocente si el rol es Docente
            ViewBag.EsDocente = rol == "Docente";

            return View(nota); // Retorna la nota a la vista
        }

        // GET: NotaController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Verificar el rol del usuario en el controlador
            var rol = User.FindFirst("Rol")?.Value;

            // Si no es un administrador, retornar Unauthorized
            if (rol != "Administrador")
            {
                return Unauthorized(); // o redirigir al acceso denegado si lo prefieres
            }

            // Buscar la nota por su id
            var nota = await _context.Notas.FindAsync(id);
            if (nota == null)
            {
                return NotFound();
            }

            return View(nota);
        }

        // POST: NotaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,NombreEstudiante,NombreAsignatura,Calificacion")] Nota nota)
        {
            if (id != nota.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Llamar al procedimiento almacenado para actualizar la nota
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC SP_ActualizarNota @Id = {0}, @Calificacion = {1}",
                        nota.Id, nota.Calificacion);

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotaExists(nota.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(nota);
        }

        // GET: NotaController/Create
        public async Task<ActionResult> Create()
        {
            // Verificar el rol del usuario en el controlador
            var rol = User.FindFirst("Rol")?.Value;

            // Si no es un administrador, retornar Unauthorized
            if (rol != "Docente")
            {
                return Unauthorized(); // o redirigir al acceso denegado si lo prefieres
            }

            // Obtener los nombres de los estudiantes
            var estudiantes = await _context.Estudiantes
                .Select(e => e.Nombre)
                .ToListAsync();

            // Obtener los nombres de las asignaturas
            var asignaturas = await _context.Materias
                .Select(m => m.Nombre)
                .ToListAsync();

            // Pasar los datos al ViewBag
            ViewBag.Estudiantes = new SelectList(estudiantes);
            ViewBag.Asignaturas = new SelectList(asignaturas);

            return View();
        }

        // POST: NotaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("NombreEstudiante,NombreAsignatura,Calificacion")] Nota nota)
        {
            if (nota.Calificacion < 0 || nota.Calificacion > 10)
            {
                // Agregar mensaje de error al ModelState
                ModelState.AddModelError("Calificacion", "Nota inválida! Ingrese una nota de 0 a 10.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(nota);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si no es válida, regresar a la vista con los errores
            // Recargar los datos del ViewBag
            var estudiantes = await _context.Estudiantes
                .Select(e => e.Nombre)
                .ToListAsync();
            var asignaturas = await _context.Materias
                .Select(m => m.Nombre)
                .ToListAsync();
            ViewBag.Estudiantes = new SelectList(estudiantes);
            ViewBag.Asignaturas = new SelectList(asignaturas);

            return View(nota);
        }

        // GET: Nota/Delete/
        public async Task<IActionResult> Delete(int id)
        {
            // Aquí se asegura que el usuario tiene el rol adecuado
            var rol = User.FindFirst("Rol")?.Value;
            if (rol != "Administrador")
            {
                return Unauthorized(); // Si el rol no es "Administrador", se deniega el acceso
            }

            // Buscar la nota por ID
            var nota = await _context.Notas
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nota == null)
            {
                return NotFound(); // Si no se encuentra la nota, retornar error 404
            }

            return View(nota); // Mostrar la vista de confirmación con los detalles de la nota
        }

        // POST: Nota/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Buscar la nota por ID
            var nota = await _context.Notas.FindAsync(id);

            if (nota == null)
            {
                return NotFound(); // Si no se encuentra la nota, retornar error 404
            }

            _context.Notas.Remove(nota); // Eliminar la nota del contexto
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos

            return RedirectToAction(nameof(Index)); // Redirigir al listado después de la eliminación
        }

        private bool NotaExists(int id)
        {
            return _context.Notas.Any(e => e.Id == id);
        }
    }
}
