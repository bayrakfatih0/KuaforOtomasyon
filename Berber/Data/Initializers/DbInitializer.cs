using Berber.Models; // Proje adınız neyse ona göre düzeltin
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Berber.Data.Initializers // Proje adınız neyse ona göre düzeltin
{
    public static class DbInitializer
    {
        // Logger'ı (ApplicationDbContext) olarak bıraktık, bu doğru
        public static async Task Initialize(ApplicationDbContext context,
                                            UserManager<ApplicationUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            ILogger<ApplicationDbContext> logger)
        {
            try
            {
                // 1. Veritabanı migrasyonu (Aynı)
                await context.Database.MigrateAsync();
                logger.LogInformation("Veritabanı migrasyonu kontrol edildi/uygulandı...");

                // 2. Rolleri Oluştur (Aynı)
                string[] roleNames = { "Admin", "Calisan", "Musteri" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation($"'{roleName}' rolü oluşturuldu.");
                    }
                }

                // 3. YENİ MANTIK: Admin Kullanıcısını ve Rolünü Kontrol Et

                // Önce kullanıcıyı bul
                var adminUser = await userManager.FindByEmailAsync("admin@berber.com");

                // Eğer kullanıcı HİÇ yoksa, oluştur
                if (adminUser == null)
                {
                    logger.LogInformation("Varsayılan Admin kullanıcısı oluşturuluyor...");
                    var newAdminUser = new ApplicationUser
                    {
                        UserName = "admin@berber.com",
                        Email = "admin@berber.com",
                        Ad = "Sistem",
                        Soyad = "Yöneticisi",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(newAdminUser, "Sifre123!");

                    if (!result.Succeeded)
                    {
                        // Hata günlüğü (Aynı)
                        logger.LogError("HATA: Admin kullanıcısı oluşturulamadı.");
                        foreach (var error in result.Errors)
                        {
                            logger.LogError($" - {error.Description}");
                        }
                        return; // Kullanıcı oluşmadıysa devam etme
                    }

                    // Kullanıcı başarıyla oluşturulduysa,
                    // onu 'adminUser' değişkenine ata ki aşağıdaki kod çalışsın
                    adminUser = newAdminUser;
                }

                // BU BLOK ARTIK DIŞARIDA:
                // Bu noktada 'adminUser' ya bulundu ya da az önce oluşturuldu.
                // Şimdi rolde olup olmadığını kontrol et.

                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    // Kullanıcı 'Admin' rolünde değilse, şimdi ekle
                    logger.LogInformation("Admin kullanıcısı 'Admin' rolüne atanıyor...");
                    var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                    if (!roleResult.Succeeded)
                    {
                        logger.LogError("HATA: Admin kullanıcısı 'Admin' rolüne atanamadı.");
                        foreach (var error in roleResult.Errors)
                        {
                            logger.LogError($" - {error.Description}");
                        }
                    }
                }
                else
                {
                    // Bu logu görüyorsanız, veritabanını temizlememişsiniz demektir
                    logger.LogInformation("Admin kullanıcısı zaten 'Admin' rolünde.");
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DbInitializer.Initialize içinde beklenmedik bir hata oluştu.");
            }
        }
    }
}