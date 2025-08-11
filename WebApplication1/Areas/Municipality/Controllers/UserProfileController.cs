using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Municipality.Controllers
{
    [Area("Municipality")]
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _context.Users
                          .Include(u => u.Department)
                          .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return NotFound("Kullanıcı profili bulunamadı.");
            }

            return View(user);
        }
    }
}