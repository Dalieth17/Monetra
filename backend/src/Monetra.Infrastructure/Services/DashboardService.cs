using Microsoft.EntityFrameworkCore;
using Monetra.Application.DTOs.Dashboard;
using Monetra.Application.Interfaces;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly MonetraDbContext _context;
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly IMarketService _marketService;

    public DashboardService(
        MonetraDbContext context,
        IAccountService accountService,
        ITransactionService transactionService,
        IMarketService marketService)
    {
        _context = context;
        _accountService = accountService;
        _transactionService = transactionService;
        _marketService = marketService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(int userId, string baseCurrency = "COP", CancellationToken cancellationToken = default)
    {
        baseCurrency = baseCurrency.ToUpperInvariant();
        var marketRate = await _marketService.GetUsdToCopRateAsync(cancellationToken);

        var accounts = await _accountService.GetUserAccountsAsync(userId, baseCurrency, cancellationToken);
        var totalNetWorth = accounts.Sum(a => a.ConvertedBalance);

        // Current month bounds
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        var monthlyTransactions = await _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth)
            .ToListAsync(cancellationToken);

        decimal monthlyIncome = 0m;
        decimal monthlyExpense = 0m;

        foreach (var t in monthlyTransactions)
        {
            var converted = await _marketService.ConvertAsync(t.Amount, t.Account.CurrencyCode, baseCurrency, cancellationToken);
            if (t.Type == TransactionType.Income)
            {
                monthlyIncome += converted;
            }
            else
            {
                monthlyExpense += converted;
            }
        }

        var recentTransactions = await _transactionService.GetTransactionsAsync(userId, from: now.AddDays(-30), cancellationToken: cancellationToken);

        return new DashboardSummaryDto
        {
            BaseCurrency = baseCurrency,
            TotalNetWorth = Math.Round(totalNetWorth, 2),
            MonthlyIncome = Math.Round(monthlyIncome, 2),
            MonthlyExpense = Math.Round(monthlyExpense, 2),
            UsdToCopExchangeRate = marketRate.Rate,
            ExchangeRateTimestamp = marketRate.LastUpdated,
            IsRateCached = marketRate.IsStale,
            Accounts = accounts,
            RecentTransactions = recentTransactions.Take(10).ToList()
        };
    }
}
