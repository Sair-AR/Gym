using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Data;
using GA_GymAssistant_.Models;
using GA_GymAssistant.Services;
using System.Security.Claims;

namespace GA_GymAssistant.Controllers
{
    public class IAController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAGymService _iaService;

        public IAController(ApplicationDbContext context, IAGymService iaService)
        {
            _context = context;
            _iaService = iaService;
        }

        // GET: Muestra el chat y el historial
        public async Task<IActionResult> Index(int? idConversacion)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // 1. CARGAR HISTORIAL (Para la barra lateral izquierda)
            var historial = await _context.Conversaciones
                .Where(c => c.IdUsuario == userId)
                .OrderByDescending(c => c.FechaInicio)
                .ToListAsync();

            // Pasamos datos a la vista usando ViewBag
            ViewBag.Historial = historial;
            ViewBag.ChatActualId = idConversacion;

            // 2. CARGAR MENSAJES (Para la zona derecha)
            if (idConversacion.HasValue)
            {
                var mensajes = await _context.IAConsultas
                    .Where(m => m.IdConversacion == idConversacion && m.IdUsuario == userId)
                    .OrderBy(m => m.Fecha)
                    .ToListAsync();
                return View(mensajes);
            }

            // Si no hay ID, es una pantalla de "Nueva Conversación" vacía
            return View(new List<IAConsulta>());
        }

        // POST: Crear una nueva conversación vacía
        [HttpPost]
        public IActionResult NuevaConversacion()
        {
            return RedirectToAction("Index"); // Recarga la página sin ID
        }

        // POST: Enviar mensaje
        [HttpPost]
        public async Task<IActionResult> Consultar(string pregunta, int? idConversacion)
        {
            if (string.IsNullOrWhiteSpace(pregunta)) return RedirectToAction("Index", new { idConversacion });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Usuarios.FindAsync(userId);

            // Si es el primer mensaje, CREAMOS la conversación en BD
            if (!idConversacion.HasValue || idConversacion == 0)
            {
                var nuevaConv = new Conversacion
                {
                    IdUsuario = userId,
                    // Usamos las primeras palabras como título
                    Titulo = pregunta.Length > 20 ? pregunta.Substring(0, 20) + "..." : pregunta,
                    FechaInicio = DateTime.Now
                };
                _context.Conversaciones.Add(nuevaConv);
                await _context.SaveChangesAsync();
                idConversacion = nuevaConv.IdConversacion; // Guardamos el ID nuevo
            }

            // Llamar a la IA
            string contexto = $"Usuario: {user.Nombre}, Objetivo: {user.Objetivo}";
            string respuesta = await _iaService.ObtenerRespuesta(pregunta, contexto);

            // Guardar el mensaje vinculado a esa conversación
            var consulta = new IAConsulta
            {
                IdUsuario = userId,
                IdConversacion = idConversacion,
                Pregunta = pregunta,
                Respuesta = respuesta,
                Fecha = DateTime.Now
            };
            _context.IAConsultas.Add(consulta);
            await _context.SaveChangesAsync();

            // Recargar la misma conversación
            return RedirectToAction("Index", new { idConversacion });
        }

        // POST: Borrar una conversación específica
        [HttpPost]
        public async Task<IActionResult> BorrarConversacion(int idConversacion)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Auth");
            int userId = int.Parse(userIdClaim.Value);

            // 1. Buscar la conversación
            var conversacion = await _context.Conversaciones
                .Include(c => c.Mensajes) // ¡IMPORTANTE! Traer los mensajes también
                .FirstOrDefaultAsync(c => c.IdConversacion == idConversacion && c.IdUsuario == userId);

            if (conversacion != null)
            {
                // 2. PRIMERO: Borrar los mensajes de esa conversación (si tiene)
                if (conversacion.Mensajes != null && conversacion.Mensajes.Any())
                {
                    _context.IAConsultas.RemoveRange(conversacion.Mensajes);
                }

                // 3. SEGUNDO: Ahora sí, borrar la conversación (ya está vacía)
                _context.Conversaciones.Remove(conversacion);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}