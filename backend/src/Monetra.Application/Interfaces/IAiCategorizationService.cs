using Monetra.Application.DTOs.Ai;

namespace Monetra.Application.Interfaces;

public interface IAiCategorizationService
{
    Task<CategorySuggestionDto> SuggestCategoryAsync(string description, CancellationToken cancellationToken = default);
}
