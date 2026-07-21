using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Rules;

/// <summary>
/// Combina la similitud, cobertura y condiciones críticas para
/// producir la confianza global y la decisión final.
/// </summary>
public sealed class DecisionRule : IConfidenceRule
{
    public string Name => nameof(DecisionRule);

    public int Priority => 900;

    public ConfidenceRuleResult Evaluate(
        ConfidenceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfidenceScore =
            Math.Clamp(
                context.WeightedSimilarity *
                context.WeightedCoverage,
                0,
                1);

        bool artistComparable =
            IsCriticalFieldComparable(
                context,
                MetadataField.Artist);

        bool titleComparable =
            IsCriticalFieldComparable(
                context,
                MetadataField.Title);

        if (context.ConfiguredWeight <= 0 ||
            context.ComparableWeight <= 0)
        {
            SetDecision(
                context,
                MetadataComparisonDecision.InsufficientData,
                requiresManualReview: true);

            return ConfidenceRuleResult.Warning(
                Name,
                "La información disponible no permite emitir una decisión fiable.");
        }

        if (context.CriticalConflicts > 0)
        {
            SetDecision(
                context,
                MetadataComparisonDecision.Rejected,
                requiresManualReview: true);

            return ConfidenceRuleResult.Critical(
                Name,
                "La coincidencia fue rechazada por conflictos en campos críticos.");
        }

        if (!artistComparable ||
            !titleComparable)
        {
            SetDecision(
                context,
                MetadataComparisonDecision.ManualReviewRequired,
                requiresManualReview: true);

            return ConfidenceRuleResult.Warning(
                Name,
                "Artist y Title deben ser comparables antes de aceptar una coincidencia.");
        }

        if (context.ConfidenceScore < 0.70 ||
            context.WeightedCoverage < 0.50)
        {
            SetDecision(
                context,
                MetadataComparisonDecision.ManualReviewRequired,
                requiresManualReview: true);

            return ConfidenceRuleResult.Warning(
                Name,
                $"La confianza global es insuficiente: " +
                $"{context.ConfidenceScore * 100.0:0.00}%.");
        }

        if (context.MissingCriticalFields > 0 ||
            context.ConfidenceScore < 0.90 ||
            context.WeightedCoverage < 0.80 ||
            context.Comparison.HasConflicts)
        {
            SetDecision(
                context,
                MetadataComparisonDecision.AcceptedWithReview,
                requiresManualReview: true);

            return ConfidenceRuleResult.Warning(
                Name,
                "La coincidencia es razonable, pero debe confirmarse manualmente.");
        }

        SetDecision(
            context,
            MetadataComparisonDecision.Accepted,
            requiresManualReview: false);

        return ConfidenceRuleResult.Success(
            Name,
            "La coincidencia alcanza los requisitos para ser aceptada.");
    }

    private static bool IsCriticalFieldComparable(
        ConfidenceContext context,
        MetadataField field)
    {
        return context.FieldEvaluations.Any(
            evaluation =>
                evaluation.Field == field &&
                evaluation.IsComparable);
    }

    private static void SetDecision(
        ConfidenceContext context,
        MetadataComparisonDecision decision,
        bool requiresManualReview)
    {
        context.Decision = decision;
        context.RequiresManualReview =
            requiresManualReview;
    }
}