using System.Text.Json.Serialization;

namespace Berber.Models
{
    public class GeminiAppointmentOutput
    {
        [JsonPropertyName("hizmetAdi")]
        public string HizmetAdi { get; set; } = string.Empty;

        [JsonPropertyName("calisanAdi")]
        public string CalisanAdi { get; set; } = string.Empty;

        [JsonPropertyName("tarih")]
        public string Tarih { get; set; } = string.Empty;

        [JsonPropertyName("saat")]
        public string Saat { get; set; } = string.Empty;

        [JsonPropertyName("basariliMi")]
        public bool BasariliMi { get; set; } = false;

        public string CalisanAd { get; set; }

        [JsonPropertyName("hataMesaji")]
        public string HataMesaji { get; set; } = string.Empty;
    }
}