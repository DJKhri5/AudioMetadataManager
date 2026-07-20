namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;

/// <summary>
/// Representa la comparación completa de un único campo
/// entre dos fuentes de metadatos.
/// </summary>
public sealed class MetadataFieldComparisonResult
{
    /// <summary>
    /// Nombre del campo comparado.
    /// Ejemplo:
    /// Title
    /// Artist
    /// Year
    /// Genre
    /// </summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>
    /// Valor obtenido desde el archivo local.
    /// </summary>
    public string? LocalValue { get; init; }

    /// <summary>
    /// Valor obtenido desde la fuente de referencia.
    /// </summary>
    public string? ReferenceValue { get; init; }

    /// <summary>
    /// Resultado de la comparación.
    /// </summary>
    public MetadataFieldComparisonStatus Status { get; init; }

    /// <summary>
    /// Similitud entre ambos valores.
    /// 0 = completamente distintos
    /// 1 = idénticos
    /// </summary>
    public double Similarity { get; init; }

    /// <summary>
    /// Explicación legible para el usuario.
    /// </summary>
    public string Explanation { get; init; } = string.Empty;

    /// <summary>
    /// Indica si ambos valores son utilizables.
    /// </summary>
    public bool HasBothValues =>
        !string.IsNullOrWhiteSpace(LocalValue) &&
        !string.IsNullOrWhiteSpace(ReferenceValue);

    /// <summary>
    /// Indica si existe una discrepancia.
    /// </summary>
    public bool HasConflict =>
        Status == MetadataFieldComparisonStatus.Conflict;

    /// <summary>
    /// Indica si existe una coincidencia.
    /// </summary>
    public bool IsMatch =>
        Status == MetadataFieldComparisonStatus.ExactMatch ||
        Status == MetadataFieldComparisonStatus.NormalizedMatch ||
        Status == MetadataFieldComparisonStatus.ProbableMatch;
}