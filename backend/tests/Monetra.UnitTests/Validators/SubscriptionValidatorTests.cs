using Monetra.Application.DTOs.Subscription;
using Monetra.Application.Validators.Subscription;
using Xunit;

namespace Monetra.UnitTests.Validators;

public class SubscriptionValidatorTests
{
    [Fact]
    public void CreateSubscriptionValidator_ShouldPass_WhenSubscriptionIsValid()
    {
        var validator = new CreateSubscriptionValidator();
        var model = new CreateSubscriptionDto
        {
            Name = "Netflix",
            Amount = 38900m,
            CurrencyCode = "COP",
            BillingCycle = "Monthly",
            NextPaymentDate = DateTime.UtcNow.AddDays(15)
        };

        var result = validator.Validate(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateSubscriptionValidator_ShouldFail_WhenNameIsEmpty()
    {
        var validator = new CreateSubscriptionValidator();
        var model = new CreateSubscriptionDto
        {
            Name = "",
            Amount = 38900m,
            BillingCycle = "Monthly"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateSubscriptionValidator_ShouldFail_WhenAmountIsZero()
    {
        var validator = new CreateSubscriptionValidator();
        var model = new CreateSubscriptionDto
        {
            Name = "Spotify",
            Amount = 0m,
            BillingCycle = "Monthly"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Theory]
    [InlineData("Daily")]
    [InlineData("Quarterly")]
    [InlineData("")]
    public void CreateSubscriptionValidator_ShouldFail_WhenBillingCycleIsInvalid(string cycle)
    {
        var validator = new CreateSubscriptionValidator();
        var model = new CreateSubscriptionDto
        {
            Name = "Spotify",
            Amount = 16900m,
            BillingCycle = cycle
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BillingCycle");
    }
}
