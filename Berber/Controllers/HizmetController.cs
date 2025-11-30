using Berber.Data;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Berber.Controllers 
{
    [Authorize(Roles = "Admin")]
    public class HizmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hizmetler = _context.Hizmetler.Include(h => h.Salon);

            return View(await hizmetler.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); 
            }

            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet == null)
            {
                return NotFound(); 
            }

            return View(hizmet);
        }

        public IActionResult Create()
        {
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hizmet hizmet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", hizmet.SalonId);

            return View(hizmet);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null)
            {
                return NotFound(); 
            }

            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", hizmet.SalonId);

            return View(hizmet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hizmet hizmet)
        {
            if (id != hizmet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hizmet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Hizmetler.Any(e => e.Id == hizmet.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", hizmet.SalonId);

            return View(hizmet);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);

            if (hizmet != null)
            {
                _context.Hizmetler.Remove(hizmet);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}