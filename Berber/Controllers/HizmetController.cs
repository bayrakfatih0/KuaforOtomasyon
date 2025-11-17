using Berber.Data; // Proje adınıza göre düzeltin
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Berber.Controllers // Proje adınıza göre düzeltin
{
    // 1. Bu controller'daki tüm sayfalara erişimi SADECE "Admin" rolüne kilitliyoruz.
    [Authorize(Roles = "Admin")]
    public class HizmetController : Controller
    {
        // 2. Veritabanı bağlantımızı tanımlıyoruz.
        private readonly ApplicationDbContext _context;

        // 3. Constructor (Yapıcı Metot) ile veritabanı bağlantısını alıyoruz.
        public HizmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 4. GET: /Hizmet veya /Hizmet/Index
        // Hizmetleri listeleyecek ana metodumuz.
        public async Task<IActionResult> Index()
        {
            // Hizmetleri alırken, onlara bağlı olan 'Salon' bilgilerini de
            // getirmesi için 'Include' kullanıyoruz.
            // Aksi halde 'SalonId' gelir ama 'Salon.Ad' gelmez (null olur).
            var hizmetler = _context.Hizmetler.Include(h => h.Salon);

            // Verileri 'Index.cshtml' sayfasına gönderip kullanıcıya gösteriyoruz.
            return View(await hizmetler.ToListAsync());
        }

        // Controllers/HizmetController.cs

        // ... (Diğer metotlarınız) ...

        // GET: /Hizmet/Details/5
        // Bir hizmetin detaylarını GÖSTERİR
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Id gelmezse 404 hatası ver
            }

            // Hizmetin bilgilerini ve bağlı olduğu Salon'un adını
            // getirmek için .Include() kullanıyoruz.
            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet == null)
            {
                return NotFound(); // Hizmet bulunamazsa 404 hatası ver
            }

            // Bulunan 'hizmet' modelini View'a gönder
            return View(hizmet);
        }

        public IActionResult Create()
        {
            // Formda bir açılır liste (dropdown) göstermek istiyoruz.
            // Bu liste, veritabanındaki tüm salonları içermeli.
            // ViewData["SalonId"] adında bir "taşıyıcı" oluşturuyoruz.
            // İçine, Salonlar tablosundaki verileri (Id = Değer, Ad = Metin)
            // formatında bir liste olarak koyuyoruz.
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad");

            // Boş formu kullanıcıya göster.
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için (CSRF saldırılarını önler)
        public async Task<IActionResult> Create(Hizmet hizmet)
        {
            // 1. Gelen veriler, Hizmet.cs modelindeki [Required] gibi
            // kurallara uyuyor mu? (Örn: Ücreti boş mu yolladı?)
            if (ModelState.IsValid)
            {
                // 2. EVET, UYUYOR:
                // Veriyi veritabanına ekle, kaydet ve listeye geri dön.
                _context.Add(hizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // /Hizmet/Index'e yönlendir
            }

            // 3. HAYIR, UYMUYOR:
            // Kullanıcıya formu tekrar gösterirken, açılır liste (dropdown)
            // boşalmasın diye onu YENİDEN doldurmamız gerekir.
            // Bu satır ÇOK ÖNEMLİDİR!
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", hizmet.SalonId);

            // Kullanıcıya, doldurduğu (hatalı) verilerle birlikte formu geri göster.
            return View(hizmet);
        }

        // Controllers/HizmetController.cs

        // ... (Create (POST) metodunuzun bittiği yer) ...

        // GET: /Hizmet/Edit/5  (5 = düzenlenecek hizmetin Id'si)
        // Düzenleme formunu GÖSTERİR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Id gelmezse 404 hatası ver
            }

            // Hizmeti veritabanından bul
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null)
            {
                return NotFound(); // Hizmet bulunamazsa 404 hatası ver
            }

            // Formdaki açılır listeyi (dropdown) oluştur
            // DİKKAT: Create'ten farklı olarak, 4. parametreyi ekliyoruz.
            // Bu, listenin o hizmetin mevcut salonunu (hizmet.SalonId)
            // otomatik olarak seçili getirmesini sağlar.
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", hizmet.SalonId);

            // Formu, dolu olan "hizmet" modeliyle birlikte kullanıcıya göster
            return View(hizmet);
        }

        // Controllers/HizmetController.cs

        // ... (Edit (GET) metodunuzun bittiği yer) ...

        // POST: /Hizmet/Edit/5
        // Formdan gelen dolu verileri ALIP GÜNCELER
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Formdan hem 'Id' hem de 'Hizmet' nesnesinin tamamı gelir
        public async Task<IActionResult> Edit(int id, Hizmet hizmet)
        {
            // Adres çubuğundaki Id ile formdan gelen gizli Id eşleşmiyor mu?
            if (id != hizmet.Id)
            {
                return NotFound(); // Eşleşmiyorsa güvenlik ihlali olabilir
            }

            // Gelen veriler kurallara uyuyor mu?
            if (ModelState.IsValid)
            {
                try
                {
                    // EVET, UYUYOR:
                    // Veritabanındaki kaydı 'güncelle'
                    _context.Update(hizmet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Kaydetmeye çalışırken başka biri o kaydı sildiyse?
                    if (!_context.Hizmetler.Any(e => e.Id == hizmet.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Başarılıysa listeye geri dön
                return RedirectToAction(nameof(Index));
            }

            // HAYIR, UYMUYOR:
            // Hatalı formu geri gösterirken açılır listeyi (dropdown)
            // yeniden doldurmayı unutmuyoruz.
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", hizmet.SalonId);

            // Kullanıcıya, doldurduğu (hatalı) verilerle birlikte formu geri göster.
            return View(hizmet);
        }

        // Controllers/HizmetController.cs

        // ... (Edit (POST) metodunuzun bittiği yer) ...

        // GET: /Hizmet/Delete/5
        // Silme ONAY sayfasını GÖSTERİR
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Silinecek hizmetin bilgilerini (Salon adıyla birlikte)
            // veritabanından çekiyoruz ki kullanıcı neyi sildiğini görsün.
            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon) // Salon adını göstermek için
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet == null)
            {
                return NotFound();
            }

            // Onay sayfasını, hizmetin bilgileriyle birlikte göster
            return View(hizmet);
        }

        // Controllers/HizmetController.cs

        // ... (Delete (GET) metodunuzun bittiği yer) ...

        // POST: /Hizmet/Delete/5
        // Silme işlemini ONAYLAR ve GERÇEKLEŞTİRİR
        [HttpPost, ActionName("Delete")] // HTML formundaki asp-action="Delete" ile eşleşir
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Hizmeti bul
            var hizmet = await _context.Hizmetler.FindAsync(id);

            if (hizmet != null)
            {
                // Veritabanından 'kaldır'
                _context.Hizmetler.Remove(hizmet);
            }

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();

            // Listeye geri dön
            return RedirectToAction(nameof(Index));
        }
    }
}