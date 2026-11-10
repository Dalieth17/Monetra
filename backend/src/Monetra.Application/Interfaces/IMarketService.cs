using Monetra.Application.DTOs.Market;

namespace Monetra.Application.Interfaces;

public interface IMarketService
{
    Task<MarketRateDto> GetUsdToCopRateAsync(CancellationToken cancellationToken = default);
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
}

