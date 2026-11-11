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
    private readonly IStockService _stockService;

    public MarketController(IMarketService marketService, IStockService stockService)
    {
        _marketService = marketService;
        _stockService = stockService;
    }

    [HttpGet("usd-cop")]
    public async Task<ActionResult<ApiResponse<MarketRateDto>>> GetUsdCopRate(CancellationToken cancellationToken = default)
    {
        var rate = await _marketService.GetUsdToCopRateAsync(cancellationToken);
        return Ok(ApiResponse<MarketRateDto>.SuccessResult(rate));
    }

    [HttpGet("stocks")]
    public async Task<ActionResult<ApiResponse<List<StockQuoteDto>>>> GetStocks(CancellationToken cancellationToken = default)
    {
        var quotes = await _stockService.GetWatchlistAsync(cancellationToken);
        return Ok(ApiResponse<List<StockQuoteDto>>.SuccessResult(quotes));
    }
}

