using Berber.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Berber.Data
{
    // ÖNEMLİ: DbContext değil, IdentityDbContext<ApplicationUser> kullanıyoruz.
    // Bu, hem bizim tablolarımızı hem de Identity'nin (Kullanıcı, Rol) tablolarını
    // aynı veritabanında yönetmesini sağlar.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Yukarıda "Models" klasöründe oluşturduğumuz her sınıfı
        // buraya DbSet olarak eklemeliyiz.
        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Calisan> Calisanlar { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<CalisanUygunluk> CalisanUygunluklari { get; set; }
        public DbSet<CalisanHizmet> CalisanHizmetleri { get; set; }


        // Çoka-çok ilişki için anahtar tanımlaması
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // CalisanHizmet tablosu için birleşik birincil anahtar (composite key)
            // (CalisanId + HizmetId) ikilisi benzersiz olmalı.
            builder.Entity<CalisanHizmet>()
                .HasKey(ch => new { ch.CalisanId, ch.HizmetId });

            builder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<CalisanHizmet>()
                .HasOne(ch => ch.Hizmet) // CalisanHizmet'in bir Hizmet'i vardır
                .WithMany(h => h.CalisanHizmetleri) // Hizmet'in birçok CalisanHizmet'i vardır
                .HasForeignKey(ch => ch.HizmetId) // Bağlantı anahtarı
                .OnDelete(DeleteBehavior.NoAction); // <-- İŞTE ÇÖZÜM BU SATIR

            builder.Entity<Randevu>()
                .HasOne(r => r.Calisan) // Randevu'nun bir Calisan'ı var
                .WithMany(c => c.Randevular) // Calisan'ın çok Randevu'su var
                .HasForeignKey(r => r.CalisanId) // Bağlantı anahtarı
                .OnDelete(DeleteBehavior.NoAction); // <-- YENİ KURALIMIZ BU
        }
    }
}