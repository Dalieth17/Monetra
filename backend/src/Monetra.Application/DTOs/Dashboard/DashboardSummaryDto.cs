using Monetra.Application.DTOs.Account;
using Monetra.Application.DTOs.Transaction;

namespace Monetra.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public string BaseCurrency { get; set; } = "COP";
    public decimal TotalNetWorth { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal NetSavingsThisMonth => MonthlyIncome - MonthlyExpense;
    public decimal UsdToCopExchangeRate { get; set; }
    public DateTime ExchangeRateTimestamp { get; set; }
    public bool IsRateCached { get; set; }

    public List<AccountDto> Accounts { get; set; } = new();
    public List<TransactionDto> RecentTransactions { get; set; } = new();
}

