using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Market;
using Monetra.Application.Interfaces;

namespace Monetra.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MarketController : ControllerBase
{
    private readonly IMarketService _marketService;

    public MarketController(IMarketService marketService)
    {
        _marketService = marketService;
    }

    [HttpGet("usd-cop")]
    public async Task<ActionResult<ApiResponse<MarketRateDto>>> GetUsdCopRate(CancellationToken cancellationToken = default)
    {
        var rate = await _marketService.GetUsdToCopRateAsync(cancellationToken);
        return Ok(ApiResponse<MarketRateDto>.SuccessResult(rate));
    }
}
