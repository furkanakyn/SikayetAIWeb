using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    public enum UserType { citizen, municipality, admin }

    [Table("users")]
    public class User
    {
        [Column("user_id")]
        public int Id { get; set; }
        
        [Column("username")]
        public string Username { get; set; }
        
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("email")]
        public string Email { get; set; }
        
        [Column("full_name")]
        public string FullName { get; set; }
        
        [Column("user_type")]
        public UserType UserType { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        // Navigation properties
        public List<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}