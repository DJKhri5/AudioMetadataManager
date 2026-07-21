using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Rules;

/// <summary>
/// Calcula qué proporción del peso configurado dispone de
/// información en al menos una de las dos fuentes.
/// </summary>
public sealed class CoverageRule : IConfidenceRule
{
    public string Name => nameof(CoverageRule);

    public int Priority => 300;

    public ConfidenceRuleResult Evaluate(
        ConfidenceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ConfiguredWeight <= 0)
        {
            context.WeightedCoverage = 0;

            return ConfidenceRuleResult.NotEvaluated(
                Name,
                "La configuración no contiene un peso total válido.");
        }

        context.WeightedCoverage =
            Math.Clamp(
                context.AvailableInformationWeight /
                context.ConfiguredWeight,
                0,
                1);

        if (context.WeightedCoverage < 0.50)
        {
            return ConfidenceRuleResult.Warning(
                Name,
                $"Cobertura ponderada baja: " +
                $"{context.WeightedCoverage * 100.0:0.00}%.");
        }

        return ConfidenceRuleResult.Success(
            Name,
            $"Cobertura ponderada: " +
            $"{context.WeightedCoverage * 100.0:0.00}%.");
    }
}