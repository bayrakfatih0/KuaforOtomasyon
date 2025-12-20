// Controllers/AIController.cs

using Berber.Models;
using Berber.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Kullanıcı ID'si için gerekli
// ... Diğer using'leriniz ...

namespace Berber.Controllers
{
    [Authorize(Roles ="Musteri")]
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

        [HttpPost]
        public async Task<JsonResult> ProcessLiveRequest(string requestText)
        {
            // 1. Gemini'den metni analiz etmesini iste
            var aiResult = await _geminiService.ParseRequestAsync(requestText);

            // DURUM A: Bilgiler henüz eksik (AI soru soruyor veya selamlıyor)
            if (!aiResult.BasariliMi)
            {
                return Json(new { message = aiResult.HataMesaji });
            }

            // DURUM B: Bilgiler TAM (Hizmet, Tarih, Saat belirlendi)
            try
            {
                // Kullanıcı ID'sini al (Authorize olduğu için User üzerinden gelir)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Randevu otomasyon servisini çağır (Daha önce yazdığımız servis)
                // Bu servis çalışan müsaitliğini kontrol eder ve kaydeder.
                var (isBooked, message) = await _bookingService.AttemptBookingAsync(aiResult, userId);

                if (isBooked)
                {
                    // Başarılıysa konfeti efekti veya şık bir onay mesajı gönderelim
                    return Json(new
                    {
                        success = true,
                        message = $"🎉 Harika! {message}"
                    });
                }
                else
                {
                    // Eğer o saatte uygun çalışan yoksa AI'nın bunu söylemesini sağla
                    return Json(new { message = $"Üzgünüm, {message}. Başka bir saat deneyebilir miyiz?" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = "Randevu kaydedilirken teknik bir sorun oluştu." });
            }
        }
    }
}