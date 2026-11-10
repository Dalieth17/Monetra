using Monetra.Application.DTOs.SavingsGoal;
using Monetra.Application.Validators.SavingsGoal;
using Xunit;

namespace Monetra.UnitTests.Validators;

public class SavingsGoalValidatorTests
{
    [Fact]
    public void CreateSavingsGoalValidator_ShouldPass_WhenGoalIsValid()
    {
        var validator = new CreateSavingsGoalValidator();
        var model = new CreateSavingsGoalDto
        {
            Name = "Viaje a San Andrés",
            TargetAmount = 2000000m,
            Deadline = DateTime.UtcNow.AddMonths(6)
        };

        var result = validator.Validate(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateSavingsGoalValidator_ShouldFail_WhenNameIsEmpty()
    {
        var validator = new CreateSavingsGoalValidator();
        var model = new CreateSavingsGoalDto
        {
            Name = "",
            TargetAmount = 2000000m,
            Deadline = DateTime.UtcNow.AddMonths(6)
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateSavingsGoalValidator_ShouldFail_WhenTargetAmountIsZero()
    {
        var validator = new CreateSavingsGoalValidator();
        var model = new CreateSavingsGoalDto
        {
            Name = "Meta sin monto",
            TargetAmount = 0m,
            Deadline = DateTime.UtcNow.AddMonths(6)
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetAmount");
    }

    [Fact]
    public void CreateSavingsGoalValidator_ShouldFail_WhenDeadlineIsInThePast()
    {
        var validator = new CreateSavingsGoalValidator();
        var model = new CreateSavingsGoalDto
        {
            Name = "Meta vencida",
            TargetAmount = 1000000m,
            Deadline = DateTime.UtcNow.AddDays(-10)
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Deadline");
    }
}
