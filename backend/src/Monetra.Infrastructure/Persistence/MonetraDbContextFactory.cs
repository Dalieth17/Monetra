using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Monetra.Infrastructure.Persistence;

/// <summary>
/// Permite a las herramientas de EF Core (dotnet ef migrations) construir el
/// contexto en tiempo de diseño con una versión de servidor fija, sin necesidad
/// de una base de datos MySQL en ejecución.
/// </summary>
public class MonetraDbContextFactory : IDesignTimeDbContextFactory<MonetraDbContext>
{
    public MonetraDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MonetraDbContext>()
            .UseMySql(
                "Server=localhost;Database=monetra_db;User=root;Password=;",
                new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        return new MonetraDbContext(options);
    }
}
