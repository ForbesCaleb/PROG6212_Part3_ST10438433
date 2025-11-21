using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POE_Part2_PROG6212.Data;
using POE_Part2_PROG6212.Models;

// Fix model aliasing
using DbClaim = POE_Part2_PROG6212.Models.Claim;

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
            ViewBag.TotalLecturers = await _db.Users.CountAsync(u => u.Role == "Lecturer");

            ViewBag.TotalApprovedClaims = await _db.Claims
                .CountAsync(c => c.Status == ClaimStatus.Approved);

            ViewBag.TotalApprovedAmount = await _db.Claims
                .Where(c => c.Status == ClaimStatus.Approved)
                .SumAsync(c => c.TotalAmount);

            return View("~/Views/Hr/Index.cshtml");
        }

        // ========================= MANAGE LECTURERS =========================
        public async Task<IActionResult> Lecturers()
        {
            var lecturers = await _db.Users
                .Where(u => u.Role == "Lecturer")
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View("~/Views/Hr/Lecturers.cshtml", lecturers);
        }

        // --------------------- CREATE LECTURER (GET) ---------------------
        [HttpGet]
        public IActionResult CreateLecturer()
        {
            return View("~/Views/Hr/CreateLecturer.cshtml");
        }

        // --------------------- CREATE LECTURER (POST) ---------------------
        [HttpPost]
        public async Task<IActionResult> CreateLecturer(AppUser user)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Hr/CreateLecturer.cshtml", user);

            user.Role = "Lecturer"; // force correct role

            // If no password supplied, default to username
            if (string.IsNullOrWhiteSpace(user.Password))
                user.Password = user.Username;

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Lecturer created successfully.";
            return RedirectToAction("Lecturers");
        }

        // --------------------- EDIT LECTURER (GET) ---------------------
        [HttpGet]
        public async Task<IActionResult> EditLecturer(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null || user.Role != "Lecturer")
                return NotFound();

            return View("~/Views/Hr/EditLecturer.cshtml", user);
        }

        // --------------------- EDIT LECTURER (POST) ---------------------
        [HttpPost]
        public async Task<IActionResult> EditLecturer(AppUser updated)
        {
            var user = await _db.Users.FindAsync(updated.Id);

            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View("~/Views/Hr/EditLecturer.cshtml", updated);

            user.FullName = updated.FullName;
            user.Username = updated.Username;
            user.HourlyRate = updated.HourlyRate;

            // Update password ONLY if provided
            if (!string.IsNullOrWhiteSpace(updated.Password))
                user.Password = updated.Password;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Lecturer updated successfully.";
            return RedirectToAction("Lecturers");
        }

        // ========================= INVOICE SUMMARY =========================
        public async Task<IActionResult> InvoiceSummary()
        {
            var result = await _db.Claims
                .Where(c => c.Status == ClaimStatus.Approved)
                .GroupBy(c => c.UserId)
                .Select(g => new LecturerInvoiceSummary
                {
                    UserId = g.Key,
                    LecturerName = g.First().User.FullName,
                    TotalHours = g.Sum(c => c.HoursWorked),
                    TotalAmount = g.Sum(c => c.TotalAmount)
                })
                .OrderBy(r => r.LecturerName)
                .ToListAsync();

            return View("~/Views/Hr/InvoiceSummary.cshtml", result);
        }

        // ========================= LECTURER CLAIMS =========================
        public async Task<IActionResult> LecturerClaims(int id)
        {
            var claims = await _db.Claims
                .Where(c => c.UserId == id && c.Status == ClaimStatus.Approved)
                .OrderByDescending(c => c.DateWorked)
                .ToListAsync();

            return View("~/Views/Hr/LecturerClaims.cshtml", claims);
        }
    }
}
