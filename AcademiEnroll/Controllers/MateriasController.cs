using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace AcademiEnroll.Controllers
{
    public class MateriasController : Controller
    {
        private readonly AcademiEnrollContext _context;
        public MateriasController(AcademiEnrollContext context)
        {
            _context = context;
        }
        // GET: Materias
        public async Task<IActionResult> Index()
        {
            var materias = await _context.Materias.Include(m => m.Docente).ToListAsync();
            return View(materias);
        }
        // GET: Materias/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var materia = await _context.Materias.Include(m => m.Docente).FirstOrDefaultAsync(m => m.Id == id);
            if (materia == null)
            {
                return NotFound();
            }
            return View(materia);
        }

        // GET: Materias/Create
        public IActionResult Create()
        {
            // Aquí pasamos la lista de docentes a la vista.
            ViewBag.Docentes = new SelectList(_context.Docentes, "IdDocente", "Nombre");
            return View();
        }


        // POST: Materias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Codigo,IdDocente")] Materia materia)
        {
            if (ModelState.IsValid)
            {
                // Usar Entity Framework para agregar la materia directamente
                _context.Materias.Add(materia);

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Si el modelo no es válido, volver a cargar la lista de docentes
            ViewData["Docentes"] = new SelectList(_context.Docentes, "IdDocente", "Nombre", materia.IdDocente);
            return View(materia);
        }

        // GET: Materias/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null)
            {
                return NotFound();
            }
            ViewData["Docentes"] = new SelectList(_context.Docentes, "IdDocente", "IdDocente", materia.IdDocente);
            return View(materia);
        }
        // POST: Materias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Codigo,IdDocente")] Materia materia)
        {
            if (id != materia.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(materia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Materias.Any(e => e.Id == id))
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
            ViewData["Docentes"] = new SelectList(_context.Docentes, "IdDocente", "IdDocente", materia.IdDocente);
            return View(materia);
        }
        // POST: Materias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia != null)
            {
                _context.Materias.Remove(materia);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}