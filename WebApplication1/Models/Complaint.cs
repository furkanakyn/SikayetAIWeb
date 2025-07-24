using System;
using System.ComponentModel.DataAnnotations;

namespace SikayetAIWeb.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

