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

        public async Task<IActionResult> AssignServices(int id)
        {
            // 1. Çalışanı ve mevcut atamalarını çekiyoruz
            var calisan = await _context.Calisanlar
                .Include(c => c.CalisanHizmetleri)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (calisan == null) return NotFound();

            // 2. Tüm hizmetleri ve ait oldukları salonları çekiyoruz
            var calisanSalonId = calisan.SalonId;

            // --- KRİTİK FİLTRELEME BURADA ---
            // 2. SADECE bu Salon ID'si ile eşleşen hizmetleri çekiyoruz.
            var tumHizmetler = _context.Hizmetler
                .Where(h => h.SalonId == calisanSalonId) // <-- YENİ FİLTRELEME KURALI
                .Include(h => h.Salon)
                .ToList();

            // 3. View'a göndermek için List<HizmetAtamasi> yapısını View Bag ile oluşturuyoruz.
            // Bu, ViewModel kullanmadığımız için veriyi paketlemenin manuel yoludur.
            var atamaListesi = new List<SelectListItem>();

            foreach (var hizmet in tumHizmetler.OrderBy(h => h.Salon.Ad).ThenBy(h => h.Ad))
            {
                // Çalışana atanmış mı kontrolü
                var atandiMi = calisan.CalisanHizmetleri
                                      .Any(ch => ch.HizmetId == hizmet.Id);

                atamaListesi.Add(new SelectListItem
                {
                    Value = hizmet.Id.ToString(),
                    Text = $"{hizmet.Ad}", 
                    Selected = atandiMi // Atanmışsa Seçili (Checked) geliyor
                });
            }
            var salon = await _context.Salonlar.FirstOrDefaultAsync(p => p.Id == calisan.SalonId);
            // 4. Gerekli tüm bilgileri ViewBag/ViewData üzerinden taşı
            ViewBag.CalisanId = calisan.Id;
            ViewBag.CalisanAdSoyad = calisan.AdSoyad;
            ViewBag.HizmetAtamaListesi = atamaListesi; // Tüm hizmet listesi
            ViewBag.SalonAdi = salon?.Ad;

            // Model olarak boş bir Calisan nesnesi veya sadece Id'yi gönderebiliriz.
            return View();
        }

        // Controllers/CalisanController.cs

        // ... (AssignServices (GET) metodunun altı) ...

        // POST: /Calisan/AssignServices/5
        // Bu metot, formdan gelen CalisanId ve seçili HizmetId'lerin listesini alır.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignServices(int calisanId, List<int> HizmetIdler)
        {
            // 1. Çalışanı ve mevcut atamalarını veritabanından çek
            var calisan = await _context.Calisanlar
                .Include(c => c.CalisanHizmetleri)
                .FirstOrDefaultAsync(c => c.Id == calisanId);

            if (calisan == null) return NotFound();

            // 2. Mevcut atamaları temizle
            calisan.CalisanHizmetleri!.Clear();

            // 3. Formdan gelen seçili HizmetId'leri tek tek ekle
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

            // 4. Değişiklikleri veritabanına kaydet
            await _context.SaveChangesAsync();

            // Çalışan listesine geri dön
            return RedirectToAction(nameof(Index));
        }
    }
}
