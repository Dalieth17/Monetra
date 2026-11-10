namespace Monetra.Application.DTOs.Debt;

public class DebtDto
{
    public int Id { get; set; }
    public string Creditor { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal InterestRate { get; set; }
    public string RateType { get; set; } = "EA";
    public DateTime DueDate { get; set; }
    public decimal EstimatedMonthlyPayment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDebtDto
{
    public string Creditor { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public string RateType { get; set; } = "EA"; // EA or EM
    public DateTime DueDate { get; set; }
}

public class RecordDebtPaymentDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
}
