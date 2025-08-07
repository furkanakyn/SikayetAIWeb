using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    public enum ComplaintStatus { pending, in_progress, resolved, rejected }

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
        public string Title { get; set; } = null!;

        [Required]
        [Column("description")]
        public string Description { get; set; } = null!;

        [Required]
        [Column("category")]
        public string Category { get; set; } = null!;

        [Column("category2")]
        public string? Category2 { get; set; }

        [Column("location")]
        public string? Location { get; set; }

        [Column("status")]
        [EnumDataType(typeof(ComplaintStatus))]
        public ComplaintStatus Status { get; set; } = ComplaintStatus.pending;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } // Nullable değil

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } // Nullable değil
       
        [Column("assigned_department_id")]
        public int? AssignedDepartmentId { get; set; }

        [Column("solution_note")]
        public string? SolutionNote { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public List<Response> Responses { get; set; } = new List<Response>();

     
    }
}