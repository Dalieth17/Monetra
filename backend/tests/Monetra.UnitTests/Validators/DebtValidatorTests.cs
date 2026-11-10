using Monetra.Application.DTOs.Debt;
using Monetra.Application.Validators.Debt;
using Xunit;

namespace Monetra.UnitTests.Validators;

public class DebtValidatorTests
{
    private static CreateDebtDto ValidDebt() => new()
    {
        Creditor = "Bancolombia",
        TotalAmount = 10000000m,
        InterestRate = 1.5m,
        RateType = "EM",
        DueDate = DateTime.UtcNow.AddYears(2)
    };

    [Fact]
    public void CreateDebtValidator_ShouldPass_WhenDebtIsValid()
    {
        var validator = new CreateDebtValidator();

        var result = validator.Validate(ValidDebt());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateDebtValidator_ShouldFail_WhenTotalAmountIsNegative()
    {
        var validator = new CreateDebtValidator();
        var model = ValidDebt();
        model.TotalAmount = -500000m;

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TotalAmount");
    }

    [Fact]
    public void CreateDebtValidator_ShouldFail_WhenInterestRateIsNegative()
    {
        var validator = new CreateDebtValidator();
        var model = ValidDebt();
        model.InterestRate = -0.5m;

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InterestRate");
    }

    [Fact]
    public void CreateDebtValidator_ShouldFail_WhenRateTypeIsInvalid()
    {
        var validator = new CreateDebtValidator();
        var model = ValidDebt();
        model.RateType = "XX";

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RateType");
    }

    [Fact]
    public void CreateDebtValidator_ShouldFail_WhenDueDateIsInThePast()
    {
        var validator = new CreateDebtValidator();
        var model = ValidDebt();
        model.DueDate = DateTime.UtcNow.AddDays(-1);

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void CreateDebtValidator_ShouldFail_WhenCreditorIsEmpty()
    {
        var validator = new CreateDebtValidator();
        var model = ValidDebt();
        model.Creditor = "";

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Creditor");
    }
}
