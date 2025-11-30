using Berber.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Berber.Data.Initializers 
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context,
                                            UserManager<ApplicationUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            ILogger<ApplicationDbContext> logger)
        {
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Veritabanı migrasyonu kontrol edildi/uygulandı...");

                string[] roleNames = { "Admin", "Calisan", "Musteri" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation($"'{roleName}' rolü oluşturuldu.");
                    }
                }

                var adminUser = await userManager.FindByEmailAsync("admin@berber.com");

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
                        logger.LogError("HATA: Admin kullanıcısı oluşturulamadı.");
                        foreach (var error in result.Errors)
                        {
                            logger.LogError($" - {error.Description}");
                        }
                        return; 
                    }

                    adminUser = newAdminUser;
                }

                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
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