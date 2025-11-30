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
                .Include(c => c.Salon) 
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calisan == null)
            {
                return NotFound();
            }
            return View(calisan);
        }

        public IActionResult Create()
        {
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Calisan calisan)
        {
            
            if (ModelState.IsValid)
            {
                _context.Add(calisan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", calisan.SalonId);
            return View(calisan);
        }

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

            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", calisan.SalonId);
            return View(calisan);
        }

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

            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", calisan.SalonId);
            return View(calisan);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calisan = await _context.Calisanlar
                .Include(c => c.Salon) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (calisan == null)
            {
                return NotFound();
            }

            return View(calisan);
        }

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

        public async Task<IActionResult> AssignServices(int id)
        {
            var calisan = await _context.Calisanlar
                .Include(c => c.CalisanHizmetleri)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (calisan == null) return NotFound();

            var calisanSalonId = calisan.SalonId;

            var tumHizmetler = _context.Hizmetler
                .Where(h => h.SalonId == calisanSalonId) 
                .Include(h => h.Salon)
                .ToList();

            var atamaListesi = new List<SelectListItem>();

            foreach (var hizmet in tumHizmetler.OrderBy(h => h.Salon.Ad).ThenBy(h => h.Ad))
            {
                var atandiMi = calisan.CalisanHizmetleri
                                      .Any(ch => ch.HizmetId == hizmet.Id);

                atamaListesi.Add(new SelectListItem
                {
                    Value = hizmet.Id.ToString(),
                    Text = $"{hizmet.Ad}", 
                    Selected = atandiMi 
                });
            }
            var salon = await _context.Salonlar.FirstOrDefaultAsync(p => p.Id == calisan.SalonId);
            ViewBag.CalisanId = calisan.Id;
            ViewBag.CalisanAdSoyad = calisan.AdSoyad;
            ViewBag.HizmetAtamaListesi = atamaListesi; 
            ViewBag.SalonAdi = salon?.Ad;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignServices(int calisanId, List<int> HizmetIdler)
        {
            var calisan = await _context.Calisanlar
                .Include(c => c.CalisanHizmetleri)
                .FirstOrDefaultAsync(c => c.Id == calisanId);

            if (calisan == null) return NotFound();

            calisan.CalisanHizmetleri!.Clear();

            if (HizmetIdler != null)
            {
                foreach (var hizmetId in HizmetIdler)
                {
                    calisan.CalisanHizmetleri.Add(new CalisanHizmet
                    {
                        CalisanId = calisanId,
                        HizmetId = hizmetId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
