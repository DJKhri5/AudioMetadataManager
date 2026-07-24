using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;

/// <summary>
/// Representa la propuesta de una fuente externa para un
/// campo concreto de metadatos.
///
/// Conserva el valor original, la procedencia y la evaluación
/// previa del candidato para garantizar trazabilidad.
/// </summary>
public sealed class MetadataConsensusContribution
{
    /// <summary>
    /// Campo al que corresponde la propuesta.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor original propuesto por la fuente.
    /// </summary>
    public string Value { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor preparado para agrupar propuestas equivalentes.
    ///
    /// No reemplaza el texto original que finalmente podría
    /// mostrarse o aplicarse.
    /// </summary>
    public string NormalizedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Plataforma que entregó la propuesta.
    /// </summary>
    public string SourceName { get; init; } =
        string.Empty;

    /// <summary>
    /// Identificador del candidato dentro de la plataforma.
    /// </summary>
    public string SourceId { get; init; } =
        string.Empty;

    /// <summary>
    /// Posición original del candidato dentro de la respuesta
    /// de la plataforma.
    /// </summary>
    public int SourceRank { get; init; }

    /// <summary>
    /// Confianza obtenida previamente al evaluar el candidato
    /// frente a la identidad local.
    ///
    /// Se expresa como un valor entre 0 y 1.
    /// </summary>
    public double CandidateConfidence { get; init; }

    /// <summary>
    /// Peso de confiabilidad asignado a la fuente para este
    /// campo concreto.
    ///
    /// Se expresa como un valor entre 0 y 1.
    /// </summary>
    public double SourceWeight { get; init; }

    /// <summary>
    /// Indica si esta propuesta requiere aprobación manual por
    /// una regla propia de su fuente.
    ///
    /// SoundCloud deberá conservar este valor en true.
    /// </summary>
    public bool RequiresManualApproval { get; init; }

    /// <summary>
    /// Indica si existe un valor que pueda participar en el
    /// consenso.
    /// </summary>
    public bool IsUsable =>
        Field != MetadataField.Unknown &&
        !string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Aporte ponderado preliminar de la propuesta.
    ///
    /// Combina la confianza del candidato y el peso asignado
    /// a la fuente.
    /// </summary>
    public double WeightedSupport =>
        Math.Clamp(
            CandidateConfidence,
            0,
            1) *
        Math.Clamp(
            SourceWeight,
            0,
            1);

    /// <summary>
    /// Procedencia preparada para informes y auditoría.
    /// </summary>
    public string SourceDisplay =>
        string.IsNullOrWhiteSpace(SourceId)
            ? SourceName
            : $"{SourceName} · {SourceId}";
}