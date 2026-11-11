using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Monetra.Application.Interfaces;
using Monetra.Infrastructure.Services;
using Monetra.UnitTests.TestSupport;

namespace Monetra.IntegrationTests;

public class MonetraWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"MonetraTestDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("UseInMemoryDatabase", "true");
        builder.UseSetting("InMemoryDatabaseName", _databaseName);

        // Las pruebas nunca salen a internet: el proveedor de cotizaciones se sustituye por una respuesta fija.
        builder.ConfigureTestServices(services =>
            services.AddHttpClient<IStockService, StockService>()
                .ConfigurePrimaryHttpMessageHandler(() => new StubHttpMessageHandler(
                    request => StubHttpMessageHandler.JsonResponse(YahooChart.Payload(request.RequestUri!.Segments[^1])))));
    }
}
