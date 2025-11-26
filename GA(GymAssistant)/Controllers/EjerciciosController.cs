using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Data;
using GA_GymAssistant_.Models;

namespace GA_GymAssistant.Controllers
{
    public class EjerciciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EjerciciosController(ApplicationDbContext context)
        {
            _context = context;
        }

 
        public async Task<IActionResult> Index()
        {
      
            var applicationDbContext = _context.Ejercicios.Include(e => e.Maquina);
            return View(await applicationDbContext.ToListAsync());
        }


        public IActionResult Create()
        {
          
            ViewData["IdMaquina"] = new SelectList(_context.Maquinas, "IdMaquina", "Nombre");
            return View();
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ejercicio ejercicio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ejercicio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMaquina"] = new SelectList(_context.Maquinas, "IdMaquina", "Nombre", ejercicio.IdMaquina);
            return View(ejercicio);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ejercicio = await _context.Ejercicios.FindAsync(id);
            if (ejercicio == null) return NotFound();

            ViewData["IdMaquina"] = new SelectList(_context.Maquinas, "IdMaquina", "Nombre", ejercicio.IdMaquina);
            return View(ejercicio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ejercicio ejercicio)
        {
            if (id != ejercicio.IdEjercicio) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ejercicio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Ejercicios.Any(e => e.IdEjercicio == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMaquina"] = new SelectList(_context.Maquinas, "IdMaquina", "Nombre", ejercicio.IdMaquina);
            return View(ejercicio);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ejercicio = await _context.Ejercicios.Include(e => e.Maquina).FirstOrDefaultAsync(m => m.IdEjercicio == id);
            if (ejercicio == null) return NotFound();
            return View(ejercicio);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ejercicio = await _context.Ejercicios.FindAsync(id);
            if (ejercicio != null) { _context.Ejercicios.Remove(ejercicio); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}