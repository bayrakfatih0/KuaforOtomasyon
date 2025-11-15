// using... satırlarına bunları eklediğinizden emin olun:
using Berber.Data;
using Berber.Data.Initializers;
using Berber.Models; // ApplicationUser için
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection string'i appsettings.json'dan al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. DbContext'i servislere ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Identity'yi servislere ekle
// Projemize ApplicationUser sınıfını ve IdentityRole (Admin, Musteri, Calisan)
// kullanacağımızı söylüyoruz.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    // Geliştirme aşamasında şifre kurallarını basit tutabiliriz
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 3;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Şifre sıfırlama vb. için


// MVC servislerini ekle (bu satır zaten vardır)
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
    try
    {
        logger.LogInformation("Başlangıç verisi (Seed) servisi çalıştırılıyor...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // DbInitializer sınıfımızdaki güncellenmiş metodu çağırıyoruz
        await DbInitializer.Initialize(context, userManager, roleManager, logger);
    }
    catch (Exception ex)
    {
        // Hata olursa konsola yazdır
        logger.LogError(ex, "Veritabanına başlangıç verisi eklenirken bir hata oluştu.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    // ... (diğer app.Use... kodları)
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Bu satırların sırası önemli
app.UseAuthorization();  // Authentication'dan sonra gelmeli

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity sayfaları için

app.Run();