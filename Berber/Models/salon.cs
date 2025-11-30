using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Berber.Models
{
    public class Salon
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Salon adı boş bırakılamaz.")]
        [StringLength(100)]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Adres boş bırakılamaz.")]
        [StringLength(250)]
        public string Adres { get; set; }

        [Required(ErrorMessage = "Çalışma saatleri boş bırakılamaz.")]
        [StringLength(50)]
        public string CalismaSaatleri { get; set; }

        
        public virtual ICollection<Calisan>? Calisanlar { get; set; }
        public virtual ICollection<Hizmet>? Hizmetler { get; set; }
    }
}