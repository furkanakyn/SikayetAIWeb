using SikayetAIWeb.Models;
namespace SikayetAIWeb.ViewModels
{

    public class ComplaintEditViewModel
    {
        public int Id { get; set; }
        public ComplaintStatus Status { get; set; }
        public string? SolutionNote { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime ComplaintDate { get; set; }
        public int? AssignedDepartmentId { get; set; }
    }
}