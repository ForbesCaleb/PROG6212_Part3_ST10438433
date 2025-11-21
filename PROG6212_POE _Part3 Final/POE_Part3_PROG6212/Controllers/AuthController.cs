using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POE_Part2_PROG6212.Data;
using POE_Part2_PROG6212.Models;
using System.Security.Claims;

namespace POE_Part2_PROG6212.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AuthController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ================== LOGIN PAGE (GET) ==================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View("~/Views/Auth/Login.cshtml",
                new LoginViewModel { ReturnUrl = returnUrl });
        }

        // ================== LOGIN (POST) ==================
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Auth/Login.cshtml", model);

            // Find user by username
            var user = await _db.Users
                .SingleOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View("~/Views/Auth/Login.cshtml", model);
            }

            // Build Authentication Claims
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.FullName),
                new System.Security.Claims.Claim(ClaimTypes.GivenName, user.Username),
                new System.Security.Claims.Claim(ClaimTypes.Role, user.Role),
                new System.Security.Claims.Claim("HourlyRate", user.HourlyRate.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                });

            // Redirect to return URL if valid
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // ================== LOGOUT ==================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ================== ACCESS DENIED ==================
        public IActionResult Denied()
        {
            return View("~/Views/Auth/Denied.cshtml");
        }
    }
}
