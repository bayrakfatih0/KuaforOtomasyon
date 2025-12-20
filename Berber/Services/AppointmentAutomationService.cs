using Berber.Data;
using Berber.Models;
using Microsoft.EntityFrameworkCore;

namespace Berber.Services
{
    public class AppointmentAutomationService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentAutomationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AttemptBookingAsync(GeminiAppointmentOutput input, string userId)
        {
            try
            {
                var service = await _context.Hizmetler
                    .FirstOrDefaultAsync(h => h.Ad.ToLower().Contains(input.HizmetAdi.ToLower()));

                if (service == null)
                    return (false, $"'{input.HizmetAdi}' isimli hizmeti bulamadım.");

                if (!DateTime.TryParse($"{input.Tarih} {input.Saat}", out DateTime requestedDateTime))
                    return (false, "Geçersiz tarih veya saat formatı.");

                var appointmentEndTime = requestedDateTime.AddMinutes(service.Sure);

                Calisan selectedEmployee = null;

                if (!string.IsNullOrEmpty(input.CalisanAd))
                {
                    selectedEmployee = await _context.Calisanlar
                        .FirstOrDefaultAsync(c => c.AdSoyad.ToLower().Contains(input.CalisanAd.ToLower()));

                    if (selectedEmployee == null)
                        return (false, $"Maalesef '{input.CalisanAd}' isminde bir çalışanımız bulunmuyor.");

                    if (await IsConflicted(selectedEmployee.Id, requestedDateTime, appointmentEndTime))
                        return (false, $"{selectedEmployee.AdSoyad} o saatte dolu. Başka bir zaman seçebilir misiniz?");
                }
                else
                {
                    var allEmployees = await _context.Calisanlar.ToListAsync();
                    foreach (var emp in allEmployees)
                    {
                        if (!await IsConflicted(emp.Id, requestedDateTime, appointmentEndTime))
                        {
                            selectedEmployee = emp;
                            break;
                        }
                    }
                }

                if (selectedEmployee == null)
                    return (false, "Üzgünüm, istediğiniz saatte uygun bir çalışanımız bulunmuyor.");

                var newAppointment = new Randevu
                {
                    MusteriId = userId,
                    CalisanId = selectedEmployee.Id,
                    HizmetId = service.Id,
                    TarihSaat = requestedDateTime,
                    Durum = OnayDurumu.Bekliyor 
                };

                _context.Randevular.Add(newAppointment);
                await _context.SaveChangesAsync();

                return (true, $"{service.Ad} randevunuz {selectedEmployee.AdSoyad} ile {requestedDateTime:dd MMMM HH:mm} tarihine başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return (false, "Randevu oluşturulurken teknik bir hata oluştu: " + ex.Message);
            }
        }

        private async Task<bool> IsConflicted(int employeeId, DateTime start, DateTime end)
        {
            var existingAppointments = await _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.CalisanId == employeeId && r.TarihSaat.Date == start.Date)
                .ToListAsync();

            return existingAppointments.Any(r =>
            {
                var existingStart = r.TarihSaat;
                var existingEnd = r.TarihSaat.AddMinutes(r.Hizmet.Sure);

                return (start < existingEnd && existingStart < end);
            });
        }
    }
}