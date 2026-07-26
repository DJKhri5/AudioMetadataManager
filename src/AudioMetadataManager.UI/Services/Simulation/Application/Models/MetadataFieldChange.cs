using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Representa un cambio aprobado que se pretende escribir en
/// un campo específico del archivo.
/// </summary>
public sealed class MetadataFieldChange
{
    /// <summary>
    /// Campo que se pretende modificar.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor leído antes de aplicar el cambio.
    /// </summary>
    public string OriginalValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor aprobado que debería escribirse.
    /// </summary>
    public string NewValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la propuesta fue aprobada manualmente.
    /// </summary>
    public bool WasManuallyApproved { get; init; }

    /// <summary>
    /// Confianza que respaldó la propuesta.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Fuentes externas que respaldaron el valor.
    /// </summary>
    public IReadOnlyList<string> SupportingSources
    { get; init; } =
            Array.Empty<string>();

    /// <summary>
    /// Indica si existe una modificación real y utilizable.
    /// </summary>
    public bool IsValidChange =>
        Field != MetadataField.Unknown &&
        !string.IsNullOrWhiteSpace(NewValue) &&
        !string.Equals(
            Normalize(OriginalValue),
            Normalize(NewValue),
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Confianza preparada para diagnóstico.
    /// </summary>
    public string ConfidenceDisplay =>
        $"{Math.Clamp(Confidence, 0, 1) * 100:0.00}%";

    /// <summary>
    /// Resumen legible del cambio.
    /// </summary>
    public string Summary =>
        $"{Field}: {DisplayValue(OriginalValue)} → " +
        $"{DisplayValue(NewValue)}.";

    private static string Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(sin información)"
            : value.Trim();
    }
}