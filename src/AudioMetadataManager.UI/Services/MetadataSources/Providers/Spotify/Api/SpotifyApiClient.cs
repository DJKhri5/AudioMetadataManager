using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Api;

/// <summary>
/// Ejecuta solicitudes HTTP contra la API de búsqueda de
/// Spotify.
///
/// Obtiene el token de acceso mediante SpotifyAuthClient antes
/// de cada búsqueda, reutilizándolo mientras siga vigente.
/// </summary>
public sealed class SpotifyApiClient : IDisposable
{
    private readonly SpotifyApiRequestBuilder
        _requestBuilder;

    private readonly SpotifyAuthClient
        _authClient;

    private readonly HttpClient
        _httpClient;

    private readonly bool
        _ownsHttpClient;

    private readonly bool
        _ownsAuthClient;

    private bool
        _disposed;

    /// <summary>
    /// Crea un cliente con la infraestructura HTTP y de
    /// autenticación predeterminadas.
    /// </summary>
    public SpotifyApiClient(
        SpotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _requestBuilder =
            new SpotifyApiRequestBuilder(
                options);

        _authClient =
            new SpotifyAuthClient(
                options);

        _httpClient =
            new HttpClient
            {
                Timeout =
                    options.RequestTimeout
            };

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        _ownsHttpClient =
            true;

        _ownsAuthClient =
            true;
    }

    /// <summary>
    /// Crea un cliente usando componentes externos.
    ///
    /// Este constructor permite pruebas e inyección
    /// de dependencias.
    /// </summary>
    public SpotifyApiClient(
        SpotifyOptions options,
        HttpClient httpClient,
        SpotifyAuthClient authClient)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _requestBuilder =
            new SpotifyApiRequestBuilder(
                options);

        _httpClient =
            httpClient ??
            throw new ArgumentNullException(
                nameof(httpClient));

        _authClient =
            authClient ??
            throw new ArgumentNullException(
                nameof(authClient));

        _ownsHttpClient =
            false;

        _ownsAuthClient =
            false;
    }

    /// <summary>
    /// Ejecuta una búsqueda de pistas en Spotify.
    /// </summary>
    public async Task<SpotifyApiResponse> SearchTracksAsync(
        SpotifySearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        SpotifyAuthResult authResult =
            await _authClient.GetAccessTokenAsync(
                cancellationToken);

        if (!authResult.IsSuccess)
        {
            return new SpotifyApiResponse
            {
                StatusCode =
                    authResult.Status ==
                        Models.SpotifyProviderStatus
                            .AuthenticationFailed
                        ? HttpStatusCode.Unauthorized
                        : HttpStatusCode.ServiceUnavailable,

                Message =
                    authResult.Message
            };
        }

        using HttpRequestMessage httpRequest =
            _requestBuilder.BuildSearchRequest(
                request,
                authResult.Token!.Value);

        return await SendAsync(
            httpRequest,
            cancellationToken);
    }

    private async Task<SpotifyApiResponse>
        SendAsync(
            HttpRequestMessage httpRequest,
            CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response =
                await _httpClient.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

            string content =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            return new SpotifyApiResponse
            {
                StatusCode =
                    response.StatusCode,

                Content =
                    content,

                RequestUri =
                    httpRequest.RequestUri,

                Message =
                    BuildResponseMessage(
                        response.StatusCode)
            };
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return new SpotifyApiResponse
            {
                StatusCode =
                    HttpStatusCode.RequestTimeout,

                RequestUri =
                    httpRequest.RequestUri,

                Message =
                    "La solicitud a Spotify superó el tiempo " +
                    "máximo permitido."
            };
        }
        catch (HttpRequestException exception)
        {
            return new SpotifyApiResponse
            {
                StatusCode =
                    HttpStatusCode.ServiceUnavailable,

                RequestUri =
                    httpRequest.RequestUri,

                Message =
                    "No fue posible conectar con Spotify: " +
                    exception.Message
            };
        }
    }

    private static string BuildResponseMessage(
        HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.OK =>
                "La solicitud a Spotify terminó correctamente.",

            HttpStatusCode.Unauthorized =>
                "Spotify rechazó el token de acceso.",

            HttpStatusCode.Forbidden =>
                "Spotify no autorizó el acceso solicitado.",

            HttpStatusCode.TooManyRequests =>
                "Spotify limitó temporalmente las solicitudes.",

            HttpStatusCode.BadRequest =>
                "Spotify rechazó los parámetros de la solicitud.",

            _ when (int)statusCode >= 500 =>
                "Spotify informó un error interno del servicio.",

            _ =>
                "Spotify respondió con el estado HTTP " +
                $"{(int)statusCode}."
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        if (_ownsAuthClient)
        {
            _authClient.Dispose();
        }

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}
