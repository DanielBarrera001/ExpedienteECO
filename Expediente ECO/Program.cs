using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using ExpedienteECO;
using ExpedienteECO.Servicios; // <--- Asegúrate de tener este using para Constantes
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE LICENCIA QUESTPDF
QuestPDF.Settings.License = LicenseType.Community;

// Política de usuarios autenticados
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

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

//Cookies de session abierta
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Usuarios/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(15); // La cookie durará 15 días
    options.SlidingExpiration = true; // Si el usuario entra al día 14, se renueva otros 15 días
});

// Configurar cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Usuarios/Login";
    options.AccessDeniedPath = "/Home/Error";
});

builder.Services.AddTransient<IServicioEmail, ServicioEmail>();

var app = builder.Build();

// --- BLOQUE DE SEEDING: ROLES Y ADMIN POR DEFECTO ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Crear el rol de Admin si no existe
    if (!await roleManager.RoleExistsAsync(Constantes.RolAdmin))
    {
        await roleManager.CreateAsync(new IdentityRole(Constantes.RolAdmin));
    }

    // 2. Opcional: Crear un usuario Admin inicial si no hay ninguno
    var emailAdmin = "admin@expediente.com"; // Cambia esto por el correo que quieras
    var usuarioAdmin = await userManager.FindByEmailAsync(emailAdmin);

    if (usuarioAdmin == null)
    {
        var nuevoAdmin = new IdentityUser { UserName = emailAdmin, Email = emailAdmin };
        await userManager.CreateAsync(nuevoAdmin, "Admin123!"); // Contraseña temporal fuerte
        await userManager.AddToRoleAsync(nuevoAdmin, Constantes.RolAdmin);
    }
}
// ----------------------------------------------------

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

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