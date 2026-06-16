namespace GLMS.Web.Services
{
    public class FileValidationService : IFileValidationService
    {
        private readonly string[] _allowedExtensions = { ".pdf" };

        public void ValidatePdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Please upload a signed agreement PDF.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only PDF files are allowed.");

            if (file.ContentType != "application/pdf")
                throw new InvalidOperationException("Invalid file type. Only PDF files are allowed.");
        }
    }
}