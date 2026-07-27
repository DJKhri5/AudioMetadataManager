using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Models;

/// <summary>
/// Describe el resultado de traducir y preparar un cambio de
/// metadatos dentro de una etiqueta TagLibSharp.
///
/// Todavía no representa un guardado físico en el archivo.
/// </summary>
public sealed class TagLibFieldMappingResult
{
    /// <summary>
    /// Campo procesado.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor leído antes de preparar el cambio.
    /// </summary>
    public string OriginalValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor solicitado por el plan aprobado.
    /// </summary>
    public string RequestedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor leído inmediatamente después de asignarlo en la
    /// representación TagLibSharp.
    /// </summary>
    public string PreparedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el campo está reconocido por el mapper.
    /// </summary>
    public bool IsSupported { get; init; }

    /// <summary>
    /// Indica si el valor quedó correctamente preparado en
    /// memoria.
    /// </summary>
    public bool ValuePrepared { get; init; }

    /// <summary>
    /// Explicación de la preparación.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la etapa de mapping fue satisfactoria.
    /// </summary>
    public bool WasSuccessful =>
        IsSupported &&
        ValuePrepared;

    /// <summary>
    /// Resumen compacto.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    $"{Field}: valor preparado correctamente.";
            }

            if (!IsSupported)
            {
                return
                    $"{Field}: campo no soportado por el mapper.";
            }

            return
                $"{Field}: el valor no pudo prepararse.";
        }
    }
}