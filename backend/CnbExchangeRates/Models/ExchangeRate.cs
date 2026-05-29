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
/// Date is an ISO string "yyyy-MM-dd" for safe JSON serialisation across all clients.
/// </summary>
public record ExchangeRatesResponse(
    string Date,
    IReadOnlyList<ExchangeRate> Rates
);

// ─── CNB API response shape (deserialised internally) ──────────────────────

internal record CnbApiResponse(
    string ValidFor,
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