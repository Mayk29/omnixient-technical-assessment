namespace CnbExchangeRates.Models;

/// <summary>
/// A single exchange rate entry as returned by the CNB API.
/// </summary>
public record ExchangeRate(
    string CurrencyCode,
    string CurrencyName,
    string Country,
    int Amount,
    decimal Rate
);

/// <summary>
/// The full response returned by our API endpoint.
/// </summary>
public record ExchangeRatesResponse(
    DateOnly Date,
    IReadOnlyList<ExchangeRate> Rates
);

// ─── CNB API response shape (deserialised internally) ──────────────────────

internal record CnbApiResponse(
    DateOnly ValidFor,
    IReadOnlyList<CnbRate> Rates
);

internal record CnbRate(
    string ValidFor,
    int Order,
    string Country,
    string Currency,
    int Amount,
    string CurrencyCode,
    decimal Rate
);
