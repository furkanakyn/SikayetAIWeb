using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using SikayetAIWeb.ViewModels;
using SikayetAIWeb.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SikayetAIWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuthCookie", Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public UsersController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

     
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");
            return View();
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                }
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Bu email adresi zaten kayıtlı.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");
                    return View(model);
                }

                _authService.CreatePasswordHash(model.Password, out string passwordHash);

                var newUser = new User
                {
                    Username = model.Username,
                    PasswordHash = passwordHash,
                    Email = model.Email,
                    FullName = model.FullName,
                    UserType = model.UserType,
                    DepartmentId = model.DepartmentId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");
            return View(model);
        }

      
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Username = user.Username,
                UserType = user.UserType,
                DepartmentId = user.DepartmentId
            };

            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName", user.DepartmentId);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userToUpdate = await _context.Users.FindAsync(id);
                if (userToUpdate == null)
                {
                    return NotFound();
                }

               
                if (userToUpdate.Username != viewModel.Username && _context.Users.Any(u => u.Username == viewModel.Username))
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                }
                if (userToUpdate.Email != viewModel.Email && _context.Users.Any(u => u.Email == viewModel.Email))
                {
                    ModelState.AddModelError("Email", "Bu email adresi zaten kayıtlı.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName", viewModel.DepartmentId);
                    return View(viewModel);
                }

                if (!string.IsNullOrEmpty(viewModel.Password))
                {
                    _authService.CreatePasswordHash(viewModel.Password, out string passwordHash);
                    userToUpdate.PasswordHash = passwordHash;
                }

                userToUpdate.FullName = viewModel.FullName;
                userToUpdate.Email = viewModel.Email;
                userToUpdate.Username = viewModel.Username;
                userToUpdate.UserType = viewModel.UserType;
                userToUpdate.DepartmentId = viewModel.DepartmentId;
                userToUpdate.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(userToUpdate);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName", viewModel.DepartmentId);
            return View(viewModel);
        }

    
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}