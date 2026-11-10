namespace Monetra.Domain.Entities;

public class Debt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Creditor { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal InterestRate { get; set; } // e.g. 1.5% EM or 18% EA
    public string RateType { get; set; } = "EA"; // "EA" (Efectiva Anual) or "EM" (Efectiva Mensual)
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

