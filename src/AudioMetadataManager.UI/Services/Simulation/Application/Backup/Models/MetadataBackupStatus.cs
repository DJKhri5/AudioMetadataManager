namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;

/// <summary>
/// Describe el estado de una operación de respaldo.
/// </summary>
public enum MetadataBackupStatus
{
    /// <summary>
    /// La operación todavía no ha comenzado.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// La solicitud de respaldo fue validada.
    /// </summary>
    Validated = 1,

    /// <summary>
    /// La carpeta de destino fue preparada.
    /// </summary>
    DestinationPrepared = 2,

    /// <summary>
    /// El archivo fue copiado al destino.
    /// </summary>
    Copied = 3,

    /// <summary>
    /// La copia fue verificada correctamente.
    /// </summary>
    Verified = 4,

    /// <summary>
    /// El respaldo terminó correctamente.
    /// </summary>
    Completed = 5,

    /// <summary>
    /// La solicitud no contiene información suficiente.
    /// </summary>
    ValidationFailed = 6,

    /// <summary>
    /// No fue posible preparar la carpeta de destino.
    /// </summary>
    DestinationPreparationFailed = 7,

    /// <summary>
    /// Ocurrió un error al copiar el archivo.
    /// </summary>
    CopyFailed = 8,

    /// <summary>
    /// El archivo copiado no superó la verificación.
    /// </summary>
    VerificationFailed = 9,

    /// <summary>
    /// La operación fue cancelada.
    /// </summary>
    Cancelled = 10,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 11
}