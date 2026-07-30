using System.IO;
using AudioMetadataManager.UI.Services.Artwork.Models;

namespace AudioMetadataManager.UI.Services.Artwork.Embedding;

/// <summary>
/// Incrusta una imagen de carátula en un archivo de audio local
/// mediante TagLibSharp.
///
/// Nunca escribe sin un respaldo verificado del archivo original,
/// siguiendo el mismo principio de seguridad que
/// TagLibMetadataWriterBase.
/// </summary>
public sealed class TagLibArtworkEmbedder
{
    /// <summary>
    /// Incrusta la imagen indicada en el archivo solicitado.
    /// </summary>
    public Task<ArtworkEmbedResult> EmbedAsync(
        ArtworkEmbedRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        return Task.Run(
            () =>
                EmbedCore(
                    request,
                    cancellationToken),
            cancellationToken);
    }

    private static ArtworkEmbedResult EmbedCore(
        ArtworkEmbedRequest request,
        CancellationToken cancellationToken)
    {
        ArtworkEmbedResult? validationFailure =
            ValidateRequest(
                request);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        int pictureCountBefore =
            0;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            using TagLib.File tagFile =
                TagLib.File.Create(
                    request.FilePath);

            TagLib.Tag tag =
                tagFile.Tag;

            pictureCountBefore =
                tag.Pictures?.Length ??
                0;

            TagLib.Picture picture =
                new(new TagLib.ByteVector(
                    request.ImageBytes))
                {
                    Type =
                        TagLib.PictureType.FrontCover,

                    MimeType =
                        request.MimeType,

                    Description =
                        string.IsNullOrWhiteSpace(
                            request.Description)
                            ? "Cover"
                            : request.Description
                };

            tag.Pictures =
                request.ReplaceExisting
                    ? new TagLib.IPicture[] { picture }
                    : (tag.Pictures ??
                        Array.Empty<TagLib.IPicture>())
                        .Append(picture)
                        .ToArray();

            cancellationToken.ThrowIfCancellationRequested();

            tagFile.Save();

            int pictureCountAfter =
                tag.Pictures?.Length ??
                0;

            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.Success,

                FilePath =
                    request.FilePath,

                PictureCountBefore =
                    pictureCountBefore,

                PictureCountAfter =
                    pictureCountAfter,

                Message =
                    $"La carátula se incrustó correctamente " +
                    $"({pictureCountBefore} -> {pictureCountAfter} " +
                    $"imagen(es))."
            };
        }
        catch (OperationCanceledException)
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.Cancelled,

                FilePath =
                    request.FilePath,

                PictureCountBefore =
                    pictureCountBefore,

                Message =
                    "La incrustación de la carátula fue cancelada."
            };
        }
        catch (TagLib.UnsupportedFormatException exception)
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.UnsupportedFormat,

                FilePath =
                    request.FilePath,

                Message =
                    "TagLibSharp no reconoce el archivo como " +
                    $"compatible: {exception.Message}"
            };
        }
        catch (TagLib.CorruptFileException exception)
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.CorruptFile,

                FilePath =
                    request.FilePath,

                Message =
                    "El archivo o sus etiquetas parecen estar " +
                    $"dañados: {exception.Message}"
            };
        }
        catch (UnauthorizedAccessException exception)
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.SaveFailed,

                FilePath =
                    request.FilePath,

                PictureCountBefore =
                    pictureCountBefore,

                Message =
                    "Windows rechazó el acceso necesario para " +
                    $"guardar el archivo: {exception.Message}"
            };
        }
        catch (IOException exception)
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.SaveFailed,

                FilePath =
                    request.FilePath,

                PictureCountBefore =
                    pictureCountBefore,

                Message =
                    "Ocurrió un error de entrada o salida durante " +
                    $"el guardado: {exception.Message}"
            };
        }
        catch (Exception exception)
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.UnexpectedError,

                FilePath =
                    request.FilePath,

                PictureCountBefore =
                    pictureCountBefore,

                Message =
                    "Ocurrió un error inesperado al incrustar la " +
                    $"carátula: {exception.Message}"
            };
        }
    }

    private static ArtworkEmbedResult? ValidateRequest(
        ArtworkEmbedRequest request)
    {
        if (!request.IsStructurallyValid)
        {
            return ArtworkEmbedResult.InvalidRequest(
                request.FilePath,
                "La solicitud no contiene todos los datos " +
                "obligatorios para incrustar una carátula.");
        }

        if (!File.Exists(
                request.FilePath))
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.FileNotFound,

                FilePath =
                    request.FilePath,

                Message =
                    $"El archivo '{request.FilePath}' no existe."
            };
        }

        if (!File.Exists(
                request.VerifiedBackupPath))
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.MissingBackup,

                FilePath =
                    request.FilePath,

                Message =
                    "No existe un respaldo verificable del " +
                    "archivo original. Se rechaza la escritura " +
                    "por seguridad."
            };
        }

        string sourcePath =
            Path.GetFullPath(
                request.FilePath);

        string backupPath =
            Path.GetFullPath(
                request.VerifiedBackupPath);

        if (string.Equals(
                sourcePath,
                backupPath,
                StringComparison.OrdinalIgnoreCase))
        {
            return new ArtworkEmbedResult
            {
                Status =
                    ArtworkEmbedStatus.InvalidRequest,

                FilePath =
                    request.FilePath,

                Message =
                    "La copia de seguridad no puede ser el mismo " +
                    "archivo que el original."
            };
        }

        return null;
    }
}
