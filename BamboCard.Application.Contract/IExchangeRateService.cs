using BamboCard.EF.Model;

namespace BamboCard.Application.Contract
{
    public interface IExchangeRateService
    {
        Task<ExchangeRate> GetLatestExchangeRate(string baseCurrency, string targetCurrency);
        Task<PaginatedResponse<Dictionary<string, Dictionary<string, decimal>>>> GetHistoricalExchangeRatesAsync(
              string baseCurrency, string startDate, string endDate, int page, int pageSize);
        Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
