namespace SikayetAIWeb.Models
{
    public enum ComplaintStatus { Pending, InProgress, Resolved, Rejected }

    public class Complaint
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int UserId { get; set; }

        // Navigation properties
        public User User { get; set; }
        public List<Response> Responses { get; set; } = new List<Response>();
    }
}