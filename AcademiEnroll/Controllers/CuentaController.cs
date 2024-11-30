using Microsoft.AspNetCore.Mvc;
using AcademiEnroll.Models;
using AcademiEnroll.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class CuentaController : Controller
{
    private readonly AcademiEnrollContext _context;

    public CuentaController(AcademiEnrollContext context)
    {
        _context = context;
    }

    // Creación de la vista principal de Login
    public IActionResult Login() => View();

    // Creación de la lógica para procesar el Login
    [HttpPost]
    public IActionResult Login(string correo, string clave)
    {
        // Buscar el usuario por correo y contraseña
        var usuario = _context.Usuarios.SingleOrDefault(u => u.Correo == correo && u.Clave == clave);

        if (usuario != null)
        {
            // Crear las claims del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim("Rol", usuario.Rol)
            };

            // Si el usuario es Docente, agregar IdDocente al claim
            if (usuario.Rol == "Docente")
            {
                var docente = _context.Docentes.SingleOrDefault(d => d.Correo == correo);
                if (docente != null)
                {
                    claims.Add(new Claim("IdDocente", docente.IdDocente.ToString()));
                }
            }

            // Si el usuario es Estudiante, agregar IdEstudiante al claim
            if (usuario.Rol == "Estudiante")
            {
                var estudiante = _context.Estudiantes.SingleOrDefault(d => d.Correo == correo);
                if (estudiante != null)
                {
                    claims.Add(new Claim("IdEstudiante", estudiante.IdEstudiante.ToString()));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties();

            // Autenticar al usuario y crear la cookie de autenticación
            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // Redirigir según el rol del usuario
            if (usuario.Rol == "Administrador") return RedirectToAction("VistaAdmin");
            if (usuario.Rol == "Docente") return RedirectToAction("VistaDocente");
            return RedirectToAction("VistaEstudiante");
        }

        // Enviar un mensaje de error si las credenciales son incorrectas
        ViewData["LoginError"] = "Credenciales incorrectas. Por favor, intente de nuevo.";
        return View();
    }

    // Creación de la vista de Registro
    public IActionResult Registro(string tipo)
    {
        var userRol = User.FindFirst("Rol")?.Value;
        if (userRol != "Administrador")
        {
            return RedirectToAction("Login");
        }

        var model = new Usuario();
        if (!string.IsNullOrEmpty(tipo))
        {
            model.Rol = tipo;
        }

        return View(model);
    }

    // Creación de la lógica que procesa el registro de un nuevo usuario
    [HttpPost]
    public IActionResult Registro(Usuario usuario, string confirmarClave)
    {
        if (ModelState.IsValid && usuario.Clave == confirmarClave)
        {
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            if (usuario.Rol == "Estudiante")
            {
                var estudiante = new Estudiante
                {
                    Nombre = usuario.Nombre,
                    Correo = usuario.Correo,
                    IdUsuario = usuario.IdUsuario
                };
                _context.Estudiantes.Add(estudiante);
            }
            else if (usuario.Rol == "Docente")
            {
                var docente = new Docente
                {
                    Nombre = usuario.Nombre,
                    Correo = usuario.Correo,
                    IdUsuario = usuario.IdUsuario
                };
                _context.Docentes.Add(docente);
            }
            else if (usuario.Rol == "Administrador")
            {
                var administrador = new AcademiEnroll.Models.Administrador
                {
                    Nombre = usuario.Nombre,
                    Correo = usuario.Correo,
                    IdUsuario = usuario.IdUsuario
                };
                _context.Administrador.Add(administrador);
            }

            _context.SaveChanges();

            ViewBag.Nombre = usuario.Nombre;
            ViewBag.Rol = usuario.Rol;

            return View("ConfirmacionRegistro");
        }

        ModelState.AddModelError("", "Error al registrar el usuario.");
        return View();
    }

    // Creación de la vista de confirmación tras el registro
    public IActionResult ConfirmacionRegistro(string correo)
    {
        ViewBag.Correo = correo;
        return View();
    }

    // Creación de la vista de "Olvidé mi contraseña"
    public IActionResult OlvidoContraseña() => View();

    // Creación de la lógica para procesar la solicitud de restablecimiento de contraseña
    [HttpPost]
    public IActionResult OlvidoContraseña(string correo)
    {
        var usuario = _context.Usuarios.SingleOrDefault(u => u.Correo == correo);
        if (usuario != null)
        {
            usuario.Clave = "nueva_contraseña123";
            _context.SaveChanges();

            ViewBag.Mensaje = "Se ha restablecido su contraseña. Revise su correo o use 'nueva_contraseña123' como clave temporal.";
        }
        else
        {
            ViewBag.Mensaje = "No se encontró una cuenta con ese correo.";
        }

        return View();
    }

    // Creación de la vista de Perfil
    [Authorize]
    public IActionResult Perfil()
    {
        var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(correoUsuario))
        {
            return RedirectToAction("Login");
        }

        var usuario = _context.Usuarios.SingleOrDefault(u => u.Correo == correoUsuario);
        if (usuario == null)
        {
            return RedirectToAction("Login");
        }

        return View(usuario);
    }

    // Creación de la vista para el Docente
    public IActionResult VistaDocente() => View("VistaDocente");

    // Creación de la vista para el Estudiante
    public IActionResult VistaEstudiante() => View("VistaEstudiante");

    // Creación de la vista para el Administrador (Dashboard)
    public async Task<IActionResult> VistaAdmin()
    {
        var dashboardData = new DashBoard();

        using (var connection = (SqlConnection)_context.Database.GetDbConnection())
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand("sp_ReporteDashboard", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        dashboardData.TotalDocentes = reader.GetInt32(0);
                        dashboardData.TotalEstudiantes = reader.GetInt32(1);
                        dashboardData.TotalAprobados = reader.GetInt32(2);
                        dashboardData.TotalReprobados = reader.GetInt32(3);
                        dashboardData.TotalMaterias = reader.GetInt32(4);
                        dashboardData.TotalUsuarios = reader.GetInt32(5);
                    }
                }
            }
        }

        return View("VistaAdmin", dashboardData);
    }
}
