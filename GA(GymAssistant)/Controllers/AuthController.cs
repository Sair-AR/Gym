using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Data;
using GA_GymAssistant.Models;

namespace GA_GymAssistant.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ==========================================
        // 2. PROCESAR EL LOGIN (API POST: /api/Auth/validar)
        // ==========================================
        [HttpPost("api/Auth/validar")]
        public async Task<IActionResult> Validar([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Faltan datos.");
            }

            // 1. Buscar usuario en la BD
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null) return Unauthorized("Usuario no encontrado.");

            // =======================================================
            // 2. NUEVA VALIDACIÓN: VERIFICAR ESTADO
            // =======================================================
            // Si Estado es false (0), denegar acceso
            if (!user.Estado)
            {
                return Unauthorized("Tu cuenta está inactiva. Contacta al soporte.");
            }
            // =======================================================

            // 3. Verificar contraseña (en producción usar hash)
            if (user.PasswordHash != request.Password)
                return Unauthorized("Contraseña incorrecta.");

            // 4. Login exitoso
            return Ok(new
            {
                message = "Bienvenido",
                userId = user.IdUsuario,
                nombre = user.Nombre,
                rol = user.TipoUsuario // Útil si necesitas saber si es Admin en el front
            });
        }
    }

    // Clase auxiliar para recibir el JSON (si no la tienes en otro lado)
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}