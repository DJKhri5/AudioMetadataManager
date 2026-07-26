using System.Diagnostics;
using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Interfaces;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Resolution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Engine;

/// <summary>
/// Coordina la validación básica, resolución y ejecución del
/// escritor compatible con el formato del archivo.
///
/// El motor no conoce los detalles internos de MP3, FLAC, WAV
/// o AIFF. Esa responsabilidad pertenece a cada implementación
/// de IMetadataFormatWriter.
/// </summary>
public sealed class MetadataWriterEngine
{
    private readonly MetadataWriterResolver
        _writerResolver;

    /// <summary>
    /// Crea el motor con los escritores de diagnóstico
    /// predeterminados.
    ///
    /// Ninguno de estos escritores modifica archivos.
    /// </summary>
    public MetadataWriterEngine()
        : this(
            CreateDefaultWriters())
    {
    }

    /// <summary>
    /// Crea el motor con la colección de escritores indicada.
    /// </summary>
    public MetadataWriterEngine(
        IEnumerable<IMetadataFormatWriter> writers)
        : this(
            new MetadataWriterResolver(
                writers))
    {
    }

    /// <summary>
    /// Crea el motor con un resolutor personalizado.
    /// </summary>
    public MetadataWriterEngine(
        MetadataWriterResolver writerResolver)
    {
        _writerResolver =
            writerResolver ??
            throw new ArgumentNullException(
                nameof(writerResolver));
    }

    /// <summary>
    /// Escritores registrados en el motor.
    /// </summary>
    public IReadOnlyList<IMetadataFormatWriter>
        Writers =>
            _writerResolver.Writers;

    /// <summary>
    /// Valida la solicitud, resuelve el escritor adecuado y
    /// ejecuta la operación correspondiente.
    ///
    /// Con los escritores registrados actualmente, esta
    /// ejecución es exclusivamente diagnóstica.
    /// </summary>
    public async Task<MetadataWriteResult> WriteAsync(
        MetadataWriteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        List<string> messages =
            new();

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

            MetadataWriterResolutionResult resolution =
                _writerResolver.Resolve(
                    request.NormalizedExtension);

            messages.Add(
                resolution.Summary);

            if (!resolution.WasResolved ||
                resolution.Writer is null)
            {
                return BuildResult(
                    request,
                    MetadataWriteStatus.UnsupportedFormat,
                    writerName:
                        string.Empty,
                    startedAtUtc,
                    stopwatch,
                    fieldResults:
                        BuildUnsupportedFieldResults(
                            request),
                    messages:
                        messages);
            }

            cancellationToken.ThrowIfCancellationRequested();

            MetadataWriteResult writerResult =
                await resolution.Writer.WriteAsync(
                    request,
                    cancellationToken);

            stopwatch.Stop();

            return MergeWriterResult(
                request,
                writerResult,
                resolution.Writer.Name,
                startedAtUtc,
                stopwatch.Elapsed,
                messages);
        }
        catch (OperationCanceledException)
        {
            messages.Add(
                "La operación de escritura fue cancelada.");

            return BuildResult(
                request,
                MetadataWriteStatus.Cancelled,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    BuildUnprocessedFieldResults(
                        request,
                        "La operación fue cancelada antes de " +
                        "procesar el campo."),
                messages:
                    messages);
        }
        catch (UnauthorizedAccessException exception)
        {
            messages.Add(
                "Windows rechazó el acceso necesario para " +
                $"procesar el archivo: {exception.Message}");

            return BuildResult(
                request,
                MetadataWriteStatus.FileOpenFailed,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    BuildUnprocessedFieldResults(
                        request,
                        "No fue posible acceder al archivo."),
                messages:
                    messages);
        }
        catch (IOException exception)
        {
            messages.Add(
                "Ocurrió un error de entrada o salida durante " +
                $"la operación: {exception.Message}");

            return BuildResult(
                request,
                MetadataWriteStatus.SaveFailed,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    BuildUnprocessedFieldResults(
                        request,
                        "La operación falló por un error de " +
                        "entrada o salida."),
                messages:
                    messages);
        }
        catch (Exception exception)
        {
            messages.Add(
                "Ocurrió un error inesperado durante la " +
                $"escritura: {exception.Message}");

            return BuildResult(
                request,
                MetadataWriteStatus.UnexpectedError,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    BuildUnprocessedFieldResults(
                        request,
                        "El campo no pudo procesarse debido a " +
                        "un error inesperado."),
                messages:
                    messages);
        }
    }

    /// <summary>
    /// Permite comprobar qué escritor procesaría una extensión
    /// sin ejecutar una solicitud de escritura.
    /// </summary>
    public MetadataWriterResolutionResult ResolveWriter(
        string? extension)
    {
        return _writerResolver.Resolve(
            extension);
    }

    private static MetadataWriteResult? ValidateRequest(
        MetadataWriteRequest request,
        DateTimeOffset startedAtUtc,
        Stopwatch stopwatch,
        List<string> messages)
    {
        if (request.WriteRequestId == Guid.Empty)
        {
            messages.Add(
                "La solicitud no contiene un identificador " +
                "de escritura válido.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    Array.Empty<MetadataFieldWriteResult>(),
                messages:
                    messages);
        }

        if (request.ApplyRequestId == Guid.Empty)
        {
            messages.Add(
                "La solicitud no contiene un identificador " +
                "de aplicación válido.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    Array.Empty<MetadataFieldWriteResult>(),
                messages:
                    messages);
        }

        if (request.PlanId == Guid.Empty)
        {
            messages.Add(
                "La solicitud no contiene un identificador " +
                "de plan válido.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    Array.Empty<MetadataFieldWriteResult>(),
                messages:
                    messages);
        }

        if (string.IsNullOrWhiteSpace(
                request.NormalizedFilePath))
        {
            messages.Add(
                "La solicitud no contiene una ruta de archivo " +
                "utilizable.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    Array.Empty<MetadataFieldWriteResult>(),
                messages:
                    messages);
        }

        if (!File.Exists(
                request.NormalizedFilePath))
        {
            messages.Add(
                "El archivo que se desea procesar no existe.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    Array.Empty<MetadataFieldWriteResult>(),
                messages:
                    messages);
        }

        if (!request.HasVerifiedBackup)
        {
            messages.Add(
                "No existe un respaldo físico asociado a la " +
                "solicitud.");

            return BuildResult(
                request,
                MetadataWriteStatus.ValidationFailed,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    BuildUnprocessedFieldResults(
                        request,
                        "El campo no puede procesarse sin un " +
                        "respaldo previo."),
                messages:
                    messages);
        }

        if (request.ValidChanges.Count == 0)
        {
            messages.Add(
                "La solicitud no contiene cambios válidos " +
                "para escribir.");

            return BuildResult(
                request,
                MetadataWriteStatus.NoWritableChanges,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    Array.Empty<MetadataFieldWriteResult>(),
                messages:
                    messages);
        }

        if (string.IsNullOrWhiteSpace(
                request.NormalizedExtension))
        {
            messages.Add(
                "No fue posible determinar la extensión del " +
                "archivo.");

            return BuildResult(
                request,
                MetadataWriteStatus.UnsupportedFormat,
                writerName:
                    string.Empty,
                startedAtUtc,
                stopwatch,
                fieldResults:
                    BuildUnsupportedFieldResults(
                        request),
                messages:
                    messages);
        }

        messages.Add(
            "La solicitud superó las comprobaciones " +
            "estructurales del motor de escritura.");

        return null;
    }

    private static MetadataWriteResult MergeWriterResult(
        MetadataWriteRequest request,
        MetadataWriteResult writerResult,
        string writerName,
        DateTimeOffset startedAtUtc,
        TimeSpan elapsedTime,
        IReadOnlyList<string> engineMessages)
    {
        List<string> mergedMessages =
            new();

        mergedMessages.AddRange(
            engineMessages);

        mergedMessages.AddRange(
            writerResult.Messages);

        return new MetadataWriteResult
        {
            WriteRequestId =
                request.WriteRequestId,

            ApplyRequestId =
                request.ApplyRequestId,

            PlanId =
                request.PlanId,

            Status =
                writerResult.Status,

            FilePath =
                request.NormalizedFilePath,

            WriterName =
                string.IsNullOrWhiteSpace(
                    writerResult.WriterName)
                        ? writerName
                        : writerResult.WriterName,

            StartedAtUtc =
                startedAtUtc,

            CompletedAtUtc =
                DateTimeOffset.UtcNow,

            ElapsedTime =
                elapsedTime,

            FieldResults =
                writerResult.FieldResults.ToArray(),

            Messages =
                mergedMessages.ToArray()
        };
    }

    private static MetadataWriteResult BuildResult(
        MetadataWriteRequest request,
        MetadataWriteStatus status,
        string writerName,
        DateTimeOffset startedAtUtc,
        Stopwatch stopwatch,
        IReadOnlyList<MetadataFieldWriteResult> fieldResults,
        IReadOnlyList<string> messages)
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
                writerName,

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

    private static IReadOnlyList<MetadataFieldWriteResult>
        BuildUnsupportedFieldResults(
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
                            change.OriginalValue,

                        RequestedValue =
                            change.NewValue,

                        IsSupported =
                            false,

                        ValuePrepared =
                            false,

                        SaveSucceeded =
                            false,

                        Message =
                            "No existe un escritor compatible " +
                            "con el formato del archivo."
                    })
            .ToArray();
    }

    private static IReadOnlyList<MetadataFieldWriteResult>
        BuildUnprocessedFieldResults(
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
                            change.OriginalValue,

                        RequestedValue =
                            change.NewValue,

                        IsSupported =
                            true,

                        ValuePrepared =
                            false,

                        SaveSucceeded =
                            false,

                        Message =
                            message
                    })
            .ToArray();
    }

    private static IReadOnlyList<IMetadataFormatWriter>
        CreateDefaultWriters()
    {
        return new IMetadataFormatWriter[]
        {
            new DiagnosticMp3MetadataWriter(),
            new DiagnosticFlacMetadataWriter(),
            new DiagnosticWavMetadataWriter(),
            new DiagnosticAiffMetadataWriter()
        };
    }
}