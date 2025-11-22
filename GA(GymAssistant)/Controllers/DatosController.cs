using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Data;
using GA_GymAssistant_.Models;
using GA_GymAssistant.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;
using GA_GymAssistant.Models;

// Este controlador maneja las interacciones del cliente con los datos de la BD y la IA.

namespace GA_GymAssistant.Controllers
{
    // Atributos esenciales para un API Controller
    [ApiController]
    [Route("api/[controller]")] // Define la ruta base como /api/Datos
    public class DatosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAGymService _iaService;
        // NOTA: En una app real, este ID vendría del token de autenticación del usuario.
        // Usamos '1' (Juan Pérez) como ID de prueba por ahora.
        private readonly int UsuarioActivoId = 1;

        // Constructor para inyectar el Contexto de la BD y el Servicio de IA (Gemini)
        public DatosController(ApplicationDbContext context, IAGymService iaService)
        {
            _context = context;
            _iaService = iaService;
        }

        // =====================================================================
        // MÉTODOS DE PERFIL Y PROGRESO (Ejemplos básicos de lectura de BD)
        // =====================================================================

        /// <summary>
        /// Endpoint: GET /api/Datos/perfil
        /// Obtiene los datos del perfil del usuario activo.
        /// </summary>
        [HttpGet("perfil")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _context.Usuarios.FindAsync(UsuarioActivoId);
            if (user == null) return NotFound("Usuario no encontrado.");
            return Ok(user);
        }

        /// <summary>
        /// Endpoint: GET /api/Datos/progreso
        /// Obtiene el historial de progreso del usuario activo.
        /// </summary>
        [HttpGet("progreso")]
        public async Task<IActionResult> GetProgressHistory()
        {
            var history = await _context.HistorialProgreso
                                        .Where(h => h.IdUsuario == UsuarioActivoId)
                                        .OrderByDescending(h => h.Fecha)
                                        .ToListAsync();
            return Ok(history);
        }

        // =====================================================================
        // MÉTODOS DE RUTINAS (Lectura)
        // =====================================================================

        /// <summary>
        /// Endpoint: GET /api/Datos/rutina
        /// Obtiene la última rutina generada para el usuario.
        /// </summary>
        [HttpGet("rutina")]
        public async Task<IActionResult> GetLatestRoutine()
        {
            // Busca la rutina más reciente, incluyendo los detalles y los datos del ejercicio relacionado.
            var latestRoutine = await _context.Rutinas
                .Where(r => r.IdUsuario == UsuarioActivoId)
                .OrderByDescending(r => r.Fecha)
                .Include(r => r.RutinaDetalle)
                    .ThenInclude(rd => rd.Ejercicio)
                .FirstOrDefaultAsync();

            if (latestRoutine == null)
            {
                return NotFound("No se encontró ninguna rutina para este usuario.");
            }

            // Mapeo a un formato simple y limpio para el frontend
            var routineData = new
            {
                IdRutina = latestRoutine.IdRutina,
                Fecha = latestRoutine.Fecha.ToString("dd/MM/yyyy"),
                NotasIA = latestRoutine.Notas,
                Ejercicios = latestRoutine.RutinaDetalle.Select(rd => new
                {
                    IdEjercicio = rd.IdEjercicio,
                    NombreEjercicio = rd.Ejercicio?.Nombre,
                    Series = rd.Series,
                    Repeticiones = rd.Repeticiones,
                    ParametrosIA = rd.ParametrosIA
                }).ToList()
            };

            return Ok(routineData);
        }

        // =====================================================================
        // MÉTODOS DE RUTINAS (Generación con IA - Gemini)
        // =====================================================================

        /// <summary>
        /// Endpoint: POST /api/Datos/rutina/generar
        /// Llama al servicio de IA (Gemini) para generar una nueva rutina y guardarla en la BD.
        /// </summary>
        [HttpPost("rutina/generar")]
        public async Task<IActionResult> GenerateRoutine()
        {
            // 1. Obtener datos del usuario y ejercicios disponibles (INPUT para Gemini)
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == UsuarioActivoId);
            if (user == null) return NotFound("Usuario no encontrado.");

            var availableExercises = await _context.Ejercicios.ToListAsync();

            try
            {
                // 2. Llamar al servicio de IA para obtener el JSON de la rutina
                string iaResponseJson = await _iaService.GenerateRoutineFromAI(user, availableExercises);

                // 3. Deserializar el JSON de la IA a nuestro objeto C# (IARoutineResponse)
                var iaResult = System.Text.Json.JsonSerializer.Deserialize<IARoutineResponse>(iaResponseJson);

                if (iaResult == null || iaResult.ejercicios_detalle == null)
                    return BadRequest("La IA no devolvió un formato JSON válido o los detalles están vacíos.");

                // 4. Crear el registro Rutina (Encabezado)
                var newRutina = new Rutina
                {
                    IdUsuario = UsuarioActivoId,
                    Fecha = DateTime.Now,
                    RutinaBase = iaResult.rutina_base,
                    RutinaIA = iaResult.rutina_ia,
                    Notas = iaResult.notas
                };
                _context.Rutinas.Add(newRutina);
                await _context.SaveChangesAsync();

                // 5. Crear los registros RutinaDetalle (Contenido)
                var detalles = iaResult.ejercicios_detalle.Select(d => new RutinaDetalle
                {
                    IdRutina = newRutina.IdRutina,
                    IdEjercicio = d.id_ejercicio,
                    Series = d.series,
                    Repeticiones = d.repeticiones,
                    ParametrosIA = d.parametros_ia
                }).ToList();

                _context.RutinaDetalle.AddRange(detalles);
                await _context.SaveChangesAsync();

                // 6. Retornar éxito
                return Ok(new
                {
                    message = "Rutina IA generada y guardada exitosamente.",
                    idRutina = newRutina.IdRutina
                });
            }
            catch (InvalidOperationException ex)
            {
                // Error de clave API no configurada (manejo desde el IAGymService)
                return StatusCode(500, $"Error de configuración: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                // Error de conexión con Gemini
                return StatusCode(503, $"Error de conexión con Gemini (HTTP): {ex.Message}");
            }
            catch (Exception ex)
            {
                // Cualquier otro error, como un JSON mal formado
                return StatusCode(500, $"Error interno al generar la rutina: {ex.Message}");
            }
        }
    }
}