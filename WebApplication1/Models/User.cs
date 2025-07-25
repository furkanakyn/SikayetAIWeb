namespace SikayetAIWeb.Models
{
    public enum UserType { Citizen, Municipality, Admin }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        // Navigation properties
        public List<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}