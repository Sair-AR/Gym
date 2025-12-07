using GA_GymAssistant.Data;
using GA_GymAssistant.Helpers; // Helper de seguridad
using GA_GymAssistant_.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GA_GymAssistant.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vista Login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            return View();
        }

        // POST: Procesar Login
        [AllowAnonymous]
        [HttpPost("api/Auth/validar")]
        public async Task<IActionResult> Validar([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Faltan datos.");
            }

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null) return Unauthorized("Usuario no encontrado.");
            if (user.Estado == false) return Unauthorized("Tu cuenta está inactiva.");

            // Verificar contraseña
            string hashPassword = Utilidades.EncriptarClave(request.Password);
            if (user.PasswordHash != hashPassword) return Unauthorized("Contraseña incorrecta.");

            // Crear Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Role, user.TipoUsuario)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // CAMBIO DE SEGURIDAD: IsPersistent = false
            // Esto hace que la sesión se cierre al cerrar el navegador.
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false, // <--- CAMBIADO
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30) // Opcional: Expira en 30 mins de inactividad
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Ok(new { message = "Login exitoso." });
        }

        // GET: Vista Registro
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Dashboard", "Home");
            return View();
        }

        // POST: Procesar Registro
        // POST: Procesar Registro
        [AllowAnonymous]
        [HttpPost("api/Auth/registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Todos los campos son obligatorios.");
            }

            bool existe = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
            if (existe) return BadRequest("El correo ya está registrado.");

            var nuevoUsuario = new Usuario
            {
                Nombre = request.Nombre,
                Email = request.Email,
                PasswordHash = Utilidades.EncriptarClave(request.Password),
                Estado = true,
                FechaRegistro = DateTime.Now,
                TipoUsuario = "Cliente",
                Nivel = "Principiante",
                Objetivo = "Por definir"
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // --- ¡IMPORTANTE! AQUÍ BORRÉ EL CÓDIGO DE "SignInAsync" ---
            // Al no haber SignInAsync, no se crea la cookie, y el usuario NO entra directo.

            return Ok(new { message = "Registro exitoso. Por favor inicia sesión." });
        }

        // POST: Cerrar Sesión
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        public class LoginRequest { public string Email { get; set; } public string Password { get; set; } }
        public class RegisterRequest { public string Nombre { get; set; } public string Email { get; set; } public string Password { get; set; } }
    }
}