using Microsoft.EntityFrameworkCore;
using AcademiEnroll.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using AcademiEnroll.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AcademiEnrollContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AcademiEnrollConnection"));
});

// Registrar MantenimientoEstudiante
builder.Services.AddScoped<MantenimientoEstudiante>();

// Registrar MantenimientoUsuario
builder.Services.AddScoped<MantenimientoUsuario>();

// Configuración de autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login"; // Ruta de la vista de Login
        options.LogoutPath = "/Home/CerrarSesion"; // Ruta para cerrar sesión
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado"; // Ruta en caso de acceso denegado
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configuración de middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Activar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");

app.Run();
