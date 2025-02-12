using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BamboCard.Application.Contract;
using BamboCard.EF.Model;

public class OpenExchangeRatesProvider : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "YOUR_OPENEXCHANGE_API_KEY";

    public OpenExchangeRatesProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

  

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        string apiUrl = $"https://openexchangerates.org/api/latest.json?app_id={_apiKey}&base={fromCurrency}";
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to fetch exchange rate.");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement.GetProperty("rates").GetProperty(toCurrency).GetDecimal();
    }


    public Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        throw new NotImplementedException();
    }


    public Task<PaginatedResponse<Dictionary<string, Dictionary<string, decimal>>>> GetHistoricalExchangeRatesAsync(string baseCurrency, string startDate, string endDate, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<ExchangeRate> GetLatestExchangeRate(string baseCurrency, string targetCurrency)
    {
        throw new NotImplementedException();
    }
}
