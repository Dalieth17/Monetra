using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Api.Extensions;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Dashboard;
using Monetra.Application.Interfaces;

namespace Monetra.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetDashboard([FromQuery] string baseCurrency = "COP", CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var summary = await _dashboardService.GetSummaryAsync(userId, baseCurrency, cancellationToken);
        return Ok(ApiResponse<DashboardSummaryDto>.SuccessResult(summary));
    }
}
