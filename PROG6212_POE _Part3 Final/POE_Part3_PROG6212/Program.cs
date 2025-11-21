using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using POE_Part2_PROG6212.Data;
using POE_Part2_PROG6212.Models;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// MVC
// ===========================
builder.Services.AddControllersWithViews();

// ===========================
// SQL Server + EF Core
// ===========================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ===========================
// IHttpContextAccessor (FIXES YOUR ERROR)
// ===========================
builder.Services.AddHttpContextAccessor();

// ===========================
// Cookie Authentication
// ===========================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";
        o.LogoutPath = "/Auth/Logout";
        o.AccessDeniedPath = "/Auth/Denied";
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// ===========================
// File Storage Service
// ===========================
builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();

var app = builder.Build();

// ===========================
// Seed Database (Optional)
// ===========================
ApplicationDbSeeder.Seed(app.Services);

// ===========================
// Error Handling
// ===========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ===========================
// Auto-redirect authenticated users away from homepage
// ===========================
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLowerInvariant();

    if (context.User.Identity?.IsAuthenticated == true &&
        (path == "/" || path == "/home" || path == "/home/index"))
    {
        context.Response.Redirect("/Dashboard/Index");
        return;
    }

    await next();
});

// ===========================
// Routes
// ===========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
