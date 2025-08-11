using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    [Table("departments")]
    public class Department
    {
        [Column("department_id")]
        public int DepartmentId { get; set; } 
        
        [Column("department_name")]
        public string DepartmentName { get; set; }
        
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
