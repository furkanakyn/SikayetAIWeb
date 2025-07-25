using Microsoft.AspNetCore.Mvc;

namespace SikayetAIWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var userType = HttpContext.Session.GetString("UserType");
                ViewBag.UserType = userType;
            }
            return View();
        }
    }
}