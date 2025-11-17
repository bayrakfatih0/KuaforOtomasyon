using Berber.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Berber.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100)]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Süre zorunludur.")]
        [Range(1, 480, ErrorMessage = "Süre 1 ile 480 dakika arasında olmalıdır.")]
        public int Sure { get; set; } // Dakika cinsinden

        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Column(TypeName = "decimal(18, 2)")] // Veritabanı tipini tekrar doğrulayalım
        [Range(0, 99999.99)]
        public decimal Ucret { get; set; }

        // --- İlişkiler ---
        // Bu hizmetin hangi salona ait olduğu (Çoklu salon desteği)
        [Required(ErrorMessage = "Hizmetin ait olduğu salon zorunludur.")]
        [Display(Name = "Salon")] // Formlarda "Salon" olarak görünmesi için
        public int SalonId { get; set; }

        [ForeignKey("SalonId")]
        public virtual Salon? Salon { get; set; }

        // Bu hizmeti hangi çalışanların verebildiği (Çoka-Çok ilişki)
        public virtual ICollection<CalisanHizmet>? CalisanHizmetleri { get; set; }
        public virtual ICollection<Randevu>? Randevular { get; set; }
    }
}