using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POE_Part2_PROG6212.Data;
using POE_Part2_PROG6212.Models;
using System.Globalization;
using System.Security.Claims;

// FIX: Claim model aliasing so no red lines
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

        // ================== DASHBOARD ==================
        public async Task<IActionResult> Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
            ViewBag.Role = role;

            // -------------------- LECTURER --------------------
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

            // -------------------- HR --------------------
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

            // -------------------- PC & AM --------------------
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

        // ================== SUBMIT CLAIM ==================
        [Authorize(Roles = "Lecturer")]
        [HttpGet]
        public IActionResult SubmitClaim()
        {
            var rateStr = User.FindFirst("HourlyRate")?.Value ?? "0";
            decimal.TryParse(rateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate);

            return View("~/Views/Dashboard/SubmitClaim.cshtml",
                new SubmitClaimViewModel
                {
                    HourlyRate = rate
                });
        }

        [Authorize(Roles = "Lecturer")]
        [HttpPost]
        public async Task<IActionResult> SubmitClaim(SubmitClaimViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var rateClaim = User.FindFirst("HourlyRate")?.Value ?? "0";
            decimal.TryParse(rateClaim, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate);
            model.HourlyRate = rate;

            if (!ModelState.IsValid)
                return View("~/Views/Dashboard/SubmitClaim.cshtml", model);

            // Validate 180h monthly limit
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

            var claim = new DbClaim
            {
                UserId = userId,
                DateWorked = model.DateWorked,
                HoursWorked = model.HoursWorked,
                Activity = model.Activity,
                HourlyRate = model.HourlyRate,
                TotalAmount = model.TotalAmount,
                Notes = model.Notes,
                Status = ClaimStatus.Submitted
            };

            // Save document if uploaded
            if (model.Document != null)
            {
                var saved = await _files.SaveAsync(model.Document);
                claim.Documents.Add(new DbClaimDocument
                {
                    FileName = saved.FileName,
                    RelativePath = saved.RelativePath,
                    UploadDate = saved.UploadDate
                });
            }

            _db.Claims.Add(claim);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Claim submitted successfully.";
            return RedirectToAction("Index");
        }

        // ================== MY CLAIMS ==================
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
