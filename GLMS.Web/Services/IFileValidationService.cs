namespace GLMS.Web.Services
{
    public interface IFileValidationService
    {
        void ValidatePdf(IFormFile file);
    }
}