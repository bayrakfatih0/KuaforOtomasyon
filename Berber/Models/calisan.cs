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
        public string UzmanlikAlanlari { get; set; } 

        [Required(ErrorMessage = "Çalışanın bağlı olduğu salon seçilmelidir.")]
        [Display(Name = "Salon")]
        public int? SalonId { get; set; }

        [Required(ErrorMessage = "Çalışanın bağlı olduğu salon seçilmelidir.")]
        [Display(Name = "Salon")]
        [ValidateNever]
        public virtual Salon? Salon { get; set; }

        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public virtual ICollection<CalisanHizmet>? CalisanHizmetleri { get; set; }

        public virtual ICollection<CalisanUygunluk>? UygunlukZamanlari { get; set; }

        public virtual ICollection<Randevu>? Randevular { get; set; }
    }
}