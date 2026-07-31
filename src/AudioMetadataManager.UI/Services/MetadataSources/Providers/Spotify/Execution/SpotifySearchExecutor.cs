using System.Net;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Api;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Mapping;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Execution;

/// <summary>
/// Ejecuta el flujo completo de búsqueda en Spotify.
///
/// Coordina el cliente HTTP (incluida la autenticación
/// "Client Credentials"), la deserialización de la respuesta
/// y la conversión de los DTO en candidatos normalizados.
/// </summary>
public sealed class SpotifySearchExecutor : IDisposable
{
    private readonly SpotifyApiClient
        _apiClient;

    private readonly SpotifySearchResponseParser
        _responseParser;

    private readonly SpotifySearchCandidateMapper
        _candidateMapper;

    private readonly bool
        _ownsApiClient;

    private bool
        _disposed;

    /// <summary>
    /// Crea un ejecutor con la infraestructura predeterminada.
    /// </summary>
    public SpotifySearchExecutor(
        SpotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _apiClient =
            new SpotifyApiClient(
                options);

        _responseParser =
            new SpotifySearchResponseParser();

        _candidateMapper =
            new SpotifySearchCandidateMapper();

        _ownsApiClient =
            true;
    }

    /// <summary>
    /// Crea un ejecutor con componentes personalizados.
    ///
    /// Este constructor será útil para pruebas y futura
    /// inyección de dependencias.
    /// </summary>
    public SpotifySearchExecutor(
        SpotifyApiClient apiClient,
        SpotifySearchResponseParser responseParser,
        SpotifySearchCandidateMapper candidateMapper)
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
    public async Task<SpotifySearchResult> ExecuteAsync(
        SpotifySearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        SpotifyApiResponse apiResponse =
            await _apiClient.SearchTracksAsync(
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
                out SpotifySearchResponseDto? responseDto,
                out string parseError))
        {
            return new SpotifySearchResult
            {
                Status =
                    SpotifyProviderStatus.InvalidResponse,

                Request =
                    request,

                Message =
                    parseError,

                HttpStatusCode =
                    (int)apiResponse.StatusCode
            };
        }

        IReadOnlyList<SpotifySearchCandidate> candidates =
            _candidateMapper.Map(
                responseDto?.Tracks?.Items);

        SpotifyProviderStatus status =
            candidates.Count > 0
                ? SpotifyProviderStatus.Success
                : SpotifyProviderStatus.NoResults;

        string message =
            candidates.Count > 0
                ? $"Spotify devolvió " +
                  $"{candidates.Count} candidatos utilizables."
                : "Spotify no encontró candidatos utilizables.";

        return new SpotifySearchResult
        {
            Status =
                status,

            Request =
                request,

            Candidates =
                candidates,

            TotalResults =
                responseDto?.Tracks?.Total ??
                    candidates.Count,

            Message =
                message,

            HttpStatusCode =
                (int)apiResponse.StatusCode,

            RetrievedAtUtc =
                DateTimeOffset.UtcNow
        };
    }

    private static SpotifySearchResult BuildFailureResult(
        SpotifySearchRequest request,
        SpotifyApiResponse apiResponse)
    {
        SpotifyProviderStatus status =
            MapStatus(
                apiResponse.StatusCode);

        return new SpotifySearchResult
        {
            Status =
                status,

            Request =
                request,

            Message =
                apiResponse.Message,

            HttpStatusCode =
                (int)apiResponse.StatusCode
        };
    }

    private static SpotifyProviderStatus MapStatus(
        HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest =>
                SpotifyProviderStatus.InvalidRequest,

            HttpStatusCode.Unauthorized =>
                SpotifyProviderStatus.AuthenticationFailed,

            HttpStatusCode.Forbidden =>
                SpotifyProviderStatus.AuthenticationFailed,

            HttpStatusCode.TooManyRequests =>
                SpotifyProviderStatus.RateLimited,

            HttpStatusCode.RequestTimeout =>
                SpotifyProviderStatus.NetworkError,

            HttpStatusCode.ServiceUnavailable =>
                SpotifyProviderStatus.NetworkError,

            _ when (int)statusCode >= 500 =>
                SpotifyProviderStatus.NetworkError,

            _ =>
                SpotifyProviderStatus.UnexpectedError
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
