using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Api.Extensions;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Subscription;
using Monetra.Application.Interfaces;
using ValidationException = Monetra.Application.Common.Exceptions.ValidationException;

namespace Monetra.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IValidator<CreateSubscriptionDto> _createValidator;

    public SubscriptionsController(ISubscriptionService subscriptionService, IValidator<CreateSubscriptionDto> createValidator)
    {
        _subscriptionService = subscriptionService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SubscriptionDto>>>> GetSubscriptions(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId, cancellationToken);
        return Ok(ApiResponse<List<SubscriptionDto>>.SuccessResult(subscriptions));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> CreateSubscription([FromBody] CreateSubscriptionDto request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.ToDictionary());
        }

        var userId = User.GetUserId();
        var subscription = await _subscriptionService.CreateSubscriptionAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<SubscriptionDto>.SuccessResult(subscription, "Suscripción registrada exitosamente."));
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> ToggleStatus(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var subscription = await _subscriptionService.ToggleStatusAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<SubscriptionDto>.SuccessResult(subscription, "Estado de la suscripción actualizado."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSubscription(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _subscriptionService.DeleteSubscriptionAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Suscripción eliminada."));
    }
}
