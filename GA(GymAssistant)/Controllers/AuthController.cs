using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Data;
using GA_GymAssistant.Models;
using System.Security.Claims; // Necesario para Claims
using Microsoft.AspNetCore.Authentication; // Necesario para SignInAsync
using Microsoft.AspNetCore.Authentication.Cookies; // Necesario para CookieAuthenticationDefaults
using Microsoft.AspNetCore.Authorization; // Importante para [AllowAnonymous]

namespace GA_GymAssistant.Controllers
{
    // Si tienes [Authorize] en este nivel, recuerda que [AllowAnonymous] lo anula en los métodos.
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. LOGIN (GET) - PERMITE ACCESO SIN AUTENTICAR
        // =========================================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            // Opcional: Si el usuario ya está logueado, redirigir al Dashboard.
            if (User.Identity.IsAuthenticated)
            {
                // Asegúrate de que "Index" y "Home" sean correctos para tu página principal
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        // =========================================================
        // 2. PROCESAR LOGIN (POST) - PERMITE ACCESO SIN AUTENTICAR
        // =========================================================
        [AllowAnonymous]
        [HttpPost("api/Auth/validar")]
        public async Task<IActionResult> Validar([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Faltan datos.");
            }

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null) return Unauthorized("Usuario no encontrado.");

            if (user.Estado == false)
            {
                return Unauthorized("Tu cuenta está inactiva. Contacta al soporte.");
            }

            // Nota: En producción, usa siempre un hash de contraseña (como BCrypt o Identity).
            if (user.PasswordHash != request.Password)
                return Unauthorized("Contraseña incorrecta.");

            // CREAR SESIÓN BASADA EN CLAIMS (COOKIES)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Role, user.TipoUsuario)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Devolver respuesta exitosa al frontend (esto es consumido por JavaScript en el Login)
            return Ok(new
            {
                message = "Login exitoso. La sesión ha sido establecida.",
                userId = user.IdUsuario,
                nombre = user.Nombre,
                rol = user.TipoUsuario
            });
        }

        // =========================================================
        // 3. CERRAR SESIÓN (POST) - NO LLEVA AllowAnonymous
        // =========================================================
        // La acción se llama simplemente [HttpPost] Logout para usarla con un <form> MVC.
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // El servidor elimina la cookie de autenticación
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // El servidor redirige al usuario a la acción Login del controlador Auth.
            return RedirectToAction("Login", "Auth");
        }
    }

    // Clase auxiliar para recibir el JSON del Login
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}