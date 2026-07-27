using System.IO;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
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
/// usando cualquier escritor compatible con
/// IMetadataFormatWriter.
///
/// La creación de la copia, el respaldo y la verificación de
/// hashes se delegan a FileIsolationTestHarness.
///
/// El archivo original nunca se entrega al escritor.
/// </summary>
public sealed class TagLibIsolatedWriteTestRunner
{
    private readonly FileIsolationTestHarness
        _isolationHarness;

    /// <summary>
    /// Crea el runner con la infraestructura de aislamiento
    /// predeterminada.
    /// </summary>
    public TagLibIsolatedWriteTestRunner()
        : this(
            new FileIsolationTestHarness())
    {
    }

    /// <summary>
    /// Crea el runner con una infraestructura de aislamiento
    /// personalizada.
    /// </summary>
    public TagLibIsolatedWriteTestRunner(
        FileIsolationTestHarness isolationHarness)
    {
        _isolationHarness =
            isolationHarness ??
            throw new ArgumentNullException(
                nameof(isolationHarness));
    }

    /// <summary>
    /// Ejecuta una prueba real de escritura exclusivamente sobre
    /// una copia temporal del archivo proporcionado.
    /// </summary>
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

        FileIsolationContext?
            isolationContext =
                null;

        FileIsolationVerificationResult?
            isolationVerification =
                null;

        string originalGenre =
            string.Empty;

        string persistedGenre =
            string.Empty;

        int pictureCountBefore =
            0;

        int pictureCountAfter =
            0;

        MetadataWriteResult?
            writeResult =
                null;

        try
        {
            isolationContext =
                await _isolationHarness.CreateAsync(
                    normalizedOriginalPath,
                    testFolderName,
                    cancellationToken);

            messages.Add(
                $"Se creó una copia aislada del archivo " +
                $"{normalizedFormatDisplayName}.");

            messages.Add(
                "Se creó y verificó un respaldo independiente " +
                "de la copia antes de ejecutar Save().");

            cancellationToken.ThrowIfCancellationRequested();

            using (TagLib.File tagFile =
                TagLib.File.Create(
                    isolationContext.WorkingCopyPath))
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
                        isolationContext.WorkingCopyPath,

                    FileName =
                        Path.GetFileName(
                            isolationContext.WorkingCopyPath),

                    VerifiedBackupPath =
                        isolationContext.WorkingBackupPath,

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

            writeResult =
                await writer.WriteAsync(
                    writeRequest,
                    cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            using (TagLib.File reopenedFile =
                TagLib.File.Create(
                    isolationContext.WorkingCopyPath))
            {
                persistedGenre =
                    JoinValues(
                        reopenedFile.Tag.Genres);

                pictureCountAfter =
                    reopenedFile.Tag.Pictures?.Length ?? 0;
            }

            isolationVerification =
                await _isolationHarness.VerifyAsync(
                    isolationContext,
                    cancellationToken);

            messages.Add(
                "El escritor real fue ejecutado únicamente " +
                "sobre la copia aislada.");

            messages.Add(
                "La copia fue reabierta para comprobar el " +
                "valor persistido.");

            messages.AddRange(
                isolationVerification.Messages);

            return BuildResult(
                normalizedFormatDisplayName,
                normalizedRequestedGenre,
                originalGenre,
                persistedGenre,
                pictureCountBefore,
                pictureCountAfter,
                isolationContext,
                isolationVerification,
                writeResult,
                messages);
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

            if (isolationContext is not null)
            {
                try
                {
                    isolationVerification =
                        await _isolationHarness.VerifyAsync(
                            isolationContext,
                            CancellationToken.None);

                    messages.AddRange(
                        isolationVerification.Messages);
                }
                catch (Exception verificationException)
                {
                    messages.Add(
                        "No fue posible completar la verificación " +
                        "del entorno aislado después del error: " +
                        verificationException.Message);
                }
            }

            return BuildResult(
                normalizedFormatDisplayName,
                normalizedRequestedGenre,
                originalGenre,
                persistedGenre,
                pictureCountBefore,
                pictureCountAfter,
                isolationContext,
                isolationVerification,
                writeResult,
                messages);
        }
    }

    private static TagLibIsolatedWriteTestResult BuildResult(
        string formatDisplayName,
        string requestedGenre,
        string originalGenre,
        string persistedGenre,
        int pictureCountBefore,
        int pictureCountAfter,
        FileIsolationContext? isolationContext,
        FileIsolationVerificationResult? isolationVerification,
        MetadataWriteResult? writeResult,
        IReadOnlyList<string> messages)
    {
        return new TagLibIsolatedWriteTestResult
        {
            FormatDisplayName =
                formatDisplayName,

            OriginalFilePath =
                isolationContext?.OriginalFilePath ??
                string.Empty,

            WorkingCopyPath =
                isolationContext?.WorkingCopyPath ??
                string.Empty,

            WorkingBackupPath =
                isolationContext?.WorkingBackupPath ??
                string.Empty,

            TestDirectoryPath =
                isolationContext?.TestDirectoryPath ??
                string.Empty,

            OriginalGenre =
                originalGenre,

            RequestedGenre =
                requestedGenre,

            PersistedGenre =
                persistedGenre,

            PictureCountBefore =
                pictureCountBefore,

            PictureCountAfter =
                pictureCountAfter,

            OriginalHashBefore =
                isolationContext?.OriginalHashBefore ??
                string.Empty,

            OriginalHashAfter =
                isolationVerification?.OriginalHashAfter ??
                string.Empty,

            WorkingCopyHashBefore =
                isolationContext?.WorkingCopyHashBefore ??
                string.Empty,

            WorkingCopyHashAfter =
                isolationVerification?.WorkingCopyHashAfter ??
                string.Empty,

            WorkingBackupHash =
                isolationContext?.WorkingBackupHash ??
                string.Empty,

            WriteResult =
                writeResult,

            Messages =
                messages.ToArray()
        };
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

    private static string NormalizePath(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
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
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(value))
                .Select(
                    value =>
                        value.Trim()));
    }
}