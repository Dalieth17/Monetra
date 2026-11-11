using System.Net;
using Microsoft.EntityFrameworkCore;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class MarketServiceTests
{
    // Forma real de la respuesta de https://open.er-api.com/v6/latest/USD (tasas recortadas).
    private const string ProviderPayload = """
        {
          "result": "success",
          "provider": "https://www.exchangerate-api.com",
          "documentation": "https://www.exchangerate-api.com/docs/free",
          "terms_of_use": "https://www.exchangerate-api.com/terms",
          "time_last_update_unix": 1794268951,
          "time_last_update_utc": "Tue, 10 Nov 2026 00:02:31 +0000",
          "time_next_update_unix": 1794356261,
          "time_next_update_utc": "Wed, 11 Nov 2026 00:17:41 +0000",
          "time_eol_unix": 0,
          "base_code": "USD",
          "rates": { "USD": 1, "COP": 4321.5, "EUR": 0.92, "MXN": 18.4 }
        }
        """;

    private const string PayloadWithoutCop = """
        { "result": "success", "base_code": "USD", "rates": { "USD": 1, "EUR": 0.92 } }
        """;

    private static DateTime SevenHoursAgo => DateTime.UtcNow.AddHours(-7);

    [Fact]
    public async Task GetRate_WithARecentCachedRate_DoesNotCallTheProvider()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m);
        var provider = StubHttpMessageHandler.Json(ProviderPayload);

        var rate = await context.CreateMarketService(provider).GetUsdToCopRateAsync();

        Assert.Equal(4_000m, rate.Rate);
        Assert.False(rate.IsStale);
        Assert.Empty(provider.Requests);
    }

    [Fact]
    public async Task GetRate_WithAnOldCachedRate_RefreshesFromTheProviderAndStoresIt()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m, SevenHoursAgo);
        var provider = StubHttpMessageHandler.Json(ProviderPayload);

        var rate = await context.CreateMarketService(provider).GetUsdToCopRateAsync();

        Assert.Equal(4_321.5m, rate.Rate);
        Assert.False(rate.IsStale);
        Assert.Equal(4_321.5m, context.ExchangeRateCaches.AsNoTracking().Single().Rate);
    }

    [Fact]
    public async Task GetRate_WhenTheProviderIsUnreachable_ReturnsTheLastKnownRateFlaggedAsStale()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m, SevenHoursAgo);

        var rate = await context.CreateMarketService(StubHttpMessageHandler.Unreachable()).GetUsdToCopRateAsync();

        Assert.Equal(4_000m, rate.Rate);
        Assert.True(rate.IsStale);
        Assert.NotEmpty(rate.WarningMessage);
    }

    [Fact]
    public async Task GetRate_WhenTheProviderAnswersWithAnError_KeepsTheLastKnownRate()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m, SevenHoursAgo);
        var provider = StubHttpMessageHandler.Status(HttpStatusCode.InternalServerError);

        var rate = await context.CreateMarketService(provider).GetUsdToCopRateAsync();

        Assert.Equal(4_000m, rate.Rate);
        Assert.True(rate.IsStale);
    }

    [Fact]
    public async Task GetRate_WhenTheProviderOmitsTheCopRate_KeepsTheLastKnownRate()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m, SevenHoursAgo);
        var provider = StubHttpMessageHandler.Json(PayloadWithoutCop);

        var rate = await context.CreateMarketService(provider).GetUsdToCopRateAsync();

        Assert.Equal(4_000m, rate.Rate);
        Assert.Equal(4_000m, context.ExchangeRateCaches.AsNoTracking().Single().Rate);
    }

    [Fact]
    public async Task GetRate_WithNoCacheAndNoProvider_StillReturnsAUsableRate()
    {
        using var context = TestDatabase.Create();
        context.ExchangeRateCaches.RemoveRange(context.ExchangeRateCaches);
        await context.SaveChangesAsync();

        var rate = await context.CreateMarketService(StubHttpMessageHandler.Unreachable()).GetUsdToCopRateAsync();

        Assert.True(rate.Rate > 0m);
    }

    [Fact]
    public async Task GetRate_WithNoCache_StoresTheRateFetchedFromTheProvider()
    {
        using var context = TestDatabase.Create();
        context.ExchangeRateCaches.RemoveRange(context.ExchangeRateCaches);
        await context.SaveChangesAsync();
        var provider = StubHttpMessageHandler.Json(ProviderPayload);

        var rate = await context.CreateMarketService(provider).GetUsdToCopRateAsync();

        Assert.Equal(4_321.5m, rate.Rate);
        Assert.Equal(4_321.5m, context.ExchangeRateCaches.AsNoTracking().Single().Rate);
    }

    [Theory]
    [InlineData(100, "USD", "COP", 400_000)]
    [InlineData(1_000_000, "COP", "USD", 250)]
    [InlineData(100, "usd", "cop", 400_000)]
    [InlineData(750, "COP", "COP", 750)]
    [InlineData(12.5, "USD", "USD", 12.5)]
    public async Task Convert_AppliesTheUsdCopRateInTheRightDirection(decimal amount, string from, string to, decimal expected)
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m);

        var converted = await context.CreateMarketService().ConvertAsync(amount, from, to);

        Assert.Equal(expected, converted);
    }

    [Fact]
    public async Task Convert_RoundsToCents()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(3_000m);

        var converted = await context.CreateMarketService().ConvertAsync(1_000m, "COP", "USD");

        Assert.Equal(0.33m, converted);
    }
}
