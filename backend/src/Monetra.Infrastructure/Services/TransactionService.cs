using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Transaction;
using Monetra.Application.Interfaces;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly MonetraDbContext _context;

    public TransactionService(MonetraDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransactionDto>> GetTransactionsAsync(int userId, int? accountId = null, int? categoryId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.Account.UserId == userId);

        if (accountId.HasValue)
        {
            query = query.Where(t => t.AccountId == accountId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(t => t.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.Date <= to.Value);
        }

        var list = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);

        return list.Select(t => new TransactionDto
        {
            Id = t.Id,
            AccountId = t.AccountId,
            AccountName = t.Account.Name,
            CurrencyCode = t.Account.CurrencyCode,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? "Sin categoría",
            CategoryIcon = t.Category?.Icon ?? "more_horiz",
            Amount = t.Amount,
            Type = t.Type,
            Date = t.Date,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    public async Task<TransactionDto> GetTransactionByIdAsync(int transactionId, int userId, CancellationToken cancellationToken = default)
    {
        var t = await _context.Transactions
            .Include(x => x.Account)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.Account.UserId == userId, cancellationToken);

        if (t == null)
        {
            throw new NotFoundException("Transacción", transactionId);
        }

        return new TransactionDto
        {
            Id = t.Id,
            AccountId = t.AccountId,
            AccountName = t.Account.Name,
            CurrencyCode = t.Account.CurrencyCode,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? "Sin categoría",
            CategoryIcon = t.Category?.Icon ?? "more_horiz",
            Amount = t.Amount,
            Type = t.Type,
            Date = t.Date,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        };
    }

    public async Task<TransactionDto> CreateTransactionAsync(int userId, CreateTransactionDto request, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException("Cuenta", request.AccountId);
        }

        Category? category = null;
        if (request.CategoryId.HasValue)
        {
            category = await _context.Categories.FindAsync([request.CategoryId.Value], cancellationToken);
        }

        var transaction = new Transaction
        {
            AccountId = account.Id,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Type = request.Type,
            Date = request.Date ?? DateTime.UtcNow,
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Atomically update balance
        if (request.Type == TransactionType.Income)
        {
            account.CurrentBalance += request.Amount;
        }
        else
        {
            account.CurrentBalance -= request.Amount;
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return new TransactionDto
        {
            Id = transaction.Id,
            AccountId = account.Id,
            AccountName = account.Name,
            CurrencyCode = account.CurrencyCode,
            CategoryId = category?.Id,
            CategoryName = category?.Name ?? "Sin categoría",
            CategoryIcon = category?.Icon ?? "more_horiz",
            Amount = transaction.Amount,
            Type = transaction.Type,
            Date = transaction.Date,
            Description = transaction.Description,
            CreatedAt = transaction.CreatedAt
        };
    }

    public async Task DeleteTransactionAsync(int transactionId, int userId, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.Account.UserId == userId, cancellationToken);

        if (transaction == null)
        {
            throw new NotFoundException("Transacción", transactionId);
        }

        // Reverse the balance impact
        if (transaction.Type == TransactionType.Income)
        {
            transaction.Account.CurrentBalance -= transaction.Amount;
        }
        else
        {
            transaction.Account.CurrentBalance += transaction.Amount;
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
