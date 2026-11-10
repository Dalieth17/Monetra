using Monetra.Application.DTOs.Account;

namespace Monetra.Application.Interfaces;

public interface IAccountService
{
    Task<List<AccountDto>> GetUserAccountsAsync(int userId, string targetCurrency = "COP", CancellationToken cancellationToken = default);
    Task<AccountDto> GetAccountByIdAsync(int accountId, int userId, string targetCurrency = "COP", CancellationToken cancellationToken = default);
    Task<AccountDto> CreateAccountAsync(int userId, CreateAccountDto request, CancellationToken cancellationToken = default);
    Task<AccountDto> UpdateAccountAsync(int accountId, int userId, UpdateAccountDto request, CancellationToken cancellationToken = default);
    Task DeleteAccountAsync(int accountId, int userId, CancellationToken cancellationToken = default);
}

