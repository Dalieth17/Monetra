using Monetra.Application.DTOs.Transaction;

namespace Monetra.Application.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionDto>> GetTransactionsAsync(int userId, int? accountId = null, int? categoryId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<TransactionDto> GetTransactionByIdAsync(int transactionId, int userId, CancellationToken cancellationToken = default);
    Task<TransactionDto> CreateTransactionAsync(int userId, CreateTransactionDto request, CancellationToken cancellationToken = default);
    Task DeleteTransactionAsync(int transactionId, int userId, CancellationToken cancellationToken = default);
}
