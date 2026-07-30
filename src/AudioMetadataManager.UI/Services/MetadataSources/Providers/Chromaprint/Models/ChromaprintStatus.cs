namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Models;

/// <summary>
/// Describe el resultado general de una operación de
/// generación de huella acústica mediante fpcalc.
/// </summary>
public enum ChromaprintStatus
{
    /// <summary>
    /// La operación todavía no fue ejecutada.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// La huella se generó correctamente.
    /// </summary>
    Success = 1,

    /// <summary>
    /// La solicitud no contiene información suficiente,
    /// o el archivo indicado no existe.
    /// </summary>
    InvalidRequest = 2,

    /// <summary>
    /// La configuración de Chromaprint no es válida.
    /// </summary>
    InvalidConfiguration = 3,

    /// <summary>
    /// No fue posible encontrar o iniciar el ejecutable fpcalc.
    /// </summary>
    ExecutableNotFound = 4,

    /// <summary>
    /// fpcalc terminó con un código de salida distinto de cero.
    /// </summary>
    ProcessError = 5,

    /// <summary>
    /// fpcalc no terminó dentro del tiempo máximo configurado.
    /// </summary>
    Timeout = 6,

    /// <summary>
    /// La operación fue cancelada por el usuario.
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// La salida de fpcalc no pudo interpretarse.
    /// </summary>
    InvalidOutput = 8,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 9
}
