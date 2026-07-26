namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Contiene el resultado completo y auditable de una operación
/// de aplicación de metadatos.
/// </summary>
public sealed class MetadataApplyResult
{
    /// <summary>
    /// Identificador de la solicitud procesada.
    /// </summary>
    public Guid RequestId { get; init; }

    /// <summary>
    /// Identificador del plan original.
    /// </summary>
    public Guid PlanId { get; init; }

    /// <summary>
    /// Ruta del archivo procesado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre del archivo procesado.
    /// </summary>
    public string FileName { get; init; } =
        string.Empty;

    /// <summary>
    /// Estado final de la operación.
    /// </summary>
    public MetadataApplyStatus Status { get; init; } =
        MetadataApplyStatus.Pending;

    /// <summary>
    /// Momento UTC de inicio.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC de finalización.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>
    /// Duración total.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Ruta del respaldo creado.
    /// Queda vacía cuando no se creó uno.
    /// </summary>
    public string BackupPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Resultados individuales por campo.
    /// </summary>
    public IReadOnlyList<MetadataFieldApplyResult>
        FieldResults
    { get; init; } =
            Array.Empty<MetadataFieldApplyResult>();

    /// <summary>
    /// Mensajes, advertencias y razones globales.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si existe un respaldo utilizable.
    /// </summary>
    public bool HasBackup =>
        !string.IsNullOrWhiteSpace(BackupPath);

    /// <summary>
    /// Cantidad de campos aplicados y verificados.
    /// </summary>
    public int SuccessfulFieldCount =>
        FieldResults.Count(
            result =>
                result.WasSuccessfullyApplied);

    /// <summary>
    /// Cantidad de campos que no se completaron.
    /// </summary>
    public int FailedFieldCount =>
        FieldResults.Count -
        SuccessfulFieldCount;

    /// <summary>
    /// Indica si la operación terminó completamente bien.
    /// </summary>
    public bool WasSuccessful =>
        Status == MetadataApplyStatus.Completed &&
        FailedFieldCount == 0;

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
                    $"{FileName}: se aplicaron y verificaron " +
                    $"{SuccessfulFieldCount} cambio(s).";
            }

            return
                $"{FileName}: estado {Status}. " +
                $"Correctos: {SuccessfulFieldCount}. " +
                $"Fallidos: {FailedFieldCount}.";
        }
    }
}