using Berber.Data;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore;

namespace Berber.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CalisanUygunlukController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalisanUygunlukController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? calisanId)
        {
            if (calisanId == null)
            {
                return NotFound();
            }

            var calisan = await _context.Calisanlar.FindAsync(calisanId);
            if (calisan == null)
            {
                return NotFound();
            }

            ViewData["CalisanAd"] = calisan.AdSoyad;
            ViewData["CalisanId"] = calisan.Id;

            var uygunluklar = _context.CalisanUygunluklari
                                      .Where(u => u.CalisanId == calisanId)
                                      .OrderBy(u => u.Gun) 
                                      .ThenBy(u => u.BaslangicSaati); 

            return View(await uygunluklar.ToListAsync());
        }

        public IActionResult Create(int? calisanId)
        {
            if (calisanId == null)
            {
                return NotFound();
            }

            ViewData["CalisanId"] = calisanId;

            var gunler = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Pazartesi" },
                new SelectListItem { Value = "2", Text = "Salı" },
                new SelectListItem { Value = "3", Text = "Çarşamba" },
                new SelectListItem { Value = "4", Text = "Perşembe" },
                new SelectListItem { Value = "5", Text = "Cuma" },
                new SelectListItem { Value = "6", Text = "Cumartesi" },
                new SelectListItem { Value = "0", Text = "Pazar" }
            };

            ViewData["Gunler"] = new SelectList(gunler, "Value", "Text");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CalisanUygunluk calisanUygunluk)
        {
            ModelState.Remove("Calisan");

            if (ModelState.IsValid)
            {
                _context.Add(calisanUygunluk);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { calisanId = calisanUygunluk.CalisanId });
            }

            var gunler = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Pazartesi" },
                new SelectListItem { Value = "2", Text = "Salı" },
                new SelectListItem { Value = "3", Text = "Çarşamba" },
                new SelectListItem { Value = "4", Text = "Perşembe" },
                new SelectListItem { Value = "5", Text = "Cuma" },
                new SelectListItem { Value = "6", Text = "Cumartesi" },
                new SelectListItem { Value = "0", Text = "Pazar" }
            };
            ViewData["Gunler"] = new SelectList(gunler, "Value", "Text", calisanUygunluk.Gun);

            ViewData["CalisanId"] = calisanUygunluk.CalisanId;

            return View(calisanUygunluk);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uygunluk = await _context.CalisanUygunluklari
                .Include(c => c.Calisan)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (uygunluk == null)
            {
                return NotFound();
            }

            return View(uygunluk);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uygunluk = await _context.CalisanUygunluklari.FindAsync(id);

            int calisanId = 0;

            if (uygunluk != null)
            {
                calisanId = uygunluk.CalisanId;

                _context.CalisanUygunluklari.Remove(uygunluk);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { calisanId = calisanId });
        }
    }
}