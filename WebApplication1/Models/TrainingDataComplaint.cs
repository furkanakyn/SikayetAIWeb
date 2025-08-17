using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    [Table("trainingdatacomplaint")]
    public class TrainingDataComplaint
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("complainttext")]
        public string ComplaintText { get; set; }

        [Required]
        [StringLength(100)]
        [Column("category1")]
        public string Category1 { get; set; }

        [Column("category2")]
        public string Category2 { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("isexported")]
        public bool IsExported { get; set; } = false;
    }
}
