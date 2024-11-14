using AcademiEnroll.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Administrador.Controllers
{
    public class EstudianteController : Controller
    {
        private readonly MantenimientoEstudiante _mantenimientoEstudiante;

        public EstudianteController(MantenimientoEstudiante mantenimientoEstudiante)
        {
            _mantenimientoEstudiante = mantenimientoEstudiante;
        }

        // GET: EstudianteController
        public async Task<ActionResult> Index()
        {
            var estudiantes = await _mantenimientoEstudiante.ListarTodos();
            return View(estudiantes);
        }

        // GET: EstudianteController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var estudiante = await _mantenimientoEstudiante.Consultar(id);
            return View(estudiante);
        }

        // GET: EstudianteController/Create
        public ActionResult Create()
        {
            return RedirectToAction("Registro", "Cuenta");
        }

        // POST: EstudianteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                var estudiante = new Estudiante
                {
                    Nombre = collection["Nombre"],                    
                    Correo = collection["Correo"],
                    IdUsuario = int.Parse(collection["IdUsuario"])
                };

                await _mantenimientoEstudiante.Ingresar(estudiante);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EstudianteController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var estudiante = await _mantenimientoEstudiante.Consultar(id);
            return View(estudiante);
        }

        // POST: EstudianteController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                var estudiante = new Estudiante
                {
                    IdEstudiante = id,
                    Nombre = collection["Nombre"],                    
                    Correo = collection["Correo"],
                    IdUsuario = int.Parse(collection["IdUsuario"])
                };

                await _mantenimientoEstudiante.Modificar(estudiante);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EstudianteController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var estudiante = await _mantenimientoEstudiante.Consultar(id);
            return View(estudiante);
        }

        // POST: EstudianteController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                await _mantenimientoEstudiante.Borrar(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
