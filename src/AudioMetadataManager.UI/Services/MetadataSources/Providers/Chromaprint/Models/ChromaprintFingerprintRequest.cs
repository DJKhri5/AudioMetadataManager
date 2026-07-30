namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Models;

/// <summary>
/// Solicitud para generar la huella acústica de un archivo
/// local mediante fpcalc.
/// </summary>
public sealed class ChromaprintFingerprintRequest
{
    /// <summary>
    /// Ruta absoluta del archivo de audio a analizar.
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
