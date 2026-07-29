namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

/// <summary>
/// Contiene el resultado auditable de una operación inmediata
/// de escritura.
///
/// No reemplaza la verificación posterior del archivo.
/// </summary>
public sealed class MetadataWriteResult
{
    /// <summary>
    /// Identificador de la solicitud procesada.
    /// </summary>
    public Guid WriteRequestId { get; init; }

    /// <summary>
    /// Identificador de la solicitud de aplicación.
    /// </summary>
    public Guid ApplyRequestId { get; init; }

    /// <summary>
    /// Identificador del plan.
    /// </summary>
    public Guid PlanId { get; init; }

    /// <summary>
    /// Estado final de esta etapa.
    /// </summary>
    public MetadataWriteStatus Status { get; init; } =
        MetadataWriteStatus.Pending;

    /// <summary>
    /// Ruta del archivo procesado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre del formato o escritor utilizado.
    /// </summary>
    public string WriterName { get; init; } =
        string.Empty;

    /// <summary>
    /// Cantidad de imágenes incrustadas observada durante la
    /// apertura utilizada para escribir.
    ///
    /// Este valor será utilizado por la etapa posterior de
    /// verificación sin realizar una lectura previa adicional.
    /// </summary>
    public int PictureCountBefore { get; init; }

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
    /// Resultado individual por campo.
    /// </summary>
    public IReadOnlyList<MetadataFieldWriteResult>
        FieldResults
    { get; init; } =
            Array.Empty<MetadataFieldWriteResult>();

    /// <summary>
    /// Mensajes globales.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Campos correctamente escritos.
    /// </summary>
    public int WrittenFieldCount =>
        FieldResults.Count(result => result.WasWritten);

    /// <summary>
    /// Campos que no pudieron escribirse.
    /// </summary>
    public int FailedFieldCount =>
        FieldResults.Count - WrittenFieldCount;

    /// <summary>
    /// Indica si el guardado terminó correctamente para todos
    /// los cambios solicitados.
    /// </summary>
    public bool WasSuccessful =>
        Status == MetadataWriteStatus.Completed &&
        FieldResults.Count > 0 &&
        FailedFieldCount == 0;

    /// <summary>
    /// Indica si hubo al menos una escritura válida.
    /// </summary>
    public bool HasWrittenFields =>
        WrittenFieldCount > 0;

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
                    $"Escritura completada mediante " +
                    $"{DisplayWriterName()}. Campos escritos: " +
                    $"{WrittenFieldCount}.";
            }

            return
                $"La escritura terminó con estado {Status}. " +
                $"Correctos: {WrittenFieldCount}. " +
                $"Fallidos: {FailedFieldCount}.";
        }
    }

    private string DisplayWriterName()
    {
        return string.IsNullOrWhiteSpace(WriterName)
            ? "(escritor sin identificar)"
            : WriterName.Trim();
    }
}