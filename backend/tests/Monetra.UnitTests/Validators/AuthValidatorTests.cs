using Monetra.Application.DTOs.Auth;
using Monetra.Application.Validators.Auth;
using Xunit;

namespace Monetra.UnitTests.Validators;

public class AuthValidatorTests
{
    [Fact]
    public void RegisterValidator_ShouldFail_WhenPasswordIsTooShort()
    {
        var validator = new RegisterRequestValidator();
        var model = new RegisterRequestDto
        {
            Name = "Luiger",
            Email = "luiger@monetra.com",
            Password = "12345"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void RegisterValidator_ShouldFail_WhenNameIsEmpty()
    {
        var validator = new RegisterRequestValidator();
        var model = new RegisterRequestDto
        {
            Name = "",
            Email = "luiger@monetra.com",
            Password = "Password123"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void RegisterValidator_ShouldFail_WhenNameExceedsMaxLength()
    {
        var validator = new RegisterRequestValidator();
        var model = new RegisterRequestDto
        {
            Name = new string('A', 101),
            Email = "luiger@monetra.com",
            Password = "Password123"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void LoginValidator_ShouldFail_WhenEmailIsEmpty()
    {
        var validator = new LoginRequestValidator();
        var model = new LoginRequestDto
        {
            Email = "",
            Password = "Password123"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginValidator_ShouldFail_WhenEmailFormatIsInvalid()
    {
        var validator = new LoginRequestValidator();
        var model = new LoginRequestDto
        {
            Email = "correo-sin-arroba",
            Password = "Password123"
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginValidator_ShouldFail_WhenPasswordIsEmpty()
    {
        var validator = new LoginRequestValidator();
        var model = new LoginRequestDto
        {
            Email = "luiger@monetra.com",
            Password = ""
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void LoginValidator_ShouldPass_WhenCredentialsAreWellFormed()
    {
        var validator = new LoginRequestValidator();
        var model = new LoginRequestDto
        {
            Email = "luiger@monetra.com",
            Password = "Password123"
        };

        var result = validator.Validate(model);

        Assert.True(result.IsValid);
    }
}
