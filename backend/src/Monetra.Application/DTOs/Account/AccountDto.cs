namespace Monetra.Application.DTOs.Account;

public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "COP";
    public decimal CurrentBalance { get; set; }
    public decimal ConvertedBalance { get; set; }
    public DateTime CreatedAt { get; set; }
}

