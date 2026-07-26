namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Describe el estado general de una operación de aplicación
/// de metadatos.
/// </summary>
public enum MetadataApplyStatus
{
    /// <summary>
    /// La operación todavía no ha comenzado.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// La solicitud fue validada y está preparada para
    /// continuar.
    /// </summary>
    Validated = 1,

    /// <summary>
    /// La copia de seguridad fue creada correctamente.
    /// </summary>
    BackupCreated = 2,

    /// <summary>
    /// Los cambios fueron escritos y verificados.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// La operación terminó parcialmente.
    /// Algunos campos pudieron aplicarse y otros no.
    /// </summary>
    PartiallyCompleted = 4,

    /// <summary>
    /// La operación fue rechazada antes de escribir el archivo.
    /// </summary>
    ValidationFailed = 5,

    /// <summary>
    /// No fue posible crear el respaldo obligatorio.
    /// </summary>
    BackupFailed = 6,

    /// <summary>
    /// Ocurrió un error durante la escritura.
    /// </summary>
    WriteFailed = 7,

    /// <summary>
    /// La escritura terminó, pero la verificación posterior
    /// detectó diferencias.
    /// </summary>
    VerificationFailed = 8,

    /// <summary>
    /// La operación fue cancelada por el usuario o por el
    /// sistema.
    /// </summary>
    Cancelled = 9
}