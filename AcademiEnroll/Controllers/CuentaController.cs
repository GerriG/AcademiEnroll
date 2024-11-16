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

public class CuentaController : Controller
{
    private readonly AcademiEnrollContext _context;

    
    public CuentaController(AcademiEnrollContext context)
    {
        _context = context;
    }

    // Creacion de la vista principal de Login
    public IActionResult Login() => View();    

    // Creacion de la logica para procesar el Login
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
            new Claim("Rol", usuario.Rol)  // Guardar el rol en una claim personalizada
        };

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

    // Creacion de  la vista de Registro    
    public IActionResult Registro(string tipo)
    {
        // Validación para asegurar que solo un administrador pueda acceder
        var userRol = User.FindFirst("Rol")?.Value;
        if (userRol != "Administrador")
        {
            return RedirectToAction("Login");
        }

        // Crear un nuevo objeto Usuario
        var model = new Usuario();

        // Si se recibe un parámetro "tipo" no vacío, asignarlo al modelo
        if (!string.IsNullOrEmpty(tipo))
        {
            model.Rol = tipo;  // Asignar "Docente" si el parámetro es "Docente"
        }

        return View(model);  // Pasar el modelo con el valor predeterminado
    }




    // Creacion de la logica que Procesa el registro de un nuevo usuario
    [HttpPost]
    public IActionResult Registro(Usuario usuario, string confirmarClave)
    {
        if (ModelState.IsValid && usuario.Clave == confirmarClave)
        {
            // Guardar el usuario en la base de datos
            _context.Usuarios.Add(usuario);
            _context.SaveChanges(); // Guarda el usuario primero

            // Dependiendo del rol, insertar en la tabla correspondiente
            if (usuario.Rol == "Estudiante")
            {
                var estudiante = new Estudiante
                {
                    Nombre = usuario.Nombre,                    
                    Correo = usuario.Correo,
                    IdUsuario = usuario.IdUsuario // Asigna el IdUsuario del usuario recién insertado
                };
                _context.Estudiantes.Add(estudiante);
            }
            else if (usuario.Rol == "Docente")
            {
                var docente = new Docente
                {
                    Nombre = usuario.Nombre,                    
                    Correo = usuario.Correo,
                    IdUsuario = usuario.IdUsuario // Asigna el IdUsuario del usuario recién insertado
                };
                _context.Docentes.Add(docente);
            }
            else if (usuario.Rol == "Administrador")
            {
                var administrador = new AcademiEnroll.Models.Administrador
                {
                    Nombre = usuario.Nombre,                    
                    Correo = usuario.Correo,
                    IdUsuario = usuario.IdUsuario // Asigna el IdUsuario del usuario recién insertado
                };
                _context.Administrador.Add(administrador);
            }

            // Guardar los cambios adicionales para las tablas de roles
            _context.SaveChanges();

            ViewBag.Nombre = usuario.Nombre;
            ViewBag.Rol = usuario.Rol;

            return View("ConfirmacionRegistro");  // Redirige a la vista de confirmación
        }

        ModelState.AddModelError("", "Error al registrar el usuario.");
        return View();
    }




    // Creacion de la vista de confirmación tras el registro
    public IActionResult ConfirmacionRegistro(string correo)
    {
        ViewBag.Correo = correo;
        return View();
    }

    // Creacion de la vista de "Olvidé mi contraseña"
    public IActionResult OlvidoContraseña() => View();

    // Creacion de la logica para procesar la solicitud de restablecimiento de contraseña
    [HttpPost]
    public IActionResult OlvidoContraseña(string correo)
    {
        var usuario = _context.Usuarios.SingleOrDefault(u => u.Correo == correo);
        if (usuario != null)
        {
            // Aquí puedes generar una contraseña temporal o un enlace de restablecimiento
            // Por ahora, vamos a simular el restablecimiento estableciendo una contraseña predeterminada.
            usuario.Clave = "nueva_contraseña123"; // Puedes usar lógica más segura en producción
            _context.SaveChanges();

            ViewBag.Mensaje = "Se ha restablecido su contraseña. Revise su correo o use 'nueva_contraseña123' como clave temporal.";
        }
        else
        {
            ViewBag.Mensaje = "No se encontró una cuenta con ese correo.";
        }

        return View();
    }


    // Creación de la vista para el Docente
    public IActionResult VistaDocente() => View("VistaDocente");

    // Creación de la vista para el Estudiante
    public IActionResult VistaEstudiante() => View("VistaEstudiante");

    // Creación de la vista para el Administrador
    public IActionResult VistaAdmin() => View("VistaAdmin");

    
}
