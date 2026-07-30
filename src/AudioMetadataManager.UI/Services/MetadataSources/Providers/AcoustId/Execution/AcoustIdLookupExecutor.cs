using System.Net;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Api;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Dtos;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Mapping;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Execution;

/// <summary>
/// Ejecuta el flujo completo de identificación en AcoustID.
///
/// Coordina el cliente HTTP, la deserialización de la respuesta
/// y la conversión de los DTO en grabaciones normalizadas.
/// </summary>
public sealed class AcoustIdLookupExecutor : IDisposable
{
    private static readonly HashSet<int> AuthenticationErrorCodes =
        new()
        {
            4,
            6
        };

    private readonly AcoustIdApiClient
        _apiClient;

    private readonly AcoustIdLookupResponseParser
        _responseParser;

    private readonly AcoustIdRecordingCandidateMapper
        _candidateMapper;

    private readonly bool
        _ownsApiClient;

    private bool
        _disposed;

    /// <summary>
    /// Crea un ejecutor con la infraestructura predeterminada.
    /// </summary>
    public AcoustIdLookupExecutor(
        AcoustIdOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _apiClient =
            new AcoustIdApiClient(
                options);

        _responseParser =
            new AcoustIdLookupResponseParser();

        _candidateMapper =
            new AcoustIdRecordingCandidateMapper();

        _ownsApiClient =
            true;
    }

    /// <summary>
    /// Crea un ejecutor con componentes personalizados.
    ///
    /// Este constructor será útil para pruebas y futura
    /// inyección de dependencias.
    /// </summary>
    public AcoustIdLookupExecutor(
        AcoustIdApiClient apiClient,
        AcoustIdLookupResponseParser responseParser,
        AcoustIdRecordingCandidateMapper candidateMapper)
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
    /// Ejecuta una identificación y devuelve un resultado
    /// normalizado.
    /// </summary>
    public async Task<AcoustIdLookupResult> ExecuteAsync(
        AcoustIdLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        AcoustIdApiResponse apiResponse =
            await _apiClient.LookupAsync(
                request,
                cancellationToken);

        if (!apiResponse.IsSuccessStatusCode)
        {
            return BuildTransportFailureResult(
                request,
                apiResponse);
        }

        if (!_responseParser.TryParse(
                apiResponse.Content,
                out AcoustIdLookupResponseDto? responseDto,
                out string parseError))
        {
            return new AcoustIdLookupResult
            {
                Status =
                    AcoustIdProviderStatus.InvalidResponse,

                Request =
                    request,

                Message =
                    parseError,

                HttpStatusCode =
                    (int)apiResponse.StatusCode
            };
        }

        if (!string.Equals(
                responseDto?.Status,
                "ok",
                StringComparison.OrdinalIgnoreCase))
        {
            return BuildApiErrorResult(
                request,
                responseDto,
                apiResponse);
        }

        IReadOnlyList<AcoustIdRecordingCandidate> candidates =
            _candidateMapper.Map(
                responseDto?.Results);

        AcoustIdProviderStatus status =
            candidates.Count > 0
                ? AcoustIdProviderStatus.Success
                : AcoustIdProviderStatus.NoResults;

        string message =
            candidates.Count > 0
                ? $"AcoustID devolvió " +
                  $"{candidates.Count} grabación(es) utilizables."
                : "AcoustID no encontró grabaciones asociadas a la huella.";

        return new AcoustIdLookupResult
        {
            Status =
                status,

            Request =
                request,

            Candidates =
                candidates,

            Message =
                message,

            HttpStatusCode =
                (int)apiResponse.StatusCode,

            RetrievedAtUtc =
                DateTimeOffset.UtcNow
        };
    }

    private static AcoustIdLookupResult BuildApiErrorResult(
        AcoustIdLookupRequest request,
        AcoustIdLookupResponseDto? responseDto,
        AcoustIdApiResponse apiResponse)
    {
        int errorCode =
            responseDto?.Error?.Code ??
            0;

        string message =
            string.IsNullOrWhiteSpace(
                responseDto?.Error?.Message)
                ? "AcoustID rechazó la solicitud sin detalles adicionales."
                : responseDto!.Error!.Message!;

        AcoustIdProviderStatus status =
            AuthenticationErrorCodes.Contains(
                errorCode)
                ? AcoustIdProviderStatus.AuthenticationFailed
                : AcoustIdProviderStatus.UnexpectedError;

        return new AcoustIdLookupResult
        {
            Status =
                status,

            Request =
                request,

            Message =
                message,

            HttpStatusCode =
                (int)apiResponse.StatusCode
        };
    }

    private static AcoustIdLookupResult BuildTransportFailureResult(
        AcoustIdLookupRequest request,
        AcoustIdApiResponse apiResponse)
    {
        AcoustIdProviderStatus status =
            MapStatus(
                apiResponse.StatusCode);

        return new AcoustIdLookupResult
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

    private static AcoustIdProviderStatus MapStatus(
        HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest =>
                AcoustIdProviderStatus.InvalidRequest,

            HttpStatusCode.Unauthorized =>
                AcoustIdProviderStatus.AuthenticationFailed,

            HttpStatusCode.Forbidden =>
                AcoustIdProviderStatus.AuthenticationFailed,

            HttpStatusCode.TooManyRequests =>
                AcoustIdProviderStatus.RateLimited,

            HttpStatusCode.RequestTimeout =>
                AcoustIdProviderStatus.NetworkError,

            HttpStatusCode.ServiceUnavailable =>
                AcoustIdProviderStatus.NetworkError,

            _ when (int)statusCode >= 500 =>
                AcoustIdProviderStatus.NetworkError,

            _ =>
                AcoustIdProviderStatus.UnexpectedError
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
