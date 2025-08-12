using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SikayetAIWeb.Models;
using SikayetAIWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SikayetAIWeb.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ComplaintService _complaintService;
        private readonly ILogger<ComplaintController> _logger;
        private readonly ApplicationDbContext _context; 

        public ComplaintController(
            ComplaintService complaintService,
            ILogger<ComplaintController> logger,
            ApplicationDbContext context) 
        {
            _complaintService = complaintService;
            _logger = logger;
            _context = context; 
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Complaint model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { error = "Kullanıcı oturumu bulunamadı" });
            }

            model.UserId = userId.Value;

            if (string.IsNullOrWhiteSpace(model.Category))
            {
                ModelState.AddModelError("Category", "Kategori alanı zorunludur");
            }

            ModelState.Remove("User");
            ModelState.Remove("Responses");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(new { errors });
            }

            try
            {
                model.User = null;
                model.Responses = null;

                model.Status = ComplaintStatus.pending;
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(model.Category2))
                {
                    model.Category2 = null;
                }

                if (string.IsNullOrWhiteSpace(model.Location))
                {
                    model.Location = null;
                }

                _complaintService.CreateComplaint(model);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şikayet oluşturulurken hata");
                return StatusCode(500, new
                {
                    error = "Şikayet kaydedilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                    detail = ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult MyComplaints()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var complaints = _complaintService.GetUserComplaints(userId.Value);
            return View(complaints);
        }

        [HttpGet]
        public IActionResult DepartmentComplaints()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (string.IsNullOrEmpty(userType) ||
                (userType != UserType.municipality.ToString().ToLower() &&
                 userType != UserType.admin.ToString().ToLower()))
            {
                return RedirectToAction("Login", "Auth");
            }

            
            var complaints = _complaintService.GetDepartmentComplaints(
                Enum.Parse<UserType>(userType, true),
                null); 

            return View(complaints);
        }

        [HttpPost]
        public IActionResult AddResponse(int complaintId, string message)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            try
            {
                var response = new Response
                {
                    ComplaintId = complaintId,
                    UserId = userId.Value,
                    Content = message,
                    CreatedAt = DateTime.UtcNow
                };

                _complaintService.AddResponse(response);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yanıt eklenirken hata");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var complaint = _complaintService.GetComplaintDetails(id);
            if (complaint == null) return NotFound();

            var userType = HttpContext.Session.GetString("UserType");
            if (complaint.UserId != userId.Value &&
                userType != UserType.municipality.ToString().ToLower() &&
                userType != UserType.admin.ToString().ToLower())
            {
                return Forbid();
            }

            var responses = _complaintService.GetComplaintResponses(id, userId.Value);
            ViewBag.Responses = responses;
            return View(complaint);
        }
    }
}