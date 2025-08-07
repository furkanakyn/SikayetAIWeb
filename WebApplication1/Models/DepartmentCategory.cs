using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    [Table("departments")]
    public class DepartmentCategory
    {
        [Column("department_id")]
        public int DepartmentId { get; set; } 
        
        [Column("department_name")]
        public string DepartmentName { get; set; } 
    }
}
