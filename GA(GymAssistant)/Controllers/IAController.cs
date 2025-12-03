using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Data;
using GA_GymAssistant.Models; // Asegúrate de que IAConsulta y Usuario están aquí
using GA_GymAssistant.Services;
using Microsoft.AspNetCore.Authorization; // Necesario para [Authorize]
using System.Security.Claims;
using GA_GymAssistant_.Models; // Necesario para ClaimTypes

namespace GA_GymAssistant.Controllers
{
    // Aplicamos [Authorize] a nivel de controlador para proteger todas las acciones de la IA
    [Authorize]
    public class IAController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAGymService _iaService;

        public IAController(ApplicationDbContext context, IAGymService iaService)
        {
            _context = context;
            _iaService = iaService;
        }

        // Método auxiliar para obtener el ID del usuario logueado
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // Ya que el controlador está protegido con [Authorize], 
            // este claim debería existir y ser válido.
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int idUsuario))
            {
                return idUsuario;
            }
            // Si falla, retorna 0 o lanza una excepción (aunque [Authorize] debería prevenir esto)
            throw new UnauthorizedAccessException("ID de usuario no encontrado en la sesión.");
        }

        public async Task<IActionResult> Index()
        {
            // OBTENEMOS EL ID REAL del usuario logueado
            int idUsuario = GetUserId();

            var historial = await _context.IAConsultas
                .Where(c => c.IdUsuario == idUsuario)
                .OrderByDescending(c => c.Fecha) // Lo más nuevo arriba
                .Take(5)
                .ToListAsync();

            return View(historial);
        }

        [HttpPost]
        public async Task<IActionResult> Consultar(string pregunta)
        {
            if (string.IsNullOrWhiteSpace(pregunta)) return RedirectToAction("Index");

            // OBTENEMOS EL ID REAL del usuario logueado
            int idUsuario = GetUserId();

            // 1. Buscamos al usuario para obtener su perfil real
            var usuario = await _context.Usuarios.FindAsync(idUsuario);

            // 2. Preparamos el contexto para la IA con datos reales
            string contexto = "Usuario Anónimo"; // Fallback, pero ya no debería ser Anónimo
            if (usuario != null)
            {
                // Aquí estamos pasando los datos del usuario logueado a la IA
                contexto = $"Nombre: {usuario.Nombre}, Objetivo: {usuario.Objetivo}, Lesiones: {usuario.Lesiones}, Nivel: {usuario.Nivel}";
            }

            // 3. Llamamos a Gemini
            string respuestaIA = await _iaService.ObtenerRespuesta(pregunta, contexto);

            // 4. Guardamos en la Base de Datos con el ID de usuario real
            var consulta = new IAConsulta
            {
                IdUsuario = idUsuario,
                Pregunta = pregunta,
                Respuesta = respuestaIA,
                Fecha = DateTime.Now
            };

            _context.Add(consulta);
            await _context.SaveChangesAsync();

            // Puedes usar TempData o ViewBag para mostrar la respuesta inmediatamente
            TempData["RespuestaIA"] = respuestaIA;

            return RedirectToAction("Index");
        }
    }
}