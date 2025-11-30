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
        public int Sure { get; set; } 

        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 99999.99)]
        public decimal Ucret { get; set; }

        [Required(ErrorMessage = "Hizmetin ait olduğu salon zorunludur.")]
        [Display(Name = "Salon")] 
        public int SalonId { get; set; }

        [ForeignKey("SalonId")]
        public virtual Salon? Salon { get; set; }

        public virtual ICollection<CalisanHizmet>? CalisanHizmetleri { get; set; }
        public virtual ICollection<Randevu>? Randevular { get; set; }
    }
}