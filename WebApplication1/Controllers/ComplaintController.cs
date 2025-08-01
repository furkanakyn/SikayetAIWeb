using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Models;
using SikayetAIWeb.Services;
using System.Threading.Tasks;

namespace SikayetAIWeb.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ComplaintService _complaintService;
        private readonly CategoryPredictionService _categoryService;

        public ComplaintController(ComplaintService complaintService, CategoryPredictionService categoryService)
        {
            _complaintService = complaintService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Create Post
        [HttpPost]
        public async Task<IActionResult> Create(Complaint model)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Auth");

                var complaint = new Complaint
                {
                    Title = model.Title,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    UserId = userId.Value
                };

                // Kategori tahmini yapıyoruz
                var categories = await _categoryService.PredictCategoriesAsync(model.Description);
                complaint.Category = categories.FirstOrDefault() ?? "Genel";

                _complaintService.CreateComplaint(complaint);

                return RedirectToAction("MyComplaints");
            }
            return View(model);
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
            if (userId == null) return RedirectToAction("Login", "Auth");

            var response = new Response
            {
                ComplaintId = complaintId,
                ResponderId = userId.Value,
                Message = message
            };

            _complaintService.AddResponse(response);
            return RedirectToAction("Details", new { id = complaintId });
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
