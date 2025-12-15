using Berber.Models;
using Microsoft.Extensions.Configuration; // IConfiguration için
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System;

// Hata veren using'leri kaldırdık: using Google.AI.GenerativeAI;
// Hata veren using'leri kaldırdık: using Google.AI.GenerativeAI.Types; 

namespace Berber.Services
{
    // Artık GenerativeModel yerine sadece temel servisimizi tutuyoruz.
    public class GeminiSchedulingService
    {
        private readonly IConfiguration _configuration;

        public GeminiSchedulingService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Gerçek API anahtarı kontrolü burada yapılabilir.
            var apiKey = configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                //throw new InvalidOperationException("Gemini API Key 'appsettings.json' dosyasında bulunamadı.");
                // Şimdilik sadece uyarı verelim ve simülasyona devam edelim.
            }
        }

        // Simülasyon: Metin çözümleme mantığı
        public async Task<GeminiAppointmentOutput> ParseRequestAsync(string naturalLanguageText)
        {
            // Bu kısım, gerçek bir LLM API çağrısının yerine geçer.
            // Amaç: Projenin Controller'ının beklendiği gibi çalışmasını sağlamak.

            await Task.Delay(100); // Asenkron çalışmayı taklit et

            var textLower = naturalLanguageText.ToLowerInvariant();

            // Örnek Simülasyon Senaryoları:
            if (textLower.Contains("saç kesimi") && (textLower.Contains("yarın") || textLower.Contains("yarın")))
            {
                // Başarılı senaryo: Saç Kesimi ve Yarın bulundu.
                return new GeminiAppointmentOutput
                {
                    BasariliMi = true,
                    HizmetAdi = "Saç Kesimi", // Veritabanındaki Hizmet Adı ile eşleşmeli
                    Tarih = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                    Saat = "15:00",
                    CalisanAdi = textLower.Contains("ali") ? "Ali Usta" : ""
                };
            }
            else if (textLower.Contains("boya") && textLower.Contains("pazartesi"))
            {
                // Başarılı senaryo 2
                return new GeminiAppointmentOutput
                {
                    BasariliMi = true,
                    HizmetAdi = "Boya",
                    Tarih = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"),
                    Saat = "10:30"
                };
            }
            else
            {
                // Başarısız senaryo: Gerekli veriler eksik.
                return new GeminiAppointmentOutput
                {
                    BasariliMi = false,
                    HataMesaji = "İstediğiniz Hizmet, Tarih veya Saat bilgisini net çıkaramadım. Lütfen isteğinizi detaylandırın."
                };
            }
        }
    }
}