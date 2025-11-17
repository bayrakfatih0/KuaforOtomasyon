// Gerekli kütüphaneleri en üste ekleyin
using Berber.Data; // Proje adınız neyse ona göre düzeltin
using Berber.Data.Initializers; // Proje adınız neyse ona göre düzeltin
using Berber.Models; // Proje adınız neyse ona göre düzeltin
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // CreateScope için
using Microsoft.Extensions.Logging; // ILogger için
using Microsoft.AspNetCore.Identity.UI.Services;
using Berber.Services; // Kendi proje adınıza göre düzeltin
using System; // Exception için

// --- 1. Builder (İnşaatçı) Oluşturuluyor ---
var builder = WebApplication.CreateBuilder(args);

// --- 2. Servisler Ekleniyor ---

// Connection string'i appsettings.json'dan al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// BİZİM DOĞRU VERİTABANI SINIFIMIZ: ApplicationDbContext
// 'BerberContext' için olası bir kaydı SİLİYORUZ. Sadece bu kalmalı.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity'yi servislere ekle (ŞİFRE KURALLARIYLA BİRLİKTE)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    // Geliştirme aşamasında şifre kurallarını basit tutuyoruz
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 3;
})
    // DOĞRU VERİTABANI SINIFINI BURADA DA KULLAN
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// MVC servislerini ekle
builder.Services.AddControllersWithViews();


// Razor Pages servisini ekle (Login/Register sayfaları için)
builder.Services.AddRazorPages();

// Sahte e-posta göndericimizi servislere ekliyoruz.
builder.Services.AddSingleton<IEmailSender, DummyEmailSender>();


// --- 3. Uygulama (app) İnşa Ediliyor ---
var app = builder.Build();

// --- 4. BAŞLANGIÇ VERİSİ (SEED DATA) KODU ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>(); // Bu hatayı çözmüştük
    try
    {
        logger.LogInformation("Başlangıç verisi (Seed) servisi çalıştırılıyor...");
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
// --- BAŞLANGIÇ VERİSİ BİTİŞ ---


// --- 5. Pipeline (Yönlendirme) Ayarları ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// --- 6. Uygulama Çalıştırılıyor ---
app.Run();