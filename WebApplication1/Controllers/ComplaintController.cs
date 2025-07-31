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

        // Şikayet oluşturma işlemi (POST) - async ve CategoryPredictionService kullanılır
        [HttpPost]
        public async Task<IActionResult> Create(Complaint model)  // Eğer ComplaintViewModel yoksa doğrudan Complaint kullanabilirsin
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                model.UserId = userId.Value;
                model.CreatedAt = DateTime.Now;

                // Yapay zeka servisi ile kategori tahmini
                model.Category = await _categoryService.PredictCategoryAsync(model.Description);

                _complaintService.CreateComplaint(model);

                return RedirectToAction("MyComplaints");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult MyComplaints()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

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
            if (userId == null)
                return RedirectToAction("Login", "Auth");

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
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var complaint = _complaintService.GetComplaintDetails(id);
            if (complaint == null)
                return NotFound();

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
