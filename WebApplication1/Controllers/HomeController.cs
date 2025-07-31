using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace SikayetAIWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ProfileView()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            return View();
        }

        public IActionResult ProfileSettings()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        public IActionResult UpdateProfile(string FullName, string Email, string currentPassword, string newPassword, string confirmNewPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Oturum süreniz dolmuş, lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }
            if (newPassword == currentPassword)
            {
                TempData["ErrorMessage"] = "Yeni şifre, mevcut şifre ile aynı olamaz.";
                return RedirectToAction("ProfileSettings");
            }

            try
            {
                // Kullanıcıyı veritabanından bul
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("ProfileSettings");
                }

                // Bilgileri güncelle
                user.FullName = FullName;
                user.Email = Email;

                _context.SaveChanges();

                // Session'ı güncelle
                HttpContext.Session.SetString("FullName", FullName);
                HttpContext.Session.SetString("Email", Email);

                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi!";
                return RedirectToAction("ProfileSettings");
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += " | Inner Exception: " + ex.InnerException.Message;

                TempData["ErrorMessage"] = "Profil güncellenirken bir hata oluştu: " + errorMessage;
                return RedirectToAction("ProfileSettings");
            }
        }

        public IActionResult MyComplaints()
        {
            
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (_context.Complaints == null)
            {
                TempData["ErrorMessage"] = "Şikayet tablosu bulunamadı.";
                return View(new List<Complaint>());
            }

            var complaints = _context.Complaints
                                       .Where(c => c.UserId == userId.Value)
                                       .OrderByDescending(c => c.CreatedAt)
                                       .ToList();

            
            return View(complaints);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // 🔹 SHA256 Hash Metodu
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
