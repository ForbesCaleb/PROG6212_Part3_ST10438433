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

        // Available system roles
        private readonly List<string> _roles = new()
        {
            "Lecturer",
            "ProgrammeCoordinator",
            "AcademicManager",
            "HR"
        };

        public HrController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ===================== HR DASHBOARD =====================

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _db.Users.CountAsync();
            ViewBag.TotalLecturers = await _db.Users.CountAsync(u => u.Role == "Lecturer");

            ViewBag.TotalClaims = await _db.Claims.CountAsync();
            ViewBag.PendingClaims = await _db.Claims.CountAsync(c => c.Status == ClaimStatus.Submitted);
            ViewBag.ApprovedClaims = await _db.Claims.CountAsync(c => c.Status == ClaimStatus.Approved);
            ViewBag.RejectedClaims = await _db.Claims.CountAsync(c => c.Status == ClaimStatus.Rejected);

            ViewBag.RecentUsers = await _db.Users
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToListAsync();

            return View("~/Views/Hr/Index.cshtml");
        }

        // ===================== USERS LIST =====================

        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.OrderBy(u => u.Id).ToListAsync();
            return View("~/Views/Hr/Users.cshtml", users);
        }

        // ===================== CREATE USER =====================

        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = _roles;
            return View("~/Views/Hr/CreateUsers.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(AppUser model)
        {
            ViewBag.Roles = _roles;

            if (!ModelState.IsValid)
                return View("~/Views/Hr/CreateUsers.cshtml", model);

            _db.Users.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User created successfully.";
            return RedirectToAction("Users");
        }

        // ===================== EDIT USER =====================

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return RedirectToAction("Users");

            ViewBag.Roles = _roles;
            return View("~/Views/Hr/EditUsers.cshtml", user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(AppUser model)
        {
            ViewBag.Roles = _roles;

            if (!ModelState.IsValid)
                return View("~/Views/Hr/EditUsers.cshtml", model);

            _db.Users.Update(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("Users");
        }

        // ===================== DELETE USER =====================

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Users");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("Users");
        }

        // ===================== INVOICE SUMMARY (FIXED) =====================

        public async Task<IActionResult> InvoiceSummary()
        {
            var summary = await _db.Claims
                .Where(c => c.Status == ClaimStatus.Approved)
                .Include(c => c.User)
                .GroupBy(c => new { c.UserId, c.User.FullName })
                .Select(g => new LecturerInvoiceSummary
                {
                    UserId = g.Key.UserId,
                    LecturerName = g.Key.FullName,
                    TotalHours = g.Sum(c => c.HoursWorked),
                    TotalAmount = g.Sum(c => c.TotalAmount)
                })
                .OrderBy(s => s.LecturerName)
                .ToListAsync();

            return View("~/Views/Hr/InvoiceSummary.cshtml", summary);
        }

        // ===================== VIEW INDIVIDUAL LECTURER CLAIMS =====================

        public async Task<IActionResult> LecturerClaims(int id)
        {
            var claims = await _db.Claims
                .Where(c => c.UserId == id)
                .OrderByDescending(c => c.DateWorked)
                .ToListAsync();

            return View("~/Views/Hr/LecturerClaims.cshtml", claims);
        }
    }
}
