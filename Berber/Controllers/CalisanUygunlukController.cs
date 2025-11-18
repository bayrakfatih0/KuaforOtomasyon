using Berber.Data;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için
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

        // GET: /CalisanUygunluk/Index?calisanId=5
        // Bu sayfa parametre olarak MUTLAKA bir calisanId almalıdır.
        public async Task<IActionResult> Index(int? calisanId)
        {
            if (calisanId == null)
            {
                return NotFound();
            }

            // 1. Seçilen çalışanın adını bulalım (Başlıkta göstermek için)
            var calisan = await _context.Calisanlar.FindAsync(calisanId);
            if (calisan == null)
            {
                return NotFound();
            }

            // Bu bilgiyi View'a taşıyalım ki "Ali Yılmaz'ın Saatleri" diyebilelim.
            ViewData["CalisanAd"] = calisan.AdSoyad;
            ViewData["CalisanId"] = calisan.Id;

            // 2. SADECE bu çalışana ait saatleri getir
            var uygunluklar = _context.CalisanUygunluklari
                                      .Where(u => u.CalisanId == calisanId)
                                      .OrderBy(u => u.Gun) // Güne göre sırala
                                      .ThenBy(u => u.BaslangicSaati); // Sonra saate göre

            return View(await uygunluklar.ToListAsync());
        }

        // GET: /CalisanUygunluk/Create?calisanId=5
        public IActionResult Create(int? calisanId)
        {
            if (calisanId == null)
            {
                return NotFound();
            }

            // 1. Hangi çalışana ekleme yapacağımızı View'a taşıyoruz
            ViewData["CalisanId"] = calisanId;

            // 2. Günler için Türkçe bir liste hazırlayalım
            // (DayOfWeek enum'u İngilizce olduğu için bunu elle yapıyoruz)
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

        // POST: /CalisanUygunluk/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CalisanUygunluk calisanUygunluk)
        {
            // İlişkili "Calisan" nesnesi formdan null geleceği için doğrulamadan çıkarıyoruz
            ModelState.Remove("Calisan");

            if (ModelState.IsValid)
            {
                _context.Add(calisanUygunluk);
                await _context.SaveChangesAsync();

                // Kayıt bitti, tekrar o çalışanın saat listesine yönlendir
                return RedirectToAction(nameof(Index), new { calisanId = calisanUygunluk.CalisanId });
            }

            // Hata varsa dropdown'ı tekrar doldurmamız lazım (yoksa hata verir)
            // (Burada tekrar aynı listeyi oluşturuyoruz)
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

            // CalisanId'yi de tekrar View'a taşı
            ViewData["CalisanId"] = calisanUygunluk.CalisanId;

            return View(calisanUygunluk);
        }

        // GET: /CalisanUygunluk/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Silinecek kaydı ve kime ait olduğunu (Calisan) getir
            var uygunluk = await _context.CalisanUygunluklari
                .Include(c => c.Calisan)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (uygunluk == null)
            {
                return NotFound();
            }

            return View(uygunluk);
        }

        // POST: /CalisanUygunluk/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uygunluk = await _context.CalisanUygunluklari.FindAsync(id);

            // Geri döneceğimiz çalışanın ID'sini saklayalım (varsayılan 0)
            int calisanId = 0;

            if (uygunluk != null)
            {
                // 1. ID'yi kap (Silmeden önce!)
                calisanId = uygunluk.CalisanId;

                // 2. Kaydı sil
                _context.CalisanUygunluklari.Remove(uygunluk);
                await _context.SaveChangesAsync();
            }

            // 3. Sakladığımız ID ile doğru listeye geri dön
            return RedirectToAction(nameof(Index), new { calisanId = calisanId });
        }
    }
}