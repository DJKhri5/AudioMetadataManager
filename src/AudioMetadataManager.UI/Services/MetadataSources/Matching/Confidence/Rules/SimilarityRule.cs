using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Rules;

/// <summary>
/// Calcula la similitud ponderada utilizando únicamente
/// los campos realmente comparables.
/// </summary>
public sealed class SimilarityRule : IConfidenceRule
{
    public string Name => nameof(SimilarityRule);

    public int Priority => 200;

    public ConfidenceRuleResult Evaluate(
        ConfidenceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ComparableWeight <= 0)
        {
            context.WeightedSimilarity = 0;

            return ConfidenceRuleResult.NotEvaluated(
                Name,
                "No existen campos comparables para calcular la similitud ponderada.");
        }

        context.WeightedSimilarity =
            Math.Clamp(
                context.WeightedContribution /
                context.ComparableWeight,
                0,
                1);

        return ConfidenceRuleResult.Success(
            Name,
            $"Similitud ponderada: " +
            $"{context.WeightedSimilarity * 100.0:0.00}%.");
    }
}