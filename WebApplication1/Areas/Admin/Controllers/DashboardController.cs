using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using SikayetAIWeb.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SikayetAIWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuthCookie", Roles = "admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            viewModel.TotalComplaints = await _context.Complaints.CountAsync();

            // Enum değerlerini string'e dönüştürürken küçük harfe çeviriyoruz
            viewModel.PendingComplaints = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.pending);
            viewModel.InProgressComplaints = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.in_progress);
            viewModel.ResolvedComplaints = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.resolved);
            viewModel.RejectedComplaints = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.rejected);

            viewModel.TotalUsers = await _context.Users.CountAsync();

            viewModel.RecentComplaints = await _context.Complaints
                .Include(c => c.User) // Şikayetlerle birlikte ilgili kullanıcıyı da getirir
                .OrderByDescending(c => c.CreatedAt) // En son eklenen şikayetleri en üstte gösterir
                .Take(5) // Sadece en son 5 şikayeti getirir
                .ToListAsync();

            return View(viewModel);
        }
    }
}