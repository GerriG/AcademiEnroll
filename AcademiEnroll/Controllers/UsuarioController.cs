using AcademiEnroll.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Administrador.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly MantenimientoUsuario _mantenimientoUsuario;

        public UsuarioController(MantenimientoUsuario mantenimientoUsuario)
        {
            _mantenimientoUsuario = mantenimientoUsuario;
        }

    // GET: UsuarioController
    public async Task<ActionResult> Index()
        {
            var usuarios = await _mantenimientoUsuario.ListarTodos();
            return View(usuarios);
        }

        // GET: UsuarioController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var usuario = await _mantenimientoUsuario.Consultar(id);
            return View(usuario);
        }

        // GET: UsuarioController/Create
        public ActionResult Create()
        {
            return RedirectToAction("Registro", "Cuenta");
        }

        // POST: UsuarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                var usuario = new Usuario
                {
                    Nombre = collection["Nombre"],
                    Correo = collection["Correo"],
                    Clave = collection["Clave"],
                    Rol = collection["Rol"]
                };

                await _mantenimientoUsuario.Ingresar(usuario);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuarioController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var usuario = await _mantenimientoUsuario.Consultar(id);
            return View(usuario);
        }

        // POST: UsuarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                var usuario = new Usuario
                {
                    IdUsuario = id,
                    Nombre = collection["Nombre"],
                    Correo = collection["Correo"],
                    Clave = collection["Clave"],
                    Rol = collection["Rol"]
                };

                await _mantenimientoUsuario.Modificar(usuario);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuarioController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var usuario = await _mantenimientoUsuario.Consultar(id);
            return View(usuario);
        }

        // POST: UsuarioController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                await _mantenimientoUsuario.Borrar(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
