using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CnbExchangeRates.Configuration;
using CnbExchangeRates.Models;
using Microsoft.Extensions.Options;

namespace CnbExchangeRates.Services;

/// <summary>
/// Retrieves and parses daily exchange rate data from the Czech National Bank (CNB) public API.
/// Endpoint: GET https://api.cnb.cz/cnbapi/exrates/daily?date={date}&amp;lang=EN
/// </summary>
public sealed class ExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly CnbApiOptions _options;
    private readonly ILogger<ExchangeRateProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ExchangeRateProvider(
        HttpClient httpClient,
        IOptions<CnbApiOptions> options,
        ILogger<ExchangeRateProvider> logger)
    {
        _httpClient = httpClient;
        _options    = options.Value;
        _logger     = logger;
    }

    /// <inheritdoc />
    public async Task<ExchangeRatesResponse> GetDailyRatesAsync(
        DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = BuildRequestUrl(date);
        _logger.LogInformation("Fetching CNB exchange rates from {Url}", requestUrl);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching CNB exchange rates");
            throw new ExchangeRateProviderException(
                "Failed to fetch exchange rates from CNB. The service may be temporarily unavailable.", ex);
        }

        CnbApiResponse? cnbResponse;
        try
        {
            cnbResponse = await response.Content.ReadFromJsonAsync<CnbApiResponse>(
                JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialise CNB response");
            throw new ExchangeRateProviderException(
                "Received an unexpected response format from the CNB API.", ex);
        }

        if (cnbResponse is null || cnbResponse.Rates is null)
            throw new ExchangeRateProviderException("CNB API returned an empty response.");

        return MapToResponse(cnbResponse);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private string BuildRequestUrl(DateOnly? date)
    {
        var path = _options.DailyExRatesPath;
        var query = date.HasValue
            ? $"?date={date.Value:yyyy-MM-dd}&lang=EN"
            : "?lang=EN";
        return $"{_options.BaseUrl}{path}{query}";
    }

    private static ExchangeRatesResponse MapToResponse(CnbApiResponse cnbResponse)
    {
        var rates = cnbResponse.Rates
            .Select(r => new ExchangeRate(
                CurrencyCode: r.CurrencyCode,
                CurrencyName: r.Currency,
                Country:      r.Country,
                Amount:       r.Amount,
                Rate:         r.Rate))
            .ToList();

        return new ExchangeRatesResponse(cnbResponse.ValidFor, rates);
    }
}
