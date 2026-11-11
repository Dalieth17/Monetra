using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monetra.Application.DTOs.Market;
using Monetra.Application.Interfaces;

namespace Monetra.Infrastructure.Services;

public class StockMarketOptions
{
    public string[] Symbols { get; set; } = ["AAPL", "MSFT", "GOOGL", "AMZN", "NVDA", "TSLA", "EC", "CIB"];
}

public class StockService : IStockService
{
    private static readonly TimeSpan FreshFor = TimeSpan.FromMinutes(15);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly TimeProvider _clock;
    private readonly ILogger<StockService> _logger;
    private readonly string[] _symbols;

    public StockService(
        HttpClient httpClient,
        IMemoryCache cache,
        TimeProvider clock,
        ILogger<StockService> logger,
        IOptions<StockMarketOptions> options)
    {
        _httpClient = httpClient;
        _cache = cache;
        _clock = clock;
        _logger = logger;
        _symbols = options.Value.Symbols;
    }

    public async Task<List<StockQuoteDto>> GetWatchlistAsync(CancellationToken cancellationToken = default)
    {
        var quotes = await Task.WhenAll(_symbols.Select(symbol => GetQuoteOrDefaultAsync(symbol, cancellationToken)));
        return quotes.OfType<StockQuoteDto>().ToList();
    }

    private async Task<StockQuoteDto?> GetQuoteOrDefaultAsync(string symbol, CancellationToken cancellationToken)
    {
        var cacheKey = $"stock-quote:{symbol}";
        var now = _clock.GetUtcNow();
        var cached = _cache.Get<CachedQuote>(cacheKey);

        if (cached != null && now - cached.FetchedAt < FreshFor)
        {
            return cached.Quote;
        }

        try
        {
            var fresh = await FetchQuoteAsync(symbol, cancellationToken);
            _cache.Set(cacheKey, new CachedQuote(fresh, now));
            return fresh;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Falla al consultar la cotización de {Symbol}. Usando el último valor conocido.", symbol);
        }

        // El proveedor externo nunca debe bloquear la app: se sirve el último valor conocido, o nada.
        return cached == null
            ? null
            : cached.Quote with
            {
                IsStale = true,
                WarningMessage = $"Cotización desactualizada (último valor registrado: {cached.Quote.LastUpdated:dd/MM/yyyy HH:mm} UTC)"
            };
    }

    private async Task<StockQuoteDto> FetchQuoteAsync(string symbol, CancellationToken cancellationToken)
    {
        var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(symbol)}?range=1mo&interval=1d";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);

        var results = document.RootElement.GetProperty("chart").GetProperty("result");
        if (results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0)
        {
            throw new InvalidDataException($"El proveedor no devolvió datos para {symbol}.");
        }

        var data = results[0];
        var meta = data.GetProperty("meta");
        var decimals = meta.TryGetProperty("priceHint", out var priceHint) ? priceHint.GetInt32() : 2;
        var price = Math.Round(meta.GetProperty("regularMarketPrice").GetDecimal(), decimals);
        var history = ReadHistory(data, decimals);

        return new StockQuoteDto
        {
            Symbol = symbol,
            Name = ReadText(meta, "longName") ?? ReadText(meta, "shortName") ?? symbol,
            Price = price,
            PreviousClose = ReadPreviousClose(meta, history, decimals) ?? price,
            Currency = ReadText(meta, "currency") ?? "USD",
            LastUpdated = meta.TryGetProperty("regularMarketTime", out var marketTime)
                ? DateTimeOffset.FromUnixTimeSeconds(marketTime.GetInt64()).UtcDateTime
                : _clock.GetUtcNow().UtcDateTime,
            History = history
        };
    }

    private static List<StockPricePointDto> ReadHistory(JsonElement data, int decimals)
    {
        var history = new List<StockPricePointDto>();
        if (!data.TryGetProperty("timestamp", out var timestamps))
        {
            return history;
        }

        var closes = data.GetProperty("indicators").GetProperty("quote")[0].GetProperty("close");
        var sessions = Math.Min(timestamps.GetArrayLength(), closes.GetArrayLength());

        for (var i = 0; i < sessions; i++)
        {
            // El proveedor envía null en las sesiones sin cierre registrado.
            if (closes[i].ValueKind == JsonValueKind.Number)
            {
                history.Add(new StockPricePointDto(
                    DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64()).UtcDateTime,
                    Math.Round(closes[i].GetDecimal(), decimals)));
            }
        }

        return history;
    }

    private static decimal? ReadPreviousClose(JsonElement meta, List<StockPricePointDto> history, int decimals)
    {
        // chartPreviousClose es el cierre anterior al inicio del rango (hace un mes), no el de ayer;
        // el cierre de la sesión previa es el penúltimo punto de la serie diaria.
        if (history.Count >= 2)
        {
            return history[^2].Close;
        }

        return meta.TryGetProperty("chartPreviousClose", out var previousClose) && previousClose.ValueKind == JsonValueKind.Number
            ? Math.Round(previousClose.GetDecimal(), decimals)
            : null;
    }

    private static string? ReadText(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private sealed record CachedQuote(StockQuoteDto Quote, DateTimeOffset FetchedAt);
}
