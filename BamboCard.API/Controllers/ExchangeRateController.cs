using BamboCard.Application.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/exchangerates")]
public class ExchangeRateController : ControllerBase
{
    private readonly CurrencyProviderFactory _factory;
    public ExchangeRateController(CurrencyProviderFactory factory)
    {
        _factory = factory;
    }

    [HttpGet("GetLatestRate")]
    public async Task<IActionResult> GetLatestRate(string provider ,string baseCurrency, string targetCurrency)
    {
        var currencyProvider = _factory.GetProvider(provider);  
        var rate = await currencyProvider.GetLatestExchangeRate(baseCurrency, targetCurrency);
        return Ok(rate);
    }



    [HttpGet("ConvertCurrency")]
    public async Task<IActionResult> ConvertCurrency(string provider,
        [FromQuery] decimal amount,
        [FromQuery] string fromCurrency,
        [FromQuery] string toCurrency)
    {
        try
        {
            var currencyProvider = _factory.GetProvider(provider);

            decimal convertedAmount = await currencyProvider.ConvertCurrencyAsync(amount, fromCurrency.ToUpper(), toCurrency.ToUpper());
            return Ok(new { ConvertedAmount = convertedAmount });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
        }
    }




    [HttpGet("GetHistoricalExchangeRatesAsync")]
    public async Task<IActionResult> GetHistoricalExchangeRates(string provider,
         [FromQuery] string baseCurrency,
         [FromQuery] string startDate,
         [FromQuery] string endDate,
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 10)
    {
        try
        {
            var currencyProvider = _factory.GetProvider(provider);

            var result = await currencyProvider.GetHistoricalExchangeRatesAsync(baseCurrency.ToUpper(), startDate, endDate, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
        }
    }

}
