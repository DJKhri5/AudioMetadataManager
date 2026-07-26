namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

/// <summary>
/// Describe el estado de una operación de escritura de
/// metadatos.
/// </summary>
public enum MetadataWriteStatus
{
    /// <summary>
    /// La operación todavía no ha comenzado.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// La solicitud fue validada y puede procesarse.
    /// </summary>
    Validated = 1,

    /// <summary>
    /// El escritor compatible fue localizado.
    /// </summary>
    WriterResolved = 2,

    /// <summary>
    /// El archivo fue abierto para escritura.
    /// </summary>
    FileOpened = 3,

    /// <summary>
    /// Los valores fueron asignados en memoria.
    /// </summary>
    ValuesPrepared = 4,

    /// <summary>
    /// Los metadatos fueron guardados en el archivo.
    /// </summary>
    Saved = 5,

    /// <summary>
    /// La operación de escritura terminó correctamente.
    ///
    /// Este estado todavía no implica que la verificación
    /// posterior haya sido realizada.
    /// </summary>
    Completed = 6,

    /// <summary>
    /// La solicitud no superó la validación previa.
    /// </summary>
    ValidationFailed = 7,

    /// <summary>
    /// No existe un escritor compatible con el formato.
    /// </summary>
    UnsupportedFormat = 8,

    /// <summary>
    /// El archivo no pudo abrirse para escritura.
    /// </summary>
    FileOpenFailed = 9,

    /// <summary>
    /// Ningún cambio válido pudo prepararse.
    /// </summary>
    NoWritableChanges = 10,

    /// <summary>
    /// La escritura produjo algunos resultados correctos y
    /// otros fallidos.
    /// </summary>
    PartiallyCompleted = 11,

    /// <summary>
    /// Ocurrió un error al guardar el archivo.
    /// </summary>
    SaveFailed = 12,

    /// <summary>
    /// La operación fue cancelada.
    /// </summary>
    Cancelled = 13,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 14
}