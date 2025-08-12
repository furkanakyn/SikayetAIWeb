using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using SikayetAIWeb.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SikayetAIWeb.Areas.Municipality.Controllers
{
    [Area("Municipality")]
    [Authorize(Roles = "municipality")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdString);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.DepartmentId == null)
            {
                return Forbid();
            }

            // Kullanıcının departman ID'sine göre ilgili kategorileri veritabanından çekiyoruz
            var relevantCategories = await _context.CategoryDepartmentMappings
                                                   .Where(m => m.DepartmentId == user.DepartmentId)
                                                   .Select(m => m.CategoryName)
                                                   .ToListAsync();

            if (!relevantCategories.Any())
            {
                return View(new MunicipalityDashboardViewModel());
            }

            var complaints = await _context.Complaints
                                           .Where(c => relevantCategories.Contains(c.Category) ||
                                                       (c.Category2 != null && relevantCategories.Contains(c.Category2)))
                                           .ToListAsync();

            var newComplaints = complaints.Where(c => c.Status == ComplaintStatus.pending).ToList();
            var inProgressComplaints = complaints.Where(c => c.Status == ComplaintStatus.in_progress).ToList();
            var completedComplaints = complaints.Where(c => c.Status == ComplaintStatus.resolved).ToList();

            var viewModel = new MunicipalityDashboardViewModel
            {
                TotalComplaintsCount = complaints.Count,
                WaitingComplaintsCount = newComplaints.Count,
                InProgressComplaintsCount = inProgressComplaints.Count,
                CompletedComplaintsCount = completedComplaints.Count,
                RecentComplaints = newComplaints.OrderByDescending(c => c.CreatedAt).Take(5).ToList()
            };

            return View(viewModel);
        }
    }
}