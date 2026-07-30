using AudioMetadataManager.UI.Services.Artwork.Configuration;
using AudioMetadataManager.UI.Services.Artwork.Download;
using AudioMetadataManager.UI.Services.Artwork.Embedding;
using AudioMetadataManager.UI.Services.Artwork.Models;

namespace AudioMetadataManager.UI.Services.Artwork;

/// <summary>
/// Obtiene e incrusta la carátula de una pista, combinando la
/// descarga de la imagen con su escritura mediante TagLibSharp.
///
/// Nunca escribe sin un respaldo verificado del archivo original.
/// </summary>
public sealed class TrackArtworkService : IDisposable
{
    private readonly ArtworkDownloader
        _downloader;

    private readonly TagLibArtworkEmbedder
        _embedder;

    private readonly bool
        _ownsDownloader;

    private bool
        _disposed;

    /// <summary>
    /// Crea el servicio con la infraestructura predeterminada.
    /// </summary>
    public TrackArtworkService()
    {
        _downloader =
            new ArtworkDownloader(
                new ArtworkDownloadOptions());

        _embedder =
            new TagLibArtworkEmbedder();

        _ownsDownloader =
            true;
    }

    /// <summary>
    /// Crea el servicio con componentes personalizados.
    ///
    /// Este constructor será útil para pruebas y futura
    /// inyección de dependencias.
    /// </summary>
    public TrackArtworkService(
        ArtworkDownloader downloader,
        TagLibArtworkEmbedder embedder)
    {
        _downloader =
            downloader ??
            throw new ArgumentNullException(
                nameof(downloader));

        _embedder =
            embedder ??
            throw new ArgumentNullException(
                nameof(embedder));

        _ownsDownloader =
            false;
    }

    /// <summary>
    /// Descarga la carátula indicada y la incrusta en el archivo
    /// solicitado.
    /// </summary>
    public async Task<TrackArtworkResult> AcquireAsync(
        TrackArtworkRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        cancellationToken.ThrowIfCancellationRequested();

        if (!request.IsStructurallyValid)
        {
            return TrackArtworkResult.InvalidRequest(
                request.FilePath,
                "La solicitud no contiene una ruta de archivo, " +
                "un respaldo verificado y una dirección de " +
                "carátula.");
        }

        ArtworkDownloadResult downloadResult =
            await _downloader.DownloadAsync(
                new ArtworkDownloadRequest
                {
                    ArtworkUrl =
                        request.ArtworkUrl
                },
                cancellationToken);

        if (!downloadResult.IsSuccess)
        {
            return new TrackArtworkResult
            {
                Status =
                    downloadResult.Status ==
                        ArtworkDownloadStatus.Cancelled
                        ? TrackArtworkStatus.Cancelled
                        : TrackArtworkStatus.DownloadFailed,

                FilePath =
                    request.FilePath,

                DownloadResult =
                    downloadResult,

                Message =
                    downloadResult.Message
            };
        }

        ArtworkEmbedResult embedResult =
            await _embedder.EmbedAsync(
                new ArtworkEmbedRequest
                {
                    FilePath =
                        request.FilePath,

                    VerifiedBackupPath =
                        request.VerifiedBackupPath,

                    ImageBytes =
                        downloadResult.ImageBytes,

                    MimeType =
                        downloadResult.MimeType,

                    ReplaceExisting =
                        request.ReplaceExisting
                },
                cancellationToken);

        TrackArtworkStatus status =
            embedResult.IsSuccess
                ? TrackArtworkStatus.Success
                : embedResult.Status ==
                    ArtworkEmbedStatus.Cancelled
                    ? TrackArtworkStatus.Cancelled
                    : TrackArtworkStatus.EmbedFailed;

        return new TrackArtworkResult
        {
            Status =
                status,

            FilePath =
                request.FilePath,

            DownloadResult =
                downloadResult,

            EmbedResult =
                embedResult,

            Message =
                embedResult.Message
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsDownloader)
        {
            _downloader.Dispose();
        }

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}
