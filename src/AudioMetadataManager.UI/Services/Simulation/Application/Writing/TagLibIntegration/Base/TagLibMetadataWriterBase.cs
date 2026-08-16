using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Interfaces;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Writing.Resolution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.FieldMapping;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Interfaces;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Models;
using System.Diagnostics;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Base;

/// <summary>
/// Implementación común para escritores reales basados en
/// TagLibSharp.
///
/// La clase valida la solicitud y el respaldo, delega la
/// traducción de campos en ITagLibFieldMapper, guarda el archivo
/// y utiliza el motor común de verificación posterior.
///
/// Las clases concretas solamente declaran el nombre técnico,
/// la familia de formato y las extensiones compatibles.
/// </summary>
public abstract class TagLibMetadataWriterBase
    : IMetadataFormatWriter,
      IMetadataWriterDescriptor
{
    private readonly IReadOnlySet<string>
        _supportedExtensions;

    private readonly ITagLibFieldMapper
        _fieldMapper;

    /// <inheritdoc />
    public MetadataWriterKind WriterKind =>
        MetadataWriterKind.Real;

    /// <inheritdoc />
    public int ResolutionPriority =>
        100;

    /// <summary>
    /// Inicializa un escritor TagLibSharp especializado.
    /// </summary>
    protected TagLibMetadataWriterBase(
        string name,
        string formatDisplayName,
        IEnumerable<string> supportedExtensions)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El escritor debe tener un nombre.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(formatDisplayName))
        {
            throw new ArgumentException(
                "El formato debe tener un nombre.",
                nameof(formatDisplayName));
        }

        ArgumentNullException.ThrowIfNull(
            supportedExtensions);

        Name =
            name.Trim();

        FormatDisplayName =
            formatDisplayName.Trim();

        _supportedExtensions =
            new HashSet<string>(
                supportedExtensions
                    .Where(extension =>
                        !string.IsNullOrWhiteSpace(extension))
                    .Select(NormalizeExtension),
                StringComparer.OrdinalIgnoreCase);

        if (_supportedExtensions.Count == 0)
        {
            throw new ArgumentException(
                "Debe existir al menos una extensión válida.",
                nameof(supportedExtensions));
        }

        _fieldMapper =
            new TagLibFieldMapper();
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Nombre legible de la familia de formato.
    /// </summary>
    protected string FormatDisplayName { get; }

    /// <inheritdoc />
    public IReadOnlySet<string> SupportedExtensions =>
        _supportedExtensions;

    /// <inheritdoc />
    public bool CanWrite(
        string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        return _supportedExtensions.Contains(
            NormalizeExtension(extension));
    }

    /// <inheritdoc />
    public Task<MetadataWriteResult> WriteAsync(
        MetadataWriteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        return Task.Run(
            () =>
                WriteCore(
                    request,
                    cancellationToken),
            cancellationToken);
    }

    private MetadataWriteResult WriteCore(
        MetadataWriteRequest request,
        CancellationToken cancellationToken)
    {
        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        List<string> messages =
            new();

        List<TagLibFieldMappingResult> preparedFields =
            new();

        int pictureCountBefore =
            0;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            MetadataWriteResult? validationFailure =
                ValidateRequest(
                    request,
                    startedAtUtc,
                    stopwatch,
                    messages);

            if (validationFailure is not null)
            {
                return validationFailure;
            }

            cancellationToken.ThrowIfCancellationRequested();

            using (TagLib.File tagFile =
                TagLib.File.Create(
                    request.NormalizedFilePath))
            {
                TagLib.Tag tag =
                    tagFile.Tag;

                pictureCountBefore =
                    tag.Pictures?.Length ?? 0;

                messages.Add(
                    $"El archivo {FormatDisplayName} fue abierto " +
                    "mediante TagLibSharp.");

                foreach (MetadataFieldChange change
                    in request.ValidChanges)
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    preparedFields.Add(
                        _fieldMapper.PrepareChange(
                            tagFile,
                            change));
                }

                bool hasPreparedFields =
                    preparedFields.Any(
                        field =>
                            field.WasSuccessful);

                if (!hasPreparedFields)
                {
                    messages.Add(
                        "Ningún campo compatible pudo " +
                        "prepararse para escritura.");

                    return BuildResult(
                        request,
                        MetadataWriteStatus.NoWritableChanges,
                        startedAtUtc,
                        stopwatch,
                        BuildFieldResults(
                            preparedFields,
                            saveSucceeded:
                                false),
                        messages);
                }

                int pictureCountAfterPreparation =
                    tag.Pictures?.Length ?? 0;

                if (request.PreserveEmbeddedPictures &&
                    pictureCountBefore !=
                    pictureCountAfterPreparation)
                {
                    messages.Add(
                        "La preparación alteró inesperadamente " +
                        "la cantidad de imágenes incrustadas.");

                    return BuildResult(
                        request,
                        MetadataWriteStatus.SaveFailed,
                        startedAtUtc,
                        stopwatch,
                        BuildFieldResults(
                            preparedFields,
                            saveSucceeded:
                                false),
                        messages);
                }

                cancellationToken.ThrowIfCancellationRequested();

                tagFile.Save();

                messages.Add(
                    "TagLib.File.Save() terminó sin lanzar " +
                    "excepciones.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<MetadataFieldWriteResult>
                fieldResults =
                    BuildFieldResults(
                        preparedFields,
                        saveSucceeded:
                            true);

            int successfulCount =
                fieldResults.Count(
                    result =>
                        result.WasWritten);

            bool allFieldsSuccessful =
                fieldResults.Count > 0 &&
                successfulCount ==
                    fieldResults.Count;

            MetadataWriteStatus finalStatus;

            if (allFieldsSuccessful)
            {
                finalStatus =
                    MetadataWriteStatus.Completed;

                messages.Add(
                    "Todos los campos solicitados fueron preparados " +
                    "y guardados correctamente.");
            }
            else if (successfulCount > 0)
            {
                finalStatus =
                    MetadataWriteStatus.PartiallyCompleted;

                messages.Add(
                    "La escritura se completó parcialmente. " +
                    "Al menos un campo no pudo prepararse o guardarse.");
            }
            else
            {
                finalStatus =
                    MetadataWriteStatus.SaveFailed;

                messages.Add(
                    "Ningún campo pudo confirmarse como guardado.");
            }

            return BuildResult(
                request,
                finalStatus,
                startedAtUtc,
                stopwatch,
                fieldResults,
                messages,
                pictureCountBefore);
        }
        catch (OperationCanceledException)
        {
            messages.Add(
                $"La escritura {FormatDisplayName} fue cancelada.");

            return BuildResult(
                request,
                MetadataWriteStatus.Cancelled,
                startedAtUtc,
                stopwatch,
                BuildFieldResults(
                    preparedFields,
                    saveSucceeded:
                        false),
                messages);
        }
        catch (TagLib.UnsupportedFormatException exception)
        {
            messages.Add(
                "TagLibSharp no reconoce el archivo como " +
                $"{FormatDisplayName} compatible: " +
                exception.Message);

            return BuildResult(
                request,
                MetadataWriteStatus.UnsupportedFormat,
                startedAtUtc,
                stopwatch,
                BuildFieldResults(
                    preparedFields,
                    saveSucceeded:
                        false),
                messages);
        }
        catch (TagLib.CorruptFileException exception)
        {
            messages.Add(
                "El archivo o sus etiquetas parecen estar " +
                $"dañados: {exception.Message}");

            return BuildResult(
                request,
                MetadataWriteStatus.FileOpenFailed,
                startedAtUtc,
                stopwatch,
                BuildFieldResults(
                    preparedFields,
                    saveSucceeded:
                        false),
                messages);
        }
        catch (UnauthorizedAccessException exception)
        {
            messages.Add(
                "Windows rechazó el acceso necesario para " +
                $"guardar el archivo: {exception.Message}");

            return BuildResult(
                request,
                MetadataWriteStatus.SaveFailed,
                startedAtUtc,
                stopwatch,
                BuildFieldResults(
                    preparedFields,
                    saveSucceeded:
                        false),
                messages);
        }
        catch (IOException exception)
        {
            messages.Add(
                "Ocurrió un error de entrada o salida durante " +
                $"el guardado: {exception.Message}");

            return BuildResult(
                request,
                MetadataWriteStatus.SaveFailed,
                startedAtUtc,
                stopwatch,
                BuildFieldResults(
                    preparedFields,
                    saveSucceeded:
                        false),
                messages);
        }
        catch (Exception exception)
        {
            messages.Add(
                "Ocurrió un error inesperado durante la " +
                $"escritura {FormatDisplayName}: " +
                exception.Message);

            return BuildResult(
                request,
                MetadataWriteStatus.UnexpectedError,
                startedAtUtc,
                stopwatch,
                BuildFieldResults(
                    preparedFields,
                    saveSucceeded:
                        false),
                messages);
        }
    }

    private MetadataWriteResult? ValidateRequest(
        MetadataWriteRequest request,
        DateTimeOffset startedAtUtc,
        Stopwatch stopwatch,
        List<string> messages)
    {
        if (!request.IsStructurallyValid)
        {
            messages.Add(
                "La solicitud no contiene todos los datos " +
                "obligatorios para una escritura segura.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                startedAtUtc,
                stopwatch,
                BuildUnprocessedResults(
                    request,
                    "La solicitud no superó la validación " +
                    "estructural."),
                messages);
        }

        if (!CanWrite(
                request.NormalizedExtension))
        {
            messages.Add(
                $"{Name} no admite la extensión " +
                $"{request.NormalizedExtension}.");

            return BuildResult(
                request,
                MetadataWriteStatus.UnsupportedFormat,
                startedAtUtc,
                stopwatch,
                BuildUnsupportedResults(
                    request),
                messages);
        }

        if (!File.Exists(
                request.VerifiedBackupPath))
        {
            messages.Add(
                "El respaldo obligatorio ya no existe.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                startedAtUtc,
                stopwatch,
                BuildUnprocessedResults(
                    request,
                    "No existe un respaldo físico verificable."),
                messages);
        }

        string sourcePath =
            Path.GetFullPath(
                request.NormalizedFilePath);

        string backupPath =
            Path.GetFullPath(
                request.VerifiedBackupPath);

        if (string.Equals(
                sourcePath,
                backupPath,
                StringComparison.OrdinalIgnoreCase))
        {
            messages.Add(
                "La ruta del respaldo coincide con la ruta " +
                "del archivo original.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                startedAtUtc,
                stopwatch,
                BuildUnprocessedResults(
                    request,
                    "La copia de seguridad no puede ser el " +
                    "mismo archivo que el original."),
                messages);
        }

        messages.Add(
            "La solicitud y el respaldo superaron las " +
            $"comprobaciones del escritor {FormatDisplayName}.");

        return null;
    }

    private static IReadOnlyList<MetadataFieldWriteResult>
        BuildFieldResults(
            IReadOnlyList<TagLibFieldMappingResult>
                preparedFields,
            bool saveSucceeded)
    {
        return preparedFields
            .Select(
                preparedField =>
                {
                    bool fieldWasSaved =
                        saveSucceeded &&
                        preparedField.IsSupported &&
                        preparedField.ValuePrepared;

                    string message;

                    if (!preparedField.IsSupported ||
                        !preparedField.ValuePrepared)
                    {
                        message =
                            preparedField.Message;
                    }
                    else if (!saveSucceeded)
                    {
                        message =
                            "El valor fue preparado, pero el " +
                            "guardado no terminó correctamente.";
                    }
                    else
                    {
                        message =
                            "El valor fue preparado por el mapper " +
                            "y TagLib.File.Save() terminó sin errores.";
                    }

                    return new MetadataFieldWriteResult
                    {
                        Field =
                            preparedField.Field,

                        OriginalValue =
                            preparedField.OriginalValue,

                        RequestedValue =
                            preparedField.RequestedValue,

                        IsSupported =
                            preparedField.IsSupported,

                        ValuePrepared =
                            preparedField.ValuePrepared,

                        SaveSucceeded =
                            fieldWasSaved,

                        Message =
                            message
                    };
                })
            .ToArray();
    }

    private IReadOnlyList<MetadataFieldWriteResult>
        BuildUnprocessedResults(
            MetadataWriteRequest request,
            string message)
    {
        return request.ValidChanges
            .Select(
                change =>
                    new MetadataFieldWriteResult
                    {
                        Field =
                            change.Field,

                        OriginalValue =
                            _fieldMapper.NormalizeValue(
                                change.OriginalValue),

                        RequestedValue =
                            _fieldMapper.NormalizeValue(
                                change.NewValue),

                        IsSupported =
                            _fieldMapper.IsSupported(
                                change.Field),

                        ValuePrepared =
                            false,

                        SaveSucceeded =
                            false,

                        Message =
                            message
                    })
            .ToArray();
    }

    private IReadOnlyList<MetadataFieldWriteResult>
        BuildUnsupportedResults(
            MetadataWriteRequest request)
    {
        return request.ValidChanges
            .Select(
                change =>
                    new MetadataFieldWriteResult
                    {
                        Field =
                            change.Field,

                        OriginalValue =
                            _fieldMapper.NormalizeValue(
                                change.OriginalValue),

                        RequestedValue =
                            _fieldMapper.NormalizeValue(
                                change.NewValue),

                        IsSupported =
                            false,

                        ValuePrepared =
                            false,

                        SaveSucceeded =
                            false,

                        Message =
                            "El formato no es compatible con " +
                            "este escritor."
                    })
            .ToArray();
    }

    private MetadataWriteResult BuildResult(
        MetadataWriteRequest request,
        MetadataWriteStatus status,
        DateTimeOffset startedAtUtc,
        Stopwatch stopwatch,
        IReadOnlyList<MetadataFieldWriteResult>
            fieldResults,
        IReadOnlyList<string> messages,
        int pictureCountBefore = 0)
    {
        stopwatch.Stop();

        return new MetadataWriteResult
        {
            WriteRequestId =
                request.WriteRequestId,

            ApplyRequestId =
                request.ApplyRequestId,

            PlanId =
                request.PlanId,

            Status =
                status,

            FilePath =
                request.NormalizedFilePath,

            WriterName =
                Name,

            PictureCountBefore =
                Math.Max(
                    0,
                    pictureCountBefore),

            StartedAtUtc =
                startedAtUtc,

            CompletedAtUtc =
                DateTimeOffset.UtcNow,

            ElapsedTime =
                stopwatch.Elapsed,

            FieldResults =
                fieldResults.ToArray(),

            Messages =
                messages.ToArray()
        };
    }

    private static string NormalizeExtension(
        string extension)
    {
        string normalized =
            extension.Trim();

        if (!normalized.StartsWith('.'))
        {
            normalized =
                "." + normalized;
        }

        return normalized.ToLowerInvariant();
    }
}
