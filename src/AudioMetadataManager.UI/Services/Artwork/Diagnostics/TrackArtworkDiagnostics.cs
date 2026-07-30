using System.Text;
using AudioMetadataManager.UI.Services.Artwork.Models;

namespace AudioMetadataManager.UI.Services.Artwork.Diagnostics;

/// <summary>
/// Ejecuta una adquisición controlada de carátula contra un
/// archivo real y produce un informe legible.
///
/// A diferencia de los diagnósticos de Chromaprint y AcoustID,
/// esta operación escribe en el archivo indicado. El llamador es
/// responsable de proveer una ruta de respaldo ya verificada,
/// idealmente apuntando a una copia aislada y no al archivo
/// original de la biblioteca del usuario.
/// </summary>
public sealed class TrackArtworkDiagnostics
{
    private readonly TrackArtworkService
        _service;

    public TrackArtworkDiagnostics()
        : this(
            new TrackArtworkService())
    {
    }

    public TrackArtworkDiagnostics(
        TrackArtworkService service)
    {
        _service =
            service ??
            throw new ArgumentNullException(
                nameof(service));
    }

    /// <summary>
    /// Descarga e incrusta la carátula indicada, y devuelve un
    /// informe con el resultado de cada etapa.
    /// </summary>
    public async Task<string> RunAsync(
        string filePath,
        string verifiedBackupPath,
        string artworkUrl,
        CancellationToken cancellationToken = default)
    {
        TrackArtworkResult result =
            await _service.AcquireAsync(
                new TrackArtworkRequest
                {
                    FilePath =
                        filePath,

                    VerifiedBackupPath =
                        verifiedBackupPath,

                    ArtworkUrl =
                        artworkUrl
                },
                cancellationToken);

        return BuildReport(
            result);
    }

    private static string BuildReport(
        TrackArtworkResult result)
    {
        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de TrackArtworkService ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Archivo: " +
            $"{result.FilePath}");

        builder.AppendLine(
            $"Estado general: " +
            $"{result.Status}");

        builder.AppendLine(
            $"Mensaje: " +
            $"{result.Message}");

        builder.AppendLine();

        if (result.DownloadResult is not null)
        {
            builder.AppendLine(
                "--- Descarga ---");

            builder.AppendLine(
                $"Estado: " +
                $"{result.DownloadResult.Status}");

            builder.AppendLine(
                $"Tipo de contenido: " +
                $"{result.DownloadResult.MimeType}");

            builder.AppendLine(
                $"Bytes descargados: " +
                $"{result.DownloadResult.ImageBytes.Length}");

            builder.AppendLine();
        }

        if (result.EmbedResult is not null)
        {
            builder.AppendLine(
                "--- Incrustación ---");

            builder.AppendLine(
                $"Estado: " +
                $"{result.EmbedResult.Status}");

            builder.AppendLine(
                $"Imágenes antes: " +
                $"{result.EmbedResult.PictureCountBefore}");

            builder.AppendLine(
                $"Imágenes después: " +
                $"{result.EmbedResult.PictureCountAfter}");

            builder.AppendLine();
        }

        builder.AppendLine(
            "=== Fin del diagnóstico ===");

        return builder.ToString();
    }
}
