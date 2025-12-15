// SERVICES/AppointmentAutomationService.cs

using Berber.Models;
using Berber.Data; // Kendi ApplicationDbContext'inizin bulunduğu namespace ile değiştirin
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace Berber.Services
{
    public class AppointmentAutomationService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentAutomationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> AttemptBookingAsync(
            GeminiAppointmentOutput aiResult, string currentUserId)
        {
            // 1. Veri Geçerliliğini Kontrol Et
            if (!DateTime.TryParse(aiResult.Tarih, out DateTime requestedDate) ||
                !TimeSpan.TryParse(aiResult.Saat, out TimeSpan requestedTime))
            {
                return (false, "Yapay zeka tarafından çözümlenen Tarih veya Saat formatı geçersiz.");
            }

            var requestedDateTime = requestedDate.Date.Add(requestedTime);

            // 2. Hizmeti Bul
            var service = await _context.Hizmetler
                .FirstOrDefaultAsync(h => h.Ad.ToLower() == aiResult.HizmetAdi.ToLower());

            if (service == null)
            {
                return (false, $"Sistemde '{aiResult.HizmetAdi}' adında bir hizmet bulunamadı.");
            }

            // 3. Çalışanı Bul (Hizmeti verebilen ilk müsait çalışanı bulma mantığı)

            // a. Hizmeti verebilen çalışanları bul
            var suitableEmployees = await _context.CalisanHizmetleri
                .Include(ch => ch.Calisan)
                .Where(ch => ch.HizmetId == service.Id)
                .Select(ch => ch.Calisan)
                .ToListAsync();

            if (!suitableEmployees.Any())
            {
                return (false, $"'{service.Ad}' hizmetini verebilecek hiç çalışan yok.");
            }

            Calisan? selectedEmployee = null;

            // b. İstenen saatte Vardiya ve Randevu Çakışma Kontrolü
            foreach (var employee in suitableEmployees)
            {
                var serviceDurationInMinutes = service.Sure; // Artık bu alan adının 'Sure' olduğunu biliyoruz.
                var appointmentEndTime = requestedDateTime.AddMinutes(serviceDurationInMinutes); // Çakışma kontrolü için bitiş zamanını hesaplayın.

                // 1. Randevuları çek: Sadece aynı günküleri çekerek performansı artır
                var existingAppointments = await _context.Randevular
                    .Include(r => r.Hizmet)
                    .Where(r => r.CalisanId == employee.Id && r.TarihSaat.Date == requestedDate.Date)
                    .ToListAsync();

                // 2. Çakışma kontrolünü C#'ta yap (Bu, EF Core hatasını çözen yöntemdir)
                bool isConflicted = existingAppointments.Any(r => {
                    // Var olan randevunun bitiş zamanını hesapla (r.Hizmet.Sure'nin doğru olduğunu varsayıyoruz)
                    var existingAppointmentEndTime = r.TarihSaat.AddMinutes(r.Hizmet.Sure);

                    // Çakışma mantığı: Yeni randevu, var olan randevu süresi içine mi düşüyor?
                    return (r.TarihSaat < appointmentEndTime && existingAppointmentEndTime > requestedDateTime);
                });

                // Vardiya Kontrolü (Şimdilik True kabul edelim.)
                bool isOnShift = true;

                if (!isConflicted && isOnShift)
                {
                    selectedEmployee = employee;
                    break; // Müsait ilk çalışanı bulduk, döngüyü sonlandır.
                }
            }

            if (selectedEmployee == null)
            {
                return (false, $"İstenen tarih ve saatte ({requestedDateTime:dd/MM HH:mm}) uygun çalışan bulunamadı. Lütfen başka bir zaman deneyin.");
            }

            // 4. Randevuyu Oluştur ve Kaydet
            var newAppointment = new Randevu
            {
                MusteriId = currentUserId,
                CalisanId = selectedEmployee.Id,
                HizmetId = service.Id,
                TarihSaat = requestedDateTime,
                Durum = OnayDurumu.Bekliyor // Varsayılan olarak "Bekliyor" durumunda başlar
            };

            _context.Randevular.Add(newAppointment);
            await _context.SaveChangesAsync();

            return (true, $"{service.Ad} randevunuz {selectedEmployee.AdSoyad} ile {requestedDateTime:dd MMMM yyyy HH:mm} tarihinde başarıyla **BEKLEYEN** duruma alınmıştır.");
        }
    }
}