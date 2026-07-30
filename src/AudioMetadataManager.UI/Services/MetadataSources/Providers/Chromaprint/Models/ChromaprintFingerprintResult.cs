namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Models;

/// <summary>
/// Contiene el resultado completo de una generación de huella
/// acústica realizada mediante fpcalc.
///
/// Esta versión solo representa la huella local. No incluye
/// todavía el identificador obtenido desde AcoustID.
/// </summary>
public sealed class ChromaprintFingerprintResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public ChromaprintStatus Status { get; init; } =
        ChromaprintStatus.Unknown;

    /// <summary>
    /// Ruta del archivo analizado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Huella acústica codificada, tal como la entrega fpcalc.
    /// </summary>
    public string Fingerprint { get; init; } =
        string.Empty;

    /// <summary>
    /// Duración del audio informada por fpcalc.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Código de salida del proceso, cuando llegó a ejecutarse.
    /// </summary>
    public int? ExitCode { get; init; }

    /// <summary>
    /// Mensaje descriptivo para interfaz o diagnóstico.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Momento UTC en que se produjo el resultado.
    /// </summary>
    public DateTimeOffset RetrievedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Indica si la huella se generó correctamente.
    /// </summary>
    public bool IsSuccess =>
        Status == ChromaprintStatus.Success &&
        HasFingerprint;

    /// <summary>
    /// Indica si el resultado contiene una huella utilizable.
    /// </summary>
    public bool HasFingerprint =>
        !string.IsNullOrWhiteSpace(
            Fingerprint);

    /// <summary>
    /// Construye un resultado para una solicitud inválida.
    /// </summary>
    public static ChromaprintFingerprintResult InvalidRequest(
        string filePath,
        string message)
    {
        return new ChromaprintFingerprintResult
        {
            Status =
                ChromaprintStatus.InvalidRequest,

            FilePath =
                filePath,

            Message =
                message
        };
    }

    /// <summary>
    /// Construye un resultado para una configuración inválida.
    /// </summary>
    public static ChromaprintFingerprintResult InvalidConfiguration(
        string filePath,
        string message)
    {
        return new ChromaprintFingerprintResult
        {
            Status =
                ChromaprintStatus.InvalidConfiguration,

            FilePath =
                filePath,

            Message =
                message
        };
    }

    /// <summary>
    /// Construye un resultado cuando fpcalc no pudo iniciarse.
    /// </summary>
    public static ChromaprintFingerprintResult ExecutableNotFound(
        string filePath,
        string executablePath)
    {
        return new ChromaprintFingerprintResult
        {
            Status =
                ChromaprintStatus.ExecutableNotFound,

            FilePath =
                filePath,

            Message =
                $"No fue posible iniciar fpcalc " +
                $"('{executablePath}'). Verifique que esté " +
                $"instalado y disponible en el PATH."
        };
    }
}
