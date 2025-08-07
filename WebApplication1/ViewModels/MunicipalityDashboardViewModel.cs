using SikayetAIWeb.Models;
using System.Collections.Generic;

namespace SikayetAIWeb.ViewModels
{
    public class MunicipalityDashboardViewModel
    {
        public int TotalComplaintsCount { get; set; }
        public List<Complaint> RecentComplaints { get; set; } = new List<Complaint>();
    }
}