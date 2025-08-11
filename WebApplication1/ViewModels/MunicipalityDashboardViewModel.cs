using SikayetAIWeb.Models;
using System.Collections.Generic;

namespace SikayetAIWeb.ViewModels
{
    public class MunicipalityDashboardViewModel
    {
        public int TotalComplaintsCount { get; set; }
        public int WaitingComplaintsCount { get; set; }
        public int InProgressComplaintsCount { get; set; }
        public int CompletedComplaintsCount { get; set; }

        public List<Complaint> RecentComplaints { get; set; } = new List<Complaint>();
    }
}