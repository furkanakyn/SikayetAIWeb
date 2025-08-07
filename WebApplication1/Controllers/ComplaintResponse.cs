using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    // Bu modelin veritabanındaki "responses" tablosuna karşılık geldiğini belirtiyoruz.
    [Table("responses")]
    public class Response
    {
        [Key]
        [Column("response_id")]
        public int ResponseId { get; set; }

        [Column("complaint_id")]
        public int ComplaintId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; } 

        [Required]
        [Column("content")]
        public string Content { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Complaint? Complaint { get; set; }
        public User? User { get; set; } // Yanıtı yazan kullanıcıya ait navigasyon property'si
    }
}