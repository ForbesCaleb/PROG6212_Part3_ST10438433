using Microsoft.EntityFrameworkCore;
using POE_Part3_PROG6212.Models;

namespace POE_Part3_PROG6212.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store ClaimStatus as string
            modelBuilder.Entity<Claim>()
                .Property(c => c.Status)
                .HasConversion<string>();

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Claims)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Claim>()
                .HasMany(c => c.Documents)
                .WithOne(d => d.Claim)
                .HasForeignKey(d => d.ClaimId);

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }

    public static class ApplicationDbSeeder
    {
        public static void Seed(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Creates DB / tables if they don't exist (no migrations required)
            db.Database.EnsureCreated();

            if (db.Users.Any()) return; // already seeded

            var lecturer = new AppUser
            {
                Username = "lecturer",
                FullName = "Demo Lecturer",
                Role = "Lecturer",
                HourlyRate = 350m,
                Password = "lecturer"
            };
            var pc = new AppUser
            {
                Username = "pc1",
                FullName = "Programme Coordinator",
                Role = "ProgrammeCoordinator",
                HourlyRate = 0,
                Password = "pc1"
            };
            var am = new AppUser
            {
                Username = "am1",
                FullName = "Academic Manager",
                Role = "AcademicManager",
                HourlyRate = 0,
                Password = "am1"
            };
            var hr = new AppUser
            {
                Username = "hr",
                FullName = "HR Super User",
                Role = "HR",
                HourlyRate = 0,
                Password = "hr"
            };

            db.Users.AddRange(lecturer, pc, am, hr);
            db.SaveChanges();
        }
    }
}
