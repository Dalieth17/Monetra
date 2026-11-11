using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class StockServiceTests
{
    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 9, 15, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private static StockService CreateService(StubHttpMessageHandler provider, ManualClock? clock = null, params string[] symbols)
    {
        return new StockService(
            new HttpClient(provider),
            new MemoryCache(new MemoryCacheOptions()),
            clock ?? new ManualClock(),
            NullLogger<StockService>.Instance,
            Options.Create(new StockMarketOptions { Symbols = symbols.Length > 0 ? symbols : ["AAPL"] }));
    }

    private static string SymbolOf(HttpRequestMessage request) => request.RequestUri!.Segments[^1];

    [Fact]
    public async Task GetWatchlist_ReturnsTheQuoteReportedByTheProvider()
    {
        var service = CreateService(StubHttpMessageHandler.Json(YahooChart.Payload("AAPL", price: 226.96m)));

        var quote = Assert.Single(await service.GetWatchlistAsync());

        Assert.Equal("AAPL", quote.Symbol);
        Assert.Equal("Apple Inc.", quote.Name);
        Assert.Equal(226.96m, quote.Price);
        Assert.Equal("USD", quote.Currency);
        Assert.Equal(new DateTime(2026, 11, 6, 21, 0, 1, DateTimeKind.Utc), quote.LastUpdated);
        Assert.False(quote.IsStale);
    }

    [Fact]
    public async Task GetWatchlist_MeasuresTheDailyChangeAgainstThePreviousSessionClose()
    {
        var service = CreateService(StubHttpMessageHandler.Json(YahooChart.Payload("AAPL", price: 226.96m)));

        var quote = Assert.Single(await service.GetWatchlistAsync());

        // Cierre anterior 225,67 → +1,29 → +0,57 %
        Assert.Equal(225.67m, quote.PreviousClose);
        Assert.Equal(1.29m, quote.Change);
        Assert.Equal(0.57m, quote.ChangePercent);
    }

    [Fact]
    public async Task GetWatchlist_HistoryHasOneRoundedPointPerSessionWithAClose()
    {
        var service = CreateService(StubHttpMessageHandler.Json(YahooChart.Payload()));

        var quote = Assert.Single(await service.GetWatchlistAsync());

        Assert.Equal(
            [221.25m, 223.45m, 225.67m, 226.96m],
            quote.History.Select(p => p.Close));
        Assert.Equal(
            [new DateTime(2026, 11, 2), new DateTime(2026, 11, 3), new DateTime(2026, 11, 5), new DateTime(2026, 11, 6)],
            quote.History.Select(p => p.Date.Date));
    }

    [Fact]
    public async Task GetWatchlist_WhenTheProviderOmitsTheCompanyName_UsesTheSymbol()
    {
        var service = CreateService(StubHttpMessageHandler.Json(YahooChart.Payload("EC", withNames: false)), symbols: ["EC"]);

        var quote = Assert.Single(await service.GetWatchlistAsync());

        Assert.Equal("EC", quote.Name);
    }

    [Fact]
    public async Task GetWatchlist_AsksTheProviderForEachConfiguredSymbolAndKeepsTheirOrder()
    {
        var provider = new StubHttpMessageHandler(request => StubHttpMessageHandler.JsonResponse(YahooChart.Payload(SymbolOf(request))));
        var service = CreateService(provider, symbols: ["MSFT", "EC", "AAPL"]);

        var quotes = await service.GetWatchlistAsync();

        Assert.Equal(["MSFT", "EC", "AAPL"], quotes.Select(q => q.Symbol));
        Assert.Equal(["AAPL", "EC", "MSFT"], provider.Requests.Select(uri => uri.Segments[^1]).Order());
    }

    [Fact]
    public async Task GetWatchlist_WithinFifteenMinutes_ServesTheCachedQuoteWithoutCallingTheProvider()
    {
        var clock = new ManualClock();
        var provider = StubHttpMessageHandler.Json(YahooChart.Payload());
        var service = CreateService(provider, clock);
        await service.GetWatchlistAsync();

        clock.Advance(TimeSpan.FromMinutes(14));
        var quote = Assert.Single(await service.GetWatchlistAsync());

        Assert.Equal(226.96m, quote.Price);
        Assert.Single(provider.Requests);
    }

    [Fact]
    public async Task GetWatchlist_AfterFifteenMinutes_FetchesTheNewPrice()
    {
        var clock = new ManualClock();
        var price = 226.96m;
        var provider = new StubHttpMessageHandler(_ => StubHttpMessageHandler.JsonResponse(YahooChart.Payload(price: price)));
        var service = CreateService(provider, clock);
        await service.GetWatchlistAsync();

        clock.Advance(TimeSpan.FromMinutes(16));
        price = 230.10m;
        var quote = Assert.Single(await service.GetWatchlistAsync());

        Assert.Equal(230.10m, quote.Price);
        Assert.False(quote.IsStale);
    }

    [Fact]
    public async Task GetWatchlist_WhenTheProviderFailsAfterAQuoteWasLoaded_ServesTheLastQuoteFlaggedAsStale()
    {
        var clock = new ManualClock();
        var providerIsUp = true;
        var provider = new StubHttpMessageHandler(_ => providerIsUp
            ? StubHttpMessageHandler.JsonResponse(YahooChart.Payload(price: 226.96m))
            : throw new HttpRequestException("Sin conexión con el proveedor."));
        var service = CreateService(provider, clock);
        await service.GetWatchlistAsync();

        clock.Advance(TimeSpan.FromMinutes(16));
        providerIsUp = false;
        var quote = Assert.Single(await service.GetWatchlistAsync());

        Assert.Equal(226.96m, quote.Price);
        Assert.True(quote.IsStale);
        Assert.NotEmpty(quote.WarningMessage);
    }

    [Fact]
    public async Task GetWatchlist_WhenTheProviderRecovers_ClearsTheStaleFlag()
    {
        var clock = new ManualClock();
        var providerIsUp = true;
        var provider = new StubHttpMessageHandler(_ => providerIsUp
            ? StubHttpMessageHandler.JsonResponse(YahooChart.Payload())
            : throw new HttpRequestException("Sin conexión con el proveedor."));
        var service = CreateService(provider, clock);
        await service.GetWatchlistAsync();
        clock.Advance(TimeSpan.FromMinutes(16));
        providerIsUp = false;
        await service.GetWatchlistAsync();

        providerIsUp = true;
        var quote = Assert.Single(await service.GetWatchlistAsync());

        Assert.False(quote.IsStale);
        Assert.Empty(quote.WarningMessage);
    }

    [Fact]
    public async Task GetWatchlist_WhenASymbolHasNeverLoaded_LeavesItOutInsteadOfFailing()
    {
        var provider = new StubHttpMessageHandler(request => SymbolOf(request) == "EC"
            ? throw new HttpRequestException("Sin conexión con el proveedor.")
            : StubHttpMessageHandler.JsonResponse(YahooChart.Payload(SymbolOf(request))));
        var service = CreateService(provider, symbols: ["AAPL", "EC", "MSFT"]);

        var quotes = await service.GetWatchlistAsync();

        Assert.Equal(["AAPL", "MSFT"], quotes.Select(q => q.Symbol));
    }

    [Fact]
    public async Task GetWatchlist_WhenTheProviderIsDownAndNothingWasLoaded_ReturnsAnEmptyList()
    {
        var service = CreateService(StubHttpMessageHandler.Unreachable(), symbols: ["AAPL", "EC"]);

        var quotes = await service.GetWatchlistAsync();

        Assert.Empty(quotes);
    }

    [Fact]
    public async Task GetWatchlist_TreatsAnErrorPayloadAsAFailedSymbol()
    {
        var service = CreateService(StubHttpMessageHandler.Json(YahooChart.ErrorPayload));

        var quotes = await service.GetWatchlistAsync();

        Assert.Empty(quotes);
    }

    [Fact]
    public async Task GetWatchlist_TreatsARateLimitResponseAsAFailedSymbol()
    {
        var service = CreateService(StubHttpMessageHandler.Status(HttpStatusCode.TooManyRequests));

        var quotes = await service.GetWatchlistAsync();

        Assert.Empty(quotes);
    }

    [Fact]
    public async Task GetWatchlist_AfterAFailure_TriesTheProviderAgainOnTheNextRequest()
    {
        var providerIsUp = false;
        var provider = new StubHttpMessageHandler(_ => providerIsUp
            ? StubHttpMessageHandler.JsonResponse(YahooChart.Payload())
            : throw new HttpRequestException("Sin conexión con el proveedor."));
        var service = CreateService(provider);
        await service.GetWatchlistAsync();

        providerIsUp = true;
        var quotes = await service.GetWatchlistAsync();

        Assert.Single(quotes);
    }
}
