using System.IO;
using System.Security.Cryptography;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Interfaces;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Testing;

/// <summary>
/// Ejecuta una prueba real de escritura sobre una copia aislada,
/// usando cualquier escritor compatible con IMetadataFormatWriter.
///
/// El archivo original nunca se entrega al escritor.
/// </summary>
public sealed class TagLibIsolatedWriteTestRunner
{
    public async Task<TagLibIsolatedWriteTestResult>
        RunAsync(
            string? originalFilePath,
            IMetadataFormatWriter writer,
            string formatDisplayName,
            string testFolderName,
            string requestedGenre = "Electronic",
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            writer);

        List<string> messages =
            new();

        string normalizedOriginalPath =
            NormalizePath(
                originalFilePath);

        string normalizedFormatDisplayName =
            NormalizeDisplayValue(
                formatDisplayName,
                "(formato sin identificar)");

        string normalizedTestFolderName =
            NormalizeFolderName(
                testFolderName);

        string normalizedRequestedGenre =
            NormalizeValue(
                requestedGenre);

        if (string.IsNullOrWhiteSpace(
                normalizedOriginalPath))
        {
            messages.Add(
                "No se recibió una ruta original válida.");

            return BuildFailure(
                normalizedFormatDisplayName,
                normalizedOriginalPath,
                normalizedRequestedGenre,
                messages);
        }

        if (!File.Exists(
                normalizedOriginalPath))
        {
            messages.Add(
                "El archivo original indicado no existe.");

            return BuildFailure(
                normalizedFormatDisplayName,
                normalizedOriginalPath,
                normalizedRequestedGenre,
                messages);
        }

        string originalExtension =
            Path.GetExtension(
                normalizedOriginalPath);

        if (!writer.CanWrite(
                originalExtension))
        {
            messages.Add(
                $"El escritor {writer.Name} no admite la " +
                $"extensión {originalExtension}.");

            return BuildFailure(
                normalizedFormatDisplayName,
                normalizedOriginalPath,
                normalizedRequestedGenre,
                messages);
        }

        if (string.IsNullOrWhiteSpace(
                normalizedRequestedGenre))
        {
            messages.Add(
                "El género solicitado no es válido.");

            return BuildFailure(
                normalizedFormatDisplayName,
                normalizedOriginalPath,
                normalizedRequestedGenre,
                messages);
        }

        cancellationToken.ThrowIfCancellationRequested();

        string originalHashBefore =
            await ComputeSha256Async(
                normalizedOriginalPath,
                cancellationToken);

        string testDirectoryPath =
            Path.Combine(
                Path.GetTempPath(),
                "AudioMetadataManager",
                normalizedTestFolderName,
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            testDirectoryPath);

        string originalFileName =
            Path.GetFileName(
                normalizedOriginalPath);

        string workingCopyPath =
            Path.Combine(
                testDirectoryPath,
                "working_" + originalFileName);

        string workingBackupPath =
            Path.Combine(
                testDirectoryPath,
                "backup_" + originalFileName);

        try
        {
            File.Copy(
                normalizedOriginalPath,
                workingCopyPath,
                overwrite:
                    false);

            File.Copy(
                workingCopyPath,
                workingBackupPath,
                overwrite:
                    false);

            messages.Add(
                $"Se creó una copia aislada del archivo " +
                $"{normalizedFormatDisplayName}.");

            messages.Add(
                "Se creó un respaldo independiente de la " +
                "copia antes de ejecutar Save().");

            cancellationToken.ThrowIfCancellationRequested();

            string workingCopyHashBefore =
                await ComputeSha256Async(
                    workingCopyPath,
                    cancellationToken);

            string workingBackupHash =
                await ComputeSha256Async(
                    workingBackupPath,
                    cancellationToken);

            string originalGenre;
            int pictureCountBefore;

            using (TagLib.File tagFile =
                TagLib.File.Create(
                    workingCopyPath))
            {
                originalGenre =
                    JoinValues(
                        tagFile.Tag.Genres);

                pictureCountBefore =
                    tagFile.Tag.Pictures?.Length ?? 0;
            }

            MetadataFieldChange genreChange =
                new()
                {
                    Field =
                        MetadataField.Genre,

                    OriginalValue =
                        originalGenre,

                    NewValue =
                        normalizedRequestedGenre,

                    WasManuallyApproved =
                        true,

                    Confidence =
                        1.0,

                    SupportingSources =
                        new[]
                        {
                            "Prueba aislada TagLibSharp"
                        }
                };

            MetadataWriteRequest writeRequest =
                new()
                {
                    WriteRequestId =
                        Guid.NewGuid(),

                    ApplyRequestId =
                        Guid.NewGuid(),

                    PlanId =
                        Guid.NewGuid(),

                    FilePath =
                        workingCopyPath,

                    FileName =
                        Path.GetFileName(
                            workingCopyPath),

                    VerifiedBackupPath =
                        workingBackupPath,

                    Changes =
                        new[]
                        {
                            genreChange
                        },

                    PreserveUnchangedMetadata =
                        true,

                    PreserveEmbeddedPictures =
                        true,

                    PreserveUnknownMetadata =
                        true
                };

            MetadataWriteResult writeResult =
                await writer.WriteAsync(
                    writeRequest,
                    cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            string persistedGenre;
            int pictureCountAfter;

            using (TagLib.File reopenedFile =
                TagLib.File.Create(
                    workingCopyPath))
            {
                persistedGenre =
                    JoinValues(
                        reopenedFile.Tag.Genres);

                pictureCountAfter =
                    reopenedFile.Tag.Pictures?.Length ?? 0;
            }

            string workingCopyHashAfter =
                await ComputeSha256Async(
                    workingCopyPath,
                    cancellationToken);

            string originalHashAfter =
                await ComputeSha256Async(
                    normalizedOriginalPath,
                    cancellationToken);

            messages.Add(
                "El escritor real fue ejecutado únicamente " +
                "sobre la copia aislada.");

            messages.Add(
                "La copia fue reabierta para comprobar el " +
                "valor persistido.");

            messages.Add(
                string.Equals(
                    originalHashBefore,
                    originalHashAfter,
                    StringComparison.OrdinalIgnoreCase)
                        ? "El hash del archivo original no cambió."
                        : "El hash del archivo original cambió " +
                          "inesperadamente.");

            return new TagLibIsolatedWriteTestResult
            {
                FormatDisplayName =
                    normalizedFormatDisplayName,

                OriginalFilePath =
                    normalizedOriginalPath,

                WorkingCopyPath =
                    workingCopyPath,

                WorkingBackupPath =
                    workingBackupPath,

                TestDirectoryPath =
                    testDirectoryPath,

                OriginalGenre =
                    originalGenre,

                RequestedGenre =
                    normalizedRequestedGenre,

                PersistedGenre =
                    persistedGenre,

                PictureCountBefore =
                    pictureCountBefore,

                PictureCountAfter =
                    pictureCountAfter,

                OriginalHashBefore =
                    originalHashBefore,

                OriginalHashAfter =
                    originalHashAfter,

                WorkingCopyHashBefore =
                    workingCopyHashBefore,

                WorkingCopyHashAfter =
                    workingCopyHashAfter,

                WorkingBackupHash =
                    workingBackupHash,

                WriteResult =
                    writeResult,

                Messages =
                    messages.ToArray()
            };
        }
        catch (OperationCanceledException)
        {
            messages.Add(
                $"La prueba aislada " +
                $"{normalizedFormatDisplayName} fue cancelada.");

            throw;
        }
        catch (Exception exception)
        {
            messages.Add(
                $"La prueba aislada " +
                $"{normalizedFormatDisplayName} terminó con " +
                $"un error: {exception.Message}");

            string originalHashAfter =
                File.Exists(normalizedOriginalPath)
                    ? await ComputeSha256Async(
                        normalizedOriginalPath,
                        CancellationToken.None)
                    : string.Empty;

            return new TagLibIsolatedWriteTestResult
            {
                FormatDisplayName =
                    normalizedFormatDisplayName,

                OriginalFilePath =
                    normalizedOriginalPath,

                WorkingCopyPath =
                    workingCopyPath,

                WorkingBackupPath =
                    workingBackupPath,

                TestDirectoryPath =
                    testDirectoryPath,

                RequestedGenre =
                    normalizedRequestedGenre,

                OriginalHashBefore =
                    originalHashBefore,

                OriginalHashAfter =
                    originalHashAfter,

                Messages =
                    messages.ToArray()
            };
        }
    }

    private static TagLibIsolatedWriteTestResult BuildFailure(
        string formatDisplayName,
        string originalFilePath,
        string requestedGenre,
        IReadOnlyList<string> messages)
    {
        return new TagLibIsolatedWriteTestResult
        {
            FormatDisplayName =
                formatDisplayName,

            OriginalFilePath =
                originalFilePath,

            RequestedGenre =
                requestedGenre,

            Messages =
                messages.ToArray()
        };
    }

    private static async Task<string> ComputeSha256Async(
        string filePath,
        CancellationToken cancellationToken)
    {
        await using FileStream stream =
            new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize:
                    81920,
                useAsync:
                    true);

        using SHA256 sha256 =
            SHA256.Create();

        byte[] hash =
            await sha256.ComputeHashAsync(
                stream,
                cancellationToken);

        return Convert.ToHexString(
            hash);
    }

    private static string NormalizePath(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetFullPath(
                filePath.Trim());
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string NormalizeValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string NormalizeDisplayValue(
        string? value,
        string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
    }

    private static string NormalizeFolderName(
        string? value)
    {
        string normalized =
            string.IsNullOrWhiteSpace(value)
                ? "TagLibWriteTests"
                : value.Trim();

        foreach (char invalidCharacter
            in Path.GetInvalidFileNameChars())
        {
            normalized =
                normalized.Replace(
                    invalidCharacter,
                    '_');
        }

        return string.IsNullOrWhiteSpace(normalized)
            ? "TagLibWriteTests"
            : normalized;
    }

    private static string JoinValues(
        IEnumerable<string>? values)
    {
        if (values is null)
        {
            return string.Empty;
        }

        return string.Join(
            ", ",
            values
                .Where(value =>
                    !string.IsNullOrWhiteSpace(value))
                .Select(value =>
                    value.Trim()));
    }
}