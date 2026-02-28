using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using ExpedienteECO;
using QuestPDF.Infrastructure; // <--- AGREGAR ESTO

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE LICENCIA QUESTPDF (Versión 2025.7.1)
QuestPDF.Settings.License = LicenseType.Community;

// Política de usuarios autenticados
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

// Add services to the container.
builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
});

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=DefaultConnection"));

// Configurar Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
{
    opciones.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configurar cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Usuarios/Login";
    options.AccessDeniedPath = "/Home/Error";
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// CORRECCIÓN AQUÍ: Primero Autenticación, luego Autorización
app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 403)
    {
        response.Redirect("/Home/Error?mensaje=Acceso%20Denegado");
    }
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();