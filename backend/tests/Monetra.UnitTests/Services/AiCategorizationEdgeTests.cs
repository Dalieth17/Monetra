using Monetra.Domain.Entities;
using Monetra.Infrastructure.Services;
using Xunit;

namespace Monetra.UnitTests.Services;

public class AiCategorizationEdgeTests
{
    private static List<Category> GetTestCategories()
    {
        return new List<Category>
        {
            new() { Id = 1, Name = "Alimentación", Icon = "restaurant" },
            new() { Id = 2, Name = "Transporte", Icon = "directions_bus" },
            new() { Id = 11, Name = "Otros", Icon = "more_horiz" }
        };
    }

    [Fact]
    public void ClassifyText_ShouldReturnOtros_WhenNoKeywordMatches()
    {
        var result = AiCategorizationService.ClassifyText("xyzabc sin relación", GetTestCategories());

        Assert.Equal("Otros", result.CategoryName);
        Assert.True(result.Confidence < 0.85);
    }

    [Fact]
    public void ClassifyText_ShouldReturnZeroConfidence_WhenDescriptionIsEmpty()
    {
        var result = AiCategorizationService.ClassifyText("", GetTestCategories());

        Assert.Equal("Otros", result.CategoryName);
        Assert.Equal(0.0, result.Confidence);
    }

    [Fact]
    public void ClassifyText_ShouldReturnZeroConfidence_WhenDescriptionIsWhitespace()
    {
        var result = AiCategorizationService.ClassifyText("   ", GetTestCategories());

        Assert.Equal("Otros", result.CategoryName);
        Assert.Equal(0.0, result.Confidence);
    }

    [Fact]
    public void ClassifyText_ShouldBeCaseInsensitive()
    {
        var result = AiCategorizationService.ClassifyText("Viaje en UBER", GetTestCategories());

        Assert.Equal("Transporte", result.CategoryName);
        Assert.True(result.Confidence >= 0.85);
    }

    [Fact]
    public void ClassifyText_ShouldNotThrow_WhenCategoriesListIsNull()
    {
        var result = AiCategorizationService.ClassifyText("Mercado en Éxito", null);

        Assert.NotNull(result);
        Assert.Equal("Alimentación", result.CategoryName);
        Assert.Null(result.CategoryId);
    }

    [Fact]
    public void ClassifyText_ShouldMapCategoryId_WhenMatchingEntityExists()
    {
        var result = AiCategorizationService.ClassifyText("Almuerzo en restaurante", GetTestCategories());

        Assert.Equal("Alimentación", result.CategoryName);
        Assert.Equal(1, result.CategoryId);
    }
}
