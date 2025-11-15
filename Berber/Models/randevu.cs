using Berber.Models;
using System;

namespace Berber.Models
{
    // Onay durumunu yönetmek için bir enum (opsiyonel ama tavsiye edilir)
    public enum OnayDurumu
    {
        Bekliyor,
        Onaylandi,
        IptalEdildi
    }

    public class Randevu
    {
        public int Id { get; set; }
        public DateTime TarihSaat { get; set; }
        public OnayDurumu Durum { get; set; }

        // --- İlişkiler ---

        // 1. Randevuyu alan "Müşteri" (Identity tablosu ile ilişki)
        public string MusteriId { get; set; }
        public virtual ApplicationUser Musteri { get; set; }

        // 2. Randevunun alındığı "Çalışan"
        public int CalisanId { get; set; }
        public virtual Calisan Calisan { get; set; }

        // 3. Randevuda alınacak "Hizmet"
        public int HizmetId { get; set; }
        public virtual Hizmet Hizmet { get; set; }
    }
}