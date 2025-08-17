using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using SikayetAIWeb.ViewModels;
using System.Text;
using System.Collections.Generic;

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

            var complaintsWithExportStatus = new List<ComplaintWithExportStatusViewModel>();
            var exportedComplaintTexts = await _context.TrainingDataComplaints
                .Where(tdc => tdc.IsExported)
                .Select(tdc => tdc.ComplaintText)
                .ToListAsync();

            foreach (var complaint in complaints)
            {
                var isExported = exportedComplaintTexts.Contains(complaint.Description);
                complaintsWithExportStatus.Add(new ComplaintWithExportStatusViewModel
                {
                    Complaint = complaint,
                    IsExported = isExported
                });
            }

            return View(complaintsWithExportStatus);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var complaint = await _context.Complaints
               .Include(c => c.AssignedDepartment)
               .Include(c => c.User)
               .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null)
                return NotFound();

            return View(complaint);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var complaint = await _context.Complaints
                .Include(c => c.AssignedDepartment)
                .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null)
                return NotFound();

            var model = new ComplaintEditViewModel
            {
                Id = complaint.ComplaintId,
                Title = complaint.Title,
                Content = complaint.Description,
                ComplaintDate = complaint.CreatedAt,
                SolutionNote = complaint.SolutionNote,
                AssignedDepartmentId = complaint.AssignedDepartmentId,
                Status = complaint.Status,
                AssignedDepartmentName = complaint.AssignedDepartment?.DepartmentName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Status,SolutionNote,AssignedDepartmentId")] ComplaintEditViewModel complaintViewModel)
        {
            if (id != complaintViewModel.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var complaintToUpdate = await _context.Complaints.FindAsync(id);
                    if (complaintToUpdate == null)
                        return NotFound();

                    bool isAlreadyInTrainingData = await _context.TrainingDataComplaints.AnyAsync(tdc => tdc.ComplaintText == complaintToUpdate.Description);

                    if (complaintViewModel.Status == ComplaintStatus.resolved && complaintToUpdate.Status != ComplaintStatus.resolved && !isAlreadyInTrainingData)
                    {
                        var trainingData = new TrainingDataComplaint
                        {
                            ComplaintText = complaintToUpdate.Description,
                            Category1 = complaintToUpdate.Category,
                            Category2 = complaintToUpdate.Category2,
                            CreatedAt = DateTime.UtcNow,
                            IsExported = false
                        };
                        _context.TrainingDataComplaints.Add(trainingData);
                    }

                    _context.Entry(complaintToUpdate).State = EntityState.Modified;
                    complaintToUpdate.Status = complaintViewModel.Status;
                    complaintToUpdate.SolutionNote = complaintViewModel.SolutionNote;
                    complaintToUpdate.AssignedDepartmentId = complaintViewModel.AssignedDepartmentId;
                    complaintToUpdate.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComplaintExists(complaintViewModel.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(complaintViewModel);
        }

        private bool ComplaintExists(int id)
        {
            return _context.Complaints.Any(e => e.ComplaintId == id);
        }

        [HttpGet]
        public async Task<IActionResult> ExportTrainingDataToCsv()
        {
            var trainingDataToExport = await _context.TrainingDataComplaints
                .Where(td => !td.IsExported)
                .Select(td => new
                {
                    td.Id,
                    td.ComplaintText,
                    td.Category1,
                    Category2 = td.Category2 ?? ""
                })
                .ToListAsync();

            if (!trainingDataToExport.Any())
            {
                return NotFound("Dışa aktarılacak yeni eğitim verisi bulunamadı.");
            }

            var csv = new StringBuilder();
            csv.AppendLine("şikayet_metni,kategori");

            foreach (var data in trainingDataToExport)
            {
                string complaintText = $"\"{data.ComplaintText.Replace("\"", "\"\"")}\"";
                string category = string.IsNullOrEmpty(data.Category2)
                    ? $"\"{data.Category1.Replace("\"", "\"\"")}\""
                    : $"\"{data.Category1.Replace("\"", "\"\"")},{data.Category2.Replace("\"", "\"\"")}\"";
                csv.AppendLine($"{complaintText},{category}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());

            // Dışa aktarılan verileri "dışa aktarıldı" olarak işaretler.
            var trainingDataIds = trainingDataToExport.Select(td => td.Id).ToList();

            // Buradaki sorguyu güncelledim
            var entitiesToUpdate = await _context.TrainingDataComplaints
                .Where(td => trainingDataIds.Contains(td.Id))
                .Select(td => new { td.Id, td.IsExported })
                .ToListAsync();

            foreach (var entity in entitiesToUpdate)
            {
                var trainingDataEntity = new TrainingDataComplaint { Id = entity.Id, IsExported = entity.IsExported };
                _context.TrainingDataComplaints.Attach(trainingDataEntity);
                trainingDataEntity.IsExported = true;
            }
            await _context.SaveChangesAsync();

            return File(csvBytes, "text/csv", "egitim_verileri.csv");
        }
    }

    public class ComplaintWithExportStatusViewModel
    {
        public Complaint Complaint { get; set; }
        public bool IsExported { get; set; }
    }
}