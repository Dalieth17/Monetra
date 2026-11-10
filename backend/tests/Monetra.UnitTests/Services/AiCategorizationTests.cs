using Monetra.Domain.Entities;
using Monetra.Infrastructure.Services;
using Xunit;

namespace Monetra.UnitTests.Services;

public class AiCategorizationTests
{
    private static List<Category> GetTestCategories()
    {
        return new List<Category>
        {
            new() { Id = 1, Name = "Alimentación", Icon = "restaurant" },
            new() { Id = 2, Name = "Transporte", Icon = "directions_bus" },
            new() { Id = 8, Name = "Suscripciones", Icon = "subscriptions" },
            new() { Id = 11, Name = "Otros", Icon = "more_horiz" }
        };
    }

    [Theory]
    [InlineData("Mercado en Éxito", "Alimentación")]
    [InlineData("Viaje en Uber al trabajo", "Transporte")]
    [InlineData("Mensualidad de Netflix", "Suscripciones")]
    public void ClassifyText_ShouldSuggestExpectedCategory(string description, string expectedCategory)
    {
        var categories = GetTestCategories();
        var result = AiCategorizationService.ClassifyText(description, categories);

        Assert.Equal(expectedCategory, result.CategoryName);
        Assert.True(result.Confidence >= 0.85);
    }
}
