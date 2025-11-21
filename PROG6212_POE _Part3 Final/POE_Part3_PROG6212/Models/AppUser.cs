using System.ComponentModel.DataAnnotations;

namespace POE_Part3_PROG6212.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = default!;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = default!;

        // Lecturer, ProgrammeCoordinator, AcademicManager, HR
        [Required, MaxLength(40)]
        public string Role { get; set; } = default!;

        // Used only for lecturers
        [Range(0, 5000)]
        public decimal HourlyRate { get; set; }

        // OPTIONAL during EDIT
        // REQUIRED during CREATE (your controller already enforces this)
        [MaxLength(100)]
        public string? Password { get; set; }

        public List<Claim> Claims { get; set; } = new();
    }
}
