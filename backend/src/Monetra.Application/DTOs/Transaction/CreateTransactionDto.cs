using Monetra.Domain.Enums;

namespace Monetra.Application.DTOs.Transaction;

public class CreateTransactionDto
{
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime? Date { get; set; }
    public string Description { get; set; } = string.Empty;
}

