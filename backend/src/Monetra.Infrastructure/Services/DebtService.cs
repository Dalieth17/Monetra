using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Debt;
using Monetra.Application.DTOs.Transaction;
using Monetra.Application.Interfaces;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class DebtService : IDebtService
{
    private readonly MonetraDbContext _context;
    private readonly ITransactionService _transactionService;

    public DebtService(MonetraDbContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    public async Task<List<DebtDto>> GetUserDebtsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var debts = await _context.Debts
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.DueDate)
            .ToListAsync(cancellationToken);

        return debts.Select(d => MapToDto(d)).ToList();
    }

    public async Task<DebtDto> CreateDebtAsync(int userId, CreateDebtDto request, CancellationToken cancellationToken = default)
    {
        var debt = new Debt
        {
            UserId = userId,
            Creditor = request.Creditor.Trim(),
            TotalAmount = request.TotalAmount,
            RemainingAmount = request.TotalAmount,
            InterestRate = request.InterestRate,
            RateType = request.RateType.ToUpperInvariant(),
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Debts.Add(debt);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(debt);
    }

    public async Task<DebtDto> RecordPaymentAsync(int debtId, int userId, RecordDebtPaymentDto request, CancellationToken cancellationToken = default)
    {
        var debt = await _context.Debts
            .FirstOrDefaultAsync(d => d.Id == debtId && d.UserId == userId, cancellationToken);

        if (debt == null)
        {
            throw new NotFoundException("Deuda", debtId);
        }

        if (request.Amount <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Amount"] = ["El monto del abono debe ser mayor a 0."]
            });
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException("Cuenta", request.AccountId);
        }

        // Apply payment to debt
        debt.RemainingAmount = Math.Max(0m, debt.RemainingAmount - request.Amount);

        // Record corresponding expense transaction
        var debtCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Otros", cancellationToken);
        await _transactionService.CreateTransactionAsync(userId, new CreateTransactionDto
        {
            AccountId = account.Id,
            CategoryId = debtCategory?.Id,
            Amount = request.Amount,
            Type = TransactionType.Expense,
            Date = DateTime.UtcNow,
            Description = $"Abono a deuda: {debt.Creditor}"
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(debt);
    }

    public async Task DeleteDebtAsync(int debtId, int userId, CancellationToken cancellationToken = default)
    {
        var debt = await _context.Debts
            .FirstOrDefaultAsync(d => d.Id == debtId && d.UserId == userId, cancellationToken);

        if (debt == null)
        {
            throw new NotFoundException("Deuda", debtId);
        }

        _context.Debts.Remove(debt);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static DebtDto MapToDto(Debt d)
    {
        // Monthly payment estimation
        var remainingMonths = Math.Max(1, ((d.DueDate.Year - DateTime.UtcNow.Year) * 12) + d.DueDate.Month - DateTime.UtcNow.Month);
        decimal monthlyPayment;

        if (d.InterestRate > 0)
        {
            // Monthly rate
            double monthlyRate = d.RateType == "EA" 
                ? Math.Pow(1.0 + (double)d.InterestRate / 100.0, 1.0 / 12.0) - 1.0 
                : (double)d.InterestRate / 100.0;

            if (monthlyRate > 0)
            {
                var factor = Math.Pow(1.0 + monthlyRate, remainingMonths);
                var paymentDouble = (double)d.RemainingAmount * (monthlyRate * factor) / (factor - 1.0);
                monthlyPayment = Math.Round((decimal)paymentDouble, 2);
            }
            else
            {
                monthlyPayment = Math.Round(d.RemainingAmount / remainingMonths, 2);
            }
        }
        else
        {
            monthlyPayment = Math.Round(d.RemainingAmount / remainingMonths, 2);
        }

        return new DebtDto
        {
            Id = d.Id,
            Creditor = d.Creditor,
            TotalAmount = d.TotalAmount,
            RemainingAmount = d.RemainingAmount,
            InterestRate = d.InterestRate,
            RateType = d.RateType,
            DueDate = d.DueDate,
            EstimatedMonthlyPayment = monthlyPayment,
            CreatedAt = d.CreatedAt
        };
    }
}

