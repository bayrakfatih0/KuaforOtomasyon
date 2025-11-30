using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Berber.Models
{
    public class CalisanUygunluk
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Gün")]
        public DayOfWeek Gun { get; set; } 

        [Required]
        [DataType(DataType.Time)] 
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan BitisSaati { get; set; }

        public int CalisanId { get; set; }

        [ForeignKey("CalisanId")]
        public virtual Calisan? Calisan { get; set; }
    }
}