using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class DashboardServiceTests
{
    private static DashboardService CreateService(MonetraDbContext context)
    {
        var market = context.CreateMarketService();
        var transactions = new TransactionService(context);
        return new DashboardService(context, new AccountService(context, market), transactions, market);
    }

    private static async Task AddTransactionAsync(MonetraDbContext context, int accountId, decimal amount, TransactionType type, DateTime date)
    {
        context.Transactions.Add(new Transaction { AccountId = accountId, Amount = amount, Type = type, Date = date });
        await context.SaveChangesAsync();
    }

    private static DateTime LastDayOfPreviousMonth()
    {
        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, 1, 12, 0, 0, DateTimeKind.Utc).AddDays(-1);
    }

    [Fact]
    public async Task GetSummary_InCop_AddsUsdAccountsAtTheExchangeRate()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m);
        var user = await context.AddUserAsync();
        await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        await context.AddAccountAsync(user.Id, "USD", 100m);

        var summary = await CreateService(context).GetSummaryAsync(user.Id, "COP");

        Assert.Equal(1_400_000m, summary.TotalNetWorth);
        Assert.Equal("COP", summary.BaseCurrency);
    }

    [Fact]
    public async Task GetSummary_InUsd_ConvertsCopAccountsAtTheExchangeRate()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m);
        var user = await context.AddUserAsync();
        await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        await context.AddAccountAsync(user.Id, "USD", 100m);

        var summary = await CreateService(context).GetSummaryAsync(user.Id, "usd");

        Assert.Equal(350m, summary.TotalNetWorth);
        Assert.Equal("USD", summary.BaseCurrency);
    }

    [Fact]
    public async Task GetSummary_MonthlyTotals_ConvertEachMovementToTheBaseCurrency()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m);
        var user = await context.AddUserAsync();
        var cop = await context.AddAccountAsync(user.Id, "COP", 0m);
        var usd = await context.AddAccountAsync(user.Id, "USD", 0m);
        await AddTransactionAsync(context, cop.Id, 500_000m, TransactionType.Income, DateTime.UtcNow);
        await AddTransactionAsync(context, usd.Id, 50m, TransactionType.Expense, DateTime.UtcNow);

        var summary = await CreateService(context).GetSummaryAsync(user.Id, "COP");

        Assert.Equal(500_000m, summary.MonthlyIncome);
        Assert.Equal(200_000m, summary.MonthlyExpense);
        Assert.Equal(300_000m, summary.NetSavingsThisMonth);
    }

    [Fact]
    public async Task GetSummary_MonthlyTotals_IgnoreMovementsFromPreviousMonths()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 0m);
        await AddTransactionAsync(context, account.Id, 70_000m, TransactionType.Expense, DateTime.UtcNow);
        await AddTransactionAsync(context, account.Id, 999_000m, TransactionType.Expense, LastDayOfPreviousMonth());
        await AddTransactionAsync(context, account.Id, 888_000m, TransactionType.Income, LastDayOfPreviousMonth());

        var summary = await CreateService(context).GetSummaryAsync(user.Id, "COP");

        Assert.Equal(70_000m, summary.MonthlyExpense);
        Assert.Equal(0m, summary.MonthlyIncome);
    }

    [Fact]
    public async Task GetSummary_RecentTransactions_AreTheTenNewest()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 0m);
        for (var daysAgo = 1; daysAgo <= 12; daysAgo++)
        {
            await AddTransactionAsync(context, account.Id, daysAgo, TransactionType.Expense, DateTime.UtcNow.AddDays(-daysAgo));
        }

        var summary = await CreateService(context).GetSummaryAsync(user.Id, "COP");

        Assert.Equal([1m, 2m, 3m, 4m, 5m, 6m, 7m, 8m, 9m, 10m], summary.RecentTransactions.Select(t => t.Amount));
    }

    [Fact]
    public async Task GetSummary_ExcludesOtherUsersAccountsAndMovements()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        await context.AddAccountAsync(user.Id, "COP", 10_000m);
        var otherAccount = await context.AddAccountAsync(other.Id, "COP", 5_000_000m);
        await AddTransactionAsync(context, otherAccount.Id, 777_000m, TransactionType.Income, DateTime.UtcNow);

        var summary = await CreateService(context).GetSummaryAsync(user.Id, "COP");

        Assert.Equal(10_000m, summary.TotalNetWorth);
        Assert.Equal(0m, summary.MonthlyIncome);
        Assert.Empty(summary.RecentTransactions);
    }

    [Fact]
    public async Task GetSummary_ReportsTheExchangeRateItUsed()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m);
        var user = await context.AddUserAsync();

        var summary = await CreateService(context).GetSummaryAsync(user.Id, "COP");

        Assert.Equal(4_000m, summary.UsdToCopExchangeRate);
        Assert.False(summary.IsRateCached);
    }
}
