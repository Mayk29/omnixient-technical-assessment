using System.Net;
using CnbExchangeRates.Configuration;
using CnbExchangeRates.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;

namespace CnbExchangeRates.Tests;

public class ExchangeRateProviderTests
{
    // ── Helpers ───────────────────────────────────────────────────────────

    private static (ExchangeRateProvider provider, MockHttpMessageHandler mockHttp) CreateSut()
    {
        var mockHttp   = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();

        var options = Options.Create(new CnbApiOptions
        {
            BaseUrl          = "https://api.cnb.cz/cnbapi",
            DailyExRatesPath = "/exrates/daily"
        });

        var provider = new ExchangeRateProvider(
            httpClient,
            options,
            NullLogger<ExchangeRateProvider>.Instance);

        return (provider, mockHttp);
    }

    private const string ValidCnbJson = """
        {
          "validFor": "2024-05-14",
          "rates": [
            {
              "validFor": "2024-05-14",
              "order": 1,
              "country": "Australia",
              "currency": "dollar",
              "amount": 1,
              "currencyCode": "AUD",
              "rate": 15.231
            },
            {
              "validFor": "2024-05-14",
              "order": 2,
              "country": "USA",
              "currency": "dollar",
              "amount": 1,
              "currencyCode": "USD",
              "rate": 22.589
            }
          ]
        }
        """;

    // ── Tests ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDailyRatesAsync_ReturnsCorrectRates_WhenApiRespondsSuccessfully()
    {
        var (sut, mockHttp) = CreateSut();

        mockHttp
            .When("https://api.cnb.cz/cnbapi/exrates/daily*")
            .Respond("application/json", ValidCnbJson);

        var result = await sut.GetDailyRatesAsync();

        result.Should().NotBeNull();
        result.Date.Should().Be("2024-05-14");
        result.Rates.Should().HaveCount(2);

        var usd = result.Rates.Single(r => r.CurrencyCode == "USD");
        usd.Rate.Should().Be(22.589m);
        usd.Country.Should().Be("USA");
        usd.Amount.Should().Be(1);
    }

    [Fact]
    public async Task GetDailyRatesAsync_AppendsDateQueryParam_WhenDateIsProvided()
    {
        var (sut, mockHttp) = CreateSut();

        mockHttp
            .When("https://api.cnb.cz/cnbapi/exrates/daily?date=2024-01-15*")
            .Respond("application/json", ValidCnbJson);

        var act = () => sut.GetDailyRatesAsync(new DateOnly(2024, 1, 15));
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetDailyRatesAsync_ThrowsExchangeRateProviderException_OnHttpError()
    {
        var (sut, mockHttp) = CreateSut();

        mockHttp
            .When("https://api.cnb.cz/cnbapi/exrates/daily*")
            .Respond(HttpStatusCode.ServiceUnavailable);

        var act = () => sut.GetDailyRatesAsync();

        await act.Should().ThrowAsync<ExchangeRateProviderException>()
            .WithMessage("*fetch exchange rates*");
    }

    [Fact]
    public async Task GetDailyRatesAsync_ThrowsExchangeRateProviderException_OnInvalidJson()
    {
        var (sut, mockHttp) = CreateSut();

        mockHttp
            .When("https://api.cnb.cz/cnbapi/exrates/daily*")
            .Respond("application/json", "{ not valid json }");

        var act = () => sut.GetDailyRatesAsync();

        await act.Should().ThrowAsync<ExchangeRateProviderException>()
            .WithMessage("*unexpected response format*");
    }

    [Fact]
    public async Task GetDailyRatesAsync_MapsRateFields_Correctly()
    {
        var (sut, mockHttp) = CreateSut();

        mockHttp
            .When("https://api.cnb.cz/cnbapi/exrates/daily*")
            .Respond("application/json", ValidCnbJson);

        var result = await sut.GetDailyRatesAsync();

        var aud = result.Rates.Single(r => r.CurrencyCode == "AUD");
        aud.CurrencyName.Should().Be("dollar");
        aud.Country.Should().Be("Australia");
        aud.Amount.Should().Be(1);
        aud.Rate.Should().Be(15.231m);
    }
}