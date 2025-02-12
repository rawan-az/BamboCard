using System.Net.Http;
using System.Text.Json;
using BamboCard.Application.Contract;
using BamboCard.EF.Model;
using Microsoft.Extensions.Caching.Memory;

public class FrankfurterCurrencyProvider : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private static readonly HashSet<string> ExcludedCurrencies = new() { "TRY", "PLN", "THB", "MXN" };
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public FrankfurterCurrencyProvider(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<ExchangeRate> GetLatestExchangeRate(string baseCurrency, string targetCurrency)
    {
        var response = await _httpClient.GetStringAsync($"https://api.frankfurter.app/latest?from={baseCurrency}&to={targetCurrency}");
        var data = JsonSerializer.Deserialize<JsonElement>(response);
        var rate = data.GetProperty("rates").GetProperty(targetCurrency).GetDecimal();

        return new ExchangeRate
        {
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            Rate = rate,
            Date = DateTime.UtcNow
        };
    }


    public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (ExcludedCurrencies.Contains(fromCurrency) || ExcludedCurrencies.Contains(toCurrency))
        {
            throw new ArgumentException("Conversion for the specified currency is not allowed.");
        }

        var exchangeRates = await GetExchangeRatesAsync(fromCurrency);
        if (!exchangeRates.ContainsKey(toCurrency))
        {
            throw new Exception("Invalid target currency.");
        }

        return amount * exchangeRates[toCurrency];
    }

    private async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency)
    {
        string apiUrl = $"https://api.frankfurter.app/latest?from={baseCurrency}";
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to fetch exchange rates.");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        var rates = doc.RootElement.GetProperty("rates");

        var exchangeRates = new Dictionary<string, decimal>();
        foreach (var rate in rates.EnumerateObject())
        {
            exchangeRates[rate.Name] = rate.Value.GetDecimal();
        }

        return exchangeRates;
    }


    public async Task<PaginatedResponse<Dictionary<string, Dictionary<string, decimal>>>> GetHistoricalExchangeRatesAsync(
           string baseCurrency, string startDate, string endDate, int page, int pageSize)
    {
        string cacheKey = $"exchange_rates_{baseCurrency}_{startDate}_{endDate}";

        if (!_cache.TryGetValue(cacheKey, out Dictionary<string, Dictionary<string, decimal>> allRates))
        {
            allRates = await FetchExchangeRatesFromAPI(baseCurrency, startDate, endDate);
            _cache.Set(cacheKey, allRates, CacheDuration);
        }

        return Paginate(allRates, page, pageSize);
    }

    private async Task<Dictionary<string, Dictionary<string, decimal>>> FetchExchangeRatesFromAPI(
        string baseCurrency, string startDate, string endDate)
    {
        string apiUrl = $"https://api.frankfurter.app/{startDate}..{endDate}?from={baseCurrency}";
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to fetch historical exchange rates.");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        var rates = doc.RootElement.GetProperty("rates");

        var allRates = new Dictionary<string, Dictionary<string, decimal>>();
        foreach (var dateEntry in rates.EnumerateObject())
        {
            var rateValues = new Dictionary<string, decimal>();
            foreach (var rate in dateEntry.Value.EnumerateObject())
            {
                rateValues[rate.Name] = rate.Value.GetDecimal();
            }
            allRates[dateEntry.Name] = rateValues;
        }

        return allRates;
    }

    private PaginatedResponse<Dictionary<string, Dictionary<string, decimal>>> Paginate(
        Dictionary<string, Dictionary<string, decimal>> data, int page, int pageSize)
    {
        var totalItems = data.Count;
        var paginatedData = new Dictionary<string, Dictionary<string, decimal>>();

        foreach (var item in data.Skip((page - 1) * pageSize).Take(pageSize))
        {
            paginatedData[item.Key] = item.Value;
        }

        return new PaginatedResponse<Dictionary<string, Dictionary<string, decimal>>>
        {
            TotalItems = totalItems,
            PageSize = pageSize,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Data = paginatedData
        };
    }

}
