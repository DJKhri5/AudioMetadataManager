namespace AudioMetadataManager.UI.Services.MetadataSources
    .Identification.Models;

/// <summary>
/// Solicitud para identificar automáticamente un archivo de audio
/// local mediante Chromaprint y AcoustID.
/// </summary>
public sealed class AudioIdentificationRequest
{
    /// <summary>
    /// Ruta absoluta del archivo de audio a identificar.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la solicitud contiene una ruta utilizable.
    /// </summary>
    public bool HasFilePath =>
        !string.IsNullOrWhiteSpace(
            FilePath);
}
