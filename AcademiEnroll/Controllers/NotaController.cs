using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult> Index()
        {
            var notas = await _context.Notas.ToListAsync();
            return View(notas);
        }

        // GET: NotaController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var nota = await _context.Notas.FindAsync(id);
            if (nota == null)
            {
                return NotFound();
            }
            return View(nota);
        }

        // GET: NotaController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
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

        private bool NotaExists(int id)
        {
            return _context.Notas.Any(e => e.Id == id);
        }
    }
}
