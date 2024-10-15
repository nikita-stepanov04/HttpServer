namespace WebApp.Models
{
    public class AppUser
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public DateOnly Birthdate { get; set; }
    }
}
