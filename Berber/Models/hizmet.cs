using Berber.Models;

namespace Berber.Models
{
    public class Hizmet
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public int Sure { get; set; } // Dakika cinsinden
        public decimal Ucret { get; set; }

        // --- İlişkiler ---
        // Bu hizmetin hangi salona ait olduğu (Çoklu salon desteği)
        public int SalonId { get; set; }
        public virtual Salon Salon { get; set; }

        // Bu hizmeti hangi çalışanların verebildiği (Çoka-Çok ilişki)
        public virtual ICollection<CalisanHizmet> CalisanHizmetleri { get; set; }
    }
}