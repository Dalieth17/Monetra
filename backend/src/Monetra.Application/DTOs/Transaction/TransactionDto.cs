using Monetra.Domain.Enums;

namespace Monetra.Application.DTOs.Transaction;

public class TransactionDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "COP";
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = "Sin categoría";
    public string CategoryIcon { get; set; } = "category";
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

