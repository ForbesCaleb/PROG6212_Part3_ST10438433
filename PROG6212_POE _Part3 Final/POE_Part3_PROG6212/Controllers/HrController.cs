using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POE_Part2_PROG6212.Data;
using POE_Part2_PROG6212.Models;

namespace POE_Part2_PROG6212.Controllers
{
    [Authorize(Roles = "HR")]
    public class HrController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HrController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ========================= HR DASHBOARD =========================
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _db.Users.CountAsync();
            ViewBag.TotalLecturers = await _db.Users.CountAsync(u => u.Role == "Lecturer");

            return View("~/Views/Hr/Index.cshtml");
        }

        // ========================= MANAGE ALL USERS =========================
        public async Task<IActionResult> Users(string? role)
        {
            var usersQuery = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
            {
                usersQuery = usersQuery.Where(u => u.Role == role);
            }

            ViewBag.SelectedRole = role ?? "All";

            ViewBag.Roles = new List<string>
            {
                "All",
                "Lecturer",
                "ProgrammeCoordinator",
                "AcademicManager",
                "HR"
            };

            var users = await usersQuery.OrderBy(u => u.FullName).ToListAsync();

            return View("~/Views/Hr/Users.cshtml", users);
        }

        // ========================= CREATE ANY USER =========================
        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = new List<string>
            {
                "Lecturer",
                "ProgrammeCoordinator",
                "AcademicManager",
                "HR"
            };

            return View("~/Views/Hr/CreateUser.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(AppUser user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string>
                {
                    "Lecturer",
                    "ProgrammeCoordinator",
                    "AcademicManager",
                    "HR"
                };
                return View("~/Views/Hr/CreateUser.cshtml", user);
            }

            if (string.IsNullOrWhiteSpace(user.Password))
                user.Password = user.Username; // default password

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User created successfully.";
            return RedirectToAction("Users");
        }

        // ========================= EDIT ANY USER =========================
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = new List<string>
            {
                "Lecturer",
                "ProgrammeCoordinator",
                "AcademicManager",
                "HR"
            };

            return View("~/Views/Hr/EditUser.cshtml", user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(AppUser model)
        {
            var user = await _db.Users.FindAsync(model.Id);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string>
                {
                    "Lecturer",
                    "ProgrammeCoordinator",
                    "AcademicManager",
                    "HR"
                };
                return View("~/Views/Hr/EditUser.cshtml", model);
            }

            user.FullName = model.FullName;
            user.Username = model.Username;
            user.Role = model.Role;
            user.HourlyRate = model.HourlyRate;

            if (!string.IsNullOrWhiteSpace(model.Password))
                user.Password = model.Password;

            await _db.SaveChangesAsync();

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("Users");
        }

        // ========================= DELETE USER =========================
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User deleted.";
            return RedirectToAction("Users");
        }
    }
}
