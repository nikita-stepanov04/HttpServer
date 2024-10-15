using WebToolkit.Models;

namespace WebApp.Models
{
    public class User
    {
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public UserRole Role { get; set; }
    }
}
