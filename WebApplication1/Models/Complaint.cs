using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    public enum ComplaintStatus { Pending, InProgress, Resolved, Rejected }
    
    [Table("complaints")]
    public class Complaint
    {
        [Key]
        [Column("complaint_id")]
        public int ComplaintId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("category")]
        public string Category { get; set; }
        public string? Category2 { get; set; }

        [Column("location")]
        public string? Location { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public List<Response> Responses { get; set; } = new List<Response>();

        [NotMapped]
        public ComplaintStatus StatusEnum
        {
            get => Enum.Parse<ComplaintStatus>(Status, true);
            set => Status = value.ToString().ToLower();
        }

    }
}