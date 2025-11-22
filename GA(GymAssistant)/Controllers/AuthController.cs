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

        // ==========================================
        // 1. MOSTRAR LA VISTA (GET: /Auth/Login)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya hay sesión (lógica opcional), redirigir al dashboard
            return View(); // Busca Views/Auth/Login.cshtml
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

            // Buscar usuario en la BD
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null) return Unauthorized("Usuario no encontrado.");

            // Verificar contraseña (en producción usar hash)
            if (user.PasswordHash != request.Password)
                return Unauthorized("Contraseña incorrecta.");

            // Login exitoso
            return Ok(new
            {
                message = "Bienvenido",
                userId = user.IdUsuario,
                nombre = user.Nombre
            });
        }
    }
}