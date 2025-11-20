using Berber.Data; // Proje adınız
using Berber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync için
using System.Diagnostics;

namespace Berber.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // 1. Veritabanı bağlantısını ekliyoruz
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Bağlantıyı al
        }

        // 2. Ana Sayfa (Index) açıldığında çalışacak kod
        public async Task<IActionResult> Index()
        {
            // Veritabanındaki tüm salonları çekip View'a gönderiyoruz
            var salonlar = await _context.Salonlar.ToListAsync();
            return View(salonlar);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}