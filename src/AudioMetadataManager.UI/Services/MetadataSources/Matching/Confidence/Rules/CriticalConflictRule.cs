using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Rules;

/// <summary>
/// Detecta conflictos en campos configurados como críticos.
/// </summary>
public sealed class CriticalConflictRule : IConfidenceRule
{
    public string Name => nameof(CriticalConflictRule);

    public int Priority => 400;

    public ConfidenceRuleResult Evaluate(
        ConfidenceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        MetadataFieldConfidenceEvaluation[] conflicts =
            context.FieldEvaluations
                .Where(evaluation =>
                    evaluation.IsCritical &&
                    evaluation.HasConflict)
                .ToArray();

        context.CriticalConflicts =
            conflicts.Length;

        if (conflicts.Length == 0)
        {
            return ConfidenceRuleResult.Success(
                Name,
                "No se detectaron conflictos en campos críticos.");
        }

        string fields = string.Join(
            ", ",
            conflicts.Select(
                evaluation =>
                    evaluation.FieldDisplay));

        return ConfidenceRuleResult.Critical(
            Name,
            $"Conflictos críticos detectados en: {fields}.");
    }
}