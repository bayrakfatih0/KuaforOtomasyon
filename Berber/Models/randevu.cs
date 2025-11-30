using Berber.Models;
using System;

namespace Berber.Models
{
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

        public string MusteriId { get; set; }
        public virtual ApplicationUser Musteri { get; set; }

        public int CalisanId { get; set; }
        public virtual Calisan Calisan { get; set; }

        public int HizmetId { get; set; }
        public virtual Hizmet Hizmet { get; set; }
    }
}