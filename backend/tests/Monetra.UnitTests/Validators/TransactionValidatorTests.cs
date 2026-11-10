using Monetra.Application.DTOs.Transaction;
using Monetra.Application.Validators.Transaction;
using Monetra.Domain.Enums;
using Xunit;

namespace Monetra.UnitTests.Validators;

public class TransactionValidatorTests
{
    [Fact]
    public void CreateTransactionValidator_ShouldFail_WhenAccountIdIsZero()
    {
        var validator = new CreateTransactionValidator();
        var model = new CreateTransactionDto
        {
            AccountId = 0,
            Amount = 50000m,
            Type = TransactionType.Expense,
            Description = "Compra"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AccountId");
    }

    [Fact]
    public void CreateTransactionValidator_ShouldFail_WhenAmountIsNegative()
    {
        var validator = new CreateTransactionValidator();
        var model = new CreateTransactionDto
        {
            AccountId = 1,
            Amount = -100m,
            Type = TransactionType.Expense,
            Description = "Compra"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public void CreateTransactionValidator_ShouldFail_WhenTypeIsInvalid()
    {
        var validator = new CreateTransactionValidator();
        var model = new CreateTransactionDto
        {
            AccountId = 1,
            Amount = 50000m,
            Type = (TransactionType)99,
            Description = "Compra"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Type");
    }

    [Fact]
    public void CreateTransactionValidator_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        var validator = new CreateTransactionValidator();
        var model = new CreateTransactionDto
        {
            AccountId = 1,
            Amount = 50000m,
            Type = TransactionType.Expense,
            Description = new string('x', 256)
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void CreateTransactionValidator_ShouldPass_WhenTransactionIsValid()
    {
        var validator = new CreateTransactionValidator();
        var model = new CreateTransactionDto
        {
            AccountId = 1,
            Amount = 150000m,
            Type = TransactionType.Income,
            Description = "Pago quincena"
        };

        var result = validator.Validate(model);

        Assert.True(result.IsValid);
    }
}
