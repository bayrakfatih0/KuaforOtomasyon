using Microsoft.AspNetCore.Identity;
namespace Berber.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
    }
}
