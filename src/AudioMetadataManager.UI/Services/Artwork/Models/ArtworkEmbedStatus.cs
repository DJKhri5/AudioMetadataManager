namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Describe el resultado general de incrustar una imagen de
/// carátula en un archivo de audio.
/// </summary>
public enum ArtworkEmbedStatus
{
    /// <summary>
    /// La operación todavía no fue ejecutada.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// La carátula se incrustó y guardó correctamente.
    /// </summary>
    Success = 1,

    /// <summary>
    /// La solicitud no contiene todos los datos obligatorios.
    /// </summary>
    InvalidRequest = 2,

    /// <summary>
    /// El archivo de audio indicado no existe.
    /// </summary>
    FileNotFound = 3,

    /// <summary>
    /// No existe un respaldo verificable del archivo original.
    /// La escritura se rechaza por seguridad.
    /// </summary>
    MissingBackup = 4,

    /// <summary>
    /// TagLibSharp no reconoce el archivo como un formato
    /// compatible.
    /// </summary>
    UnsupportedFormat = 5,

    /// <summary>
    /// El archivo o sus etiquetas existentes parecen estar
    /// dañados.
    /// </summary>
    CorruptFile = 6,

    /// <summary>
    /// No fue posible guardar el archivo.
    /// </summary>
    SaveFailed = 7,

    /// <summary>
    /// La operación fue cancelada por el usuario.
    /// </summary>
    Cancelled = 8,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 9
}
