using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Models;
using Microsoft.AspNetCore.Http; // HttpContext.Session için
using System.Diagnostics; // 'Activity' sınıfı için

namespace SikayetAIWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [ResponseCache(NoStore = true, Duration = 0)] // Bu öznitelik, sayfanın önbelleğe alınmamasını sağlar.
        public IActionResult Index()
        {
            return View();
        }

        // Profili Görüntüle sayfası
        public IActionResult ProfileView()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        // Profil Ayarları sayfası (Profili Güncelle olarak kullanılacak)
        public IActionResult ProfileSettings()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        // Şikayetlerim sayfası
        public IActionResult MyComplaints()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
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
    }
}