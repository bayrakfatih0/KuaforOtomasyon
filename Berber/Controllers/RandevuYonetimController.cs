using Berber.Data;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Berber.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin girebilir
    public class RandevuYonetimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuYonetimController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /RandevuYonetim/Index
        public async Task<IActionResult> Index()
        {
            // Tüm randevuları çekiyoruz (Müşteri, Çalışan, Hizmet ve Salon bilgileriyle)
            var randevular = await _context.Randevular
                .Include(r => r.Musteri) // Müşteri adını görmek için
                .Include(r => r.Calisan).ThenInclude(c => c.Salon) // Salon adını da görmek için
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.TarihSaat) // En yeni randevu en üstte
                .ToListAsync();

            return View(randevular);
        }

        // POST: Randevuyu Onayla
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

        // POST: Randevuyu İptal Et
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