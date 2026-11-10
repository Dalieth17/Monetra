using Monetra.Application.DTOs.Subscription;
using Xunit;

namespace Monetra.UnitTests.Domain;

public class SubscriptionDtoTests
{
    [Fact]
    public void SubscriptionDto_DaysUntilPayment_ShouldBePositive_WhenPaymentIsInFuture()
    {
        var subscription = new SubscriptionDto
        {
            Name = "Netflix",
            NextPaymentDate = DateTime.UtcNow.AddDays(10)
        };

        Assert.Equal(10, subscription.DaysUntilPayment);
    }

    [Fact]
    public void SubscriptionDto_DaysUntilPayment_ShouldBeZero_WhenPaymentDateAlreadyPassed()
    {
        var subscription = new SubscriptionDto
        {
            Name = "Spotify",
            NextPaymentDate = DateTime.UtcNow.AddDays(-5)
        };

        Assert.Equal(0, subscription.DaysUntilPayment);
    }

    [Fact]
    public void SubscriptionDto_DaysUntilPayment_ShouldBeZero_WhenPaymentIsToday()
    {
        var subscription = new SubscriptionDto
        {
            Name = "Disney+",
            NextPaymentDate = DateTime.UtcNow
        };

        Assert.Equal(0, subscription.DaysUntilPayment);
    }
}
