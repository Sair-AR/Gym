using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Data;
using GA_GymAssistant_.Models;
using GA_GymAssistant.Services;

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

        public async Task<IActionResult> Index()
        {
            // ⚠️ SIMULACIÓN: Asumimos que el usuario logueado es el ID 1.
            // (Más adelante implementaremos Login real)
            int idUsuario = 1;

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

            int idUsuario = 1; // ID Simulado

            // 1. Buscamos al usuario para saber sus lesiones, peso, etc.
            var usuario = await _context.Usuarios.FindAsync(idUsuario);

            // 2. Preparamos el contexto para la IA
            string contexto = "Usuario Anónimo";
            if (usuario != null)
            {
                contexto = $"Nombre: {usuario.Nombre}, Objetivo: {usuario.Objetivo}, Lesiones: {usuario.Lesiones}, Nivel: {usuario.Nivel}";
            }

            // 3. Llamamos a Gemini
            string respuestaIA = await _iaService.ObtenerRespuesta(pregunta, contexto);

            // 4. Guardamos en la Base de Datos
            var consulta = new IAConsulta
            {
                IdUsuario = idUsuario,
                Pregunta = pregunta,
                Respuesta = respuestaIA,
                Fecha = DateTime.Now
            };

            _context.Add(consulta);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}