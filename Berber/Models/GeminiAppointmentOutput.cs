using System.Text.Json.Serialization;

namespace Berber.Models
{
    // LLM'den beklediğimiz JSON çıktısının yapısı
    public class GeminiAppointmentOutput
    {
        [JsonPropertyName("hizmetAdi")]
        public string HizmetAdi { get; set; } = string.Empty;

        [JsonPropertyName("calisanAdi")]
        public string CalisanAdi { get; set; } = string.Empty;

        [JsonPropertyName("tarih")]
        // Tarihi YYYY-MM-DD formatında alacağız.
        public string Tarih { get; set; } = string.Empty;

        [JsonPropertyName("saat")]
        // Saati HH:MM formatında alacağız.
        public string Saat { get; set; } = string.Empty;

        [JsonPropertyName("basariliMi")]
        public bool BasariliMi { get; set; } = false;

        [JsonPropertyName("hataMesaji")]
        // Başarısız olursa, nedeni buraya yazılacak.
        public string HataMesaji { get; set; } = string.Empty;
    }
}