using System.Text;
using System.Text.Json;
using Berber.Models;
using System.Net.Http;

namespace Berber.Services
{
    public class GeminiSchedulingService
    {
        // Google AI Studio'dan aldığınız API Key'i buraya yapıştırın
        private readonly string _apiKey = "AIzaSyCP1vUfg9XbggtL_qILOBHWpfic5Vpss2k";
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
        public async Task<GeminiAppointmentOutput> ParseRequestAsync(string userText)
        {
            using var client = new HttpClient();

            // AI'ya verilen Sistem Talimatı (Prompt Engineering)
            // Bu kısım AI'nın "canlı" ve "mantıklı" davranmasını sağlar.
            var prompt = $@"
            Sen bir kuaför salonunun akıllı asistanısın. Kullanıcının mesajı: '{userText}'
            Bugünün tarihi ve saati: {DateTime.Now:dd MMMM yyyy dddd, HH:mm}.

            GÖREVİN:
            1. Eğer kullanıcı sadece selam veriyorsa (merhaba, nasılsın vb.), BasariliMi: false yap ve HataMesaji kısmına nazik bir karşılama yaz.
            2. Eğer kullanıcı randevu almak istiyorsa, mesajdan 'HizmetAdi', 'Tarih' ve 'Saat' bilgilerini ayıkla.
            3. Eksik bilgi varsa (örneğin sadece 'saç kesimi' dedi ama zaman belirtmedi), BasariliMi: false yap ve HataMesaji kısmına kullanıcıdan eksik bilgiyi istemek için bir soru yaz.
            4. Eğer tüm bilgiler (Hizmet, Tarih, Saat) netse, BasariliMi: true yap.

            ÖNEMLİ KURALLAR:
            - Tarihi her zaman YYYY-MM-DD formatında döndür.
            - Saati her zaman HH:MM formatında döndür.
            - Yanıtını SADECE aşağıdaki JSON formatında ver, başka açıklama ekleme.

            ÇIKTI FORMATI:
            {{
              ""BasariliMi"": false,
              ""HizmetAdi"": """",
              ""Tarih"": """",
              ""Saat"": """",
              ""HataMesaji"": """"
            }}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            try
            {
                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // API İsteği
                var response = await client.PostAsync($"{_apiUrl}?key={_apiKey}", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    return new GeminiAppointmentOutput
                    {
                        BasariliMi = false,
                        HataMesaji = $"API Hatası ({response.StatusCode}): {errorDetail}"
                    };
                }

                // Gemini'den gelen ham yanıtı işle
                return ExtractJsonResponse(responseString);
            }
            catch (Exception ex)
            {
                return new GeminiAppointmentOutput { BasariliMi = false, HataMesaji = "Bir hata oluştu: " + ex.Message };
            }
        }

        private GeminiAppointmentOutput ExtractJsonResponse(string responseString)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseString);
                // Gemini API hiyerarşisinde metne ulaşma: candidates[0].content.parts[0].text
                var rawText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                // AI bazen yanıtı ```json ... ``` blokları içine alabilir, onları temizliyoruz
                var cleanJson = rawText.Replace("```json", "").Replace("```", "").Trim();

                return JsonSerializer.Deserialize<GeminiAppointmentOutput>(cleanJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new GeminiAppointmentOutput { BasariliMi = false, HataMesaji = "Üzgünüm, isteğinizi tam anlayamadım." };
            }
        }
    }
}