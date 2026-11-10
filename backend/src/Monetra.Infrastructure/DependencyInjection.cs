using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monetra.Application.Interfaces;
using Monetra.Infrastructure.Persistence;
using Monetra.Infrastructure.Services;

namespace Monetra.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            var databaseName = configuration["InMemoryDatabaseName"] ?? "MonetraTestDb";
            services.AddDbContext<MonetraDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=monetra_db;User=root;Password=;";

            var serverVersion = ServerVersion.AutoDetect(connectionString);
            services.AddDbContext<MonetraDbContext>(options =>
                options.UseMySql(connectionString, serverVersion, mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                }));
        }

        services.AddHttpClient<IMarketService, MarketService>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDebtService, DebtService>();
        services.AddScoped<ISavingsGoalService, SavingsGoalService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IAiCategorizationService, AiCategorizationService>();

        return services;
    }
}
