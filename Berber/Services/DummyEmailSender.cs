// Gerekli kütüphaneyi ekliyoruz
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Berber.Services // Proje adınız neyse (KuaforOtomasyon vb.) onu kullanın
{
    // Bu, IEmailSender arayüzünü kullanan sahte servisimizdir.
    public class DummyEmailSender : IEmailSender
    {
        // IEmailSender bizden bu metodu yazmamızı istiyor.
        // İçi boş olacak, çünkü mail göndermeyeceğiz.
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Hiçbir şey yapma, sadece "tamamlandım" de.
            return Task.CompletedTask;
        }
    }
}