namespace Monetra.Application.DTOs.Market;

public record StockQuoteDto
{
    public string Symbol { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal PreviousClose { get; init; }
    public decimal Change => Price - PreviousClose;
    public decimal ChangePercent => PreviousClose > 0 ? Math.Round(Change / PreviousClose * 100m, 2) : 0m;
    public string Currency { get; init; } = "USD";
    public DateTime LastUpdated { get; init; }
    public bool IsStale { get; init; }
    public string WarningMessage { get; init; } = string.Empty;
    public List<StockPricePointDto> History { get; init; } = new();
}

public record StockPricePointDto(DateTime Date, decimal Close);
