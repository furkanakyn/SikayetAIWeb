using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Models;
using SikayetAIWeb.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly AuthService _authService;

        public AccountController(
            ApplicationDbContext context,
            ILogger<AccountController> logger,
            AuthService authService)
        {
            _context = context;
            _logger = logger;
            _authService = authService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(UserType.admin.ToString()))
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else if (User.IsInRole(UserType.municipality.ToString()))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Municipality" });
                }
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            [FromForm] string username,
            [FromForm] string password,
            [FromForm] string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Kullanıcı adı ve şifre gereklidir.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null || !_authService.VerifyPasswordHash(password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                if (user.UserType != UserType.admin && user.UserType != UserType.municipality)
                {
                    ModelState.AddModelError("", "Bu alana sadece admin ve belediye çalışanları girebilir.");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.UserType.ToString()),
                    new Claim("FullName", user.FullName)
                };

                var identity = new ClaimsIdentity(claims, "AdminAuthCookie");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    "AdminAuthCookie",
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    });

                user.LastLogin = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"{user.UserType} girişi başarılı: {username}");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    if (user.UserType == UserType.admin)
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Municipality" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş hatası: {Username}", username);
                ModelState.AddModelError("", "Giriş işlemi sırasında bir hata oluştu.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = "AdminAuthCookie")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;

            await HttpContext.SignOutAsync("AdminAuthCookie");

            _logger.LogInformation($"{username} çıkış yaptı.");

            Response.Cookies.Delete("AdminAuthCookie");

            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }
    }
}