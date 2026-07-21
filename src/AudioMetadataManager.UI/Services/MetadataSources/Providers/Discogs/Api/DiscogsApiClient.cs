using System.Net;
using System.Net.Http;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Api;

/// <summary>
/// Ejecuta solicitudes HTTP contra la API de Discogs.
///
/// Su responsabilidad comprende transporte, cancelación,
/// lectura de respuestas, límites de solicitudes y
/// clasificación básica de errores.
/// </summary>
public sealed class DiscogsApiClient : IDisposable
{
    private readonly DiscogsOptions
        _options;

    private readonly DiscogsApiRequestBuilder
        _requestBuilder;

    private readonly HttpClient
        _httpClient;

    private readonly bool
        _ownsHttpClient;

    private bool
        _disposed;

    /// <summary>
    /// Crea un cliente con la infraestructura HTTP
    /// predeterminada.
    /// </summary>
    public DiscogsApiClient(
        DiscogsOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _requestBuilder =
            new DiscogsApiRequestBuilder(
                _options);

        DiscogsAuthenticationHandler handler =
            new(
                _options,
                new HttpClientHandler());

        _httpClient =
            new HttpClient(
                handler)
            {
                BaseAddress =
                    _options.BaseAddress,

                Timeout =
                    _options.RequestTimeout
            };

        _ownsHttpClient =
            true;
    }

    /// <summary>
    /// Crea un cliente usando un HttpClient externo.
    ///
    /// Este constructor permite pruebas e inyección
    /// de dependencias.
    /// </summary>
    public DiscogsApiClient(
        DiscogsOptions options,
        HttpClient httpClient)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _httpClient =
            httpClient ??
            throw new ArgumentNullException(
                nameof(httpClient));

        _requestBuilder =
            new DiscogsApiRequestBuilder(
                _options);

        _ownsHttpClient =
            false;
    }

    /// <summary>
    /// Ejecuta una búsqueda en la base de datos de Discogs.
    /// </summary>
    public Task<DiscogsApiResponse>
        SearchDatabaseAsync(
            DiscogsSearchRequest request,
            CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        HttpRequestMessage httpRequest =
            _requestBuilder.BuildDatabaseSearchRequest(
                request);

        return SendAsync(
            httpRequest,
            cancellationToken);
    }

    /// <summary>
    /// Comprueba la identidad asociada al token configurado.
    ///
    /// Una respuesta satisfactoria confirma que Discogs
    /// reconoce las credenciales enviadas.
    /// </summary>
    public Task<DiscogsApiResponse>
        GetIdentityAsync(
            CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        HttpRequestMessage httpRequest =
            _requestBuilder.BuildIdentityRequest();

        return SendAsync(
            httpRequest,
            cancellationToken);
    }

    /// <summary>
    /// Ejecuta una solicitud HTTP y normaliza la respuesta.
    /// </summary>
    private async Task<DiscogsApiResponse>
        SendAsync(
            HttpRequestMessage httpRequest,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            httpRequest);

        using (httpRequest)
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

                DiscogsRateLimitInfo rateLimit =
                    ReadRateLimit(
                        response);

                return new DiscogsApiResponse
                {
                    StatusCode =
                        response.StatusCode,

                    Content =
                        content,

                    RateLimit =
                        rateLimit,

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
                return new DiscogsApiResponse
                {
                    StatusCode =
                        HttpStatusCode.RequestTimeout,

                    RequestUri =
                        httpRequest.RequestUri,

                    Message =
                        "La solicitud a Discogs superó el tiempo máximo permitido."
                };
            }
            catch (HttpRequestException exception)
            {
                return new DiscogsApiResponse
                {
                    StatusCode =
                        HttpStatusCode.ServiceUnavailable,

                    RequestUri =
                        httpRequest.RequestUri,

                    Message =
                        "No fue posible conectar con Discogs: " +
                        exception.Message
                };
            }
        }
    }

    private static DiscogsRateLimitInfo ReadRateLimit(
        HttpResponseMessage response)
    {
        return new DiscogsRateLimitInfo
        {
            Limit =
                ReadIntegerHeader(
                    response,
                    "X-Discogs-Ratelimit"),

            Used =
                ReadIntegerHeader(
                    response,
                    "X-Discogs-Ratelimit-Used"),

            Remaining =
                ReadIntegerHeader(
                    response,
                    "X-Discogs-Ratelimit-Remaining")
        };
    }

    private static int? ReadIntegerHeader(
        HttpResponseMessage response,
        string headerName)
    {
        if (!response.Headers.TryGetValues(
                headerName,
                out IEnumerable<string>? values))
        {
            return null;
        }

        string? firstValue =
            values.FirstOrDefault();

        return int.TryParse(
            firstValue,
            out int parsedValue)
                ? parsedValue
                : null;
    }

    private static string BuildResponseMessage(
        HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.OK =>
                "La solicitud a Discogs terminó correctamente.",

            HttpStatusCode.Unauthorized =>
                "Discogs rechazó las credenciales proporcionadas.",

            HttpStatusCode.Forbidden =>
                "Discogs no autorizó el acceso solicitado.",

            HttpStatusCode.NotFound =>
                "El recurso solicitado no fue encontrado.",

            HttpStatusCode.TooManyRequests =>
                "Discogs limitó temporalmente las solicitudes.",

            HttpStatusCode.BadRequest =>
                "Discogs rechazó los parámetros de la solicitud.",

            _ when (int)statusCode >= 500 =>
                "Discogs informó un error interno del servicio.",

            _ =>
                "Discogs respondió con el estado HTTP " +
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

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}