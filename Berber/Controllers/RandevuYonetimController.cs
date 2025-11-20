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
        // Controllers/RandevuYonetimController.cs - Index() metodu


        // Controllers/RandevuYonetimController.cs - Index() metodu

        public async Task<IActionResult> Index()
        {
            var randevular = await _context.Randevular
                .Where(r => r.TarihSaat.Date >= DateTime.Today)

                // --- İLİŞKİLERİ GERİ YÜKLEDİK ---
                .Include(r => r.Musteri) // Müşteri Ad/Email için
                .Include(r => r.Hizmet) // Hizmet Adı için
                .Include(r => r.Calisan) // Çalışan Adı için (Bu, Calisan.AdSoyad'ı çekmemizi sağlar)
                                         // Not: Calisan.Salon.Ad ilişkisini kasten eklemiyoruz, çünkü o çöküyordu.

                .OrderByDescending(r => r.Durum == OnayDurumu.Bekliyor)
                .ThenBy(r => r.TarihSaat)
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