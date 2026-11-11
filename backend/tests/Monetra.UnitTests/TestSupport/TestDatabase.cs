using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Monetra.Domain.Entities;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;

namespace Monetra.UnitTests.TestSupport;

internal static class TestDatabase
{
    public static MonetraDbContext Create()
    {
        var options = new DbContextOptionsBuilder<MonetraDbContext>()
            .UseInMemoryDatabase($"monetra-tests-{Guid.NewGuid():N}")
            .Options;

        var context = new MonetraDbContext(options);
        context.Database.EnsureCreated(); // aplica el seed de roles, categorías y TRM
        return context;
    }

    public static async Task<User> AddUserAsync(this MonetraDbContext context, string email = "luiger@monetra.com")
    {
        var user = new User { Name = "Usuario de prueba", Email = email, PasswordHash = "hash", RoleId = 1 };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public static async Task<Account> AddAccountAsync(this MonetraDbContext context, int userId, string currencyCode, decimal balance)
    {
        var account = new Account
        {
            UserId = userId,
            Name = $"Cuenta {currencyCode}",
            CurrencyCode = currencyCode,
            CurrentBalance = balance
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }

    public static async Task SetUsdCopRateAsync(this MonetraDbContext context, decimal rate, DateTime? lastUpdatedAt = null)
    {
        var cached = await context.ExchangeRateCaches.SingleAsync(e => e.BaseCurrency == "USD" && e.TargetCurrency == "COP");
        cached.Rate = rate;
        cached.LastUpdatedAt = lastUpdatedAt ?? DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public static MarketService CreateMarketService(this MonetraDbContext context, StubHttpMessageHandler? handler = null)
    {
        return new MarketService(
            context,
            NullLogger<MarketService>.Instance,
            new HttpClient(handler ?? StubHttpMessageHandler.Unreachable()));
    }

    public static decimal BalanceOf(this MonetraDbContext context, int accountId)
    {
        return context.Accounts.AsNoTracking().Single(a => a.Id == accountId).CurrentBalance;
    }
}
