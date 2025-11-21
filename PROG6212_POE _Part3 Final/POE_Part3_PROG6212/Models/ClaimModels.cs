using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace POE_Part2_PROG6212.Models
{
    // ---------- View model for lecturer form ----------
    public class SubmitClaimViewModel
    {
        [Required, DataType(DataType.Date)]
        [Display(Name = "Date worked")]
        public DateTime DateWorked { get; set; } = DateTime.Today;

        [Required, Range(0.25, 24,
            ErrorMessage = "You can only claim between 0.25 and 24 hours per day.")]
        [Display(Name = "Hours worked")]
        public decimal HoursWorked { get; set; } = 1m;

        [Required, StringLength(60)]
        [Display(Name = "Activity / task")]
        public string Activity { get; set; } = "Lecture";

        [Display(Name = "Hourly rate (from HR)")]
        [DataType(DataType.Currency)]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Total amount")]
        public decimal TotalAmount => Math.Round(HourlyRate * HoursWorked, 2);

        [Display(Name = "Supporting document")]
        public IFormFile? Document { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    // ---------- For file storage ----------
    public class SupportingDocumentDto
    {
        public Guid DocumentId { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = default!;
        public string RelativePath { get; set; } = default!;
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }

    // ---------- HR invoice summary ----------
    public class LecturerInvoiceSummary
    {
        public int UserId { get; set; }
        public string LecturerName { get; set; } = default!;
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
    }
}



