using Microsoft.AspNetCore.Http;

namespace POE_Part2_PROG6212.Models
{
    public interface IFileStorage
    {
        Task<SupportingDocumentDto> SaveAsync(IFormFile file);
    }

    public class LocalFileStorage : IFileStorage
    {
        private static readonly string[] Allowed = new[] { ".pdf", ".docx", ".xlsx" };
        private const long MaxBytes = 5 * 1024 * 1024;
        private readonly string _rootPath;

        public LocalFileStorage(IWebHostEnvironment env)
        {
            _rootPath = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(_rootPath);
        }

        public async Task<SupportingDocumentDto> SaveAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file selected.");

            if (file.Length > MaxBytes)
                throw new InvalidOperationException("File too large. Max 5MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Allowed.Contains(ext))
                throw new InvalidOperationException("Only .pdf, .docx, .xlsx allowed.");

            var name = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(_rootPath, name);
            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            return new SupportingDocumentDto
            {
                FileName = file.FileName,
                RelativePath = $"/uploads/{name}"
            };
        }
    }
}

