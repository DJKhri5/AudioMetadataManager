namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

/// <summary>
/// Contiene el resultado global producido por el futuro
/// MetadataConfidenceEngine.
///
/// Este objeto reúne la puntuación, la cobertura, la decisión
/// final y las explicaciones necesarias para la revisión.
/// </summary>
public sealed class MetadataConfidenceResult
{
    /// <summary>
    /// Indica si la evaluación pudo ejecutarse.
    /// </summary>
    public bool EvaluationCompleted { get; init; }

    /// <summary>
    /// Confianza global de coincidencia.
    ///
    /// Se expresa como un valor entre 0 y 1.
    /// </summary>
    public double ConfidenceScore { get; init; }

    /// <summary>
    /// Similitud ponderada calculada únicamente con los campos
    /// realmente comparables.
    ///
    /// Se expresa como un valor entre 0 y 1.
    /// </summary>
    public double WeightedSimilarity { get; init; }

    /// <summary>
    /// Proporción del peso total para el cual existe al menos
    /// información en una de las fuentes.
    ///
    /// Se expresa como un valor entre 0 y 1.
    /// </summary>
    public double WeightedCoverage { get; init; }

    /// <summary>
    /// Peso total de los campos configurados.
    ///
    /// Con la configuración predeterminada debería ser 1.
    /// </summary>
    public double ConfiguredWeight { get; init; }

    /// <summary>
    /// Peso correspondiente a campos realmente comparables.
    /// </summary>
    public double ComparableWeight { get; init; }

    /// <summary>
    /// Peso correspondiente a campos con información en al
    /// menos una fuente.
    /// </summary>
    public double AvailableInformationWeight { get; init; }

    /// <summary>
    /// Cantidad de conflictos encontrados en campos críticos.
    /// </summary>
    public int CriticalConflicts { get; init; }

    /// <summary>
    /// Cantidad de campos críticos que no pudieron compararse
    /// por falta de información.
    /// </summary>
    public int MissingCriticalFields { get; init; }

    /// <summary>
    /// Decisión global obtenida.
    /// </summary>
    public MetadataComparisonDecision Decision { get; init; } =
        MetadataComparisonDecision.Unknown;

    /// <summary>
    /// Indica si el resultado debe ser confirmado manualmente
    /// antes de aplicar metadatos.
    /// </summary>
    public bool RequiresManualReview { get; init; }

    /// <summary>
    /// Explicación resumida para la interfaz y los informes.
    /// </summary>
    public string Summary { get; init; } =
        string.Empty;

    /// <summary>
    /// Motivos o advertencias individuales que respaldan
    /// la decisión global.
    /// </summary>
    public IReadOnlyList<string> Reasons { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Detalle ordenado de cómo participó cada campo en el
    /// cálculo de confianza.
    ///
    /// Esta colección proporciona trazabilidad para diagnósticos,
    /// interfaz, revisión manual y modo simulación.
    /// </summary>
    public IReadOnlyList<MetadataFieldConfidenceEvaluation>
        FieldEvaluations
    { get; init; } =
            Array.Empty<MetadataFieldConfidenceEvaluation>();

    /// <summary>
    /// Confianza global en formato legible.
    /// </summary>
    public string ConfidenceDisplay =>
        $"{Math.Clamp(ConfidenceScore, 0, 1) * 100.0:0.00}%";

    /// <summary>
    /// Similitud ponderada en formato legible.
    /// </summary>
    public string WeightedSimilarityDisplay =>
        $"{Math.Clamp(WeightedSimilarity, 0, 1) * 100.0:0.00}%";

    /// <summary>
    /// Cobertura ponderada en formato legible.
    /// </summary>
    public string WeightedCoverageDisplay =>
        $"{Math.Clamp(WeightedCoverage, 0, 1) * 100.0:0.00}%";

    /// <summary>
    /// Nombre legible de la decisión final.
    /// </summary>
    public string DecisionDisplay =>
        Decision switch
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