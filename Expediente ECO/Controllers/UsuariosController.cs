using ExpedienteECO.Models;
using ExpedienteECO.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpedienteECO.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ApplicationDbContext dbContext;
        private readonly IServicioEmail _servicioEmail;

        public UsuariosController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext dbContext,
            IServicioEmail servicioEmail)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbContext = dbContext;
            this._servicioEmail = servicioEmail;
        }

        [AllowAnonymous]
        public IActionResult Registro() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            var usuario = new IdentityUser() { Email = modelo.Email, UserName = modelo.Email };
            var resultado = await userManager.CreateAsync(usuario, password: modelo.Password);

            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(modelo);
        }

        [AllowAnonymous]
        public IActionResult Login(string mensaje = null)
        {
            if (mensaje is not null) ViewData["mensaje"] = mensaje;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            // El tercer parámetro (modelo.Recuerdame) es el que hace la magia
            var resultado = await signInManager.PasswordSignInAsync(
                modelo.Email,
                modelo.Password,
                isPersistent: modelo.Recuerdame, // <--- AQUÍ
                lockoutOnFailure: false);

            if (resultado.Succeeded) return RedirectToAction("Index", "Home");

            ModelState.AddModelError(string.Empty, "Nombre de usuario o contraseña incorrecto");
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        // --- SECCIÓN ADMINISTRATIVA ---

        [HttpGet]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Listado(string mensaje = null)
        {
            var usuarios = await dbContext.Users.Select(u => new UsuarioViewModel
            {
                Email = u.Email,
            }).ToListAsync();

            var modelo = new UsuariosListadoViewModel { Usuarios = usuarios, Mensaje = mensaje };
            return View(modelo);
        }

        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> HacerAdmin(string email)
        {
            var usuario = await dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
            if (usuario is null) return NotFound();

            await userManager.AddToRoleAsync(usuario, Constantes.RolAdmin);

            return RedirectToAction("Listado",
                new { mensaje = "Rol asignado correctamente a " + email });
        }

        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> RemoverAdmin(string email)
        {
            var usuario = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return NotFound();

            if (usuario.Email == User.Identity.Name)
            {
                TempData["Error"] = "No puedes quitarte el rol de admin a ti mismo.";
                return RedirectToAction("Listado");
            }

            await userManager.RemoveFromRoleAsync(usuario, Constantes.RolAdmin);

            return RedirectToAction("Listado",
                new { mensaje = "Rol removido correctamente a " + email });
        }

        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> EliminarUsuario(string email)
        {
            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario == null) return NotFound();

            if (usuario.Email == User.Identity.Name)
            {
                return RedirectToAction("Listado", new { mensaje = "Error: No puedes eliminar tu propia cuenta." });
            }

            var resultado = await userManager.DeleteAsync(usuario);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Listado", new { mensaje = "Usuario " + email + " eliminado correctamente." });
            }

            return RedirectToAction("Listado", new { mensaje = "Error al intentar eliminar al usuario." });
        }

        // --- SECCIÓN RECUPERACIÓN DE CONTRASEÑA ---

        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvidePassword() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvidePassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return View();

            var usuario = await userManager.FindByEmailAsync(email);

            if (usuario != null)
            {
                var codigo = await userManager.GeneratePasswordResetTokenAsync(usuario);

                var enlace = Url.Action("RecuperarPassword", "Usuarios",
                    new { email = email, codigo = codigo }, Request.Scheme);

                await _servicioEmail.EnviarEmailCambioPassword(email, enlace);
            }

            return RedirectToAction("ConfirmacionEnvioEmail");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecuperarPassword(string email, string codigo)
        {
            if (email == null || codigo == null) return RedirectToAction("Login");

            // Pasamos el email y el código al ViewModel para que la vista los oculte en inputs hidden
            var modelo = new RecuperarPasswordViewModel
            {
                Email = email,
                CodigoReseteo = codigo
            };
            return View(modelo);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            var usuario = await userManager.FindByEmailAsync(modelo.Email);

            if (usuario is null)
            {
                return RedirectToAction("PasswordCambiado");
            }

            // Aquí Identity usa el CodigoReseteo oculto para validar la operación
            var resultado = await userManager.ResetPasswordAsync(usuario, modelo.CodigoReseteo, modelo.Password);

            if (resultado.Succeeded)
            {
                return RedirectToAction("PasswordCambiado");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(modelo);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordCambiado() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmacionEnvioEmail() => View();
    }


}