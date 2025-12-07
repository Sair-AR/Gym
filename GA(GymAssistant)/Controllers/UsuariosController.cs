using GA_GymAssistant.Data;
using GA_GymAssistant.Helpers; 
using GA_GymAssistant_.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GA_GymAssistant.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================================================
        //  LISTADO Y DETALLES (ADMIN)
        // =============================================================
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Usuarios == null) return NotFound();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // =============================================================
        //  CREAR USUARIO (ADMIN)
        // =============================================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            // Validaciones básicas manuales si ModelState falla por campos no requeridos aquí
            if (ModelState.IsValid)
            {
                // 1. Configuración por defecto
                usuario.FechaRegistro = DateTime.Now;
                usuario.Estado = true;

                // 2. Contraseña por defecto o la que venga
                string passToHash = string.IsNullOrEmpty(usuario.PasswordHash) ? "user123" : usuario.PasswordHash;

                // 3. ENCRIPTAR (Mejora de seguridad)
                usuario.PasswordHash = Utilidades.EncriptarClave(passToHash);

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // =============================================================
        //  EDITAR USUARIO (SOLO ADMIN - Redirige a Index)
        // =============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario); // Usa la vista Edit.cshtml (la completa)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // MEJORA: Recuperar datos sensibles originales para no perderlos
                    var usuarioOriginal = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);
                    if (usuarioOriginal != null)
                    {
                        usuario.PasswordHash = usuarioOriginal.PasswordHash; // Mantiene la clave vieja
                        usuario.FechaRegistro = usuarioOriginal.FechaRegistro; // Mantiene fecha registro
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index)); // Admin vuelve al listado
            }
            return View(usuario);
        }

        // =============================================================
        //  COMPLETAR PERFIL (CLIENTE - Redirige a Dashboard)
        // =============================================================

        // GET: Usuarios/Perfil/5
        public async Task<IActionResult> Perfil(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario); // Usa la vista Perfil.cshtml (la bonita)
        }

        // POST: Usuarios/Perfil/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario) return NotFound();

            // Quitamos validación de password porque en el perfil no se edita
            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid)
            {
                try
                {
                    // Recuperamos datos originales para protegerlos
                    var dbUser = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);
                    if (dbUser != null)
                    {
                        usuario.PasswordHash = dbUser.PasswordHash; // Protegido
                        usuario.Estado = dbUser.Estado;             // Protegido
                        usuario.TipoUsuario = dbUser.TipoUsuario;   // Protegido
                        usuario.FechaRegistro = dbUser.FechaRegistro;
                        usuario.Email = dbUser.Email;               // Protegido
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario)) return NotFound();
                    else throw;
                }
                // CLAVE: El cliente vuelve a su Dashboard
                return RedirectToAction("Dashboard", "Home");
            }
            return View(usuario);
        }

        // =============================================================
        //  ELIMINAR (Soft Delete)
        // =============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Usuarios == null) return NotFound();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Usuarios == null) return Problem("Entity set is null.");

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario != null)
            {
                // Soft Delete: Solo cambiamos el estado
                usuario.Estado = false;
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return (_context.Usuarios?.Any(e => e.IdUsuario == id)).GetValueOrDefault();
        }
    }
}