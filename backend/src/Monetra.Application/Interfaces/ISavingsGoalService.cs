using Monetra.Application.DTOs.SavingsGoal;

namespace Monetra.Application.Interfaces;

public interface ISavingsGoalService
{
    Task<List<SavingsGoalDto>> GetUserGoalsAsync(int userId, CancellationToken cancellationToken = default);
    Task<SavingsGoalDto> CreateGoalAsync(int userId, CreateSavingsGoalDto request, CancellationToken cancellationToken = default);
    Task<SavingsGoalDto> DepositAsync(int goalId, int userId, DepositSavingsGoalDto request, CancellationToken cancellationToken = default);
    Task DeleteGoalAsync(int goalId, int userId, CancellationToken cancellationToken = default);
}
