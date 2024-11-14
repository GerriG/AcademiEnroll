using AcademiEnroll.Data;
using AcademiEnroll.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

		// GET: DocenteController
		public async Task<ActionResult> Index()
		{
			var docentes = await _context.Docentes.ToListAsync();
			return View(docentes);
		}

		// GET: DocenteController/Details/5
		public async Task<ActionResult> Details(int id)
		{
			var docente = await _context.Docentes.FindAsync(id);
			if (docente == null)
			{
				return NotFound();
			}
			return View(docente);
		}

		// GET: DocenteController/Create
		public ActionResult Create()
		{
			return RedirectToAction("Registro", "Cuenta");
		}

		// POST: DocenteController/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind("Nombre,Apellido,Correo,IdUsuario")] Docente docente)
		{
			if (ModelState.IsValid)
			{
				_context.Add(docente);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(docente);
		}

		// GET: DocenteController/Edit/5
		public async Task<ActionResult> Edit(int id)
		{
			var docente = await _context.Docentes.FindAsync(id);
			if (docente == null)
			{
				return NotFound();
			}
			return View(docente);
		}

		// POST: DocenteController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(int id, [Bind("IdDocente,Nombre,Apellido,Correo,IdUsuario")] Docente docente)
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

		// GET: DocenteController/Delete/5
		public async Task<ActionResult> Delete(int id)
		{
			var docente = await _context.Docentes.FindAsync(id);
			if (docente == null)
			{
				return NotFound();
			}
			return View(docente);
		}

		// POST: DocenteController/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmed(int id)
		{
			var docente = await _context.Docentes.FindAsync(id);
			_context.Docentes.Remove(docente);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool DocenteExists(int id)
		{
			return _context.Docentes.Any(e => e.IdDocente == id);
		}
	}
}
