using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Debt;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class DebtServiceTests
{
    private static DebtService CreateService(MonetraDbContext context) => new(context, new TransactionService(context));

    private static async Task<Debt> AddDebtAsync(
        MonetraDbContext context,
        int userId,
        decimal remaining,
        decimal interestRate = 0m,
        string rateType = "EA",
        DateTime? dueDate = null,
        string creditor = "Bancolombia")
    {
        var debt = new Debt
        {
            UserId = userId,
            Creditor = creditor,
            TotalAmount = remaining,
            RemainingAmount = remaining,
            InterestRate = interestRate,
            RateType = rateType,
            DueDate = dueDate ?? DateTime.UtcNow.AddMonths(12)
        };
        context.Debts.Add(debt);
        await context.SaveChangesAsync();
        return debt;
    }

    [Fact]
    public async Task CreateDebt_StartsOwingTheFullAmount()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();

        var debt = await CreateService(context).CreateDebtAsync(user.Id, new CreateDebtDto
        {
            Creditor = "  Davivienda ",
            TotalAmount = 5_000_000m,
            InterestRate = 1.2m,
            RateType = "em",
            DueDate = DateTime.UtcNow.AddMonths(24)
        });

        Assert.Equal(5_000_000m, debt.RemainingAmount);
        Assert.Equal("Davivienda", debt.Creditor);
        Assert.Equal("EM", debt.RateType);
    }

    [Fact]
    public async Task EstimatedMonthlyPayment_WithoutInterest_SplitsTheBalanceEvenly()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        await AddDebtAsync(context, user.Id, 1_200_000m, interestRate: 0m, dueDate: DateTime.UtcNow.AddMonths(12));

        var debt = Assert.Single(await CreateService(context).GetUserDebtsAsync(user.Id));

        Assert.Equal(100_000m, debt.EstimatedMonthlyPayment);
    }

    [Fact]
    public async Task EstimatedMonthlyPayment_WithMonthlyRate_UsesTheAnnuityFormula()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        await AddDebtAsync(context, user.Id, 1_200_000m, interestRate: 1.5m, rateType: "EM", dueDate: DateTime.UtcNow.AddMonths(12));

        var debt = Assert.Single(await CreateService(context).GetUserDebtsAsync(user.Id));

        // 1.200.000 a 12 meses al 1,5 % mensual: P·r(1+r)^n / ((1+r)^n − 1)
        Assert.Equal(110_015.99m, debt.EstimatedMonthlyPayment);
    }

    [Fact]
    public async Task EstimatedMonthlyPayment_WithAnnualRate_ConvertsItToItsMonthlyEquivalent()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        await AddDebtAsync(context, user.Id, 1_200_000m, interestRate: 18m, rateType: "EA", dueDate: DateTime.UtcNow.AddMonths(12));

        var debt = Assert.Single(await CreateService(context).GetUserDebtsAsync(user.Id));

        // 18 % EA equivale a 1,3888 % mensual: (1,18)^(1/12) − 1
        Assert.Equal(109_255.65m, debt.EstimatedMonthlyPayment);
    }

    [Fact]
    public async Task EstimatedMonthlyPayment_WhenOverdue_IsTheWholeBalance()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        await AddDebtAsync(context, user.Id, 500_000m, interestRate: 0m, dueDate: DateTime.UtcNow.AddMonths(-3));

        var debt = Assert.Single(await CreateService(context).GetUserDebtsAsync(user.Id));

        Assert.Equal(500_000m, debt.EstimatedMonthlyPayment);
    }

    [Fact]
    public async Task RecordPayment_ReducesTheDebtAndChargesTheAccount()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 2_000_000m);
        var debt = await AddDebtAsync(context, user.Id, 1_000_000m);

        var result = await CreateService(context).RecordPaymentAsync(
            debt.Id, user.Id, new RecordDebtPaymentDto { AccountId = account.Id, Amount = 300_000m });

        Assert.Equal(700_000m, result.RemainingAmount);
        Assert.Equal(700_000m, context.Debts.AsNoTracking().Single().RemainingAmount);
        Assert.Equal(1_700_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task RecordPayment_LeavesAnExpenseMovementNamingTheCreditor()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 2_000_000m);
        var debt = await AddDebtAsync(context, user.Id, 1_000_000m, creditor: "Bancolombia");

        await CreateService(context).RecordPaymentAsync(
            debt.Id, user.Id, new RecordDebtPaymentDto { AccountId = account.Id, Amount = 300_000m });

        var movement = Assert.Single(context.Transactions.AsNoTracking());
        Assert.Equal(300_000m, movement.Amount);
        Assert.Equal(TransactionType.Expense, movement.Type);
        Assert.Equal(account.Id, movement.AccountId);
        Assert.Contains("Bancolombia", movement.Description);
    }

    [Fact]
    public async Task RecordPayment_LargerThanTheBalance_NeverLeavesANegativeDebt()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 2_000_000m);
        var debt = await AddDebtAsync(context, user.Id, 100_000m);

        var result = await CreateService(context).RecordPaymentAsync(
            debt.Id, user.Id, new RecordDebtPaymentDto { AccountId = account.Id, Amount = 150_000m });

        Assert.Equal(0m, result.RemainingAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50_000)]
    public async Task RecordPayment_WithNonPositiveAmount_IsRejectedAndChangesNothing(int amount)
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 2_000_000m);
        var debt = await AddDebtAsync(context, user.Id, 1_000_000m);

        await Assert.ThrowsAsync<ValidationException>(() => CreateService(context).RecordPaymentAsync(
            debt.Id, user.Id, new RecordDebtPaymentDto { AccountId = account.Id, Amount = amount }));

        Assert.Equal(1_000_000m, context.Debts.AsNoTracking().Single().RemainingAmount);
        Assert.Equal(2_000_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task RecordPayment_FromAnotherUsersAccount_IsRejectedAndChangesNothing()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        var othersAccount = await context.AddAccountAsync(other.Id, "COP", 2_000_000m);
        var debt = await AddDebtAsync(context, user.Id, 1_000_000m);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(context).RecordPaymentAsync(
            debt.Id, user.Id, new RecordDebtPaymentDto { AccountId = othersAccount.Id, Amount = 300_000m }));

        Assert.Equal(1_000_000m, context.Debts.AsNoTracking().Single().RemainingAmount);
        Assert.Equal(2_000_000m, context.BalanceOf(othersAccount.Id));
    }

    [Fact]
    public async Task RecordPayment_OnAnotherUsersDebt_IsRejected()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        var account = await context.AddAccountAsync(user.Id, "COP", 2_000_000m);
        var othersDebt = await AddDebtAsync(context, other.Id, 1_000_000m);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(context).RecordPaymentAsync(
            othersDebt.Id, user.Id, new RecordDebtPaymentDto { AccountId = account.Id, Amount = 300_000m }));

        Assert.Equal(2_000_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task GetUserDebts_ReturnsOwnDebtsSoonestDueFirst()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        await AddDebtAsync(context, user.Id, 1m, dueDate: DateTime.UtcNow.AddMonths(9), creditor: "Tercera");
        await AddDebtAsync(context, user.Id, 1m, dueDate: DateTime.UtcNow.AddMonths(2), creditor: "Primera");
        await AddDebtAsync(context, user.Id, 1m, dueDate: DateTime.UtcNow.AddMonths(5), creditor: "Segunda");
        await AddDebtAsync(context, other.Id, 1m, dueDate: DateTime.UtcNow.AddMonths(1), creditor: "Ajena");

        var debts = await CreateService(context).GetUserDebtsAsync(user.Id);

        Assert.Equal(["Primera", "Segunda", "Tercera"], debts.Select(d => d.Creditor));
    }

    [Fact]
    public async Task DeleteDebt_RemovesTheDebt()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var debt = await AddDebtAsync(context, user.Id, 1_000_000m);

        await CreateService(context).DeleteDebtAsync(debt.Id, user.Id);

        Assert.Empty(context.Debts.AsNoTracking());
    }

    [Fact]
    public async Task DeleteDebt_OfAnotherUser_IsRejectedAndKeepsTheDebt()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var debt = await AddDebtAsync(context, owner.Id, 1_000_000m);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(context).DeleteDebtAsync(debt.Id, intruder.Id));

        Assert.Single(context.Debts.AsNoTracking());
    }
}
