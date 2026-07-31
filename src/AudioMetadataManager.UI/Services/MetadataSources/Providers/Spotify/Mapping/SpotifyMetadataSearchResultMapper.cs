using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Mapping;

/// <summary>
/// Convierte el resultado específico de Spotify al modelo
/// común utilizado por MetadataSourceManager.
///
/// Mantiene encapsulados los modelos propios de Spotify.
/// </summary>
public sealed class SpotifyMetadataSearchResultMapper
{
    /// <summary>
    /// Convierte un resultado de Spotify al contrato común.
    /// </summary>
    public MetadataSearchResult Map(
        SpotifySearchResult sourceResult,
        MetadataSearchRequest originalRequest,
        TimeSpan elapsedTime,
        bool requiresManualApproval)
    {
        ArgumentNullException.ThrowIfNull(
            sourceResult);

        ArgumentNullException.ThrowIfNull(
            originalRequest);

        List<MetadataCandidate> candidates =
            sourceResult.Candidates
                .Select(
                    (candidate, index) =>
                        MapCandidate(
                            candidate,
                            index + 1))
                .Where(candidate =>
                    candidate.HasIdentity)
                .ToList();

        MetadataSourceStatus status =
            MapStatus(
                sourceResult.Status);

        bool wasSuccessful =
            status is
                MetadataSourceStatus.Success or
                MetadataSourceStatus.NoResults;

        return new MetadataSearchResult
        {
            SourceName =
                "Spotify",

            Status =
                status,

            QueryUsed =
                sourceResult.Request?.SearchDisplay ??
                originalRequest.PrimaryQuery,

            WasSuccessful =
                wasSuccessful,

            ErrorMessage =
                wasSuccessful
                    ? string.Empty
                    : sourceResult.Message,

            ElapsedTime =
                elapsedTime,

            RequiresManualApproval =
                requiresManualApproval,

            HttpStatusCode =
                sourceResult.HttpStatusCode,

            ExternalTotalResults =
                sourceResult.TotalResults,

            RetrievedAtUtc =
                sourceResult.RetrievedAtUtc,

            Candidates =
                candidates
        };
    }

    private static MetadataCandidate MapCandidate(
        SpotifySearchCandidate candidate,
        int sourceRank)
    {
        ArgumentNullException.ThrowIfNull(
            candidate);

        return new MetadataCandidate
        {
            SourceName =
                "Spotify",

            SourceId =
                candidate.Id,

            SourceUrl =
                candidate.SpotifyUri?.ToString() ??
                string.Empty,

            Artist =
                candidate.Artist ??
                string.Empty,

            Title =
                candidate.Title ??
                string.Empty,

            ReleaseTitle =
                candidate.Album ??
                string.Empty,

            Year =
                ParseYear(
                    candidate.ReleaseDate),

            Duration =
                candidate.Duration,

            ArtworkUrl =
                candidate.ArtworkUrl ??
                string.Empty,

            SourceRank =
                sourceRank
        };
    }

    private static uint ParseYear(
        string? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(
                releaseDate))
        {
            return 0;
        }

        string yearPart =
            releaseDate.Length >= 4
                ? releaseDate[..4]
                : releaseDate;

        return uint.TryParse(
            yearPart,
            out uint year)
                ? year
                : 0;
    }

    private static MetadataSourceStatus MapStatus(
        SpotifyProviderStatus status)
    {
        return status switch
        {
            SpotifyProviderStatus.Success =>
                MetadataSourceStatus.Success,

            SpotifyProviderStatus.NoResults =>
                MetadataSourceStatus.NoResults,

            SpotifyProviderStatus.InvalidRequest =>
                MetadataSourceStatus.InvalidRequest,

            SpotifyProviderStatus.InvalidConfiguration =>
                MetadataSourceStatus.InvalidConfiguration,

            SpotifyProviderStatus.AuthenticationFailed =>
                MetadataSourceStatus.AuthenticationFailed,

            SpotifyProviderStatus.RateLimited =>
                MetadataSourceStatus.RateLimited,

            SpotifyProviderStatus.NetworkError =>
                MetadataSourceStatus.NetworkError,

            SpotifyProviderStatus.InvalidResponse =>
                MetadataSourceStatus.InvalidResponse,

            SpotifyProviderStatus.UnexpectedError =>
                MetadataSourceStatus.UnexpectedError,

            _ =>
                MetadataSourceStatus.Unknown
        };
    }
}
