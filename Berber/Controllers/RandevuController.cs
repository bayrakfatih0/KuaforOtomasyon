using Berber.Data;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berber.Controllers
{
    [Authorize] // Sadece giriş yapmış (Müşteri/Admin) kullanıcılar erişebilir
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADIM 1: HİZMET SEÇİMİ
        // GET: /Randevu/Index?salonId=3
        public async Task<IActionResult> Index(int? salonId)
        {
            if (salonId == null)
            {
                return RedirectToAction("Index", "Home"); // Salon seçilmediyse ana sayfaya at
            }

            // 1. Seçilen salonun bilgilerini al (Adını göstermek için)
            var salon = await _context.Salonlar.FindAsync(salonId);
            if (salon == null) return NotFound();

            ViewData["SalonAd"] = salon.Ad;
            ViewData["SalonId"] = salon.Id;

            // 2. Sadece O SALONA ait hizmetleri getir
            var hizmetler = await _context.Hizmetler
                                    .Where(h => h.SalonId == salonId)
                                    .ToListAsync();

            return View(hizmetler);
        }

        // Controllers/RandevuController.cs

        // ... (Index metodunuzun bittiği yer) ...

        // ADIM 2: ÇALIŞAN SEÇİMİ
        // GET: /Randevu/CalisanSec?hizmetId=1&salonId=3
        public async Task<IActionResult> CalisanSec(int hizmetId, int salonId)
        {
            // Gerekli bilgileri alıyoruz
            var salon = await _context.Salonlar.FindAsync(salonId);
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            // View için başlık ve taşıyıcı verileri hazırlıyoruz
            ViewData["SalonAd"] = salon?.Ad;
            ViewData["HizmetAd"] = hizmet?.Ad;
            ViewData["HizmetId"] = hizmetId;
            ViewData["SalonId"] = salonId;

            // --- KRİTİK SORGULAMA ---
            // 1. O salona ait çalışanları bul
            // 2. Bu çalışanlardan, seçilen HizmetId'ye ATANMIŞ olanları getir
            var calisanlar = await _context.Calisanlar
                .Include(c => c.CalisanHizmetleri) // Hizmet eşleşmelerini kontrol etmek için
                .Where(c => c.SalonId == salonId)
                .Where(c => c.CalisanHizmetleri.Any(ch => ch.HizmetId == hizmetId))
                .ToListAsync();

            return View(calisanlar);
        }

        // Controllers/RandevuController.cs

        // ... (Diğer metotlar) ...

        // ADIM 3: TARİH VE SAAT SEÇİMİ (GET)
        // Bu metot, Çalışan Seçimi sayfasından gelen 3 ID'yi alır.
        // Controllers/RandevuController.cs

        // ... (Diğer metotlar) ...

        // ADIM 3: TARİH VE SAAT SEÇİMİ (GET)
        // GET: /Randevu/SaatSecimi?hizmetId=1&calisanId=5&secilenTarih=2025-11-20
        public async Task<IActionResult> SaatSecimi(int hizmetId, int salonId, int calisanId, DateTime? secilenTarih)
        {
            // Verileri çekme
            var calisan = await _context.Calisanlar.FindAsync(calisanId);
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            if (calisan == null || hizmet == null)
            {
                return NotFound();
            }

            // Tarih seçilmemişse, bugünü (veya yarını) varsayılan olarak al
            DateTime tarih = secilenTarih?.Date ?? DateTime.Today;

            // Gerekli verileri View'a taşıma
            ViewData["CalisanAd"] = calisan.AdSoyad;
            ViewData["HizmetAd"] = hizmet.Ad;
            ViewData["HizmetSuresi"] = hizmet.Sure;
            ViewData["CalisanId"] = calisanId;
            ViewData["HizmetId"] = hizmetId;
            ViewData["SalonId"] = salonId;
            ViewData["SecilenTarih"] = tarih.ToString("yyyy-MM-dd"); // HTML input formatı

            // --- KRİTİK: MÜSAİT SAAT HESAPLAMA ---
            var musaitSaatler = await GetMusaitSaatlerAsync(calisanId, tarih, hizmet.Sure);
            ViewData["MusaitSaatler"] = musaitSaatler;

            return View();
        }

        // ... (ŞİMDİLİK BOŞ OLAN POST METODU BURAYA GELECEK) ...

        // Controllers/RandevuController.cs

        // ... (SaatSecimi metodunun altına ekleyin) ...

        // ADIM 4: RANDEVUYU ONAYLA VE KAYDET (POST)
        // Controllers/RandevuController.cs - RandevuOnay metodu

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOnay(
            int calisanId, int hizmetId, int salonId,
            DateTime secilenTarih, string secilenSaat)
        {
            // 1. Gelen tarih ve saati birleştirme (Önceki kodunuz)
            if (!TimeSpan.TryParse(secilenSaat, out TimeSpan saatDilimi))
            {
                return BadRequest("Geçersiz saat formatı.");
            }
            DateTime randevuTarihi = secilenTarih.Date.Add(saatDilimi);

            // 2. Randevu nesnesini oluşturma
            var yeniRandevu = new Randevu
            {
                TarihSaat = randevuTarihi,
                CalisanId = calisanId,
                HizmetId = hizmetId,
                Durum = OnayDurumu.Bekliyor, // Doğru duruma set ettik
                MusteriId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            };

            // --- BURASI KRİTİK NOKTA: SaveChanges'i Try/Catch ile koruyoruz ---
            try
            {
                _context.Add(yeniRandevu);
                await _context.SaveChangesAsync();

                // BAŞARI: Onay sayfasına yönlendir
                return RedirectToAction("RandevuOnaylandi", new { randevuId = yeniRandevu.Id });
            }
            catch (Exception ex)
            {
                // HATA OLURSA, GELİŞTİRME EKRANINDA HATAYI GÖRMEK İÇİN
                // View'a geri dönüyoruz. Normalde loglama yapılır.
                ViewData["ErrorMessage"] = "Randevu kaydı sırasında beklenmedik bir veritabanı hatası oluştu. Lütfen girilen verileri kontrol edin.";

                // Konsolda tam hatayı görmek için (Debug yapmıyorsak)
                Console.WriteLine($"Randevu Kayıt Hatası: {ex.Message}");

                // Hata View'ını göster
                return View("Error"); // Projenizin varsayılan Error View'ına yönlendiriyoruz
            }
        }

        // ... (Yeni bir onay sayfası ekleyelim) ...

        public IActionResult RandevuOnaylandi(int randevuId)
        {
            ViewData["RandevuId"] = randevuId;
            return View();
        }

        // Controllers/RandevuController.cs

        // ... (Diğer metotlarınız burada biter) ...

        // RANDEVU HESAPLAMA YARDIMCI METODU
        private async Task<List<string>> GetMusaitSaatlerAsync(int calisanId, DateTime tarih, int sure)
        {
            // 1. Çalışanın O GÜNKÜ UYGUNLUK SAATLERİNİ BULMA
            var gun = tarih.DayOfWeek;
            // Bu, o gün için tanımlanmış tüm vardiyaları içerir (örn: sabah 09:00-13:00 ve akşam 14:00-18:00)
            var uygunlukList = await _context.CalisanUygunluklari
                .Where(u => u.CalisanId == calisanId && u.Gun == gun)
                .ToListAsync();

            // 2. O GÜNKÜ TÜM DOLU RANDEVULARI BULMA
            // Randevunun süresini (Hizmet.Sure) kontrol etmek için Include kullanıyoruz.
            var doluRandevular = await _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.CalisanId == calisanId && r.TarihSaat.Date == tarih.Date)
                .OrderBy(r => r.TarihSaat)
                .ToListAsync();

            // 3. MÜSAİT ZAMAN DİLİMLERİNİ HESAPLAMA
            var musaitSaatler = new List<string>();

            // Her uygunluk dilimini (vardiyayı) kontrol et
            foreach (var uygunluk in uygunlukList)
            {
                // 30 dakikalık aralıklarla saat dilimlerini kontrol edeceğiz.
                TimeSpan mevcutSaat = uygunluk.BaslangicSaati;
                TimeSpan bitisSaat = uygunluk.BitisSaati;
                TimeSpan increment = TimeSpan.FromMinutes(30); // 30 dakikalık artış

                while (mevcutSaat < bitisSaat)
                {
                    // Potansiyel randevu başlangıç ve bitiş zamanı
                    DateTime slotBaslangic = tarih.Date.Add(mevcutSaat);
                    DateTime slotBitis = slotBaslangic.AddMinutes(sure);

                    // Randevunun Hizmet Süresini (sure) karşılayıp karşılamadığını ve
                    // vardiya bitiş saatinden taşmadığını kontrol et
                    if (slotBitis.TimeOfDay > bitisSaat)
                    {
                        // Bu slot vardiyadan taşıyor, bu vardiyayı bitir.
                        break;
                    }

                    // Çakışma kontrolü
                    bool cakismaVar = doluRandevular.Any(d =>
                        (slotBaslangic < d.TarihSaat.AddMinutes(d.Hizmet.Sure) && d.TarihSaat < slotBitis)
                    );

                    if (!cakismaVar)
                    {
                        musaitSaatler.Add(mevcutSaat.ToString(@"hh\:mm"));
                    }

                    // Bir sonraki 30 dakikalık dilime geç
                    mevcutSaat = mevcutSaat.Add(increment);
                }
            }

            return musaitSaatler;
        }


        // Controllers/RandevuController.cs

        // ... (Diğer metotlar) ...

        // GET: /Randevu/Randevularim
        [Authorize] // Sadece giriş yapanlar görebilir
        public async Task<IActionResult> Randevularim()
        {
            // Şu anki giriş yapan kullanıcının ID'sini bul
            var musteriId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sadece BU müşteriye ait randevuları getir
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
    }
}