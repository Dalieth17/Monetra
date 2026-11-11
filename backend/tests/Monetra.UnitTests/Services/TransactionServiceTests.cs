using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Transaction;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class TransactionServiceTests
{
    [Fact]
    public async Task CreateTransaction_Income_AddsAmountToAccountBalance()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 100_000m);
        var service = new TransactionService(context);

        await service.CreateTransactionAsync(user.Id, new CreateTransactionDto
        {
            AccountId = account.Id,
            Amount = 50_000m,
            Type = TransactionType.Income,
            Description = "Pago quincena"
        });

        Assert.Equal(150_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task CreateTransaction_Expense_SubtractsAmountFromAccountBalance()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 100_000m);
        var service = new TransactionService(context);

        await service.CreateTransactionAsync(user.Id, new CreateTransactionDto
        {
            AccountId = account.Id,
            Amount = 30_000m,
            Type = TransactionType.Expense,
            Description = "Mercado"
        });

        Assert.Equal(70_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task CreateTransaction_ReturnsCategoryAndAccountDetails()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "USD", 500m);
        var service = new TransactionService(context);

        var result = await service.CreateTransactionAsync(user.Id, new CreateTransactionDto
        {
            AccountId = account.Id,
            CategoryId = 1,
            Amount = 20m,
            Type = TransactionType.Expense,
            Description = "  Almuerzo  "
        });

        Assert.Equal("Alimentación", result.CategoryName);
        Assert.Equal("restaurant", result.CategoryIcon);
        Assert.Equal("USD", result.CurrencyCode);
        Assert.Equal("Almuerzo", result.Description);
    }

    [Fact]
    public async Task CreateTransaction_OnAnotherUsersAccount_IsRejectedAndChangesNothing()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var account = await context.AddAccountAsync(owner.Id, "COP", 100_000m);
        var service = new TransactionService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateTransactionAsync(intruder.Id, new CreateTransactionDto
        {
            AccountId = account.Id,
            Amount = 99_000m,
            Type = TransactionType.Expense
        }));

        Assert.Equal(100_000m, context.BalanceOf(account.Id));
        Assert.Empty(context.Transactions.AsNoTracking());
    }

    [Fact]
    public async Task CreateTransaction_WithANonexistentCategory_IsRejectedAndChangesNothing()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 100_000m);
        var service = new TransactionService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateTransactionAsync(user.Id, new CreateTransactionDto
        {
            AccountId = account.Id,
            CategoryId = 9_999,
            Amount = 30_000m,
            Type = TransactionType.Expense
        }));

        Assert.Equal(100_000m, context.BalanceOf(account.Id));
        Assert.Empty(context.Transactions.AsNoTracking());
    }

    [Fact]
    public async Task DeleteTransaction_Expense_ReturnsAmountToAccountBalance()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 100_000m);
        var service = new TransactionService(context);
        var expense = await service.CreateTransactionAsync(user.Id, new CreateTransactionDto
        {
            AccountId = account.Id,
            Amount = 30_000m,
            Type = TransactionType.Expense
        });

        await service.DeleteTransactionAsync(expense.Id, user.Id);

        Assert.Equal(100_000m, context.BalanceOf(account.Id));
        Assert.Empty(context.Transactions.AsNoTracking());
    }

    [Fact]
    public async Task DeleteTransaction_Income_RemovesAmountFromAccountBalance()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 100_000m);
        var service = new TransactionService(context);
        var income = await service.CreateTransactionAsync(user.Id, new CreateTransactionDto
        {
            AccountId = account.Id,
            Amount = 40_000m,
            Type = TransactionType.Income
        });

        await service.DeleteTransactionAsync(income.Id, user.Id);

        Assert.Equal(100_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task DeleteTransaction_OfAnotherUser_IsRejectedAndKeepsTransaction()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var account = await context.AddAccountAsync(owner.Id, "COP", 100_000m);
        var service = new TransactionService(context);
        var expense = await service.CreateTransactionAsync(owner.Id, new CreateTransactionDto
        {
            AccountId = account.Id,
            Amount = 30_000m,
            Type = TransactionType.Expense
        });

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteTransactionAsync(expense.Id, intruder.Id));

        Assert.Equal(70_000m, context.BalanceOf(account.Id));
        Assert.Single(context.Transactions.AsNoTracking());
    }

    [Fact]
    public async Task GetTransactionById_OfAnotherUser_IsRejected()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var account = await context.AddAccountAsync(owner.Id, "COP", 0m);
        var transaction = await AddTransactionAsync(context, account.Id, 10_000m, DateTime.UtcNow);
        var service = new TransactionService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetTransactionByIdAsync(transaction.Id, intruder.Id));
    }

    [Fact]
    public async Task GetTransactions_ReturnsOnlyTheUsersOwnTransactions()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        var ownAccount = await context.AddAccountAsync(user.Id, "COP", 0m);
        var otherAccount = await context.AddAccountAsync(other.Id, "COP", 0m);
        await AddTransactionAsync(context, ownAccount.Id, 11_000m, DateTime.UtcNow);
        await AddTransactionAsync(context, otherAccount.Id, 22_000m, DateTime.UtcNow);
        var service = new TransactionService(context);

        var result = await service.GetTransactionsAsync(user.Id);

        var only = Assert.Single(result);
        Assert.Equal(11_000m, only.Amount);
    }

    [Fact]
    public async Task GetTransactions_FiltersByAccount()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var cop = await context.AddAccountAsync(user.Id, "COP", 0m);
        var usd = await context.AddAccountAsync(user.Id, "USD", 0m);
        await AddTransactionAsync(context, cop.Id, 11_000m, DateTime.UtcNow);
        await AddTransactionAsync(context, usd.Id, 22m, DateTime.UtcNow);
        var service = new TransactionService(context);

        var result = await service.GetTransactionsAsync(user.Id, accountId: usd.Id);

        var only = Assert.Single(result);
        Assert.Equal(22m, only.Amount);
    }

    [Fact]
    public async Task GetTransactions_FiltersByCategory()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 0m);
        await AddTransactionAsync(context, account.Id, 11_000m, DateTime.UtcNow, categoryId: 1);
        await AddTransactionAsync(context, account.Id, 22_000m, DateTime.UtcNow, categoryId: 2);
        var service = new TransactionService(context);

        var result = await service.GetTransactionsAsync(user.Id, categoryId: 2);

        var only = Assert.Single(result);
        Assert.Equal("Transporte", only.CategoryName);
    }

    [Fact]
    public async Task GetTransactions_FiltersByInclusiveDateRange()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 0m);
        await AddTransactionAsync(context, account.Id, 1m, new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc));
        await AddTransactionAsync(context, account.Id, 2m, new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc));
        await AddTransactionAsync(context, account.Id, 3m, new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc));
        await AddTransactionAsync(context, account.Id, 4m, new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc));
        var service = new TransactionService(context);

        var result = await service.GetTransactionsAsync(
            user.Id,
            from: new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            to: new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal([3m, 2m], result.Select(t => t.Amount));
    }

    [Fact]
    public async Task GetTransactions_ReturnsNewestFirst()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 0m);
        await AddTransactionAsync(context, account.Id, 1m, new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));
        await AddTransactionAsync(context, account.Id, 2m, new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc));
        await AddTransactionAsync(context, account.Id, 3m, new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc));
        var service = new TransactionService(context);

        var result = await service.GetTransactionsAsync(user.Id);

        Assert.Equal([2m, 3m, 1m], result.Select(t => t.Amount));
    }

    private static async Task<Transaction> AddTransactionAsync(
        MonetraDbContext context,
        int accountId,
        decimal amount,
        DateTime date,
        int? categoryId = null)
    {
        var transaction = new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = amount,
            Type = TransactionType.Expense,
            Date = date
        };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }
}
