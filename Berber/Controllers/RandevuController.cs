using Berber.Data;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berber.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? salonId)
        {
            if (salonId == null)
            {
                return RedirectToAction("Index", "Home"); 
            }

            var salon = await _context.Salonlar.FindAsync(salonId);
            if (salon == null) return NotFound();

            ViewData["SalonAd"] = salon.Ad;
            ViewData["SalonId"] = salon.Id;

            var hizmetler = await _context.Hizmetler
                                    .Where(h => h.SalonId == salonId)
                                    .ToListAsync();

            return View(hizmetler);
        }

        public async Task<IActionResult> CalisanSec(int hizmetId, int salonId)
        {
            var salon = await _context.Salonlar.FindAsync(salonId);
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            ViewData["SalonAd"] = salon?.Ad;
            ViewData["HizmetAd"] = hizmet?.Ad;
            ViewData["HizmetId"] = hizmetId;
            ViewData["SalonId"] = salonId;

            var calisanlar = await _context.Calisanlar
                .Include(c => c.CalisanHizmetleri) 
                .Where(c => c.SalonId == salonId)
                .Where(c => c.CalisanHizmetleri.Any(ch => ch.HizmetId == hizmetId))
                .ToListAsync();

            return View(calisanlar);
        }

        public async Task<IActionResult> SaatSecimi(int hizmetId, int salonId, int calisanId, DateTime? secilenTarih)
        {
            var calisan = await _context.Calisanlar.FindAsync(calisanId);
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            if (calisan == null || hizmet == null)
            {
                return NotFound();
            }

            DateTime tarih = secilenTarih?.Date ?? DateTime.Today;

            ViewData["CalisanAd"] = calisan.AdSoyad;
            ViewData["HizmetAd"] = hizmet.Ad;
            ViewData["HizmetSuresi"] = hizmet.Sure;
            ViewData["CalisanId"] = calisanId;
            ViewData["HizmetId"] = hizmetId;
            ViewData["SalonId"] = salonId;
            ViewData["SecilenTarih"] = tarih.ToString("yyyy-MM-dd");

            var musaitSaatler = await GetMusaitSaatlerAsync(calisanId, tarih, hizmet.Sure);
            ViewData["MusaitSaatler"] = musaitSaatler;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOnay(
            int calisanId, int hizmetId, int salonId,
            DateTime secilenTarih, string secilenSaat)
        {
            if (!TimeSpan.TryParse(secilenSaat, out TimeSpan saatDilimi))
            {
                return BadRequest("Geçersiz saat formatı.");
            }
            DateTime randevuTarihi = secilenTarih.Date.Add(saatDilimi);

            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);
            if (hizmet == null) return NotFound("Hizmet bulunamadı.");

            string? errorMessage = await CheckForConflictsAsync(randevuTarihi, calisanId, hizmet.Sure);

            if (errorMessage != null)
            {
                TempData["ErrorMessage"] = errorMessage;

                return RedirectToAction(nameof(SaatSecimi), new { hizmetId, salonId, calisanId });
            }

            var yeniRandevu = new Randevu
            {
                TarihSaat = randevuTarihi,
                CalisanId = calisanId,
                HizmetId = hizmetId,
                Durum = OnayDurumu.Bekliyor,
                MusteriId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            };

            _context.Add(yeniRandevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(RandevuOnaylandi), new { randevuId = yeniRandevu.Id });
        }


        public IActionResult RandevuOnaylandi(int randevuId)
        {
            ViewData["RandevuId"] = randevuId;
            return View();
        }

        private async Task<List<string>> GetMusaitSaatlerAsync(int calisanId, DateTime tarih, int sure)
        {
            var gun = tarih.DayOfWeek;
            var uygunlukList = await _context.CalisanUygunluklari
                .Where(u => u.CalisanId == calisanId && u.Gun == gun)
                .ToListAsync();

            var doluRandevular = await _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.CalisanId == calisanId && r.TarihSaat.Date == tarih.Date)
                .OrderBy(r => r.TarihSaat)
                .ToListAsync();

            var musaitSaatler = new List<string>();

            foreach (var uygunluk in uygunlukList)
            {
                TimeSpan mevcutSaat = uygunluk.BaslangicSaati;
                TimeSpan bitisSaat = uygunluk.BitisSaati;
                TimeSpan increment = TimeSpan.FromMinutes(30); 

                while (mevcutSaat < bitisSaat)
                {
                    DateTime slotBaslangic = tarih.Date.Add(mevcutSaat);
                    DateTime slotBitis = slotBaslangic.AddMinutes(sure);

                    if (slotBitis.TimeOfDay > bitisSaat)
                    {
                        break;
                    }

                    bool cakismaVar = doluRandevular.Any(d =>
                        (slotBaslangic < d.TarihSaat.AddMinutes(d.Hizmet.Sure) && d.TarihSaat < slotBitis)
                    );

                    if (!cakismaVar)
                    {
                        musaitSaatler.Add(mevcutSaat.ToString(@"hh\:mm"));
                    }

                    mevcutSaat = mevcutSaat.Add(increment);
                }
            }

            return musaitSaatler;
        }

        [Authorize] 
        public async Task<IActionResult> Randevularim()
        {
            var musteriId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var randevular = await _context.Randevular
                .Include(r => r.Calisan).ThenInclude(c => c.Salon)
                .Include(r => r.Hizmet)
                .Where(r => r.MusteriId == musteriId)
                .OrderByDescending(r => r.TarihSaat)
                .ToListAsync();

            return View(randevular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if(randevu != null)
            {
                randevu.Durum = OnayDurumu.IptalEdildi;

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Randevularim));
        }

        private async Task<string?> CheckForConflictsAsync(DateTime startTime, int calisanId, int serviceDuration)
        {
            var gun = startTime.DayOfWeek;
            TimeSpan requestStartTime = startTime.TimeOfDay;
            TimeSpan requestEndTime = requestStartTime.Add(TimeSpan.FromMinutes(serviceDuration));

            var uygunluk = await _context.CalisanUygunluklari
                .Where(u => u.CalisanId == calisanId && u.Gun == gun)
                .FirstOrDefaultAsync(u => u.BaslangicSaati <= requestStartTime && u.BitisSaati >= requestEndTime);

            if (uygunluk == null)
            {
                return "Çalışanın seçilen saatte tanımlı vardiyası bulunmamaktadır. Lütfen başka bir saat seçin.";
            }

            var doluRandevular = await _context.Randevular
                .Include(r => r.Hizmet) 
                .Where(r => r.CalisanId == calisanId && r.TarihSaat.Date == startTime.Date)
                .ToListAsync();

            DateTime slotBitis = startTime.AddMinutes(serviceDuration);

            bool conflictExists = doluRandevular.Any(existing =>
            {
                DateTime existingEndTime = existing.TarihSaat.AddMinutes(existing.Hizmet.Sure);
                return (slotBitis > existing.TarihSaat && existingEndTime > startTime);
            });

            if (conflictExists)
            {
                return "Seçilen saat dilimi, mevcut bir randevu ile çakışmaktadır. Lütfen başka bir zaman seçin.";
            }

            return null;
        }
    }
}