using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Rules;

/// <summary>
/// Convierte los resultados técnicos de comparación en
/// evaluaciones ponderadas por campo.
///
/// Esta regla constituye la base de todos los cálculos
/// posteriores de similitud, cobertura y confianza.
/// </summary>
public sealed class FieldEvaluationRule : IConfidenceRule
{
    public string Name => nameof(FieldEvaluationRule);

    public int Priority => 100;

    public ConfidenceRuleResult Evaluate(
        ConfidenceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.FieldEvaluations.Clear();

        context.ComparableWeight = 0;
        context.AvailableInformationWeight = 0;
        context.WeightedContribution = 0;

        foreach (KeyValuePair<
                     MetadataField,
                     MetadataFieldWeight> entry
                 in context.Weights.OrderBy(item => item.Key))
        {
            MetadataField field = entry.Key;
            MetadataFieldWeight fieldWeight = entry.Value;

            if (!fieldWeight.IsValid)
            {
                continue;
            }

            MetadataFieldComparisonResult? comparisonField =
                FindComparisonField(
                    context.Comparison,
                    field);

            MetadataFieldComparisonStatus status =
                comparisonField?.Status ??
                MetadataFieldComparisonStatus.MissingBothValues;

            string? localValue =
                comparisonField?.LocalValue;

            string? referenceValue =
                comparisonField?.ReferenceValue;

            bool hasLocalValue =
                !string.IsNullOrWhiteSpace(localValue);

            bool hasReferenceValue =
                !string.IsNullOrWhiteSpace(referenceValue);

            bool isComparable =
                hasLocalValue &&
                hasReferenceValue &&
                IsComparableStatus(status);

            bool hasAnyValue =
                hasLocalValue ||
                hasReferenceValue;

            bool hasConflict =
                status ==
                MetadataFieldComparisonStatus.Conflict;

            double similarity =
                isComparable
                    ? Math.Clamp(
                        comparisonField?.Similarity ?? 0,
                        0,
                        1)
                    : 0;

            double weightedContribution =
                isComparable
                    ? fieldWeight.Weight * similarity
                    : 0;

            if (isComparable)
            {
                context.ComparableWeight +=
                    fieldWeight.Weight;

                context.WeightedContribution +=
                    weightedContribution;
            }

            if (hasAnyValue)
            {
                context.AvailableInformationWeight +=
                    fieldWeight.Weight;
            }

            context.FieldEvaluations.Add(
                new MetadataFieldConfidenceEvaluation
                {
                    Field = field,
                    ComparisonStatus = status,
                    LocalValue = localValue,
                    ReferenceValue = referenceValue,
                    ConfiguredWeight =
                        fieldWeight.Weight,
                    Similarity = similarity,
                    WeightedContribution =
                        weightedContribution,
                    IsCritical =
                        fieldWeight.IsCritical,
                    IsComparable =
                        isComparable,
                    HasAnyValue =
                        hasAnyValue,
                    HasConflict =
                        hasConflict,
                    Explanation =
                        BuildExplanation(
                            field,
                            status,
                            isComparable,
                            hasAnyValue,
                            weightedContribution)
                });
        }

        if (context.FieldEvaluations.Count == 0)
        {
            return ConfidenceRuleResult.NotEvaluated(
                Name,
                "No existen campos ponderados disponibles para evaluar.");
        }

        return ConfidenceRuleResult.Success(
            Name,
            $"Se evaluaron {context.FieldEvaluations.Count} campos ponderados.");
    }

    private static MetadataFieldComparisonResult?
        FindComparisonField(
            MetadataComparisonResult comparison,
            MetadataField field)
    {
        MetadataFieldComparisonResult? strongMatch =
            comparison.Fields.FirstOrDefault(
                result => result.Field == field);

        if (strongMatch is not null)
        {
            return strongMatch;
        }

        string expectedName = field.ToString();

        return comparison.Fields.FirstOrDefault(
            result =>
                string.Equals(
                    result.EffectiveFieldName,
                    expectedName,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsComparableStatus(
        MetadataFieldComparisonStatus status)
    {
        return status is
            MetadataFieldComparisonStatus.ExactMatch or
            MetadataFieldComparisonStatus.NormalizedMatch or
            MetadataFieldComparisonStatus.ProbableMatch or
            MetadataFieldComparisonStatus.Conflict;
    }

    private static string BuildExplanation(
        MetadataField field,
        MetadataFieldComparisonStatus status,
        bool isComparable,
        bool hasAnyValue,
        double weightedContribution)
    {
        if (!hasAnyValue)
        {
            return
                $"{field}: no existe información en ninguna fuente.";
        }

        if (!isComparable)
        {
            return
                $"{field}: existe información solamente en una fuente.";
        }

        if (status ==
            MetadataFieldComparisonStatus.Conflict)
        {
            return
                $"{field}: se detectó un conflicto entre las fuentes.";
        }

        return
            $"{field}: aportó " +
            $"{weightedContribution * 100.0:0.00}% " +
            "a la evaluación ponderada.";
    }
}