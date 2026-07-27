using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

/// <summary>
/// Describe el resultado de preparar en memoria un campo
/// individual mediante TagLibSharp.
///
/// Este resultado no implica que el archivo haya sido guardado.
/// </summary>
public sealed class TagLibMp3FieldPreparationResult
{
    /// <summary>
    /// Campo solicitado.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor leído antes de preparar el cambio.
    /// </summary>
    public string OriginalValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor solicitado por el plan.
    /// </summary>
    public string RequestedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor presente en memoria después de la asignación.
    /// </summary>
    public string PreparedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si esta versión del adaptador reconoce el campo.
    /// </summary>
    public bool IsSupported { get; init; }

    /// <summary>
    /// Indica si el valor pudo asignarse al objeto TagLib.Tag.
    /// </summary>
    public bool WasPrepared { get; init; }

    /// <summary>
    /// Indica si el valor preparado coincide con el solicitado.
    /// </summary>
    public bool MatchesRequestedValue { get; init; }

    /// <summary>
    /// Explicación del resultado.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Resultado satisfactorio de la preparación en memoria.
    /// </summary>
    public bool WasSuccessful =>
        IsSupported &&
        WasPrepared &&
        MatchesRequestedValue;

    /// <summary>
    /// Resumen legible.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    $"{Field}: cambio preparado correctamente " +
                    "en memoria.";
            }

            if (!IsSupported)
            {
                return
                    $"{Field}: campo no soportado todavía por " +
                    "el preparador MP3.";
            }

            return
                $"{Field}: el valor no pudo prepararse " +
                "correctamente.";
        }
    }
}