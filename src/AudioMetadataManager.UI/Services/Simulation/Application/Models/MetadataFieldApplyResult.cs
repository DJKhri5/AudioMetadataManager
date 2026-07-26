using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Describe el resultado de aplicar y verificar un campo
/// individual.
/// </summary>
public sealed class MetadataFieldApplyResult
{
    /// <summary>
    /// Campo procesado.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor existente antes de la operación.
    /// </summary>
    public string OriginalValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor solicitado.
    /// </summary>
    public string RequestedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor leído después de la escritura.
    /// </summary>
    public string VerifiedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el campo fue escrito sin excepción.
    /// </summary>
    public bool WriteSucceeded { get; init; }

    /// <summary>
    /// Indica si el valor posterior coincide con el solicitado.
    /// </summary>
    public bool VerificationSucceeded { get; init; }

    /// <summary>
    /// Mensaje de error o explicación.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el campo quedó completamente aplicado.
    /// </summary>
    public bool WasSuccessfullyApplied =>
        WriteSucceeded &&
        VerificationSucceeded;

    /// <summary>
    /// Resumen legible del resultado.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessfullyApplied)
            {
                return
                    $"{Field}: cambio aplicado y verificado.";
            }

            if (!WriteSucceeded)
            {
                return
                    $"{Field}: no fue posible escribir el valor.";
            }

            return
                $"{Field}: el valor escrito no pudo verificarse.";
        }
    }
}