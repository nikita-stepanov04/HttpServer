namespace WebApp.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public UserRole Role { get; set; }

        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public DateOnly Birthdate { get; set; }
    }
}
