using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Monetra.Application.DTOs.Market;
using Monetra.Application.Interfaces;
using Monetra.Domain.Entities;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class MarketService : IMarketService
{
    private readonly MonetraDbContext _context;
    private readonly ILogger<MarketService> _logger;
    private readonly HttpClient _httpClient;

    public MarketService(MonetraDbContext context, ILogger<MarketService> logger, HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    public async Task<MarketRateDto> GetUsdToCopRateAsync(CancellationToken cancellationToken = default)
    {
        // Check cache in database first
        var cachedRate = await _context.ExchangeRateCaches
            .FirstOrDefaultAsync(e => e.BaseCurrency == "USD" && e.TargetCurrency == "COP", cancellationToken);

        var isCacheStale = cachedRate == null || (DateTime.UtcNow - cachedRate.LastUpdatedAt).TotalHours > 6;

        if (isCacheStale)
        {
            try
            {
                // Attempt to query Yahoo Finance or fallback exchange rate provider
                // e.g. https://query1.finance.yahoo.com/v8/finance/chart/COP=X
                var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("rates", out var rates) && rates.TryGetProperty("COP", out var copRate))
                    {
                        var freshRate = copRate.GetDecimal();
                        if (cachedRate == null)
                        {
                            cachedRate = new ExchangeRateCache
                            {
                                BaseCurrency = "USD",
                                TargetCurrency = "COP",
                                Rate = freshRate,
                                LastUpdatedAt = DateTime.UtcNow,
                                Source = "OpenExchangeRates"
                            };
                            _context.ExchangeRateCaches.Add(cachedRate);
                        }
                        else
                        {
                            cachedRate.Rate = freshRate;
                            cachedRate.LastUpdatedAt = DateTime.UtcNow;
                            cachedRate.Source = "OpenExchangeRates";
                        }
                        await _context.SaveChangesAsync(cancellationToken);

                        return new MarketRateDto
                        {
                            BaseCurrency = "USD",
                            TargetCurrency = "COP",
                            Rate = freshRate,
                            LastUpdated = cachedRate.LastUpdatedAt,
                            IsStale = false,
                            WarningMessage = string.Empty
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falla al consultar la API externa de tasas cambiarias. Usando tasa en caché local.");
            }
        }

        // Return cached rate (or hardcoded safe baseline if cache empty)
        var rateValue = cachedRate?.Rate ?? 4150.00m;
        var lastUpdated = cachedRate?.LastUpdatedAt ?? DateTime.UtcNow.AddHours(-1);
        var isStale = (DateTime.UtcNow - lastUpdated).TotalHours > 2;

        return new MarketRateDto
        {
            BaseCurrency = "USD",
            TargetCurrency = "COP",
            Rate = rateValue,
            LastUpdated = lastUpdated,
            IsStale = isStale,
            WarningMessage = isStale 
                ? $"Tasa de cambio desactualizada (último valor registrado: {lastUpdated:dd/MM/yyyy HH:mm} UTC)" 
                : string.Empty
        };
    }

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        if (fromCurrency == toCurrency)
        {
            return amount;
        }

        var marketRate = await GetUsdToCopRateAsync(cancellationToken);
        var usdToCop = marketRate.Rate;

        if (fromCurrency == "USD" && toCurrency == "COP")
        {
            return Math.Round(amount * usdToCop, 2);
        }

        if (fromCurrency == "COP" && toCurrency == "USD")
        {
            return usdToCop > 0 ? Math.Round(amount / usdToCop, 2) : 0m;
        }

        return amount;
    }
}
