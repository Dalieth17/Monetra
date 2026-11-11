using Monetra.Application.DTOs.Market;

namespace Monetra.Application.Interfaces;

public interface IStockService
{
    Task<List<StockQuoteDto>> GetWatchlistAsync(CancellationToken cancellationToken = default);
}
