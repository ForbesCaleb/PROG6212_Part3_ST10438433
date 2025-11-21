using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POE_Part2_PROG6212.Models
{
    public enum ClaimStatus
    {
        Draft,
        Submitted,
        UnderReview,
        Approved,
        Rejected
    }

    public class Claim
    {
        public int Id { get; set; }

        // Foreign key to AppUser (lecturer)
        public int UserId { get; set; }
        public AppUser User { get; set; } = default!;

        [DataType(DataType.Date)]
        public DateTime DateWorked { get; set; }

        [Range(0.25, 24)]
        public decimal HoursWorked { get; set; }

        [Required, MaxLength(60)]
        public string Activity { get; set; } = default!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;

        public string? Notes { get; set; }

        public List<ClaimDocument> Documents { get; set; } = new();
    }

    public class ClaimDocument
    {
        public int Id { get; set; }

        public int ClaimId { get; set; }
        public Claim Claim { get; set; } = default!;

        [Required]
        public string FileName { get; set; } = default!;

        [Required]
        public string RelativePath { get; set; } = default!;

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }
}
