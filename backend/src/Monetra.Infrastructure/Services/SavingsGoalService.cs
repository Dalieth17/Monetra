using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.SavingsGoal;
using Monetra.Application.DTOs.Transaction;
using Monetra.Application.Interfaces;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class SavingsGoalService : ISavingsGoalService
{
    private readonly MonetraDbContext _context;
    private readonly ITransactionService _transactionService;

    public SavingsGoalService(MonetraDbContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    public async Task<List<SavingsGoalDto>> GetUserGoalsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var goals = await _context.SavingsGoals
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.Deadline)
            .ToListAsync(cancellationToken);

        return goals.Select(g => new SavingsGoalDto
        {
            Id = g.Id,
            Name = g.Name,
            TargetAmount = g.TargetAmount,
            CurrentAmount = g.CurrentAmount,
            Deadline = g.Deadline,
            CreatedAt = g.CreatedAt
        }).ToList();
    }

    public async Task<SavingsGoalDto> CreateGoalAsync(int userId, CreateSavingsGoalDto request, CancellationToken cancellationToken = default)
    {
        var goal = new SavingsGoal
        {
            UserId = userId,
            Name = request.Name.Trim(),
            TargetAmount = request.TargetAmount,
            CurrentAmount = 0m,
            Deadline = request.Deadline,
            CreatedAt = DateTime.UtcNow
        };

        _context.SavingsGoals.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);

        return new SavingsGoalDto
        {
            Id = goal.Id,
            Name = goal.Name,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            Deadline = goal.Deadline,
            CreatedAt = goal.CreatedAt
        };
    }

    public async Task<SavingsGoalDto> DepositAsync(int goalId, int userId, DepositSavingsGoalDto request, CancellationToken cancellationToken = default)
    {
        var goal = await _context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId, cancellationToken);

        if (goal == null)
        {
            throw new NotFoundException("Meta de ahorro", goalId);
        }

        if (request.Amount <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Amount"] = ["El monto del depósito debe ser mayor a 0."]
            });
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException("Cuenta", request.AccountId);
        }

        goal.CurrentAmount += request.Amount;

        // Record savings transfer transaction
        var savingsCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Inversiones" || c.Name == "Otros", cancellationToken);
        await _transactionService.CreateTransactionAsync(userId, new CreateTransactionDto
        {
            AccountId = account.Id,
            CategoryId = savingsCategory?.Id,
            Amount = request.Amount,
            Type = TransactionType.Expense,
            Date = DateTime.UtcNow,
            Description = $"Depósito a meta: {goal.Name}"
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new SavingsGoalDto
        {
            Id = goal.Id,
            Name = goal.Name,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            Deadline = goal.Deadline,
            CreatedAt = goal.CreatedAt
        };
    }

    public async Task DeleteGoalAsync(int goalId, int userId, CancellationToken cancellationToken = default)
    {
        var goal = await _context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId, cancellationToken);

        if (goal == null)
        {
            throw new NotFoundException("Meta de ahorro", goalId);
        }

        _context.SavingsGoals.Remove(goal);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
