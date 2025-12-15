// Controllers/AIController.cs

using Berber.Models;
using Berber.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Kullanıcı ID'si için gerekli
// ... Diğer using'leriniz ...

namespace Berber.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly GeminiSchedulingService _geminiService;
        private readonly AppointmentAutomationService _bookingService; // Yeni bağımlılık

        // Constructor'ı güncelleyin
        public AIController(GeminiSchedulingService geminiService, AppointmentAutomationService bookingService)
        {
            _geminiService = geminiService;
            _bookingService = bookingService;
            // Artık DbContext'i doğrudan burada tutmamıza gerek yok, bookingService üzerinden kullanıyoruz.
        }

        // ... (Index action'ı aynı kalır) ...

        [HttpPost]
        public async Task<IActionResult> ProcessRequest(string requestText)
        {
            if (string.IsNullOrEmpty(requestText))
            {
                TempData["ErrorMessage"] = "Lütfen randevu isteğinizi yazın.";
                return RedirectToAction("Index", "Home");
            }

            // 1. Metni Yapay Zeka Servisine gönder ve yapılandırılmış çıktıyı al
            var result = await _geminiService.ParseRequestAsync(requestText);

            if (!result.BasariliMi)
            {
                // 2. Eğer LLM veriyi çıkaramazsa, hata mesajını göster.
                TempData["ErrorMessage"] = $"İsteğiniz çözümlenemedi: {result.HataMesaji}";
                return RedirectToAction("Index", "Home");
            }

            // 3. Randevu Otomasyon Servisini Çağır (Asıl İşlem)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giriş yapan kullanıcının ID'sini al

            var (success, message) = await _bookingService.AttemptBookingAsync(result, currentUserId);

            if (success)
            {
                // Randevu başarılıysa, başarı mesajını göster
                TempData["SuccessMessage"] = message;
            }
            else
            {
                // Randevu uygunluk nedeniyle başarısızsa, hata mesajını göster
                TempData["ErrorMessage"] = message;
            }

            // Sonuç ne olursa olsun, kullanıcıyı anasayfaya döndür.
            return RedirectToAction("Index", "Home");
        }
    }
}