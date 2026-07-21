using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Rules;

/// <summary>
/// Detecta campos críticos que no pudieron compararse porque
/// una o ambas fuentes carecen de información.
/// </summary>
public sealed class MissingCriticalFieldRule : IConfidenceRule
{
    public string Name =>
        nameof(MissingCriticalFieldRule);

    public int Priority => 500;

    public ConfidenceRuleResult Evaluate(
        ConfidenceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        MetadataFieldConfidenceEvaluation[] missingFields =
            context.FieldEvaluations
                .Where(evaluation =>
                    evaluation.IsCritical &&
                    !evaluation.IsComparable)
                .ToArray();

        context.MissingCriticalFields =
            missingFields.Length;

        if (missingFields.Length == 0)
        {
            return ConfidenceRuleResult.Success(
                Name,
                "Todos los campos críticos pudieron compararse.");
        }

        string fields = string.Join(
            ", ",
            missingFields.Select(
                evaluation =>
                    evaluation.FieldDisplay));

        return ConfidenceRuleResult.Warning(
            Name,
            $"Campos críticos no comparables: {fields}.");
    }
}