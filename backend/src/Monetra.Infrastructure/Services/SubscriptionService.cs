using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Subscription;
using Monetra.Application.Interfaces;
using Monetra.Domain.Entities;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly MonetraDbContext _context;

    public SubscriptionService(MonetraDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriptionDto>> GetUserSubscriptionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.NextPaymentDate)
            .ToListAsync(cancellationToken);

        return subscriptions.Select(s => new SubscriptionDto
        {
            Id = s.Id,
            Name = s.Name,
            Amount = s.Amount,
            CurrencyCode = s.CurrencyCode,
            BillingCycle = s.BillingCycle,
            NextPaymentDate = s.NextPaymentDate,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        }).ToList();
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(int userId, CreateSubscriptionDto request, CancellationToken cancellationToken = default)
    {
        var subscription = new Subscription
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
            BillingCycle = request.BillingCycle,
            NextPaymentDate = request.NextPaymentDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriptionDto
        {
            Id = subscription.Id,
            Name = subscription.Name,
            Amount = subscription.Amount,
            CurrencyCode = subscription.CurrencyCode,
            BillingCycle = subscription.BillingCycle,
            NextPaymentDate = subscription.NextPaymentDate,
            IsActive = subscription.IsActive,
            CreatedAt = subscription.CreatedAt
        };
    }

    public async Task<SubscriptionDto> ToggleStatusAsync(int subscriptionId, int userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Suscripción", subscriptionId);
        }

        subscription.IsActive = !subscription.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriptionDto
        {
            Id = subscription.Id,
            Name = subscription.Name,
            Amount = subscription.Amount,
            CurrencyCode = subscription.CurrencyCode,
            BillingCycle = subscription.BillingCycle,
            NextPaymentDate = subscription.NextPaymentDate,
            IsActive = subscription.IsActive,
            CreatedAt = subscription.CreatedAt
        };
    }

    public async Task DeleteSubscriptionAsync(int subscriptionId, int userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Suscripción", subscriptionId);
        }

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
