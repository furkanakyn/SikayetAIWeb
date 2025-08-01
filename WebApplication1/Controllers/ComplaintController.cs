using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Models;
using SikayetAIWeb.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SikayetAIWeb.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ComplaintService _complaintService;
        private readonly ILogger<ComplaintController> _logger;

        public ComplaintController(
            ComplaintService complaintService,
            ILogger<ComplaintController> logger)
        {
            _complaintService = complaintService;
            _logger = logger;
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

            // UserId'yi manuel olarak ekleyerek "User field required" hatasını çözüyoruz
            model.UserId = userId.Value;

            // Kategori validasyonu (sadece kategori1 zorunlu)
            if (string.IsNullOrWhiteSpace(model.Category))
            {
                ModelState.AddModelError("Category", "Kategori alanı zorunludur");
            }

            // Navigation property'leri ModelState validasyonundan çıkar
            ModelState.Remove("User");
            ModelState.Remove("Responses");

            // UserId alanını da çıkar (manuel olarak ayarladığımız için)
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
                // Navigation property'leri temizle
                model.User = null;
                model.Responses = null;

                // Gerekli alanları otomatik doldur
                model.Status = "pending";

                // Zaman damgalarını ayarla
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;

                // Kategori2 boşsa null yap
                if (string.IsNullOrWhiteSpace(model.Category2))
                {
                    model.Category2 = null;
                }

                // Şikayeti veritabanına kaydet
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
                (userType != UserType.municipality.ToString() &&
                 userType != UserType.admin.ToString()))
            {
                return RedirectToAction("Login", "Auth");
            }

            var complaints = _complaintService.GetDepartmentComplaints(
                Enum.Parse<UserType>(userType),
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
                    ResponderId = userId.Value,
                    Message = message,
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
                userType != UserType.municipality.ToString() &&
                userType != UserType.admin.ToString())
            {
                return Forbid();
            }

            var responses = _complaintService.GetComplaintResponses(id, userId.Value);
            ViewBag.Responses = responses;
            return View(complaint);
        }
    }
}