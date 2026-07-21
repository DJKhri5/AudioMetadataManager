using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Providers;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Rules;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Engine;

/// <summary>
/// Coordina la evaluación de confianza sobre un resultado
/// técnico de comparación de metadatos.
///
/// El motor no contiene reglas específicas. Ejecuta las reglas
/// registradas y transforma el contexto acumulado en un
/// MetadataConfidenceResult.
/// </summary>
public sealed class MetadataConfidenceEngine
{
    private readonly IMetadataFieldWeightProvider
        _weightProvider;

    private readonly IReadOnlyList<IConfidenceRule>
        _rules;

    /// <summary>
    /// Crea el motor con la configuración predeterminada.
    /// </summary>
    public MetadataConfidenceEngine()
        : this(
            new DefaultMetadataFieldWeightProvider(),
            CreateDefaultRules())
    {
    }

    /// <summary>
    /// Crea un motor utilizando el proveedor y las reglas
    /// recibidas.
    /// </summary>
    public MetadataConfidenceEngine(
        IMetadataFieldWeightProvider weightProvider,
        IEnumerable<IConfidenceRule> rules)
    {
        ArgumentNullException.ThrowIfNull(
            weightProvider);

        ArgumentNullException.ThrowIfNull(
            rules);

        _weightProvider = weightProvider;

        _rules = rules
            .OrderBy(rule => rule.Priority)
            .ToList();
    }

    /// <summary>
    /// Ejecuta la evaluación global de confianza.
    /// </summary>
    public MetadataConfidenceResult Evaluate(
        MetadataComparisonResult comparison)
    {
        ArgumentNullException.ThrowIfNull(
            comparison);

        IReadOnlyDictionary<
            MetadataField,
            MetadataFieldWeight> weights =
                _weightProvider.GetWeights();

        ConfidenceContext context = new()
        {
            Comparison = comparison,
            Weights = weights,
            ConfiguredWeight = weights
                .Values
                .Where(weight => weight.IsValid)
                .Sum(weight => weight.Weight)
        };

        List<ConfidenceRuleResult> ruleResults =
            new();

        foreach (IConfidenceRule rule in _rules)
        {
            ConfidenceRuleResult ruleResult =
                rule.Evaluate(context);

            ruleResults.Add(ruleResult);

            if (!string.IsNullOrWhiteSpace(
                    ruleResult.Message))
            {
                context.Reasons.Add(
                    ruleResult.Message);
            }
        }

        bool evaluationCompleted =
            _rules.Count > 0 &&
            ruleResults.All(result =>
                result.EvaluationCompleted);

        string summary =
            BuildSummary(
                context,
                evaluationCompleted);

        return new MetadataConfidenceResult
        {
            EvaluationCompleted =
                evaluationCompleted,

            ConfidenceScore = Math.Clamp(
                context.ConfidenceScore,
                0,
                1),

            WeightedSimilarity = Math.Clamp(
                context.WeightedSimilarity,
                0,
                1),

            WeightedCoverage = Math.Clamp(
                context.WeightedCoverage,
                0,
                1),

            ConfiguredWeight = Math.Max(
                0,
                context.ConfiguredWeight),

            ComparableWeight = Math.Max(
                0,
                context.ComparableWeight),

            AvailableInformationWeight =
                Math.Max(
                    0,
                    context.AvailableInformationWeight),

            CriticalConflicts =
                context.CriticalConflicts,

            MissingCriticalFields =
                context.MissingCriticalFields,

            Decision =
                context.Decision,

            RequiresManualReview =
                context.RequiresManualReview,

            Summary =
                summary,

            Reasons =
                context.Reasons.ToArray(),

            FieldEvaluations =
                context.FieldEvaluations.ToArray()
        };
    }

    private static IReadOnlyList<IConfidenceRule>
        CreateDefaultRules()
    {
        return new IConfidenceRule[]
        {
            new FieldEvaluationRule(),
            new SimilarityRule(),
            new CoverageRule(),
            new CriticalConflictRule(),
            new MissingCriticalFieldRule(),
            new DecisionRule()
        };
    }

    private static string BuildSummary(
        ConfidenceContext context,
        bool evaluationCompleted)
    {
        if (!evaluationCompleted)
        {
            return
                "La evaluación de confianza no pudo completarse.";
        }

        return
            $"Confianza global: " +
            $"{context.ConfidenceScore * 100.0:0.00}%. " +
            $"Decisión: {GetDecisionDisplay(context.Decision)}. " +
            $"Revisión manual: " +
            $"{(context.RequiresManualReview ? "Sí" : "No")}.";
    }

    private static string GetDecisionDisplay(
        MetadataComparisonDecision decision)
    {
        return decision switch
        {
            MetadataComparisonDecision.Accepted =>
                "Coincidencia aceptada",

            MetadataComparisonDecision.AcceptedWithReview =>
                "Coincidencia aceptable con revisión",

            MetadataComparisonDecision.ManualReviewRequired =>
                "Revisión manual requerida",

            MetadataComparisonDecision.Rejected =>
                "Coincidencia rechazada",

            MetadataComparisonDecision.InsufficientData =>
                "Información insuficiente",

            _ =>
                "Sin evaluación"
        };
    }
}