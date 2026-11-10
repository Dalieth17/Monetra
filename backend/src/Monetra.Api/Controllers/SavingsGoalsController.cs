using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Api.Extensions;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.SavingsGoal;
using Monetra.Application.Interfaces;
using ValidationException = Monetra.Application.Common.Exceptions.ValidationException;

namespace Monetra.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SavingsGoalsController : ControllerBase
{
    private readonly ISavingsGoalService _savingsGoalService;
    private readonly IValidator<CreateSavingsGoalDto> _createValidator;

    public SavingsGoalsController(ISavingsGoalService savingsGoalService, IValidator<CreateSavingsGoalDto> createValidator)
    {
        _savingsGoalService = savingsGoalService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SavingsGoalDto>>>> GetGoals(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var goals = await _savingsGoalService.GetUserGoalsAsync(userId, cancellationToken);
        return Ok(ApiResponse<List<SavingsGoalDto>>.SuccessResult(goals));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SavingsGoalDto>>> CreateGoal([FromBody] CreateSavingsGoalDto request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.ToDictionary());
        }

        var userId = User.GetUserId();
        var goal = await _savingsGoalService.CreateGoalAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<SavingsGoalDto>.SuccessResult(goal, "Meta de ahorro creada exitosamente."));
    }

    [HttpPost("{id:int}/deposit")]
    public async Task<ActionResult<ApiResponse<SavingsGoalDto>>> Deposit(int id, [FromBody] DepositSavingsGoalDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var goal = await _savingsGoalService.DepositAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<SavingsGoalDto>.SuccessResult(goal, "Aporte registrado exitosamente a la meta."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteGoal(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _savingsGoalService.DeleteGoalAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Meta eliminada."));
    }
}
