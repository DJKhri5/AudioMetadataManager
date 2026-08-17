namespace AudioMetadataManager.UI.Services.Renaming.Models;

/// <summary>
/// Estados de validación del proceso de renombrado seguro.
/// </summary>
public enum RenameValidationStatus
{
    /// <summary>
    /// El nombre propuesto es válido, seguro y está listo para ejecutarse.
    /// </summary>
    ReadyToRename,

    /// <summary>
    /// El nombre propuesto es exactamente igual al nombre actual (no requiere cambios).
    /// </summary>
    IdenticalNameNoOp,

    /// <summary>
    /// No existe una propuesta calculada o el nombre propuesto está vacío.
    /// </summary>
    NoProposalAvailable,

    /// <summary>
    /// Ya existe un archivo diferente en disco con el nombre de destino propuesto.
    /// </summary>
    DestinationCollisionDisk,

    /// <summary>
    /// Dos o más archivos en la misma biblioteca/lote intentan renombrarse al mismo destino.
    /// </summary>
    DestinationCollisionBatch,

    /// <summary>
    /// El nombre contiene caracteres inválidos o viola restricciones del sistema de archivos.
    /// </summary>
    InvalidSyntaxOrCharacters,

    /// <summary>
    /// La ruta completa resultante superaría la longitud máxima permitida en el sistema.
    /// </summary>
    PathTooLong,

    /// <summary>
    /// El archivo de origen se encuentra bloqueado por otro proceso.
    /// </summary>
    SourceFileLocked,

    /// <summary>
    /// El archivo de origen no existe en la ruta esperada.
    /// </summary>
    SourceFileNotFound
}
