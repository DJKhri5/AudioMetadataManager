using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

/// <summary>
/// Conserva el estado acumulado durante la evaluación de
/// confianza de una comparación de metadatos.
///
/// Las reglas consultan y actualizan este contexto. El motor
/// solamente coordina su ejecución y construye el resultado final.
/// </summary>
public sealed class ConfidenceContext
{
    /// <summary>
    /// Resultado técnico que será evaluado.
    /// </summary>
    public required MetadataComparisonResult Comparison { get; init; }

    /// <summary>
    /// Configuración de pesos utilizada en la evaluación.
    /// </summary>
    public required IReadOnlyDictionary<
        MetadataField,
        MetadataFieldWeight> Weights
    { get; init; }

    /// <summary>
    /// Peso total de los campos configurados.
    /// </summary>
    public double ConfiguredWeight { get; set; }

    /// <summary>
    /// Peso acumulado de los campos con valores en ambas fuentes.
    /// </summary>
    public double ComparableWeight { get; set; }

    /// <summary>
    /// Peso acumulado de los campos que contienen información
    /// en al menos una de las fuentes.
    /// </summary>
    public double AvailableInformationWeight { get; set; }

    /// <summary>
    /// Suma del aporte ponderado de los campos comparables.
    /// </summary>
    public double WeightedContribution { get; set; }

    /// <summary>
    /// Similitud ponderada normalizada entre campos comparables.
    /// </summary>
    public double WeightedSimilarity { get; set; }

    /// <summary>
    /// Cobertura ponderada respecto del peso configurado.
    /// </summary>
    public double WeightedCoverage { get; set; }

    /// <summary>
    /// Confianza global resultante.
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// Cantidad de conflictos en campos críticos.
    /// </summary>
    public int CriticalConflicts { get; set; }

    /// <summary>
    /// Cantidad de campos críticos que no pudieron compararse.
    /// </summary>
    public int MissingCriticalFields { get; set; }

    /// <summary>
    /// Decisión acumulada durante la evaluación.
    /// </summary>
    public MetadataComparisonDecision Decision { get; set; } =
        MetadataComparisonDecision.Unknown;

    /// <summary>
    /// Indica si debe intervenir el usuario.
    /// </summary>
    public bool RequiresManualReview { get; set; }

    /// <summary>
    /// Explicaciones acumuladas por las reglas.
    /// </summary>
    public List<string> Reasons { get; } = new();

    /// <summary>
    /// Evaluaciones detalladas de cada campo.
    /// </summary>
    public List<MetadataFieldConfidenceEvaluation>
        FieldEvaluations
    { get; } = new();
}