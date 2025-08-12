using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SikayetAIWeb.Areas.Municipality.Controllers
{
    [Area("Municipality")]
    [Authorize(Roles = "municipality")]
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintsController(ApplicationDbContext context)
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

            var relevantCategories = await _context.CategoryDepartmentMappings
                                                     .Where(m => m.DepartmentId == user.DepartmentId)
                                                     .Select(m => m.CategoryName)
                                                     .ToListAsync();

            if (!relevantCategories.Any())
            {
                return View(new List<Complaint>());
            }

           
            var complaints = await _context.Complaints
                                            .Include(c => c.User) 
                                            .Where(c => relevantCategories.Contains(c.Category) ||
                                                        (c.Category2 != null && relevantCategories.Contains(c.Category2)))
                                            .ToListAsync();

            return View(complaints);
        }

        public async Task<IActionResult> Details(int id)
        {
            var complaint = await _context.Complaints
                                             .Include(c => c.User)
                                             .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAndReply(int id, ComplaintStatus status, string? reply, string? solutionNote)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
            {
                return NotFound();
            }
            complaint.Status = status;
            complaint.Reply = reply;
            complaint.SolutionNote = solutionNote;
            complaint.UpdatedAt = DateTime.UtcNow;

            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}