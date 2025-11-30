using Berber.Data;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Berber.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class RandevuYonetimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuYonetimController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var randevular = await _context.Randevular
                .Where(r => r.TarihSaat.Date >= DateTime.Today)

                .Include(r => r.Musteri) 
                .Include(r => r.Hizmet) 
                .Include(r => r.Calisan)
                .OrderByDescending(r => r.Durum == OnayDurumu.Bekliyor)
                .ThenBy(r => r.TarihSaat)
                .ToListAsync();

            return View(randevular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = OnayDurumu.Onaylandi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = OnayDurumu.IptalEdildi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}