using Microsoft.EntityFrameworkCore;
using AcademiEnroll.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de servicios
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AcademiEnrollContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AcademiEnrollConnection"));

});



// Configuraci�n de autenticaci�n por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login"; // Ruta de la vista de Login
        options.LogoutPath = "/Home/CerrarSesion"; // Ruta para cerrar sesi�n
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado"; // Ruta en caso de acceso denegado
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configuraci�n de middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Activar autenticaci�n y autorizaci�n
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");

app.Run();
