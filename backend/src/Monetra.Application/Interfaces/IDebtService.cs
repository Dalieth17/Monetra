using Monetra.Application.DTOs.Debt;

namespace Monetra.Application.Interfaces;

public interface IDebtService
{
    Task<List<DebtDto>> GetUserDebtsAsync(int userId, CancellationToken cancellationToken = default);
    Task<DebtDto> CreateDebtAsync(int userId, CreateDebtDto request, CancellationToken cancellationToken = default);
    Task<DebtDto> RecordPaymentAsync(int debtId, int userId, RecordDebtPaymentDto request, CancellationToken cancellationToken = default);
    Task DeleteDebtAsync(int debtId, int userId, CancellationToken cancellationToken = default);
}

