using System.Collections.Generic;

namespace Berber.Models
{
    public class Salon
    {
        public int Id { get; set; } // Primary Key (Benzersiz ID)
        public string Ad { get; set; }
        public string Adres { get; set; }
        public string CalismaSaatleri { get; set; } // "09:00-18:00" gibi

        // --- İlişkiler ---
        // Bir salonun birden çok çalışanı olabilir
        public virtual ICollection<Calisan> Calisanlar { get; set; }

        // Bir salonun birden çok hizmeti olabilir
        public virtual ICollection<Hizmet> Hizmetler { get; set; }
    }
}