namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

/// <summary>
/// Explica por qué terminó o se detuvo una ejecución del
/// pipeline de aplicación.
/// </summary>
public enum MetadataApplicationStopReason
{
    /// <summary>
    /// El pipeline todavía no ha finalizado.
    /// </summary>
    None = 0,

    /// <summary>
    /// Todas las etapas obligatorias terminaron correctamente.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// La solicitud no superó la validación previa.
    /// </summary>
    ValidationFailed = 2,

    /// <summary>
    /// No fue posible crear el respaldo obligatorio.
    /// </summary>
    BackupFailed = 3,

    /// <summary>
    /// Ocurrió un error durante la escritura de metadatos.
    /// </summary>
    MetadataWriteFailed = 4,

    /// <summary>
    /// Los valores escritos no pudieron verificarse.
    /// </summary>
    VerificationFailed = 5,

    /// <summary>
    /// La ejecución fue cancelada.
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 7
}