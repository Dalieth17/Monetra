namespace Monetra.Application.DTOs.SavingsGoal;

public class SavingsGoalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public double ProgressPercentage => TargetAmount > 0 ? (double)Math.Min(100m, Math.Round((CurrentAmount / TargetAmount) * 100m, 2)) : 0.0;
    public DateTime Deadline { get; set; }
    public bool IsCompleted => CurrentAmount >= TargetAmount;
    public DateTime CreatedAt { get; set; }
}

public class CreateSavingsGoalDto
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime Deadline { get; set; }
}

public class DepositSavingsGoalDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
}

