using Microsoft.EntityFrameworkCore;
using Monetra.Application.DTOs.Ai;
using Monetra.Application.Interfaces;
using Monetra.Domain.Entities;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class AiCategorizationService : IAiCategorizationService
{
    private readonly MonetraDbContext? _context;

    private static readonly Dictionary<string, string[]> KeywordCategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Alimentación"] = ["éxito", "exito", "carulla", "jumbo", "d1", "ara", "olimpica", "supermercado", "restaurante", "almuerzo", "cena", "desayuno", "pizza", "hamburguesa", "mcdonalds", "rappi", "café", "cafe", "starbucks", "comida", "mercado", "panadería", "panaderia"],
        ["Transporte"] = ["uber", "didi", "cabify", "gasolina", "peaje", "transmilenio", "metro", "bus", "taxi", "parqueadero", "taller", "combustible", "avianca", "latam", "vuelo"],
        ["Vivienda"] = ["arriendo", "alquiler", "administración", "administracion", "hipoteca", "mantenimiento hogar", "muebles", "ferreteria", "homecenter"],
        ["Servicios Básicos"] = ["enel", "codensa", "epm", "acueducto", "gas natural", "vanti", "claro", "tigo", "movistar", "internet", "agua", "luz", "recibo"],
        ["Salud"] = ["droguería", "drogueria", "farmacia", "cruz verde", "farmatodo", "médico", "medico", "odontología", "eps", "medicina", "hospital", "cita medica", "laboratorio"],
        ["Entretenimiento"] = ["cine", "cinemark", "cinecolombia", "concierto", "juego", "steam", "playstation", "xbox", "bar", "cerveza", "discoteca", "fiesta", "teatro"],
        ["Suscripciones"] = ["netflix", "spotify", "youtube", "amazon prime", "disney", "apple", "icloud", "chatgpt", "openai", "github", "antigravity", "duolingo"],
        ["Educación"] = ["universidad", "colegio", "curso", "udemy", "platzi", "coursera", "libros", "matricula", "pension"],
        ["Salario"] = ["salario", "sueldo", "nomina", "nómina", "pago empresa", "honorarios", "quincena", "prima"],
        ["Inversiones"] = ["dividendo", "acciones", "bvc", "tyba", "trii", "cripto", "binance", "cdt", "rendimientos"]
    };

    public AiCategorizationService(MonetraDbContext? context = null)
    {
        _context = context;
    }

    public static CategorySuggestionDto ClassifyText(string description, IEnumerable<Category>? categories = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return new CategorySuggestionDto
            {
                CategoryId = null,
                CategoryName = "Otros",
                Confidence = 0.0,
                Reason = "Descripción vacía"
            };
        }

        var normalizedDesc = description.ToLowerInvariant();
        string bestCategoryMatch = "Otros";
        double highestScore = 0.0;
        string matchedKeyword = string.Empty;

        foreach (var (categoryName, keywords) in KeywordCategoryMap)
        {
            foreach (var kw in keywords)
            {
                if (normalizedDesc.Contains(kw))
                {
                    double score = 0.85 + (kw.Length > 4 ? 0.10 : 0.05);
                    if (score > highestScore)
                    {
                        highestScore = Math.Min(0.98, score);
                        bestCategoryMatch = categoryName;
                        matchedKeyword = kw;
                    }
                }
            }
        }

        var matchedEntity = categories?.FirstOrDefault(c => c.Name.Equals(bestCategoryMatch, StringComparison.OrdinalIgnoreCase))
                           ?? categories?.FirstOrDefault(c => c.Name == "Otros");

        if (highestScore > 0)
        {
            return new CategorySuggestionDto
            {
                CategoryId = matchedEntity?.Id,
                CategoryName = matchedEntity?.Name ?? bestCategoryMatch,
                Confidence = Math.Round(highestScore, 2),
                Reason = $"Identificado patrón clave \"{matchedKeyword}\" en la descripción"
            };
        }

        return new CategorySuggestionDto
        {
            CategoryId = matchedEntity?.Id,
            CategoryName = matchedEntity?.Name ?? "Otros",
            Confidence = 0.30,
            Reason = "Clasificación general por defecto"
        };
    }

    public async Task<CategorySuggestionDto> SuggestCategoryAsync(string description, CancellationToken cancellationToken = default)
    {
        List<Category>? categories = null;
        if (_context != null)
        {
            categories = await _context.Categories.ToListAsync(cancellationToken);
        }

        return ClassifyText(description, categories);
    }
}
