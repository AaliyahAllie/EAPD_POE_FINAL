using System.Text.Json;

namespace GLMS.Web.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CurrencyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate)
        {
            if (usdAmount < 0)
                throw new InvalidOperationException("USD amount cannot be negative.");

            if (exchangeRate <= 0)
                throw new InvalidOperationException("Exchange rate must be greater than zero.");

            return Math.Round(usdAmount * exchangeRate, 2);
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            try
            {
                var url = _configuration["CurrencyApi:BaseUrl"];
                var response = await _httpClient.GetStringAsync(url);

                using var document = JsonDocument.Parse(response);

                return document.RootElement
                    .GetProperty("rates")
                    .GetProperty("ZAR")
                    .GetDecimal();
            }
            catch
            {
                return 18.50m;
            }
        }
    }
}