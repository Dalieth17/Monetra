using Monetra.Application.DTOs.Subscription;

namespace Monetra.Application.Interfaces;

public interface ISubscriptionService
{
    Task<List<SubscriptionDto>> GetUserSubscriptionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> CreateSubscriptionAsync(int userId, CreateSubscriptionDto request, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> ToggleStatusAsync(int subscriptionId, int userId, CancellationToken cancellationToken = default);
    Task DeleteSubscriptionAsync(int subscriptionId, int userId, CancellationToken cancellationToken = default);
}
