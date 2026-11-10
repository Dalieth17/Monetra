namespace Monetra.Domain.Entities;

public class ExchangeRateCache
{
    public int Id { get; set; }
    public string BaseCurrency { get; set; } = "USD";
    public string TargetCurrency { get; set; } = "COP";
    public decimal Rate { get; set; }
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "YahooFinance";
}
