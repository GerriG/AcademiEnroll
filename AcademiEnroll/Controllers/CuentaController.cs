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

        if (ModelState.IsValid)
        {
            // Validar que las contraseñas coincidan
            if (usuario.Clave != confirmarClave)
            {
                TempData["ErrorMessage"] = "Las contrase&ntilde;as no coinciden.";
                return View(usuario); // Regresa a la vista con el mensaje de error
            }

            // Verificar si el correo ya está registrado
            var usuarioExistente = _context.Usuarios.SingleOrDefault(u => u.Correo == usuario.Correo);
            if (usuarioExistente != null)
            {
                TempData["ErrorMessage"] = "Este correo electrónico ya está registrado.";
                return View(usuario); // Si ya existe, muestra el error
            }

            // Registrar al usuario en la base de datos
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Asignar roles específicos según el tipo de usuario
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

            // Guardar el mensaje de éxito en TempData
            TempData["SuccessMessage"] = "Usuario agregado exitosamente.";

            // Recargar la vista de registro con el mensaje de éxito
            return View();
        }

        // Si el modelo no es válido, regresar a la vista con errores de validación
        return View(usuario);
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
    public async Task<IActionResult> VistaDocente()
    {
        // Obtener el IdDocente del usuario logeado desde los claims
        var idDocenteClaim = User.FindFirst("IdDocente")?.Value;
        if (string.IsNullOrEmpty(idDocenteClaim))
        {
            return Unauthorized("Estimado Usuario, usted no es un Docente.");
        }

        int idDocente = int.Parse(idDocenteClaim);
        List<ReporteDocente> reportesDocente = new List<ReporteDocente>();

        using (var connection = (SqlConnection)_context.Database.GetDbConnection())
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand("SPReporteDocente", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@IdDocente", idDocente));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var reporte = new ReporteDocente
                        {
                            NombreDocente = reader.GetString(0),
                            MateriasImpartidas = reader.GetInt32(1),
                            TotalEstudiantes = reader.GetInt32(2),
                            TotalAprobados = reader.GetInt32(3),
                            TotalReprobados = reader.GetInt32(4)
                        };
                        reportesDocente.Add(reporte);
                    }
                }
            }
        }

        return View(reportesDocente);
    }


    // Creación de la vista para el Estudiante
    public IActionResult VistaEstudiante()
    {
        // Obtener el correo del usuario desde los claims
        var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;

        // Verificar si el correo es nulo o vacío
        if (string.IsNullOrEmpty(correoUsuario))
        {
            // Redirigir al usuario a la página de login si no está autenticado
            return RedirectToAction("Login", "Cuenta");
        }

        // Obtener el rol del usuario desde los claims
        var userRol = User.FindFirst("Rol")?.Value;

        // Verificar si el rol es "Estudiante"
        if (userRol != "Estudiante")
        {
            // Si el rol no es "Estudiante", devolver un Unauthorized
            return Unauthorized("Estimado Usuario, usted no es un Estudiante.");
        }

        // Si las validaciones pasan, retornar la vista de Estudiante
        return View("VistaEstudiante");
    }


    // Creación de la vista para el Administrador (Dashboard)
    public async Task<IActionResult> VistaAdmin()
    {
        var dashboardData = new DashBoard();

        var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(correoUsuario))
        {
            return RedirectToAction("Login");
        }

        var userRol = User.FindFirst("Rol")?.Value;
        if (userRol != "Administrador")
        {
            return Unauthorized("Estimado Usuario, no tiene acceso al Dahsboard debido a que no es un administrador.");
        }

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
