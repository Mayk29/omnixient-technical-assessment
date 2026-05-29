using CnbExchangeRates.Models;
using CnbExchangeRates.Services;
using Microsoft.AspNetCore.Mvc;

namespace CnbExchangeRates.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateProvider _provider;
    private readonly ILogger<ExchangeRatesController> _logger;

    public ExchangeRatesController(
        IExchangeRateProvider provider,
        ILogger<ExchangeRatesController> logger)
    {
        _provider = provider;
        _logger   = logger;
    }

    /// <summary>
    /// Returns daily exchange rates from the Czech National Bank.
    /// </summary>
    /// <param name="date">
    /// Optional date in <c>yyyy-MM-dd</c> format.
    /// Defaults to today's rates if omitted.
    /// </param>
    [HttpGet]
    [ProducesResponseType(typeof(ExchangeRatesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetRates(
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        try
        {
            var rates = await _provider.GetDailyRatesAsync(date, cancellationToken);
            return Ok(rates);
        }
        catch (ExchangeRateProviderException ex)
        {
            _logger.LogWarning(ex, "Exchange rate provider error");
            return StatusCode(StatusCodes.Status502BadGateway,
                new ProblemDetails
                {
                    Title  = "Upstream service error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status502BadGateway
                });
        }
    }
}
