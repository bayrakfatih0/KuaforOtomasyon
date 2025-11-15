using Berber.Models;
using System.Collections.Generic;

namespace Berber.Models
{
    public class Calisan
    {
        public int Id { get; set; }
        public string UzmanlikAlanlari { get; set; } // "Renklendirme, Kesim"

        // --- İlişkiler ---

        // 1. Bu çalışanın "kullanıcı" hesabı
        // (Identity tablosu ile ilişki)
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        // 2. Bu çalışanın "salonu"
        public int SalonId { get; set; }
        public virtual Salon Salon { get; set; }

        // 3. Bu çalışanın verebildiği "hizmetler" (Çoka-Çok)
        public virtual ICollection<CalisanHizmet> CalisanHizmetleri { get; set; }

        // 4. Bu çalışanın "uygunluk" zamanları
        public virtual ICollection<CalisanUygunluk> UygunlukZamanlari { get; set; }

        // 5. Bu çalışana ait "randevular"
        public virtual ICollection<Randevu> Randevular { get; set; }
    }
}