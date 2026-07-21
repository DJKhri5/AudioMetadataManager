using System.Net;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Api;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Dtos;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Mapping;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Execution;

/// <summary>
/// Ejecuta el flujo completo de búsqueda en Discogs.
///
/// Coordina el cliente HTTP, la deserialización de la respuesta
/// y la conversión de los DTO en candidatos normalizados.
/// </summary>
public sealed class DiscogsSearchExecutor : IDisposable
{
    private readonly DiscogsApiClient
        _apiClient;

    private readonly DiscogsSearchResponseParser
        _responseParser;

    private readonly DiscogsSearchCandidateMapper
        _candidateMapper;

    private readonly bool
        _ownsApiClient;

    private bool
        _disposed;

    /// <summary>
    /// Crea un ejecutor con la infraestructura predeterminada.
    /// </summary>
    public DiscogsSearchExecutor(
        DiscogsOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _apiClient =
            new DiscogsApiClient(
                options);

        _responseParser =
            new DiscogsSearchResponseParser();

        _candidateMapper =
            new DiscogsSearchCandidateMapper();

        _ownsApiClient =
            true;
    }

    /// <summary>
    /// Crea un ejecutor con componentes personalizados.
    ///
    /// Este constructor será útil para pruebas y futura
    /// inyección de dependencias.
    /// </summary>
    public DiscogsSearchExecutor(
        DiscogsApiClient apiClient,
        DiscogsSearchResponseParser responseParser,
        DiscogsSearchCandidateMapper candidateMapper)
    {
        _apiClient =
            apiClient ??
            throw new ArgumentNullException(
                nameof(apiClient));

        _responseParser =
            responseParser ??
            throw new ArgumentNullException(
                nameof(responseParser));

        _candidateMapper =
            candidateMapper ??
            throw new ArgumentNullException(
                nameof(candidateMapper));

        _ownsApiClient =
            false;
    }

    /// <summary>
    /// Ejecuta una búsqueda y devuelve un resultado normalizado.
    /// </summary>
    public async Task<DiscogsSearchResult> ExecuteAsync(
        DiscogsSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        DiscogsApiResponse apiResponse =
            await _apiClient.SearchDatabaseAsync(
                request,
                cancellationToken);

        if (!apiResponse.IsSuccessStatusCode)
        {
            return BuildFailureResult(
                request,
                apiResponse);
        }

        if (!_responseParser.TryParse(
                apiResponse.Content,
                out DiscogsSearchResponseDto? responseDto,
                out string parseError))
        {
            return new DiscogsSearchResult
            {
                Status =
                    DiscogsProviderStatus.InvalidResponse,

                Request =
                    request,

                Message =
                    parseError,

                HttpStatusCode =
                    (int)apiResponse.StatusCode,

                RemainingRequests =
                    apiResponse.RateLimit.Remaining
            };
        }

        IReadOnlyList<DiscogsSearchCandidate> candidates =
            _candidateMapper.Map(
                responseDto?.Results);

        DiscogsPaginationDto? pagination =
            responseDto?.Pagination;

        DiscogsProviderStatus status =
            candidates.Count > 0
                ? DiscogsProviderStatus.Success
                : DiscogsProviderStatus.NoResults;

        string message =
            candidates.Count > 0
                ? $"Discogs devolvió " +
                  $"{candidates.Count} candidatos utilizables."
                : "Discogs no encontró candidatos utilizables.";

        return new DiscogsSearchResult
        {
            Status =
                status,

            Request =
                request,

            Candidates =
                candidates,

            Page =
                pagination?.Page ??
                request.Page,

            TotalPages =
                pagination?.Pages ??
                0,

            TotalResults =
                pagination?.TotalItems ??
                candidates.Count,

            Message =
                message,

            HttpStatusCode =
                (int)apiResponse.StatusCode,

            RemainingRequests =
                apiResponse.RateLimit.Remaining,

            RetrievedAtUtc =
                DateTimeOffset.UtcNow
        };
    }

    private static DiscogsSearchResult BuildFailureResult(
        DiscogsSearchRequest request,
        DiscogsApiResponse apiResponse)
    {
        DiscogsProviderStatus status =
            MapStatus(
                apiResponse.StatusCode);

        return new DiscogsSearchResult
        {
            Status =
                status,

            Request =
                request,

            Message =
                apiResponse.Message,

            HttpStatusCode =
                (int)apiResponse.StatusCode,

            RemainingRequests =
                apiResponse.RateLimit.Remaining,

            RetrievedAtUtc =
                DateTimeOffset.UtcNow
        };
    }

    private static DiscogsProviderStatus MapStatus(
        HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest =>
                DiscogsProviderStatus.InvalidRequest,

            HttpStatusCode.Unauthorized =>
                DiscogsProviderStatus.AuthenticationFailed,

            HttpStatusCode.Forbidden =>
                DiscogsProviderStatus.AuthenticationFailed,

            HttpStatusCode.TooManyRequests =>
                DiscogsProviderStatus.RateLimited,

            HttpStatusCode.RequestTimeout =>
                DiscogsProviderStatus.NetworkError,

            HttpStatusCode.ServiceUnavailable =>
                DiscogsProviderStatus.NetworkError,

            _ when (int)statusCode >= 500 =>
                DiscogsProviderStatus.NetworkError,

            _ =>
                DiscogsProviderStatus.UnexpectedError
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsApiClient)
        {
            _apiClient.Dispose();
        }

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}