using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Auth;
using Monetra.Application.DTOs.Market;
using Xunit;

namespace Monetra.IntegrationTests;

public class MarketIntegrationTests : IClassFixture<MonetraWebApplicationFactory>
{
    private readonly MonetraWebApplicationFactory _factory;

    public MarketIntegrationTests(MonetraWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var registration = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Name = "Inversionista",
            Email = $"mercado_{Guid.NewGuid():N}@monetra.com",
            Password = "Password123*"
        });
        var session = await registration.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.Data!.Token);
        return client;
    }

    [Fact]
    public async Task Stocks_WithoutToken_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/market/stocks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Stocks_WithToken_ShouldReturnTheWatchlistQuotes()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/market/stocks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<StockQuoteDto>>>();
        Assert.True(body!.Success);
        var apple = Assert.Single(body.Data!, quote => quote.Symbol == "AAPL");
        Assert.Equal(226.96m, apple.Price);
        Assert.Equal(225.67m, apple.PreviousClose);
        Assert.Equal(4, apple.History.Count);
    }

    [Fact]
    public async Task Stocks_ShouldExposeTheDailyChangeInTheJsonContract()
    {
        var client = await CreateAuthenticatedClientAsync();

        var json = await client.GetStringAsync("/api/market/stocks");

        Assert.Contains("\"changePercent\":0.57", json);
        Assert.Contains("\"isStale\":false", json);
    }
}
