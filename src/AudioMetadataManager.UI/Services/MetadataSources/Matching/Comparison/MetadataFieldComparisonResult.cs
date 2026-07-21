using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;

/// <summary>
/// Representa la comparación completa de un único campo
/// entre dos fuentes de metadatos.
/// </summary>
public sealed class MetadataFieldComparisonResult
{
    /// <summary>
    /// Identificador fuerte del campo comparado.
    ///
    /// Esta propiedad será utilizada por los motores de
    /// confianza y consenso para evitar depender de textos.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Nombre del campo comparado.
    ///
    /// Se conserva temporalmente para mantener compatibilidad
    /// con los comparadores existentes durante la migración.
    /// </summary>
    public string FieldName { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre efectivo del campo.
    ///
    /// Prioriza el identificador fuerte. Si el resultado todavía
    /// no ha sido migrado, utiliza el nombre textual existente.
    /// </summary>
    public string EffectiveFieldName =>
        Field != MetadataField.Unknown
            ? Field.ToString()
            : FieldName;

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
    ///
    /// 0 = completamente distintos.
    /// 1 = idénticos.
    /// </summary>
    public double Similarity { get; init; }

    /// <summary>
    /// Explicación legible para el usuario.
    /// </summary>
    public string Explanation { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el resultado utiliza el identificador fuerte.
    /// </summary>
    public bool HasStrongFieldIdentifier =>
        Field != MetadataField.Unknown;

    /// <summary>
    /// Indica si ambos valores son utilizables.
    /// </summary>
    public bool HasBothValues =>
        !string.IsNullOrWhiteSpace(LocalValue) &&
        !string.IsNullOrWhiteSpace(ReferenceValue);

    /// <summary>
    /// Indica si existe información en al menos una de las
    /// fuentes comparadas.
    /// </summary>
    public bool HasAnyValue =>
        !string.IsNullOrWhiteSpace(LocalValue) ||
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