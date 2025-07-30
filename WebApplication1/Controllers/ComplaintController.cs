using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Models;
using SikayetAIWeb.Services;

namespace SikayetAIWeb.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ComplaintService _complaintService;

        public ComplaintController(ComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Complaint complaint)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            complaint.UserId = userId.Value;
            _complaintService.CreateComplaint(complaint);
            return RedirectToAction("MyComplaints");
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
                null); // Add category filter if needed

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

            // Check if user has access
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