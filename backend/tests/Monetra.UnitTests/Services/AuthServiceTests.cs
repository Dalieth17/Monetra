using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Auth;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;
using Xunit;

namespace Monetra.UnitTests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService(MonetraDbContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "UnitTestSecretKeyThatIsAtLeast32BytesLong!!"
            })
            .Build();
        return new AuthService(context, new JwtTokenGenerator(configuration));
    }

    private static RegisterRequestDto ValidRegistration(string email = "luiger@monetra.com") => new()
    {
        Name = "Luiger",
        Email = email,
        Password = "Password123*"
    };

    [Fact]
    public async Task Register_StoresThePasswordHashedNotInPlainText()
    {
        using var context = TestDatabase.Create();

        await CreateService(context).RegisterAsync(ValidRegistration());

        var stored = context.Users.AsNoTracking().Single().PasswordHash;
        Assert.NotEqual("Password123*", stored);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123*", stored));
    }

    [Fact]
    public async Task Register_OpensAZeroBalanceAccountInCopAndInUsd()
    {
        using var context = TestDatabase.Create();

        var registered = await CreateService(context).RegisterAsync(ValidRegistration());

        var accounts = context.Accounts.AsNoTracking().Where(a => a.UserId == registered.UserId).ToList();
        Assert.Equal(["COP", "USD"], accounts.Select(a => a.CurrencyCode).Order());
        Assert.All(accounts, a => Assert.Equal(0m, a.CurrentBalance));
    }

    [Fact]
    public async Task Register_WithAnEmailAlreadyInUse_IsRejectedRegardlessOfCase()
    {
        using var context = TestDatabase.Create();
        var service = CreateService(context);
        await service.RegisterAsync(ValidRegistration("luiger@monetra.com"));

        var error = await Assert.ThrowsAsync<ValidationException>(
            () => service.RegisterAsync(ValidRegistration("  LUIGER@Monetra.com ")));

        Assert.Contains("Email", error.Errors.Keys);
        Assert.Single(context.Users.AsNoTracking());
    }

    [Fact]
    public async Task Register_WithAnUnknownRole_FallsBackToRegularUser()
    {
        using var context = TestDatabase.Create();
        var request = ValidRegistration();
        request.Role = "SuperJefe";

        var registered = await CreateService(context).RegisterAsync(request);

        Assert.Equal("Usuario", registered.Role);
    }

    [Fact]
    public async Task Login_WithTheRegisteredCredentials_ReturnsAToken()
    {
        using var context = TestDatabase.Create();
        var service = CreateService(context);
        await service.RegisterAsync(ValidRegistration("luiger@monetra.com"));

        var session = await service.LoginAsync(new LoginRequestDto { Email = " LUIGER@monetra.com ", Password = "Password123*" });

        Assert.False(string.IsNullOrWhiteSpace(session.Token));
        Assert.Equal("luiger@monetra.com", session.Email);
    }

    [Fact]
    public async Task Login_WithAWrongPassword_IsRejected()
    {
        using var context = TestDatabase.Create();
        var service = CreateService(context);
        await service.RegisterAsync(ValidRegistration());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(new LoginRequestDto { Email = "luiger@monetra.com", Password = "OtraClave999" }));
    }

    [Fact]
    public async Task Login_WithAnUnregisteredEmail_IsRejected()
    {
        using var context = TestDatabase.Create();

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => CreateService(context).LoginAsync(new LoginRequestDto { Email = "nadie@monetra.com", Password = "Password123*" }));
    }
}
