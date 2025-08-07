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
        public UserType UserType { get; set; } = UserType.citizen;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("department_id")]
        public int? DepartmentId { get; set; }
        // Navigation properties
        public List<Complaint> Complaints { get; set; } = new List<Complaint>();

        public List<Response> Responses { get; set; } = new List<Response>();
    }
}