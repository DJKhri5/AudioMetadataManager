using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

/// <summary>
/// Describe el resultado inmediato de preparar o escribir un
/// campo individual.
///
/// La comprobación del valor guardado pertenecerá al futuro
/// motor de verificación posterior.
/// </summary>
public sealed class MetadataFieldWriteResult
{
    /// <summary>
    /// Campo procesado.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor existente antes de la escritura.
    /// </summary>
    public string OriginalValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor solicitado.
    /// </summary>
    public string RequestedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el escritor reconoce el campo.
    /// </summary>
    public bool IsSupported { get; init; }

    /// <summary>
    /// Indica si el nuevo valor fue asignado correctamente en
    /// la representación de metadatos.
    /// </summary>
    public bool ValuePrepared { get; init; }

    /// <summary>
    /// Indica si la operación de guardado general terminó sin
    /// errores para este campo.
    /// </summary>
    public bool SaveSucceeded { get; init; }

    /// <summary>
    /// Mensaje técnico o explicación.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el campo atravesó correctamente la etapa de
    /// escritura.
    ///
    /// Aún falta la verificación posterior.
    /// </summary>
    public bool WasWritten =>
        IsSupported &&
        ValuePrepared &&
        SaveSucceeded;

    /// <summary>
    /// Resumen legible.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasWritten)
            {
                return
                    $"{Field}: valor preparado y guardado.";
            }

            if (!IsSupported)
            {
                return
                    $"{Field}: campo no soportado por el " +
                    "escritor seleccionado.";
            }

            if (!ValuePrepared)
            {
                return
                    $"{Field}: no fue posible preparar el " +
                    "nuevo valor.";
            }

            return
                $"{Field}: el archivo no pudo guardarse.";
        }
    }
}