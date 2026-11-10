using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.Models;
using Monetra.Application.DTOs.Ai;
using Monetra.Application.Interfaces;

namespace Monetra.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiCategorizationService _aiService;

    public AiController(IAiCategorizationService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet("suggest-category")]
    public async Task<ActionResult<ApiResponse<CategorySuggestionDto>>> SuggestCategory([FromQuery] string description, CancellationToken cancellationToken = default)
    {
        var suggestion = await _aiService.SuggestCategoryAsync(description, cancellationToken);
        return Ok(ApiResponse<CategorySuggestionDto>.SuccessResult(suggestion));
    }
}

