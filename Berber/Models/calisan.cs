using Berber.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Berber.Models
{
    public class Calisan
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Çalışan Adı/Soyadı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanları belirtilmelidir.")]
        [StringLength(200)]
        [Display(Name = "Uzmanlık Alanları")]
        public string UzmanlikAlanlari { get; set; } // "Renklendirme, Kesim"

        [Required(ErrorMessage = "Çalışanın bağlı olduğu salon seçilmelidir.")]
        [Display(Name = "Salon")]
        // 2. Bu çalışanın "salonu"
        public int? SalonId { get; set; }

        [Required(ErrorMessage = "Çalışanın bağlı olduğu salon seçilmelidir.")]
        [Display(Name = "Salon")]
        [ValidateNever]
        public virtual Salon? Salon { get; set; }

        // --- İlişkiler ---

        // 1. Bu çalışanın "kullanıcı" hesabı
        // (Identity tablosu ile ilişki)
        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }

        // 3. Bu çalışanın verebildiği "hizmetler" (Çoka-Çok)
        public virtual ICollection<CalisanHizmet>? CalisanHizmetleri { get; set; }

        // 4. Bu çalışanın "uygunluk" zamanları
        public virtual ICollection<CalisanUygunluk>? UygunlukZamanlari { get; set; }

        // 5. Bu çalışana ait "randevular"
        public virtual ICollection<Randevu>? Randevular { get; set; }
    }
}