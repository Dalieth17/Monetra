namespace Monetra.Application.DTOs.Account;

public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "COP"; // e.g. "COP" or "USD"
    public decimal InitialBalance { get; set; } = 0m;
}

public class UpdateAccountDto
{
    public string Name { get; set; } = string.Empty;
}

