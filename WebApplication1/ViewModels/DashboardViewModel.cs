using SikayetAIWeb.Models;
using System.Collections.Generic;

namespace SikayetAIWeb.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalComplaints { get; set; }
        public int PendingComplaints { get; set; }
        public int InProgressComplaints { get; set; } 
        public int ResolvedComplaints { get; set; } 
        public int RejectedComplaints { get; set; } 
        public int TotalUsers { get; set; }
        public List<Complaint> RecentComplaints { get; set; } = new List<Complaint>();
    }
}