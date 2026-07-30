using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Api;

/// <summary>
/// Ejecuta solicitudes HTTP contra la API de AcoustID.
///
/// Su responsabilidad comprende transporte, cancelación,
/// lectura de respuestas y clasificación básica de errores.
/// </summary>
public sealed class AcoustIdApiClient : IDisposable
{
    private readonly AcoustIdApiRequestBuilder
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
    public AcoustIdApiClient(
        AcoustIdOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _requestBuilder =
            new AcoustIdApiRequestBuilder(
                options);

        _httpClient =
            new HttpClient
            {
                BaseAddress =
                    options.BaseAddress,

                Timeout =
                    options.RequestTimeout
            };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "AudioMetadataManager/0.2");

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        _ownsHttpClient =
            true;
    }

    /// <summary>
    /// Crea un cliente usando un HttpClient externo.
    ///
    /// Este constructor permite pruebas e inyección
    /// de dependencias.
    /// </summary>
    public AcoustIdApiClient(
        AcoustIdOptions options,
        HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _requestBuilder =
            new AcoustIdApiRequestBuilder(
                options);

        _httpClient =
            httpClient ??
            throw new ArgumentNullException(
                nameof(httpClient));

        _ownsHttpClient =
            false;
    }

    /// <summary>
    /// Ejecuta una consulta de identificación de huella
    /// acústica.
    /// </summary>
    public Task<AcoustIdApiResponse> LookupAsync(
        AcoustIdLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        HttpRequestMessage httpRequest =
            _requestBuilder.BuildLookupRequest(
                request);

        return SendAsync(
            httpRequest,
            cancellationToken);
    }

    private async Task<AcoustIdApiResponse>
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

                return new AcoustIdApiResponse
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
                return new AcoustIdApiResponse
                {
                    StatusCode =
                        HttpStatusCode.RequestTimeout,

                    RequestUri =
                        httpRequest.RequestUri,

                    Message =
                        "La solicitud a AcoustID superó el tiempo máximo permitido."
                };
            }
            catch (HttpRequestException exception)
            {
                return new AcoustIdApiResponse
                {
                    StatusCode =
                        HttpStatusCode.ServiceUnavailable,

                    RequestUri =
                        httpRequest.RequestUri,

                    Message =
                        "No fue posible conectar con AcoustID: " +
                        exception.Message
                };
            }
        }
    }

    private static string BuildResponseMessage(
        HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.OK =>
                "La solicitud a AcoustID terminó correctamente.",

            HttpStatusCode.Unauthorized =>
                "AcoustID rechazó la clave de cliente proporcionada.",

            HttpStatusCode.Forbidden =>
                "AcoustID no autorizó el acceso solicitado.",

            HttpStatusCode.TooManyRequests =>
                "AcoustID limitó temporalmente las solicitudes.",

            HttpStatusCode.BadRequest =>
                "AcoustID rechazó los parámetros de la solicitud.",

            _ when (int)statusCode >= 500 =>
                "AcoustID informó un error interno del servicio.",

            _ =>
                "AcoustID respondió con el estado HTTP " +
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
