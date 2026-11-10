using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Auth;
using Xunit;

namespace Monetra.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerEndpoint_ShouldReturnSuccessAndJsonContent()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("application/json", response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task ProtectedEndpoints_WithoutToken_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAndLogin_ShouldSucceed_WithValidCredentials()
    {
        var client = _factory.CreateClient();
        var uniqueEmail = $"luiger_{Guid.NewGuid():N}@monetra.com";

        // Register
        var registerRequest = new RegisterRequestDto
        {
            Name = "Luiger Tester",
            Email = uniqueEmail,
            Password = "Password123*"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerContent = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        Assert.NotNull(registerContent);
        Assert.True(registerContent.Success);
        Assert.NotNull(registerContent.Data);
        Assert.False(string.IsNullOrEmpty(registerContent.Data.Token));
        Assert.Equal(uniqueEmail, registerContent.Data.Email);

        // Login
        var loginRequest = new LoginRequestDto
        {
            Email = uniqueEmail,
            Password = "Password123*"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        Assert.NotNull(loginContent);
        Assert.True(loginContent.Success);
        Assert.NotNull(loginContent.Data);
        Assert.False(string.IsNullOrEmpty(loginContent.Data.Token));
    }
}
