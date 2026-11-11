using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.SavingsGoal;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class SavingsGoalServiceTests
{
    private static SavingsGoalService CreateService(MonetraDbContext context) => new(context, new TransactionService(context));

    private static async Task<SavingsGoal> AddGoalAsync(
        MonetraDbContext context,
        int userId,
        decimal target,
        decimal current = 0m,
        string name = "Viaje a San Andrés",
        DateTime? deadline = null)
    {
        var goal = new SavingsGoal
        {
            UserId = userId,
            Name = name,
            TargetAmount = target,
            CurrentAmount = current,
            Deadline = deadline ?? DateTime.UtcNow.AddMonths(6)
        };
        context.SavingsGoals.Add(goal);
        await context.SaveChangesAsync();
        return goal;
    }

    [Fact]
    public async Task CreateGoal_StartsWithNothingSaved()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();

        var goal = await CreateService(context).CreateGoalAsync(user.Id, new CreateSavingsGoalDto
        {
            Name = "  Fondo de emergencia ",
            TargetAmount = 3_000_000m,
            Deadline = DateTime.UtcNow.AddMonths(12)
        });

        Assert.Equal(0m, goal.CurrentAmount);
        Assert.Equal(0.0, goal.ProgressPercentage);
        Assert.Equal("Fondo de emergencia", goal.Name);
    }

    [Fact]
    public async Task Deposit_AddsToTheGoalAndChargesTheAccount()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        var goal = await AddGoalAsync(context, user.Id, target: 2_000_000m, current: 100_000m);

        var result = await CreateService(context).DepositAsync(
            goal.Id, user.Id, new DepositSavingsGoalDto { AccountId = account.Id, Amount = 400_000m });

        Assert.Equal(500_000m, result.CurrentAmount);
        Assert.Equal(25.0, result.ProgressPercentage);
        Assert.Equal(500_000m, context.SavingsGoals.AsNoTracking().Single().CurrentAmount);
        Assert.Equal(600_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task Deposit_LeavesAnExpenseMovementNamingTheGoal()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        var goal = await AddGoalAsync(context, user.Id, target: 2_000_000m, name: "Moto nueva");

        await CreateService(context).DepositAsync(
            goal.Id, user.Id, new DepositSavingsGoalDto { AccountId = account.Id, Amount = 400_000m });

        var movement = Assert.Single(context.Transactions.AsNoTracking());
        Assert.Equal(400_000m, movement.Amount);
        Assert.Equal(TransactionType.Expense, movement.Type);
        Assert.Contains("Moto nueva", movement.Description);
    }

    [Fact]
    public async Task Deposit_ThatReachesTheTarget_CompletesTheGoal()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        var goal = await AddGoalAsync(context, user.Id, target: 500_000m, current: 400_000m);

        var result = await CreateService(context).DepositAsync(
            goal.Id, user.Id, new DepositSavingsGoalDto { AccountId = account.Id, Amount = 100_000m });

        Assert.True(result.IsCompleted);
        Assert.Equal(100.0, result.ProgressPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10_000)]
    public async Task Deposit_WithNonPositiveAmount_IsRejectedAndChangesNothing(int amount)
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        var goal = await AddGoalAsync(context, user.Id, target: 2_000_000m, current: 100_000m);

        await Assert.ThrowsAsync<ValidationException>(() => CreateService(context).DepositAsync(
            goal.Id, user.Id, new DepositSavingsGoalDto { AccountId = account.Id, Amount = amount }));

        Assert.Equal(100_000m, context.SavingsGoals.AsNoTracking().Single().CurrentAmount);
        Assert.Equal(1_000_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task Deposit_FromAnotherUsersAccount_IsRejectedAndChangesNothing()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        var othersAccount = await context.AddAccountAsync(other.Id, "COP", 1_000_000m);
        var goal = await AddGoalAsync(context, user.Id, target: 2_000_000m);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(context).DepositAsync(
            goal.Id, user.Id, new DepositSavingsGoalDto { AccountId = othersAccount.Id, Amount = 400_000m }));

        Assert.Equal(0m, context.SavingsGoals.AsNoTracking().Single().CurrentAmount);
        Assert.Equal(1_000_000m, context.BalanceOf(othersAccount.Id));
    }

    [Fact]
    public async Task Deposit_IntoAnotherUsersGoal_IsRejected()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        var account = await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        var othersGoal = await AddGoalAsync(context, other.Id, target: 2_000_000m);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(context).DepositAsync(
            othersGoal.Id, user.Id, new DepositSavingsGoalDto { AccountId = account.Id, Amount = 400_000m }));

        Assert.Equal(1_000_000m, context.BalanceOf(account.Id));
    }

    [Fact]
    public async Task GetUserGoals_ReturnsOwnGoalsNearestDeadlineFirst()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        await AddGoalAsync(context, user.Id, 1m, name: "Segunda", deadline: DateTime.UtcNow.AddMonths(8));
        await AddGoalAsync(context, user.Id, 1m, name: "Primera", deadline: DateTime.UtcNow.AddMonths(3));
        await AddGoalAsync(context, other.Id, 1m, name: "Ajena", deadline: DateTime.UtcNow.AddMonths(1));

        var goals = await CreateService(context).GetUserGoalsAsync(user.Id);

        Assert.Equal(["Primera", "Segunda"], goals.Select(g => g.Name));
    }

    [Fact]
    public async Task DeleteGoal_RemovesTheGoal()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var goal = await AddGoalAsync(context, user.Id, 1_000_000m);

        await CreateService(context).DeleteGoalAsync(goal.Id, user.Id);

        Assert.Empty(context.SavingsGoals.AsNoTracking());
    }

    [Fact]
    public async Task DeleteGoal_OfAnotherUser_IsRejectedAndKeepsTheGoal()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var goal = await AddGoalAsync(context, owner.Id, 1_000_000m);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(context).DeleteGoalAsync(goal.Id, intruder.Id));

        Assert.Single(context.SavingsGoals.AsNoTracking());
    }
}
