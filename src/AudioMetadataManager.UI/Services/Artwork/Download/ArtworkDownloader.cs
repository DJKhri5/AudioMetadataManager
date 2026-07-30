using System.IO;
using System.Net.Http;
using AudioMetadataManager.UI.Services.Artwork.Configuration;
using AudioMetadataManager.UI.Services.Artwork.Models;

namespace AudioMetadataManager.UI.Services.Artwork.Download;

/// <summary>
/// Descarga imágenes de carátula propuestas por fuentes externas
/// de metadatos.
///
/// Aplica límites de tamaño y de tipo de contenido antes de
/// aceptar una imagen, incluso cuando el servidor no informa
/// Content-Length con anticipación.
/// </summary>
public sealed class ArtworkDownloader : IDisposable
{
    private readonly ArtworkDownloadOptions
        _options;

    private readonly HttpClient
        _httpClient;

    private readonly bool
        _ownsHttpClient;

    private bool
        _disposed;

    /// <summary>
    /// Crea un descargador con la infraestructura HTTP
    /// predeterminada.
    /// </summary>
    public ArtworkDownloader(
        ArtworkDownloadOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _httpClient =
            new HttpClient
            {
                Timeout =
                    options.RequestTimeout
            };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "AudioMetadataManager/0.2");

        _ownsHttpClient =
            true;
    }

    /// <summary>
    /// Crea un descargador usando un HttpClient externo.
    ///
    /// Este constructor permite pruebas e inyección
    /// de dependencias.
    /// </summary>
    public ArtworkDownloader(
        ArtworkDownloadOptions options,
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

        _ownsHttpClient =
            false;
    }

    /// <summary>
    /// Descarga la imagen indicada.
    /// </summary>
    public async Task<ArtworkDownloadResult> DownloadAsync(
        ArtworkDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        if (!request.HasArtworkUrl ||
            !Uri.TryCreate(
                request.ArtworkUrl,
                UriKind.Absolute,
                out Uri? uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            return ArtworkDownloadResult.InvalidRequest(
                request.ArtworkUrl,
                "La solicitud no contiene una dirección HTTP o " +
                "HTTPS válida.");
        }

        try
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    uri,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ArtworkDownloadResult
                {
                    Status =
                        ArtworkDownloadStatus.NetworkError,

                    SourceUrl =
                        request.ArtworkUrl,

                    Message =
                        $"El servidor respondió con el estado HTTP " +
                        $"{(int)response.StatusCode}."
                };
            }

            string? mimeType =
                response.Content.Headers.ContentType?.MediaType;

            if (string.IsNullOrWhiteSpace(mimeType) ||
                !_options.AllowedMimeTypes.Contains(mimeType))
            {
                return new ArtworkDownloadResult
                {
                    Status =
                        ArtworkDownloadStatus.UnexpectedContentType,

                    SourceUrl =
                        request.ArtworkUrl,

                    Message =
                        $"Tipo de contenido no admitido: " +
                        $"'{mimeType ?? "(desconocido)"}'."
                };
            }

            long? contentLength =
                response.Content.Headers.ContentLength;

            if (contentLength.HasValue &&
                contentLength.Value > _options.MaxSizeBytes)
            {
                return new ArtworkDownloadResult
                {
                    Status =
                        ArtworkDownloadStatus.TooLarge,

                    SourceUrl =
                        request.ArtworkUrl,

                    Message =
                        $"La imagen anunciada ({contentLength.Value} " +
                        $"bytes) supera el máximo permitido " +
                        $"({_options.MaxSizeBytes} bytes)."
                };
            }

            byte[]? imageBytes =
                await ReadBoundedAsync(
                    response,
                    cancellationToken);

            if (imageBytes is null)
            {
                return new ArtworkDownloadResult
                {
                    Status =
                        ArtworkDownloadStatus.TooLarge,

                    SourceUrl =
                        request.ArtworkUrl,

                    Message =
                        $"La imagen superó el máximo permitido " +
                        $"({_options.MaxSizeBytes} bytes) durante " +
                        $"la descarga."
                };
            }

            if (imageBytes.Length == 0)
            {
                return new ArtworkDownloadResult
                {
                    Status =
                        ArtworkDownloadStatus.EmptyContent,

                    SourceUrl =
                        request.ArtworkUrl,

                    Message =
                        "La respuesta no contiene datos de imagen."
                };
            }

            return new ArtworkDownloadResult
            {
                Status =
                    ArtworkDownloadStatus.Success,

                SourceUrl =
                    request.ArtworkUrl,

                ImageBytes =
                    imageBytes,

                MimeType =
                    mimeType,

                Message =
                    $"Se descargaron {imageBytes.Length} bytes de " +
                    $"tipo '{mimeType}'."
            };
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return new ArtworkDownloadResult
            {
                Status =
                    ArtworkDownloadStatus.Cancelled,

                SourceUrl =
                    request.ArtworkUrl,

                Message =
                    "La descarga de la carátula fue cancelada."
            };
        }
        catch (OperationCanceledException)
        {
            return new ArtworkDownloadResult
            {
                Status =
                    ArtworkDownloadStatus.NetworkError,

                SourceUrl =
                    request.ArtworkUrl,

                Message =
                    "La descarga superó el tiempo máximo permitido."
            };
        }
        catch (HttpRequestException exception)
        {
            return new ArtworkDownloadResult
            {
                Status =
                    ArtworkDownloadStatus.NetworkError,

                SourceUrl =
                    request.ArtworkUrl,

                Message =
                    "No fue posible descargar la carátula: " +
                    exception.Message
            };
        }
    }

    /// <summary>
    /// Lee la respuesta en bloques, abortando en cuanto se supera
    /// el tamaño máximo configurado, incluso si el servidor nunca
    /// informó Content-Length por adelantado.
    /// </summary>
    private async Task<byte[]?> ReadBoundedAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await using Stream stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        using MemoryStream buffer =
            new();

        byte[] chunk =
            new byte[81920];

        long totalRead =
            0;

        int bytesRead;

        while ((bytesRead =
                    await stream.ReadAsync(
                        chunk,
                        cancellationToken)) > 0)
        {
            totalRead +=
                bytesRead;

            if (totalRead > _options.MaxSizeBytes)
            {
                return null;
            }

            buffer.Write(
                chunk,
                0,
                bytesRead);
        }

        return buffer.ToArray();
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
