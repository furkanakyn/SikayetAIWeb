using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    [Table("category_departments")]
    public class CategoryDepartmentMapping
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("category_name")]
        public string CategoryName { get; set; } = null!;

        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Column("department_name")]
        public string DepartmentName { get; set; } = null!;

        // Navigation property
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
    }
}