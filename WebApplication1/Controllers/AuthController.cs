using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Models;
using SikayetAIWeb.Services;
using System.Diagnostics;

namespace SikayetAIWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                var user = _authService.Login(username, password);

                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserType", user.UserType.ToString());
                HttpContext.Session.SetString("FullName", user.FullName);

                _logger.LogInformation($"User {username} logged in successfully");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new User());
        }

        [HttpPost]
        public IActionResult Register(User user, string password)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(user);
                }

                var registeredUser = _authService.Register(user, password);

                _logger.LogInformation($"New user registered: {user.Username}");

                // Otomatik giriş yap
                HttpContext.Session.SetInt32("UserId", registeredUser.Id);
                HttpContext.Session.SetString("Username", registeredUser.Username);
                HttpContext.Session.SetString("UserType", registeredUser.UserType.ToString());
                HttpContext.Session.SetString("FullName", registeredUser.FullName);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                ViewBag.Error = ex.Message;
                return View(user);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}