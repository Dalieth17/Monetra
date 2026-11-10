namespace Monetra.Domain.Entities;

public class Account
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "COP"; // e.g. "COP", "USD"
    public decimal CurrentBalance { get; set; } = 0m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
