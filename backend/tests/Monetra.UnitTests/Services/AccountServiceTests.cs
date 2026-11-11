using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Account;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class AccountServiceTests
{
    private static AccountService CreateService(MonetraDbContext context) => new(context, context.CreateMarketService());

    [Fact]
    public async Task CreateAccount_WithInitialBalance_RecordsTheOpeningDepositExactlyOnce()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var service = CreateService(context);

        var created = await service.CreateAccountAsync(user.Id, new CreateAccountDto
        {
            Name = "Ahorros",
            CurrencyCode = "COP",
            InitialBalance = 250_000m
        });

        Assert.Equal(250_000m, context.BalanceOf(created.Id));
        var opening = Assert.Single(context.Transactions.AsNoTracking());
        Assert.Equal(250_000m, opening.Amount);
        Assert.Equal(TransactionType.Income, opening.Type);
    }

    [Fact]
    public async Task CreateAccount_WithZeroBalance_RecordsNoTransaction()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var service = CreateService(context);

        await service.CreateAccountAsync(user.Id, new CreateAccountDto { Name = "Billetera", CurrencyCode = "COP" });

        Assert.Empty(context.Transactions.AsNoTracking());
    }

    [Fact]
    public async Task CreateAccount_NormalizesCurrencyCodeAndName()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var service = CreateService(context);

        var created = await service.CreateAccountAsync(user.Id, new CreateAccountDto { Name = "  Dólares  ", CurrencyCode = " usd " });

        Assert.Equal("USD", created.CurrencyCode);
        Assert.Equal("Dólares", created.Name);
    }

    [Fact]
    public async Task GetUserAccounts_ConvertsUsdBalancesToCop()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m);
        var user = await context.AddUserAsync();
        await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        await context.AddAccountAsync(user.Id, "USD", 100m);
        var service = CreateService(context);

        var accounts = await service.GetUserAccountsAsync(user.Id, "COP");

        Assert.Equal([1_000_000m, 400_000m], accounts.Select(a => a.ConvertedBalance));
        Assert.Equal([1_000_000m, 100m], accounts.Select(a => a.CurrentBalance));
    }

    [Fact]
    public async Task GetUserAccounts_ConvertsCopBalancesToUsd()
    {
        using var context = TestDatabase.Create();
        await context.SetUsdCopRateAsync(4_000m);
        var user = await context.AddUserAsync();
        await context.AddAccountAsync(user.Id, "COP", 1_000_000m);
        await context.AddAccountAsync(user.Id, "USD", 100m);
        var service = CreateService(context);

        var accounts = await service.GetUserAccountsAsync(user.Id, "USD");

        Assert.Equal([250m, 100m], accounts.Select(a => a.ConvertedBalance));
    }

    [Fact]
    public async Task GetUserAccounts_ReturnsOnlyTheUsersOwnAccounts()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        await context.AddAccountAsync(user.Id, "COP", 111m);
        await context.AddAccountAsync(other.Id, "COP", 999m);
        var service = CreateService(context);

        var accounts = await service.GetUserAccountsAsync(user.Id);

        var only = Assert.Single(accounts);
        Assert.Equal(111m, only.CurrentBalance);
    }

    [Fact]
    public async Task GetAccountById_OfAnotherUser_IsRejected()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var account = await context.AddAccountAsync(owner.Id, "COP", 100m);
        var service = CreateService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAccountByIdAsync(account.Id, intruder.Id));
    }

    [Fact]
    public async Task UpdateAccount_RenamesTheAccount()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 100m);
        var service = CreateService(context);

        await service.UpdateAccountAsync(account.Id, user.Id, new UpdateAccountDto { Name = " Fondo de viaje " });

        Assert.Equal("Fondo de viaje", context.Accounts.AsNoTracking().Single().Name);
    }

    [Fact]
    public async Task UpdateAccount_OfAnotherUser_IsRejectedAndKeepsTheName()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var account = await context.AddAccountAsync(owner.Id, "COP", 100m);
        var service = CreateService(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAccountAsync(account.Id, intruder.Id, new UpdateAccountDto { Name = "Robada" }));

        Assert.Equal("Cuenta COP", context.Accounts.AsNoTracking().Single().Name);
    }

    [Fact]
    public async Task DeleteAccount_RemovesTheAccount()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var account = await context.AddAccountAsync(user.Id, "COP", 100m);
        var service = CreateService(context);

        await service.DeleteAccountAsync(account.Id, user.Id);

        Assert.Empty(context.Accounts.AsNoTracking());
    }

    [Fact]
    public async Task DeleteAccount_OfAnotherUser_IsRejectedAndKeepsTheAccount()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var account = await context.AddAccountAsync(owner.Id, "COP", 100m);
        var service = CreateService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAccountAsync(account.Id, intruder.Id));

        Assert.Single(context.Accounts.AsNoTracking());
    }
}
