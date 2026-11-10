using Monetra.Application.DTOs.SavingsGoal;
using Xunit;

namespace Monetra.UnitTests.Domain;

public class SavingsGoalTests
{
    [Fact]
    public void SavingsGoalDto_ShouldCalculateCorrectProgressPercentage()
    {
        var goal = new SavingsGoalDto
        {
            Id = 1,
            Name = "Viaje a San Andrés",
            TargetAmount = 2000000m,
            CurrentAmount = 500000m,
            Deadline = DateTime.UtcNow.AddMonths(6)
        };

        Assert.Equal(25.0, goal.ProgressPercentage);
        Assert.False(goal.IsCompleted);
    }

    [Fact]
    public void SavingsGoalDto_ShouldMarkCompleted_WhenCurrentReachesTarget()
    {
        var goal = new SavingsGoalDto
        {
            Id = 1,
            Name = "Fondo de Emergencia",
            TargetAmount = 5000000m,
            CurrentAmount = 5000000m,
            Deadline = DateTime.UtcNow.AddMonths(12)
        };

        Assert.Equal(100.0, goal.ProgressPercentage);
        Assert.True(goal.IsCompleted);
    }
}
