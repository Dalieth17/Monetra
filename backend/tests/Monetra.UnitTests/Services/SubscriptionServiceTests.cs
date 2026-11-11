using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Subscription;
using Monetra.Domain.Entities;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class SubscriptionServiceTests
{
    private static async Task<Subscription> AddSubscriptionAsync(
        MonetraDbContext context,
        int userId,
        string name = "Netflix",
        DateTime? nextPaymentDate = null,
        bool isActive = true)
    {
        var subscription = new Subscription
        {
            UserId = userId,
            Name = name,
            Amount = 38_900m,
            CurrencyCode = "COP",
            BillingCycle = "Monthly",
            NextPaymentDate = nextPaymentDate ?? DateTime.UtcNow.AddDays(10),
            IsActive = isActive
        };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();
        return subscription;
    }

    [Fact]
    public async Task CreateSubscription_IsActiveAndNormalizesItsFields()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();

        var created = await new SubscriptionService(context).CreateSubscriptionAsync(user.Id, new CreateSubscriptionDto
        {
            Name = "  Spotify ",
            Amount = 5.99m,
            CurrencyCode = " usd ",
            BillingCycle = "Monthly",
            NextPaymentDate = DateTime.UtcNow.AddDays(5)
        });

        Assert.True(created.IsActive);
        Assert.Equal("Spotify", created.Name);
        Assert.Equal("USD", created.CurrencyCode);
        Assert.Equal(5, created.DaysUntilPayment);
    }

    [Fact]
    public async Task ToggleStatus_PausesAnActiveSubscription()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var subscription = await AddSubscriptionAsync(context, user.Id, isActive: true);

        var result = await new SubscriptionService(context).ToggleStatusAsync(subscription.Id, user.Id);

        Assert.False(result.IsActive);
        Assert.False(context.Subscriptions.AsNoTracking().Single().IsActive);
    }

    [Fact]
    public async Task ToggleStatus_ReactivatesAPausedSubscription()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var subscription = await AddSubscriptionAsync(context, user.Id, isActive: false);

        var result = await new SubscriptionService(context).ToggleStatusAsync(subscription.Id, user.Id);

        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ToggleStatus_OfAnotherUser_IsRejectedAndKeepsTheStatus()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var subscription = await AddSubscriptionAsync(context, owner.Id, isActive: true);

        await Assert.ThrowsAsync<NotFoundException>(
            () => new SubscriptionService(context).ToggleStatusAsync(subscription.Id, intruder.Id));

        Assert.True(context.Subscriptions.AsNoTracking().Single().IsActive);
    }

    [Fact]
    public async Task GetUserSubscriptions_ReturnsOwnSubscriptionsNextChargeFirst()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync("user@monetra.com");
        var other = await context.AddUserAsync("other@monetra.com");
        await AddSubscriptionAsync(context, user.Id, "Segunda", DateTime.UtcNow.AddDays(20));
        await AddSubscriptionAsync(context, user.Id, "Primera", DateTime.UtcNow.AddDays(2));
        await AddSubscriptionAsync(context, other.Id, "Ajena", DateTime.UtcNow.AddDays(1));

        var subscriptions = await new SubscriptionService(context).GetUserSubscriptionsAsync(user.Id);

        Assert.Equal(["Primera", "Segunda"], subscriptions.Select(s => s.Name));
    }

    [Fact]
    public async Task DeleteSubscription_RemovesTheSubscription()
    {
        using var context = TestDatabase.Create();
        var user = await context.AddUserAsync();
        var subscription = await AddSubscriptionAsync(context, user.Id);

        await new SubscriptionService(context).DeleteSubscriptionAsync(subscription.Id, user.Id);

        Assert.Empty(context.Subscriptions.AsNoTracking());
    }

    [Fact]
    public async Task DeleteSubscription_OfAnotherUser_IsRejectedAndKeepsTheSubscription()
    {
        using var context = TestDatabase.Create();
        var owner = await context.AddUserAsync("owner@monetra.com");
        var intruder = await context.AddUserAsync("intruder@monetra.com");
        var subscription = await AddSubscriptionAsync(context, owner.Id);

        await Assert.ThrowsAsync<NotFoundException>(
            () => new SubscriptionService(context).DeleteSubscriptionAsync(subscription.Id, intruder.Id));

        Assert.Single(context.Subscriptions.AsNoTracking());
    }
}
