using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GLMS.Tests
{
    public class FileValidationServiceTests
    {
        [Fact]
        public void ValidatePdf_ShouldAllowPdfFile()
        {
            var service = new FileValidationService();

            var file = new FormFile(
                new MemoryStream(new byte[] { 1, 2, 3 }),
                0,
                3,
                "file",
                "agreement.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            var exception = Record.Exception(() => service.ValidatePdf(file));

            Assert.Null(exception);
        }

        [Fact]
        public void ValidatePdf_ShouldRejectExeFile()
        {
            var service = new FileValidationService();

            var file = new FormFile(
                new MemoryStream(new byte[] { 1, 2, 3 }),
                0,
                3,
                "file",
                "malware.exe")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/x-msdownload"
            };

            Assert.Throws<InvalidOperationException>(() => service.ValidatePdf(file));
        }

        [Fact]
        public void ValidatePdf_ShouldRejectEmptyFile()
        {
            var service = new FileValidationService();

            var file = new FormFile(
                new MemoryStream(Array.Empty<byte>()),
                0,
                0,
                "file",
                "empty.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            Assert.Throws<InvalidOperationException>(() => service.ValidatePdf(file));
        }
    }
}