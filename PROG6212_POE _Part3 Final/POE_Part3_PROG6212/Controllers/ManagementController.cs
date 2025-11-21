using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POE_Part3_PROG6212.Data;
using POE_Part3_PROG6212.Models;

// Alias to avoid name conflict with System.Security.Claims
using DbClaim = POE_Part3_PROG6212.Models.Claim;

namespace POE_Part3_PROG6212.Controllers
{
    [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
    public class ManagementController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ManagementController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ========================= REVIEW CLAIMS LIST =========================
        public async Task<IActionResult> ReviewClaims()
        {
            var claims = await _db.Claims
                .Include(c => c.User)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View("~/Views/Management/ReviewClaims.cshtml", claims);
        }

        // ========================= APPROVE CLAIM =========================
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null)
                return NotFound();

            claim.Status = ClaimStatus.Approved;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Claim approved successfully.";
            return RedirectToAction("ReviewClaims");
        }

        // ========================= REJECT CLAIM =========================
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null)
                return NotFound();

            claim.Status = ClaimStatus.Rejected;

            await _db.SaveChangesAsync();

            TempData["Error"] = "Claim rejected.";
            return RedirectToAction("ReviewClaims");
        }
    }
}
