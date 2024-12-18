﻿using AcademiEnroll.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

            var estudiantes = await _mantenimientoEstudiante.ListarTodos();
            return View(estudiantes);
        }

        // GET: EstudianteController/Details/5
        public async Task<ActionResult> Details(int id)
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

            var estudiante = await _mantenimientoEstudiante.Consultar(id);
            return View(estudiante);
        }

        // GET: EstudianteController/Create
        public ActionResult Create()
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
                // Obtener el estudiante por su Id
                var estudiante = await _mantenimientoEstudiante.Consultar(id);

                if (estudiante == null)
                {
                    TempData["ErrorMessage"] = "El estudiante no fue encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener el IdUsuario asociado al estudiante
                var idUsuario = estudiante.IdUsuario;

                // Validar que el IdUsuario exista antes de intentar eliminarlo
                if (idUsuario <= 0)
                {
                    TempData["ErrorMessage"] = "El usuario asociado al estudiante no es válido.";
                    return RedirectToAction(nameof(Index));
                }

                // Eliminar el usuario en la tabla Usuarios (esto elimina en cascada el estudiante)
                await _mantenimientoEstudiante.BorrarUsuario(idUsuario);

                // Agregar mensaje de éxito
                TempData["SuccessMessage"] = "Estudiante eliminado correctamente.";

                // Redirigir a la vista Index
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // En caso de un error inesperado
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el estudiante: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
