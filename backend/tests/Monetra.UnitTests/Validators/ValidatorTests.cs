using Monetra.Application.DTOs.Account;
using Monetra.Application.DTOs.Auth;
using Monetra.Application.DTOs.Transaction;
using Monetra.Application.Validators.Account;
using Monetra.Application.Validators.Auth;
using Monetra.Application.Validators.Transaction;
using Monetra.Domain.Enums;
using Xunit;

namespace Monetra.UnitTests.Validators;

public class ValidatorTests
{
    [Fact]
    public void RegisterValidator_ShouldFail_WhenEmailIsInvalid()
    {
        var validator = new RegisterRequestValidator();
        var model = new RegisterRequestDto
        {
            Name = "Luiger",
            Email = "invalid-email-format",
            Password = "Password123"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void RegisterValidator_ShouldPass_WhenAllFieldsAreValid()
    {
        var validator = new RegisterRequestValidator();
        var model = new RegisterRequestDto
        {
            Name = "Luiger",
            Email = "luiger@monetra.com",
            Password = "SecurePassword123"
        };

        var result = validator.Validate(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateTransactionValidator_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        var validator = new CreateTransactionValidator();
        var model = new CreateTransactionDto
        {
            AccountId = 1,
            Amount = 0m,
            Type = TransactionType.Expense,
            Description = "Test"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public void CreateAccountValidator_ShouldFail_WhenCurrencyIsUnsupported()
    {
        var validator = new CreateAccountValidator();
        var model = new CreateAccountDto
        {
            Name = "Billetera Extra",
            CurrencyCode = "XYZ",
            InitialBalance = 100m
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CurrencyCode");
    }
}

