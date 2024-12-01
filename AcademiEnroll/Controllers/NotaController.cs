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
                    .OrderByDescending(n => n.Id) // Ordenar de más reciente a más antiguo por ID (equivalente a ORDER BY Id DESC)
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
                    .OrderByDescending(n => n.Id) // Ordenar de más reciente a más antiguo por ID (equivalente a ORDER BY Id DESC)
                    .ToListAsync();

                ViewBag.EsDocente = false;  // No es Docente
                ViewBag.EsAdministrador = false;  // No es Administrador
                return View(notasEstudiante);
            }

            // Si es un Administrador (o cualquier otro rol), devolver todas las notas
            var todasLasNotas = await _context.Notas
                .OrderBy(n => n.Id) // Ordenar de más reciente a más antiguo por ID (equivalente a ORDER BY Id DESC)
                .ToListAsync();

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
            var rol = User.FindFirst("Rol")?.Value;

            if (rol != "Docente")
            {
                return Unauthorized();
            }

            var idDocenteClaim = User.FindFirst("IdDocente")?.Value;
            if (string.IsNullOrEmpty(idDocenteClaim) || !int.TryParse(idDocenteClaim, out var idDocente))
            {
                return Unauthorized("No se encontró el Id del docente.");
            }

            // Obtener los estudiantes inscritos en materias impartidas por el docente actual
            var estudiantes = await _context.Inscripciones
                .Where(i => i.Materia.IdDocente == idDocente)
                .Select(i => i.Estudiante)
                .Distinct()
                .ToListAsync();

            ViewBag.Estudiantes = new SelectList(estudiantes, "IdEstudiante", "Nombre");

            return View();
        }


        // POST: NotaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("NombreEstudiante,NombreAsignatura,Calificacion")] Nota nota)
        {
            // Validación de la calificación
            if (nota.Calificacion < 0 || nota.Calificacion > 10)
            {
                // Agregar mensaje de error al ModelState
                ModelState.AddModelError("Calificacion", "Nota inválida! Ingrese una nota de 0 a 10.");
            }

            if (ModelState.IsValid)
            {
                // Obtener el nombre del estudiante usando su ID
                var estudiante = await _context.Estudiantes
                    .Where(e => e.IdEstudiante == int.Parse(nota.NombreEstudiante)) // NombreEstudiante contiene el ID
                    .Select(e => e.Nombre)
                    .FirstOrDefaultAsync();

                // Obtener el nombre de la asignatura usando su ID
                var materia = await _context.Materias
                    .Where(m => m.Id == int.Parse(nota.NombreAsignatura)) // NombreAsignatura contiene el ID
                    .Select(m => m.Nombre)
                    .FirstOrDefaultAsync();

                if (estudiante == null || materia == null)
                {
                    ModelState.AddModelError("", "Datos inválidos seleccionados.");
                    return View(nota);
                }

                // Asignar los nombres obtenidos al objeto nota
                nota.NombreEstudiante = estudiante;
                nota.NombreAsignatura = materia;

                // Guardar en la base de datos
                _context.Add(nota);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Si no es válida, regresar a la vista con los errores
            // Recargar los datos del ViewBag
            var estudiantes = await _context.Estudiantes
                .Select(e => new { e.IdEstudiante, e.Nombre })
                .ToListAsync();
            var asignaturas = await _context.Materias
                .Select(m => new { m.Id, m.Nombre })
                .ToListAsync();

            ViewBag.Estudiantes = new SelectList(estudiantes, "IdEstudiante", "Nombre");
            ViewBag.Asignaturas = new SelectList(asignaturas, "Id", "Nombre");

            return View(nota);
        }


        [HttpGet]
        public async Task<IActionResult> GetMateriasPorEstudiante(int estudianteId)
        {
            var rol = User.FindFirst("Rol")?.Value;

            if (rol != "Docente")
            {
                return Unauthorized();
            }

            var idDocenteClaim = User.FindFirst("IdDocente")?.Value;
            if (string.IsNullOrEmpty(idDocenteClaim) || !int.TryParse(idDocenteClaim, out var idDocente))
            {
                return Unauthorized("No se encontró el Id del docente.");
            }

            // Filtrar materias inscritas por el estudiante bajo el docente actual
            var materias = await _context.Inscripciones
                .Where(i => i.IdEstudiante == estudianteId && i.Materia.IdDocente == idDocente)
                .Select(i => new { i.Materia.Id, i.Materia.Nombre })
                .ToListAsync();

            return Json(materias);
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
