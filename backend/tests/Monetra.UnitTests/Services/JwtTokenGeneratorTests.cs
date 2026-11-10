using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Monetra.Domain.Entities;
using Monetra.Infrastructure.Services;
using Xunit;

namespace Monetra.UnitTests.Services;

public class JwtTokenGeneratorTests
{
    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "UnitTestSecretKeyThatIsAtLeast32BytesLong!!",
                ["Jwt:Issuer"] = "MonetraApi",
                ["Jwt:Audience"] = "MonetraMobileApp",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
    }

    private static User GetTestUser() => new()
    {
        Id = 42,
        Name = "Luiger Tester",
        Email = "luiger@monetra.com",
        Role = new Role { Id = 1, Name = "Usuario" }
    };

    [Fact]
    public void GenerateToken_ShouldReturnNonEmptyToken()
    {
        var generator = new JwtTokenGenerator(BuildConfiguration());

        var token = generator.GenerateToken(GetTestUser());

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_ShouldContainExpectedClaims()
    {
        var generator = new JwtTokenGenerator(BuildConfiguration());

        var token = generator.GenerateToken(GetTestUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Type == "nameid" && c.Value == "42");
        Assert.Contains(jwt.Claims, c => c.Type == "email" && c.Value == "luiger@monetra.com");
        Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == "Usuario");
        Assert.Equal("MonetraApi", jwt.Issuer);
        Assert.Contains("MonetraMobileApp", jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ShouldSetFutureExpiration()
    {
        var generator = new JwtTokenGenerator(BuildConfiguration());

        var token = generator.GenerateToken(GetTestUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ShouldDefaultRoleToUsuario_WhenRoleIsMissing()
    {
        var generator = new JwtTokenGenerator(BuildConfiguration());
        var user = GetTestUser();
        user.Role = null!;

        var token = generator.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == "Usuario");
    }
}
