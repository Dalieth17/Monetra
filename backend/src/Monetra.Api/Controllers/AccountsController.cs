using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Api.Extensions;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Account;
using Monetra.Application.Interfaces;
using ValidationException = Monetra.Application.Common.Exceptions.ValidationException;

namespace Monetra.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IValidator<CreateAccountDto> _createValidator;

    public AccountsController(IAccountService accountService, IValidator<CreateAccountDto> createValidator)
    {
        _accountService = accountService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AccountDto>>>> GetAccounts([FromQuery] string targetCurrency = "COP", CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var accounts = await _accountService.GetUserAccountsAsync(userId, targetCurrency, cancellationToken);
        return Ok(ApiResponse<List<AccountDto>>.SuccessResult(accounts));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccountById(int id, [FromQuery] string targetCurrency = "COP", CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var account = await _accountService.GetAccountByIdAsync(id, userId, targetCurrency, cancellationToken);
        return Ok(ApiResponse<AccountDto>.SuccessResult(account));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount([FromBody] CreateAccountDto request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.ToDictionary());
        }

        var userId = User.GetUserId();
        var account = await _accountService.CreateAccountAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, ApiResponse<AccountDto>.SuccessResult(account, "Cuenta creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> UpdateAccount(int id, [FromBody] UpdateAccountDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var account = await _accountService.UpdateAccountAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<AccountDto>.SuccessResult(account, "Cuenta actualizada."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAccount(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _accountService.DeleteAccountAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Cuenta eliminada exitosamente."));
    }
}
