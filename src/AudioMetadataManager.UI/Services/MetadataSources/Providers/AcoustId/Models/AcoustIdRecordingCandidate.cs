namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

/// <summary>
/// Representa una grabación de MusicBrainz asociada a una
/// huella acústica, según lo informado por AcoustID.
///
/// Este modelo evita que el resto de la aplicación dependa
/// directamente de la estructura JSON de la API.
/// </summary>
public sealed class AcoustIdRecordingCandidate
{
    /// <summary>
    /// Identificador AcoustID de la coincidencia de huella.
    /// </summary>
    public string AcoustId { get; init; } =
        string.Empty;

    /// <summary>
    /// Confianza de la coincidencia informada por AcoustID,
    /// entre 0 y 1.
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Identificador MBID de la grabación en MusicBrainz.
    /// </summary>
    public string RecordingId { get; init; } =
        string.Empty;

    /// <summary>
    /// Título de la grabación, cuando MusicBrainz lo informa.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Nombre combinado de los artistas acreditados.
    /// </summary>
    public string? ArtistName { get; init; }

    /// <summary>
    /// Duración de la grabación, cuando está disponible.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Indica si el candidato contiene datos mínimos utilizables.
    /// </summary>
    public bool HasUsableMetadata =>
        !string.IsNullOrWhiteSpace(RecordingId);

    /// <summary>
    /// Nombre resumido del candidato.
    /// </summary>
    public string DisplayName
    {
        get
        {
            string artist =
                string.IsNullOrWhiteSpace(ArtistName)
                    ? "(artista desconocido)"
                    : ArtistName.Trim();

            string title =
                string.IsNullOrWhiteSpace(Title)
                    ? "(título desconocido)"
                    : Title.Trim();

            return $"{artist} - {title}";
        }
    }
}
