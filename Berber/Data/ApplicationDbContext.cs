using Berber.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Berber.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Calisan> Calisanlar { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<CalisanUygunluk> CalisanUygunluklari { get; set; }
        public DbSet<CalisanHizmet> CalisanHizmetleri { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CalisanHizmet>()
                .HasKey(ch => new { ch.CalisanId, ch.HizmetId });

            builder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<CalisanHizmet>()
                .HasOne(ch => ch.Hizmet) 
                .WithMany(h => h.CalisanHizmetleri) 
                .HasForeignKey(ch => ch.HizmetId) 
                .OnDelete(DeleteBehavior.NoAction); 

            builder.Entity<Randevu>()
                .HasOne(r => r.Calisan) 
                .WithMany(c => c.Randevular) 
                .HasForeignKey(r => r.CalisanId) 
                .OnDelete(DeleteBehavior.NoAction); 
        }
    }
}