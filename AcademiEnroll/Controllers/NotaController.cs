using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Web;

namespace AcademiEnroll.Controllers
{
    public class NotaController : Controller
    {
        private readonly AcademiEnrollContext _context;

        public NotaController(AcademiEnrollContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correoUsuario))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var rol = User.FindFirst("Rol")?.Value;
            var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value;
            var idDocenteClaim = User.FindFirst("IdDocente")?.Value;

            // Filtrar las materias aprobadas usando el IdMateria
            var materiasAprobadas = await _context.MateriasAprobadas
                                                   .Select(ma => new { ma.IdEstudiante, ma.IdMateria })
                                                   .ToListAsync();

            // Obtener el valor del periodoGlobal
            var periodoGlobal = await _context.PeriodoGlobal                                               
                                               .Select(c => c.Periodo)
                                               .FirstOrDefaultAsync();

            if (rol == "Docente" && !string.IsNullOrEmpty(idDocenteClaim) && int.TryParse(idDocenteClaim, out int idDocente))
            {
                // Obtener las notas del docente
                var notasDocente = await (from n in _context.Notas
                                          join d in _context.Docentes on n.IdDocente equals d.IdDocente
                                          join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                                          join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                                          where periodoGlobal == 5 || // Mostrar todas las notas aprobadas si el periodo es 5
                                                !_context.MateriasAprobadas
                                                .Any(ma => ma.IdEstudiante.ToString() == n.NombreEstudiante && ma.IdMateria.ToString() == n.NombreAsignatura) // Excluir materias aprobadas si el periodo es diferente
                                          select new NotaViewModel
                                          {
                                              Id = n.Id,
                                              NombreEstudiante = e.Nombre,
                                              Nombre = m.Nombre,
                                              Calificacion = n.Calificacion,
                                              NombreDocente = d.Nombre,
                                              Periodo = n.Periodo
                                          })
                                         .OrderBy(n => n.Id)
                                         .ToListAsync();

                ViewBag.EsDocente = true;
                ViewBag.EsAdministrador = false;
                return View(notasDocente);
            }

            if (rol == "Estudiante")
            {
                // Obtener las notas del estudiante
                var notasEstudiante = await (from n in _context.Notas
                                             join d in _context.Docentes on n.IdDocente equals d.IdDocente
                                             join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                                             join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                                             where periodoGlobal == 5 || // Mostrar todas las notas aprobadas si el periodo es 5
                                                   !_context.MateriasAprobadas
                                                   .Any(ma => ma.IdEstudiante.ToString() == n.NombreEstudiante && ma.IdMateria.ToString() == n.NombreAsignatura) // Excluir materias aprobadas si el periodo es diferente
                                             select new NotaViewModel
                                             {
                                                 Id = n.Id,
                                                 NombreEstudiante = e.Nombre,
                                                 Nombre = m.Nombre,
                                                 Calificacion = n.Calificacion,
                                                 NombreDocente = d.Nombre,
                                                 Periodo = n.Periodo
                                             })
                                              .OrderBy(n => n.Id)
                                              .ToListAsync();

                ViewBag.EsDocente = false;
                ViewBag.EsAdministrador = false;
                return View(notasEstudiante);
            }

            if (rol == "Administrador")
            {
                // Obtener todas las notas sin restricciones de materias aprobadas o reprobadas si el periodo es 5
                var todasLasNotas = await (from n in _context.Notas
                                           join d in _context.Docentes on n.IdDocente equals d.IdDocente
                                           join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                                           join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                                           where periodoGlobal == 5 || // Mostrar todas las notas aprobadas si el periodo es 5
                                                 !_context.MateriasAprobadas
                                                 .Any(ma => ma.IdEstudiante.ToString() == n.NombreEstudiante && ma.IdMateria.ToString() == n.NombreAsignatura) // Excluir materias aprobadas si el periodo es 1
                                           select new NotaViewModel
                                           {
                                               Id = n.Id,
                                               NombreEstudiante = e.Nombre,
                                               Nombre = m.Nombre,
                                               Calificacion = n.Calificacion,
                                               NombreDocente = d.Nombre,
                                               Periodo = n.Periodo
                                           })
                                          .OrderBy(n => n.Id)
                                          .ToListAsync();

                ViewBag.EsDocente = false;
                ViewBag.EsAdministrador = true;
                return View(todasLasNotas);
            }

            // Redirigir en caso de rol no identificado
            return RedirectToAction("Login", "Cuenta");
        }






        // GET: NotaController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correoUsuario))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            // Buscar la nota por su id e incluir los datos relacionados
            var nota = await (from n in _context.Notas
                              join d in _context.Docentes on n.IdDocente equals d.IdDocente
                              join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                              join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                              where n.Id == id
                              select new NotaViewModel
                              {
                                  Id = n.Id,
                                  NombreEstudiante = e.Nombre, // Nombre del estudiante
                                  Nombre = m.Nombre, // Nombre de la materia
                                  Calificacion = n.Calificacion,
                                  NombreDocente = d.Nombre,
                                  Periodo = n.Periodo
                              }).FirstOrDefaultAsync(); // Obtiene solo la primera coincidencia

            if (nota == null)
            {
                return NotFound();
            }

            // Obtener el rol del usuario
            var rol = User.FindFirst("Rol")?.Value;

            // Asignar a ViewBag.EsDocente si el rol es Docente
            ViewBag.EsDocente = rol == "Docente";

            return View(nota); // Retorna la nota a la vista con la información relacionada
        }


        // GET: NotaController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correoUsuario))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var userRol = User.FindFirst("Rol")?.Value;
            if (userRol != "Administrador")
            {
                return Unauthorized("Estimado Usuario, usted no es un Administrador.");
            }

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

            // Validar que la calificación esté entre 0 y 10
            if (nota.Calificacion < 0 || nota.Calificacion > 10)
            {
                ModelState.AddModelError("Calificacion", "La calificación debe estar entre 0 y 10.");
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
            var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correoUsuario))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var userRol = User.FindFirst("Rol")?.Value;
            if (userRol != "Docente")
            {
                return Unauthorized("Estimado Usuario, usted no es un Docente.");
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
            var idDocenteClaim = User.FindFirst("IdDocente")?.Value;
            if (string.IsNullOrEmpty(idDocenteClaim) || !int.TryParse(idDocenteClaim, out var idDocente))
            {
                return Unauthorized("No se encontró el Id del docente.");
            }

            nota.IdDocente = idDocente; // Asignar el IdDocente del usuario actual

            // Validación de la calificación
            if (nota.Calificacion < 0 || nota.Calificacion > 10)
            {
                TempData["ErrorMessage"] = "Nota inválida. Ingrese una calificación entre 0 y 10.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener el periodo global actual
            var periodoGlobal = await _context.PeriodoGlobal.FirstOrDefaultAsync();
            if (periodoGlobal == null)
            {
                TempData["ErrorMessage"] = "No se encontró un periodo activo. Contacte al administrador.";
                return RedirectToAction(nameof(Index));
            }

            // Asignar el periodo al objeto Nota
            nota.Periodo = periodoGlobal.Periodo;

            // Verificar si ya existe una nota para este estudiante, materia, periodo y docente
            var notaExistente = await (from n in _context.Notas
                                       join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                                       join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                                       where n.Periodo == nota.Periodo
                                             && n.NombreEstudiante == nota.NombreEstudiante
                                             && n.NombreAsignatura == nota.NombreAsignatura
                                             && n.IdDocente == idDocente
                                       select new
                                       {
                                           n.Id,
                                           NombreEstudiante = e.Nombre, // Obtenemos el nombre del estudiante
                                           NombreMateria = m.Nombre // Nombre de la materia
                                       }).FirstOrDefaultAsync();

            if (notaExistente != null)
            {
                TempData["ErrorMessage"] = $"El estudiante {notaExistente.NombreEstudiante} ya tiene una nota registrada para el periodo {nota.Periodo} en la materia {notaExistente.NombreMateria}.";
                return RedirectToAction(nameof(Index));
            }

            // Guardar la nota
            _context.Notas.Add(nota);
            await _context.SaveChangesAsync();

            // Verificar promedio después de guardar la nota
            await VerificarPromedio(int.Parse(nota.NombreAsignatura), int.Parse(nota.NombreEstudiante));

            TempData["SuccessMessage"] = "Nota creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }




        [HttpGet]
        public async Task<IActionResult> GetMateriasPorEstudiante(int estudianteId)
        {
            var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correoUsuario))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var userRol = User.FindFirst("Rol")?.Value;
            if (userRol != "Docente")
            {
                return Unauthorized("Estimado Usuario, usted no es un Docente.");
            }

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


        public async Task<IActionResult> Delete(int id)
        {
            var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correoUsuario))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var userRol = User.FindFirst("Rol")?.Value;
            if (userRol != "Administrador")
            {
                return Unauthorized("Estimado Usuario, usted no es un Administrador.");
            }

            // Aquí se asegura que el usuario tiene el rol adecuado
            var rol = User.FindFirst("Rol")?.Value;
            if (rol != "Administrador")
            {
                return Unauthorized(); // Si el rol no es "Administrador", se deniega el acceso
            }

            // Buscar la nota por ID e incluir los datos relacionados de Estudiantes, Docentes y Materias
            var nota = await (from n in _context.Notas
                              join d in _context.Docentes on n.IdDocente equals d.IdDocente
                              join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                              join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                              where n.Id == id
                              select new NotaViewModel
                              {
                                  Id = n.Id,
                                  NombreEstudiante = e.Nombre, // Nombre del estudiante
                                  Nombre = m.Nombre, // Nombre de la materia
                                  Calificacion = n.Calificacion,
                                  NombreDocente = d.Nombre,
                                  Periodo = n.Periodo
                              }).FirstOrDefaultAsync(); // Obtiene solo la primera coincidencia

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
                TempData["ErrorMessage"] = "No se encontró la nota a eliminar."; // Mensaje de error
                return RedirectToAction(nameof(Index)); // Redirigir a la vista Index
            }

            _context.Notas.Remove(nota); // Eliminar la nota del contexto
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos

            TempData["SuccessMessage"] = "La nota fue eliminada exitosamente."; // Mensaje de éxito
            return RedirectToAction(nameof(Index)); // Redirigir al listado después de la eliminación
        }


        private bool NotaExists(int id)
        {
            return _context.Notas.Any(e => e.Id == id);
        }

        //Verificar Promedio
        public async Task<IActionResult> VerificarPromedio(int idMateria, int idEstudiante)
        {
            // Obtener las notas de la materia en los 5 periodos
            var notas = await _context.Notas
                .Where(n => n.NombreAsignatura == idMateria.ToString() && n.NombreEstudiante == idEstudiante.ToString())
                .ToListAsync();

            if (notas.Count == 5)
            {
                // Calcular el promedio con una sola decimal
                var promedio = Math.Round(notas.Average(n => n.Calificacion), 1);

                // Determinar el estado dependiendo del promedio
                string estado = (promedio >= 6.0m) ? "Aprobado" : "Reprobado";
                DateTime? fechaAprobacion = (promedio >= 6.0m) ? DateTime.Now : (DateTime?)null;

                // Revisar si ya existe en MateriasAprobadas
                var materiaExistente = await _context.MateriasAprobadas
                    .FirstOrDefaultAsync(ma => ma.IdMateria == idMateria && ma.IdEstudiante == idEstudiante);

                if (materiaExistente == null)
                {
                    // Mover a MateriasAprobadas con el nuevo estado y la fecha de aprobación (null si reprobado)
                    var materiaAprobada = new MateriasAprobadas
                    {
                        IdMateria = idMateria,
                        IdEstudiante = idEstudiante,
                        Promedio = promedio,
                        Estado = estado,  // Asignar el estado calculado
                        FechaAprobacion = fechaAprobacion  // Asignar la fecha (null si reprobado)
                    };

                    _context.MateriasAprobadas.Add(materiaAprobada);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = (estado == "Aprobado")
                        ? "Materia aprobada y movida exitosamente."
                        : "Materia reprobada, pero movida a la lista de materias con estado Reprobado.";
                }
                else
                {
                    // Si ya existe la materia, actualizar el estado, promedio y fecha
                    materiaExistente.Promedio = promedio;
                    materiaExistente.Estado = estado;  // Actualizar el estado
                    materiaExistente.FechaAprobacion = fechaAprobacion;  // Actualizar la fecha de aprobación

                    _context.MateriasAprobadas.Update(materiaExistente);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = (estado == "Aprobado")
                        ? "Materia ya aprobada, se actualizó el estado y el promedio."
                        : "Materia reprobada, se actualizó el estado a Reprobado y el promedio.";
                }
            }
            else
            {
                TempData["Error"] = "No se tienen las 5 notas necesarias para calcular el promedio.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
