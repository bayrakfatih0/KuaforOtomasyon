using Berber.Models;
using System;

namespace Berber.Models
{
    public class CalisanUygunluk
    {
        public int Id { get; set; }

        // (Pazartesi=1, Salı=2... Pazar=7 veya DayOfWeek enum'u)
        public DayOfWeek Gun { get; set; }
        public TimeSpan BaslangicSaati { get; set; } // Örn: 09:00
        public TimeSpan BitisSaati { get; set; }   // Örn: 17:00

        // --- İlişkiler ---
        // Bu uygunluğun hangi çalışana ait olduğu
        public int CalisanId { get; set; }
        public virtual Calisan Calisan { get; set; }
    }
}