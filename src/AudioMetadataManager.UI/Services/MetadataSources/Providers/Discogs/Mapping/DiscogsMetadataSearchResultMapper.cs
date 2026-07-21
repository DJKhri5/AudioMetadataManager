using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Mapping;

/// <summary>
/// Convierte el resultado específico de Discogs al modelo
/// común utilizado por MetadataSourceManager.
///
/// Mantiene encapsulados los modelos propios de Discogs.
/// </summary>
public sealed class DiscogsMetadataSearchResultMapper
{
    /// <summary>
    /// Convierte un resultado de Discogs al contrato común.
    /// </summary>
    public MetadataSearchResult Map(
        DiscogsSearchResult sourceResult,
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
                "Discogs",

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

            RemainingRequests =
                sourceResult.RemainingRequests,

            ExternalTotalResults =
                sourceResult.TotalResults,

            RetrievedAtUtc =
                sourceResult.RetrievedAtUtc,

            Candidates =
                candidates
        };
    }

    private static MetadataCandidate MapCandidate(
        DiscogsSearchCandidate candidate,
        int sourceRank)
    {
        ArgumentNullException.ThrowIfNull(
            candidate);

        return new MetadataCandidate
        {
            SourceName =
                "Discogs",

            SourceId =
                candidate.Id > 0
                    ? candidate.Id.ToString()
                    : string.Empty,

            SourceUrl =
                candidate.DiscogsUri?.ToString() ??
                string.Empty,

            Artist =
                candidate.Artist ??
                string.Empty,

            Title =
                candidate.Title ??
                string.Empty,

            Version =
                candidate.Version ??
                string.Empty,

            ReleaseTitle =
                candidate.Album ??
                string.Empty,

            Label =
                candidate.Label ??
                string.Empty,

            Genre =
                candidate.Genre ??
                candidate.Style ??
                string.Empty,

            Year =
                candidate.Year is > 0
                    ? (uint)candidate.Year.Value
                    : 0,

            Duration =
                TimeSpan.Zero,

            ArtworkUrl =
                candidate.CoverImageUri?.ToString() ??
                string.Empty,

            SourceRank =
                sourceRank
        };
    }

    private static MetadataSourceStatus MapStatus(
        DiscogsProviderStatus status)
    {
        return status switch
        {
            DiscogsProviderStatus.Success =>
                MetadataSourceStatus.Success,

            DiscogsProviderStatus.NoResults =>
                MetadataSourceStatus.NoResults,

            DiscogsProviderStatus.InvalidRequest =>
                MetadataSourceStatus.InvalidRequest,

            DiscogsProviderStatus.InvalidConfiguration =>
                MetadataSourceStatus.InvalidConfiguration,

            DiscogsProviderStatus.AuthenticationRequired =>
                MetadataSourceStatus.AuthenticationRequired,

            DiscogsProviderStatus.AuthenticationFailed =>
                MetadataSourceStatus.AuthenticationFailed,

            DiscogsProviderStatus.RateLimited =>
                MetadataSourceStatus.RateLimited,

            DiscogsProviderStatus.NetworkError =>
                MetadataSourceStatus.NetworkError,

            DiscogsProviderStatus.InvalidResponse =>
                MetadataSourceStatus.InvalidResponse,

            DiscogsProviderStatus.UnexpectedError =>
                MetadataSourceStatus.UnexpectedError,

            _ =>
                MetadataSourceStatus.Unknown
        };
    }
}