using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Account;
using Monetra.Application.Interfaces;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly MonetraDbContext _context;
    private readonly IMarketService _marketService;

    public AccountService(MonetraDbContext context, IMarketService marketService)
    {
        _context = context;
        _marketService = marketService;
    }

    public async Task<List<AccountDto>> GetUserAccountsAsync(int userId, string targetCurrency = "COP", CancellationToken cancellationToken = default)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Id)
            .ToListAsync(cancellationToken);

        var dtoList = new List<AccountDto>();
        foreach (var a in accounts)
        {
            var converted = await _marketService.ConvertAsync(a.CurrentBalance, a.CurrencyCode, targetCurrency, cancellationToken);
            dtoList.Add(new AccountDto
            {
                Id = a.Id,
                Name = a.Name,
                CurrencyCode = a.CurrencyCode,
                CurrentBalance = a.CurrentBalance,
                ConvertedBalance = converted,
                CreatedAt = a.CreatedAt
            });
        }

        return dtoList;
    }

    public async Task<AccountDto> GetAccountByIdAsync(int accountId, int userId, string targetCurrency = "COP", CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException("Cuenta", accountId);
        }

        var converted = await _marketService.ConvertAsync(account.CurrentBalance, account.CurrencyCode, targetCurrency, cancellationToken);

        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            CurrencyCode = account.CurrencyCode,
            CurrentBalance = account.CurrentBalance,
            ConvertedBalance = converted,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<AccountDto> CreateAccountAsync(int userId, CreateAccountDto request, CancellationToken cancellationToken = default)
    {
        var currency = request.CurrencyCode.Trim().ToUpperInvariant();

        var account = new Account
        {
            UserId = userId,
            Name = request.Name.Trim(),
            CurrencyCode = currency,
            CurrentBalance = request.InitialBalance,
            CreatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        // If initial balance > 0, register initial deposit transaction
        if (request.InitialBalance > 0m)
        {
            var defaultCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Salario" || c.Name == "Otros", cancellationToken);
            _context.Transactions.Add(new Transaction
            {
                AccountId = account.Id,
                CategoryId = defaultCategory?.Id,
                Amount = request.InitialBalance,
                Type = TransactionType.Income,
                Date = DateTime.UtcNow,
                Description = "Saldo inicial de apertura",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            CurrencyCode = account.CurrencyCode,
            CurrentBalance = account.CurrentBalance,
            ConvertedBalance = account.CurrentBalance,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<AccountDto> UpdateAccountAsync(int accountId, int userId, UpdateAccountDto request, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException("Cuenta", accountId);
        }

        account.Name = request.Name.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            CurrencyCode = account.CurrencyCode,
            CurrentBalance = account.CurrentBalance,
            ConvertedBalance = account.CurrentBalance,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task DeleteAccountAsync(int accountId, int userId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException("Cuenta", accountId);
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
