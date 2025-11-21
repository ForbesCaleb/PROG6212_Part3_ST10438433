using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POE_Part2_PROG6212.Data;
using POE_Part2_PROG6212.Models;
using System.Globalization;
using System.Security.Claims;

// Aliases
using DbClaim = POE_Part2_PROG6212.Models.Claim;
using DbClaimDocument = POE_Part2_PROG6212.Models.ClaimDocument;

namespace POE_Part2_PROG6212.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorage _files;

        public DashboardController(ApplicationDbContext db, IFileStorage files)
        {
            _db = db;
            _files = files;
        }

        // ==================== DASHBOARD HOME ====================
        public async Task<IActionResult> Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
            ViewBag.Role = role;

            // ========= LECTURER DASHBOARD =========
            if (role == "Lecturer")
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var query = _db.Claims.Where(c => c.UserId == userId);

                ViewBag.PendingCount = await query.CountAsync(c =>
                    c.Status == ClaimStatus.Submitted ||
                    c.Status == ClaimStatus.UnderReview);

                ViewBag.ApprovedCount = await query.CountAsync(c => c.Status == ClaimStatus.Approved);
                ViewBag.RejectedCount = await query.CountAsync(c => c.Status == ClaimStatus.Rejected);

                ViewBag.RecentClaims = await query
                    .OrderByDescending(c => c.SubmittedAt)
                    .Take(5)
                    .ToListAsync();

                return View("~/Views/Dashboard/Index.cshtml");
            }

            // ========= HR DASHBOARD =========
            if (role == "HR")
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

                return View("~/Views/Dashboard/Index.cshtml");
            }

            // ========= PC & AM DASHBOARD =========
            if (role == "ProgrammeCoordinator" || role == "AcademicManager")
            {
                ViewBag.PendingApproval = await _db.Claims.CountAsync(c =>
                    c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview);

                ViewBag.TotalClaims = await _db.Claims.CountAsync();

                ViewBag.RecentClaims = await _db.Claims
                    .Include(c => c.User)
                    .OrderByDescending(c => c.SubmittedAt)
                    .Take(5)
                    .ToListAsync();

                return View("~/Views/Dashboard/Index.cshtml");
            }

            return View("~/Views/Dashboard/Index.cshtml");
        }

        // ==================== SUBMIT CLAIM (GET) ====================
        [Authorize(Roles = "Lecturer")]
        [HttpGet]
        public IActionResult SubmitClaim()
        {
            var hourlyRateStr = User.FindFirst("HourlyRate")?.Value ?? "0";
            decimal.TryParse(hourlyRateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate);

            return View("~/Views/Dashboard/SubmitClaim.cshtml",
                new DbClaim
                {
                    HourlyRate = rate,
                    DateWorked = DateTime.Today
                });
        }

        // ==================== SUBMIT CLAIM (POST) ====================
        [Authorize(Roles = "Lecturer")]
        [HttpPost]
        public async Task<IActionResult> SubmitClaim([FromForm] DbClaim model, IFormFile? file)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.UserId = userId;

            // ====== FIX DECIMAL PARSING (comma issue "1,00" -> 1.00) ======
            var hoursRaw = Request.Form["HoursWorked"].ToString();
            decimal.TryParse(hoursRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var fixedHours);
            model.HoursWorked = fixedHours;

            // ====== Ensure HourlyRate comes from HR, not from the form ======
            var rateStr = User.FindFirst("HourlyRate")?.Value ?? "0";
            decimal.TryParse(rateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate);
            model.HourlyRate = rate;

            // ====== Monthly hour limit ======
            var monthlyHours = await _db.Claims
                .Where(c => c.UserId == userId &&
                            c.DateWorked.Year == model.DateWorked.Year &&
                            c.DateWorked.Month == model.DateWorked.Month)
                .SumAsync(c => c.HoursWorked);

            if (monthlyHours + model.HoursWorked > 180)
            {
                ModelState.AddModelError(nameof(model.HoursWorked),
                    $"You cannot exceed 180 hours per month. New total: {monthlyHours + model.HoursWorked}.");
                return View("~/Views/Dashboard/SubmitClaim.cshtml", model);
            }

            // ====== Calculate total ======
            model.TotalAmount = model.HoursWorked * model.HourlyRate;

            // ====== Save Supporting Document ======
            if (file != null)
            {
                var saved = await _files.SaveAsync(file);

                model.Documents.Add(new DbClaimDocument
                {
                    FileName = saved.FileName,
                    RelativePath = saved.RelativePath,
                    UploadDate = DateTime.UtcNow
                });
            }

            _db.Claims.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Claim submitted successfully.";
            return RedirectToAction("Index");
        }

        // ==================== MY CLAIMS ====================
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> MyClaims()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var claims = await _db.Claims
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View("~/Views/Dashboard/MyClaims.cshtml", claims);
        }
    }
}
