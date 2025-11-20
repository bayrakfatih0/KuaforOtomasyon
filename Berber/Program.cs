// GEREKLİ KÜTÜPHANELER
using Berber.Data;
using Berber.Data.Initializers;
using Berber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims; // FindFirstValue için

// --- 1. Builder (Inşaatçı) Oluşturuluyor ---
var builder = WebApplication.CreateBuilder(args);

// --- 2. SERVİSLER (Configuration) ---

// Connection string'i appsettings.json'dan al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DBContext'i servislere ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity'yi servislere ekle (GLOBAL AUTHORIZATION ve COOKE AYARLARI)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Şifre Ayarları
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 3;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// KRİTİK YOL FİXİ: Yönlendirme yolları için ConfigureApplicationCookie kullanılır.


// MVC Servisleri ve GLOBAL YETKİLENDİRME FİLTRESİ
builder.Services.AddControllersWithViews();

// Razor Pages servisini ekle (Identity UI için)
builder.Services.AddRazorPages();


// --- 3. Uygulama (app) İnşa Ediliyor ---
var app = builder.Build();

app.Use(async (context, next) =>
{
    // Eğer gelen istek yanlış path'le başlıyorsa (yani /Account/Login'e gidiyorsa)
    if (context.Request.Path.StartsWithSegments("/Account/Login"))
    {
        // Onu doğru adres olan /Identity/Account/Login'e yönlendir
        context.Response.Redirect("/Identity/Account/Login");
        return;
    }
    // İstek doğruysa normal akışa devam et
    await next();
});

// --- 4. BAŞLANGIÇ VERİSİ (SEED DATA) KODU ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.Initialize(context, userManager, roleManager, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanına başlangıç verisi eklenirken bir hata oluştu.");
    }
}


// --- 5. PIPELINE (İstek Sırası) AYARLARI ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 1. Yönlendirme (Routing)
app.UseRouting();

// 2. Kimlik Doğrulama (Authentication) - KİM olduğunu belirler
app.UseAuthentication();

// 3. Yetki Kontrolü (Authorization) - NE YAPABİLECEĞİNE karar verir
app.UseAuthorization();

// 4. Endpoint'leri (Controller'ları) haritala
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// --- 6. Uygulama Çalıştırılıyor ---
app.Run();