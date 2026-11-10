using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Api.Extensions;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Transaction;
using Monetra.Application.Interfaces;
using ValidationException = Monetra.Application.Common.Exceptions.ValidationException;

namespace Monetra.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IValidator<CreateTransactionDto> _createValidator;

    public TransactionsController(ITransactionService transactionService, IValidator<CreateTransactionDto> createValidator)
    {
        _transactionService = transactionService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetTransactions(
        [FromQuery] int? accountId,
        [FromQuery] int? categoryId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var transactions = await _transactionService.GetTransactionsAsync(userId, accountId, categoryId, from, to, cancellationToken);
        return Ok(ApiResponse<List<TransactionDto>>.SuccessResult(transactions));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransactionById(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var transaction = await _transactionService.GetTransactionByIdAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<TransactionDto>.SuccessResult(transaction));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> CreateTransaction([FromBody] CreateTransactionDto request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.ToDictionary());
        }

        var userId = User.GetUserId();
        var transaction = await _transactionService.CreateTransactionAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, ApiResponse<TransactionDto>.SuccessResult(transaction, "Transacción registrada exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTransaction(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _transactionService.DeleteTransactionAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Transacción eliminada exitosamente."));
    }
}
