namespace GLMS.API.Services
{
    public interface ICurrencyService
    {
        decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate);
        Task<decimal> GetUsdToZarRateAsync();
    }
}
