using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Monetra.IntegrationTests;

public class MonetraWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"MonetraTestDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("UseInMemoryDatabase", "true");
        builder.UseSetting("InMemoryDatabaseName", _databaseName);
    }
}
