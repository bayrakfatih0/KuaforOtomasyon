using Berber.Data;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Berber.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CalisanController : Controller

    {
        private readonly ApplicationDbContext _context;

        public CalisanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Çalışanları getirirken Salon bilgilerini de getir (Join)
            var calisanlar = _context.Calisanlar.Include(c => c.Salon);
            return View(await calisanlar.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var calisan = await _context.Calisanlar
                .Include(c => c.Salon) // Salon bilgilerini de getir
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calisan == null)
            {
                return NotFound();
            }
            return View(calisan);
        }

        // ... (Index metodu üstte) ...

        // 1. GET: /Calisan/Create
        // Formu ve Salon listesini hazırlar
        public IActionResult Create()
        {
            // Salonları "Id" (değer) ve "Ad" (görünen metin) olarak listeye çevir
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad");
            return View();
        }

        // 2. POST: /Calisan/Create
        // Formdan gelen veriyi kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Calisan calisan)
        {
            
            // Model doğrulaması (AdSoyad dolu mu? Salon seçili mi?)
            if (ModelState.IsValid)
            {
                _context.Add(calisan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa (örn: boş alan), Salon listesini tekrar doldurup formu geri ver
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", calisan.SalonId);
            return View(calisan);
        }

        // GET: /Calisan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calisan = await _context.Calisanlar.FindAsync(id);
            if (calisan == null)
            {
                return NotFound();
            }

            // Salon listesini doldur (Mevcut salonu seçili getir)
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", calisan.SalonId);
            return View(calisan);
        }

        // POST: /Calisan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Calisan calisan)
        {
            if (id != calisan.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(calisan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Calisanlar.Any(e => e.Id == calisan.Id))
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

            // Hata varsa listeyi tekrar doldur
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", calisan.SalonId);
            return View(calisan);
        }

        // GET: /Calisan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calisan = await _context.Calisanlar
                .Include(c => c.Salon) // Salon adını göstermek için
                .FirstOrDefaultAsync(m => m.Id == id);

            if (calisan == null)
            {
                return NotFound();
            }

            return View(calisan);
        }

        // POST: /Calisan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var calisan = await _context.Calisanlar.FindAsync(id);
            if (calisan != null)
            {
                _context.Calisanlar.Remove(calisan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
