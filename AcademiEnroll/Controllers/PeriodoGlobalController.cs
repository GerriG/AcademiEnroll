using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AcademiEnroll.Controllers
{
    public class PeriodoGlobalController : Controller
    {
        private readonly AcademiEnrollContext _context;

        public PeriodoGlobalController(AcademiEnrollContext context)
        {
            _context = context;
        }

        // GET: PeriodoGlobal/Edit
        public async Task<IActionResult> Edit()
        {
            // Intentar obtener el primer periodo de la base de datos con Id = 1
            var periodo = await _context.PeriodoGlobal.FindAsync(1);

            // Si no existe un periodo con Id = 1, crear uno nuevo con valor predeterminado
            if (periodo == null)
            {
                periodo = new PeriodoGlobal
                {
                    Id = 1,    // Asignar el Id fijo de 1
                    Periodo = 1 // Asignar el valor predeterminado de 1
                };

                // Agregar el nuevo periodo a la base de datos
                _context.PeriodoGlobal.Add(periodo);
                await _context.SaveChangesAsync();
            }

            // Crear una lista de valores para el campo Periodo (de 1 a 5)
            var periodos = Enumerable.Range(1, 5).Select(p => new { Value = p, Text = $"Periodo {p}" }).ToList();
            ViewData["Periodos"] = new SelectList(periodos, "Value", "Text", periodo.Periodo);

            // Retornar la vista con el periodo (ya sea existente o recién creado)
            return View(periodo);
        }

        // POST: PeriodoGlobal/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int periodo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Buscar el periodo en la base de datos
                    var periodoGlobal = await _context.PeriodoGlobal.FindAsync(1);
                    if (periodoGlobal != null)
                    {
                        // Actualizar el valor del periodo
                        periodoGlobal.Periodo = periodo;
                        _context.Update(periodoGlobal);
                        await _context.SaveChangesAsync();
                    }

                    // Redirigir a Index con mensaje de éxito
                    TempData["Message"] = "Periodo asignado correctamente";
                    TempData["MessageType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Redirigir a Index con mensaje de error
                    TempData["Message"] = "Ocurrió un problema al asignar el periodo";
                    TempData["MessageType"] = "error";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(periodo);
        }

        // GET: PeriodoGlobal/Index
        public async Task<IActionResult> Index()
        {
            var periodoGlobal = await _context.PeriodoGlobal.FindAsync(1);
            return View(periodoGlobal);
        }

        private bool PeriodoGlobalExists(int id)
        {
            return _context.PeriodoGlobal.Any(e => e.Id == id);
        }
    }
}