namespace SikayetAIWeb.Models
{
    public class Response
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // Foreign keys
        public int ComplaintId { get; set; }
        public int ResponderId { get; set; }

        // Navigation properties
        public Complaint Complaint { get; set; }
        public User Responder { get; set; }
    }
}