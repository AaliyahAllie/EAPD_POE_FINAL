using GLMS.Web.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GLMS.Tests
{
    public class CurrencyServiceTests
    {
        [Fact]
        public void ConvertUsdToZar_ShouldReturnCorrectAmount()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "CurrencyApi:BaseUrl", "https://test.com" }
                })
                .Build();

            var service = new CurrencyService(new HttpClient(), configuration);

            var result = service.ConvertUsdToZar(100m, 18.50m);

            Assert.Equal(1850.00m, result);
        }

        [Fact]
        public void ConvertUsdToZar_ShouldThrowError_WhenUsdAmountIsNegative()
        {
            var configuration = new ConfigurationBuilder().Build();
            var service = new CurrencyService(new HttpClient(), configuration);

            Assert.Throws<InvalidOperationException>(() =>
                service.ConvertUsdToZar(-10m, 18.50m));
        }

        [Fact]
        public void ConvertUsdToZar_ShouldThrowError_WhenExchangeRateIsZero()
        {
            var configuration = new ConfigurationBuilder().Build();
            var service = new CurrencyService(new HttpClient(), configuration);

            Assert.Throws<InvalidOperationException>(() =>
                service.ConvertUsdToZar(100m, 0m));
        }
    }
}