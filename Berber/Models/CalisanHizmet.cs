using Berber.Models;

namespace Berber.Models
{
    public class CalisanHizmet
    {
        public int CalisanId { get; set; }
        public virtual Calisan Calisan { get; set; }

        public int HizmetId { get; set; }
        public virtual Hizmet Hizmet { get; set; }
    }
}