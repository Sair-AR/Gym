using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Data;
using GA_GymAssistant_.Models;

namespace GA_GymAssistant.Controllers
{
    public class MaquinasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaquinasController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            return View(await _context.Maquinas.ToListAsync());
        }


        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Maquinas maquinas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(maquinas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(maquinas);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var maquinas = await _context.Maquinas.FindAsync(id);
            if (maquinas == null) return NotFound();
            return View(maquinas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Maquinas maquinas)
        {
            if (id != maquinas.IdMaquina) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maquinas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Maquinas.Any(e => e.IdMaquina == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(maquinas);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var maquinas = await _context.Maquinas.FirstOrDefaultAsync(m => m.IdMaquina == id);
            if (maquinas == null) return NotFound();

            return View(maquinas);
        }

    
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maquinas = await _context.Maquinas.FindAsync(id);
            if (maquinas != null)
            {
                _context.Maquinas.Remove(maquinas);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}