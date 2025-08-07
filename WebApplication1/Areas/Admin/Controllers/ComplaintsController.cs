using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using SikayetAIWeb.ViewModels;

namespace SikayetAIWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuthCookie", Roles = "admin")]
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var complaints = await _context.Complaints
              .Include(c => c.User)
              .OrderByDescending(c => c.CreatedAt)
              .ToListAsync();
            return View(complaints);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complaint = await _context.Complaints
              .Include(c => c.User)
              .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
            {
                return NotFound();
            }

            var viewModelStatus = ComplaintStatus.pending;                                     

            var model = new ComplaintEditViewModel
            {
                Id = complaint.ComplaintId,
                Title = complaint.Title,
                Content = complaint.Description,
                ComplaintDate = complaint.CreatedAt,
                SolutionNote = complaint.SolutionNote,
                AssignedDepartmentId = complaint.AssignedDepartmentId,
                Status = viewModelStatus
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Status,SolutionNote,AssignedDepartmentId")] ComplaintEditViewModel complaintViewModel)
        {
            if (id != complaintViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var complaintToUpdate = await _context.Complaints.FindAsync(id);
                    if (complaintToUpdate == null)
                    {
                        return NotFound();
                    }

                    // ✅ KRİTİK DÜZELTME: Enum değerini string'e çevirip küçük harfe dönüştürüyoruz
                    complaintToUpdate.Status = complaintViewModel.Status;
                    complaintToUpdate.SolutionNote = complaintViewModel.SolutionNote;
                    complaintToUpdate.AssignedDepartmentId = complaintViewModel.AssignedDepartmentId;
                    complaintToUpdate.UpdatedAt = DateTime.UtcNow;

                    _context.Update(complaintToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComplaintExists(complaintViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(complaintViewModel);
        }

        private bool ComplaintExists(int id)
        {
            return _context.Complaints.Any(e => e.ComplaintId == id);
        }
    }
}