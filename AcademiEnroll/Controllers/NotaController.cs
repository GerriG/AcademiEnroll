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
                // Obtener las notas del docente (sin filtrarlas por periodo)
                var notasDocente = await (from n in _context.Notas
                                          join d in _context.Docentes on n.IdDocente equals d.IdDocente
                                          join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                                          join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                                          // Eliminar la restricción que oculta notas en el periodo 1
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
                // Obtener el ID del estudiante desde el claim "IdEstudiante"
                var idEstudiante = User.Claims.FirstOrDefault(c => c.Type == "IdEstudiante")?.Value;

                if (!string.IsNullOrEmpty(idEstudiante))
                {
                    // Obtener las notas únicamente del estudiante logueado
                    var notasEstudiante = await (from n in _context.Notas
                                                 join d in _context.Docentes on n.IdDocente equals d.IdDocente
                                                 join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                                                 join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                                                 where e.IdEstudiante.ToString() == idEstudiante // Filtrar por el ID del estudiante
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

                // Si no se encuentra el claim, redirigir o manejar el error
                return RedirectToAction("Login", "Cuenta");
            }


            if (rol == "Administrador")
            {
                // Obtener todas las notas (sin filtrar por periodo)
                var todasLasNotas = await (from n in _context.Notas
                                           join d in _context.Docentes on n.IdDocente equals d.IdDocente
                                           join e in _context.Estudiantes on n.NombreEstudiante equals e.IdEstudiante.ToString()
                                           join m in _context.Materias on n.NombreAsignatura equals m.Id.ToString()
                                           // Eliminar la restricción que oculta notas en el periodo 1
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

                    // Obtener los ID de la materia y el estudiante relacionados con la nota
                    int idMateria = int.Parse(nota.NombreAsignatura); // Asumir que "NombreAsignatura" es el ID de la materia
                    int idEstudiante = int.Parse(nota.NombreEstudiante); // Asumir que "NombreEstudiante" es el ID del estudiante

                    // Llamar a VerificarPromedio para recalcular el promedio del estudiante en esa materia
                    await VerificarPromedio(idMateria, idEstudiante);

                    // Redirigir a la vista de índice después de la actualización
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

            // Obtener los ID de la materia y el estudiante relacionados con la nota
            int idMateria = int.Parse(nota.NombreAsignatura); // Asumir que "NombreAsignatura" es el ID de la materia
            int idEstudiante = int.Parse(nota.NombreEstudiante); // Asumir que "NombreEstudiante" es el ID del estudiante

            // Eliminar la nota del contexto
            _context.Notas.Remove(nota);
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos

            // Llamar al método VerificarPromedio para recalcular el promedio y actualizar el estado de la materia
            await VerificarPromedio(idMateria, idEstudiante);

            TempData["SuccessMessage"] = "La nota fue eliminada exitosamente y el promedio fue recalculado."; // Mensaje de éxito
            return RedirectToAction(nameof(Index)); // Redirigir al listado después de la eliminación
        }



        private bool NotaExists(int id)
        {
            return _context.Notas.Any(e => e.Id == id);
        }

        public async Task<IActionResult> VerificarPromedio(int idMateria, int idEstudiante)
        {
            // Obtener las notas de la materia
            var notas = await _context.Notas
                .Where(n => n.NombreAsignatura == idMateria.ToString() && n.NombreEstudiante == idEstudiante.ToString())
                .ToListAsync();

            // Calcular el promedio sumando las notas presentes y dividiendo entre 5
            var sumaNotas = notas.Sum(n => n.Calificacion);
            var promedio = Math.Round(sumaNotas / 5, 1); // Siempre dividir entre 5 para considerar las notas faltantes como 0

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
            }
            else
            {
                // Si ya existe la materia, actualizar el estado, promedio y fecha
                materiaExistente.Promedio = promedio;
                materiaExistente.Estado = estado;  // Actualizar el estado
                materiaExistente.FechaAprobacion = fechaAprobacion;  // Actualizar la fecha de aprobación

                _context.MateriasAprobadas.Update(materiaExistente);
                await _context.SaveChangesAsync();
            }

            // Obtener el periodo actual desde la base de datos
            var periodoActual = await _context.PeriodoGlobal.FirstOrDefaultAsync();

            // Depurar el valor del periodo actual
            TempData["PeriodoActual"] = periodoActual?.Periodo.ToString() ?? "Periodo no encontrado";

            // Verificar si el periodo es 1 y si el estudiante tiene 5 notas registradas
            if (periodoActual?.Periodo == 1 && notas.Count == 5)
            {
                TempData["NotasContadas"] = notas.Count;

                // Buscar la inscripción correspondiente
                var inscripcion = await _context.Inscripciones
                    .FirstOrDefaultAsync(i => i.IdEstudiante == idEstudiante && i.IdMateria == idMateria);

                if (inscripcion != null)
                {
                    // Eliminar la inscripción
                    _context.Inscripciones.Remove(inscripcion);
                    await _context.SaveChangesAsync();

                    TempData["Info"] = "Materia retirada automáticamente al completar las 5 notas.";
                }
                else
                {
                    TempData["Info"] = "No se encontró la inscripción para esta materia.";
                }
            }
            else
            {
                TempData["Info"] = "No se cumplen las condiciones para retirar la materia (Periodo no es 1 o no hay 5 notas).";
            }

            TempData["Success"] = (estado == "Aprobado")
                ? "Materia procesada exitosamente."
                : "Materia reprobada, pero procesada correctamente.";

            return RedirectToAction(nameof(Index));
        }




        // GET: NotaController/NotasPorEstudiante
        public async Task<IActionResult> NotasPorEstudiante()
        {
            var userRol = User.FindFirst("Rol")?.Value;
            if (userRol != "Estudiante")
            {
                return Unauthorized("Estimado Usuario, usted no es un Estudiante.");
            }

            // Obtener el IdEstudiante del Claim
            var idEstudianteClaim = User.FindFirst("IdEstudiante")?.Value;

            if (string.IsNullOrEmpty(idEstudianteClaim))
            {
                return Unauthorized("No se encontró el Id del estudiante.");
            }

            if (!int.TryParse(idEstudianteClaim, out int idEstudiante))
            {
                return Unauthorized("El Id del estudiante no es válido.");
            }

            // Consultar las materias aprobadas con el nombre de la materia
            var materiasAprobadasEstudiante = await (from ma in _context.MateriasAprobadas
                                                     join m in _context.Materias on ma.IdMateria equals m.Id
                                                     where ma.IdEstudiante == idEstudiante
                                                     select new
                                                     {
                                                         ma.Id,
                                                         ma.IdMateria,
                                                         NombreMateria = m.Nombre,
                                                         ma.Promedio,
                                                         ma.Estado,
                                                         ma.FechaAprobacion
                                                     })
                                                     .ToListAsync();

            if (!materiasAprobadasEstudiante.Any())
            {
                return NotFound("No se encontraron materias aprobadas para este estudiante.");
            }

            // Pasar los datos como ViewData para evitar conflictos con modelos
            ViewData["MateriasAprobadas"] = materiasAprobadasEstudiante;

            return View();
        }
    }
}
