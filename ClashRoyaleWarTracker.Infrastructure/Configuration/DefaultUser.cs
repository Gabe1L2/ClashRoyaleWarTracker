namespace ClashRoyaleWarTracker.Infrastructure.Configuration
{
    public class DefaultUser
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; } = "Member"; // Default to Member role
    }
}