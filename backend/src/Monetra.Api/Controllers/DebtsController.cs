using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Api.Extensions;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Debt;
using Monetra.Application.Interfaces;
using ValidationException = Monetra.Application.Common.Exceptions.ValidationException;

namespace Monetra.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DebtsController : ControllerBase
{
    private readonly IDebtService _debtService;
    private readonly IValidator<CreateDebtDto> _createValidator;

    public DebtsController(IDebtService debtService, IValidator<CreateDebtDto> createValidator)
    {
        _debtService = debtService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DebtDto>>>> GetDebts(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var debts = await _debtService.GetUserDebtsAsync(userId, cancellationToken);
        return Ok(ApiResponse<List<DebtDto>>.SuccessResult(debts));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DebtDto>>> CreateDebt([FromBody] CreateDebtDto request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.ToDictionary());
        }

        var userId = User.GetUserId();
        var debt = await _debtService.CreateDebtAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<DebtDto>.SuccessResult(debt, "Deuda registrada exitosamente."));
    }

    [HttpPost("{id:int}/payments")]
    public async Task<ActionResult<ApiResponse<DebtDto>>> RecordPayment(int id, [FromBody] RecordDebtPaymentDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var debt = await _debtService.RecordPaymentAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<DebtDto>.SuccessResult(debt, "Abono aplicado exitosamente a la deuda."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDebt(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _debtService.DeleteDebtAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Deuda eliminada exitosamente."));
    }
}
