using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Identification.Models;

/// <summary>
/// Contiene el resultado completo de un intento de identificación
/// automática de una pista, combinando la huella acústica local y
/// la consulta a AcoustID.
/// </summary>
public sealed class AudioIdentificationResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public AudioIdentificationStatus Status { get; init; } =
        AudioIdentificationStatus.Unknown;

    /// <summary>
    /// Ruta del archivo identificado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Resultado de la generación local de la huella acústica.
    /// Queda nulo cuando la solicitud fue inválida antes de
    /// invocar a Chromaprint.
    /// </summary>
    public ChromaprintFingerprintResult? FingerprintResult { get; init; }

    /// <summary>
    /// Resultado de la consulta a AcoustID.
    /// Queda nulo cuando la generación de la huella falló y la
    /// consulta nunca llegó a ejecutarse.
    /// </summary>
    public AcoustIdLookupResult? LookupResult { get; init; }

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
    /// Indica si la identificación terminó con al menos una
    /// grabación utilizable.
    /// </summary>
    public bool IsSuccess =>
        Status == AudioIdentificationStatus.Success;

    /// <summary>
    /// Mejor grabación encontrada, cuando la consulta a AcoustID
    /// llegó a ejecutarse.
    /// </summary>
    public AcoustIdRecordingCandidate? BestCandidate =>
        LookupResult?.BestCandidate;

    /// <summary>
    /// Construye un resultado para una solicitud inválida.
    /// </summary>
    public static AudioIdentificationResult InvalidRequest(
        string filePath,
        string message)
    {
        return new AudioIdentificationResult
        {
            Status =
                AudioIdentificationStatus.InvalidRequest,

            FilePath =
                filePath,

            Message =
                message
        };
    }
}
