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
        private readonly ApplicationDbContext _db;
        public AuthController(AuthService authService, ILogger<AuthController> logger, ApplicationDbContext db)
        {
            _authService = authService;
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var user = _authService.Login(model.Username, model.Password);
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserType", user.UserType.ToString());
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Email", user.Email);
                _logger.LogInformation($"User {model.Username} logged in successfully");
                // Eğer kullanıcı belediye ise departman kontrolü yap
                if (user.UserType == UserType.municipality)
                {
                    var hasDepartment = _db.Departments
                      .Any(d => d.DepartmentId == user.DepartmentId);
                    if (!hasDepartment)
                    {
                        return RedirectToAction("SelectDepartment", "Department");
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user: {Username}", model.Username);
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı: " + ex.Message;
                return View(model);
            }
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    FullName = model.FullName,
                    UserType = UserType.citizen,

                };
                var registeredUser = _authService.Register(user, model.Password);

                _logger.LogInformation($"New user registered: {model.Username}");
                // Kayıt sonrası otomatik giriş yap
                HttpContext.Session.SetInt32("UserId", registeredUser.Id);
                HttpContext.Session.SetString("Username", registeredUser.Username);
                HttpContext.Session.SetString("UserType", registeredUser.UserType.ToString());
                HttpContext.Session.SetString("FullName", registeredUser.FullName);
                HttpContext.Session.SetString("UserEmail", registeredUser.Email);

                TempData["SuccessMessage"] = "Kayıt işleminiz başarıyla tamamlandı! Lütfen giriş yapın.";
                // Kayıt başarılı sonrası doğrudan Login sayfasına yönlendir
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for user: {Username}", model.Username);
                ViewBag.Error = ex.Message;
                return View(model);
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