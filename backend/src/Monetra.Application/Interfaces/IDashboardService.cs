using Monetra.Application.DTOs.Dashboard;

namespace Monetra.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int userId, string baseCurrency = "COP", CancellationToken cancellationToken = default);
}

