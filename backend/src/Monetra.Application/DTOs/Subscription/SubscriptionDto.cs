namespace Monetra.Application.DTOs.Subscription;

public class SubscriptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "COP";
    public string BillingCycle { get; set; } = "Monthly";
    public DateTime NextPaymentDate { get; set; }
    public bool IsActive { get; set; }
    public int DaysUntilPayment => Math.Max(0, (NextPaymentDate.Date - DateTime.UtcNow.Date).Days);
    public DateTime CreatedAt { get; set; }
}

public class CreateSubscriptionDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "COP";
    public string BillingCycle { get; set; } = "Monthly"; // Monthly, Annual, Weekly
    public DateTime NextPaymentDate { get; set; }
}

