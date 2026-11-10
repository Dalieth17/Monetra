using Monetra.Application.DTOs.SavingsGoal;
using Xunit;

namespace Monetra.UnitTests.Domain;

public class SavingsGoalEdgeTests
{
    [Fact]
    public void SavingsGoalDto_ShouldReturnZeroProgress_WhenCurrentAmountIsZero()
    {
        var goal = new SavingsGoalDto
        {
            Name = "Meta nueva",
            TargetAmount = 1000000m,
            CurrentAmount = 0m
        };

        Assert.Equal(0.0, goal.ProgressPercentage);
        Assert.False(goal.IsCompleted);
    }

    [Fact]
    public void SavingsGoalDto_ShouldCapProgressAt100_WhenCurrentExceedsTarget()
    {
        var goal = new SavingsGoalDto
        {
            Name = "Meta superada",
            TargetAmount = 1000000m,
            CurrentAmount = 1500000m
        };

        Assert.Equal(100.0, goal.ProgressPercentage);
        Assert.True(goal.IsCompleted);
    }

    [Fact]
    public void SavingsGoalDto_ShouldReturnZeroProgress_WhenTargetAmountIsZero()
    {
        var goal = new SavingsGoalDto
        {
            Name = "Meta sin objetivo",
            TargetAmount = 0m,
            CurrentAmount = 500000m
        };

        Assert.Equal(0.0, goal.ProgressPercentage);
    }

    [Fact]
    public void SavingsGoalDto_ShouldRoundProgressToTwoDecimals()
    {
        var goal = new SavingsGoalDto
        {
            Name = "Meta con decimales",
            TargetAmount = 3000000m,
            CurrentAmount = 1000000m
        };

        Assert.Equal(33.33, goal.ProgressPercentage);
    }
}
