using CnbExchangeRates.Models;

namespace CnbExchangeRates.Services;

public interface IExchangeRateProvider
{
    /// <summary>
    /// Fetches daily exchange rates from the CNB API.
    /// </summary>
    /// <param name="date">
    /// The date for which rates are requested.
    /// Pass <c>null</c> to retrieve the most recent available rates.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    Task<ExchangeRatesResponse> GetDailyRatesAsync(
        DateOnly? date = null,
        CancellationToken cancellationToken = default);
}
