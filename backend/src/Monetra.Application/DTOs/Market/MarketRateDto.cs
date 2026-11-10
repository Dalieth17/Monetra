namespace Monetra.Application.DTOs.Market;

public class MarketRateDto
{
    public string BaseCurrency { get; set; } = "USD";
    public string TargetCurrency { get; set; } = "COP";
    public decimal Rate { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsStale { get; set; }
    public string WarningMessage { get; set; } = string.Empty;
}
