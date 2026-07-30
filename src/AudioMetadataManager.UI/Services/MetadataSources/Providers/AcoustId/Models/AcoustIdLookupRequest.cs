namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

/// <summary>
/// Representa una consulta de identificación que será enviada
/// al proveedor AcoustID a partir de una huella Chromaprint.
/// </summary>
public sealed class AcoustIdLookupRequest
{
    /// <summary>
    /// Huella acústica generada previamente por fpcalc.
    /// </summary>
    public string Fingerprint { get; init; } =
        string.Empty;

    /// <summary>
    /// Duración del audio, en segundos, informada junto
    /// con la huella.
    /// </summary>
    public int DurationSeconds { get; init; }

    /// <summary>
    /// Indica si la solicitud contiene los datos mínimos
    /// necesarios para consultar AcoustID.
    /// </summary>
    public bool HasMinimumData =>
        !string.IsNullOrWhiteSpace(Fingerprint) &&
        DurationSeconds > 0;

    /// <summary>
    /// Texto resumido para diagnósticos.
    /// </summary>
    public string SearchDisplay =>
        HasMinimumData
            ? $"Huella de {DurationSeconds}s " +
              $"({Fingerprint.Length} caracteres)"
            : "(consulta vacía)";
}
