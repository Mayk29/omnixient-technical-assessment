namespace CnbExchangeRates.Configuration;

public class CnbApiOptions
{
    public const string SectionName = "CnbApi";

    /// <summary>Base URL for the CNB public API.</summary>
    public string BaseUrl { get; set; } = "https://api.cnb.cz/cnbapi";

    /// <summary>Relative path for the daily exchange rates endpoint.</summary>
    public string DailyExRatesPath { get; set; } = "/exrates/daily";
}
