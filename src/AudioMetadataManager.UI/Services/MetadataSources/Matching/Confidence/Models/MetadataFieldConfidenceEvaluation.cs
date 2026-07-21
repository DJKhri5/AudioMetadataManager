using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

/// <summary>
/// Describe cómo un único campo participó en el cálculo
/// de confianza global.
///
/// Permite explicar el peso, la similitud, el aporte obtenido
/// y cualquier advertencia detectada durante la evaluación.
/// </summary>
public sealed class MetadataFieldConfidenceEvaluation
{
    /// <summary>
    /// Campo evaluado.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Estado producido por el motor de comparación.
    /// </summary>
    public MetadataFieldComparisonStatus ComparisonStatus { get; init; }

    /// <summary>
    /// Valor local analizado.
    /// </summary>
    public string? LocalValue { get; init; }

    /// <summary>
    /// Valor de referencia analizado.
    /// </summary>
    public string? ReferenceValue { get; init; }

    /// <summary>
    /// Peso configurado para el campo.
    ///
    /// Se expresa entre 0 y 1.
    /// </summary>
    public double ConfiguredWeight { get; init; }

    /// <summary>
    /// Similitud original del campo.
    ///
    /// Se expresa entre 0 y 1.
    /// </summary>
    public double Similarity { get; init; }

    /// <summary>
    /// Aporte ponderado obtenido por el campo.
    ///
    /// Normalmente corresponde a:
    /// peso configurado × similitud.
    /// </summary>
    public double WeightedContribution { get; init; }

    /// <summary>
    /// Indica si el campo se considera crítico.
    /// </summary>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Indica si existían valores utilizables en ambas fuentes.
    /// </summary>
    public bool IsComparable { get; init; }

    /// <summary>
    /// Indica si existía información en al menos una fuente.
    /// </summary>
    public bool HasAnyValue { get; init; }

    /// <summary>
    /// Indica si el campo produjo un conflicto.
    /// </summary>
    public bool HasConflict { get; init; }

    /// <summary>
    /// Explicación concreta del papel del campo dentro
    /// de la evaluación global.
    /// </summary>
    public string Explanation { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre legible del campo.
    /// </summary>
    public string FieldDisplay =>
        Field switch
        {
            MetadataField.Artist => "Artist",
            MetadataField.Title => "Title",
            MetadataField.Version => "Version",
            MetadataField.Album => "Album",
            MetadataField.Label => "Label",
            MetadataField.Genre => "Genre",
            _ => "Unknown"
        };

    /// <summary>
    /// Peso configurado en formato legible.
    /// </summary>
    public string ConfiguredWeightDisplay =>
        $"{Math.Clamp(ConfiguredWeight, 0, 1) * 100.0:0.##}%";

    /// <summary>
    /// Similitud del campo en formato legible.
    /// </summary>
    public string SimilarityDisplay =>
        $"{Math.Clamp(Similarity, 0, 1) * 100.0:0.00}%";

    /// <summary>
    /// Aporte ponderado en formato legible.
    /// </summary>
    public string WeightedContributionDisplay =>
        $"{Math.Clamp(WeightedContribution, 0, 1) * 100.0:0.00}%";
}