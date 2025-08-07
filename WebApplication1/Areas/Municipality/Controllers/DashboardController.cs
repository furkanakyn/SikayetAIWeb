using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using SikayetAIWeb.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SikayetAIWeb.Areas.Municipality.Controllers
{
    [Area("Municipality")]
    [Authorize(Roles = "municipality")] // Sadece belediye çalışanları bu alana girebilir
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Giriş yapan kullanıcının ID'sini al
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdString);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.DepartmentId == null)
            {
                return Forbid(); // Departmanı olmayan kullanıcılar için erişimi reddet
            }

            // Kullanıcının departmanına ait bekleyen şikayetleri al
            var newComplaints = await _context.Complaints
                                              .Where(c => c.AssignedDepartmentId == user.DepartmentId && c.Status == ComplaintStatus.pending)
                                              .ToListAsync();

            // Dashboard için bir ViewModel oluştur
            var viewModel = new MunicipalityDashboardViewModel
            {
                TotalComplaintsCount = newComplaints.Count,
                RecentComplaints = newComplaints
            };

            return View(viewModel);
        }
    }
}