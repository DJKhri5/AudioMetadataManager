using System.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Engine;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Engine;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline;

/// <summary>
/// Coordina de forma segura las etapas necesarias para aplicar
/// modificaciones de metadatos.
///
/// La implementación actual ejecuta validación, respaldo real
/// y resolución diagnóstica del escritor. La verificación
/// posterior permanece pendiente y ningún metadato se modifica
/// todavía.
/// </summary>
public sealed class MetadataApplicationPipeline
{
    private readonly MetadataApplyRequestValidator
        _requestValidator;

    private readonly MetadataBackupEngine
        _backupEngine;

    private readonly MetadataWriterEngine
        _writerEngine;

    /// <summary>
    /// Crea el pipeline con sus componentes predeterminados.
    /// </summary>
    public MetadataApplicationPipeline()
        : this(
            new MetadataApplyRequestValidator(),
            new MetadataBackupEngine(),
            new MetadataWriterEngine())
    {
    }

    /// <summary>
    /// Crea el pipeline con un validador personalizado.
    /// </summary>
    public MetadataApplicationPipeline(
        MetadataApplyRequestValidator requestValidator,
        MetadataBackupEngine backupEngine,
        MetadataWriterEngine writerEngine)
    {
        _requestValidator =
            requestValidator ??
            throw new ArgumentNullException(
                nameof(requestValidator));

        _backupEngine =
            backupEngine ??
            throw new ArgumentNullException(
                nameof(backupEngine));

        _writerEngine =
            writerEngine ??
            throw new ArgumentNullException(
                nameof(writerEngine));
    }

    /// <summary>
    /// Ejecuta el pipeline de aplicación.
    ///
    /// Esta versión crea y verifica el respaldo, pero utiliza
    /// escritores de diagnóstico que no modifican metadatos.
    public Task<MetadataApplicationPipelineResult>
        ExecuteAsync(
            MetadataApplyRequest request,
            IProgress<MetadataApplicationProgress>? progress = null,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        return Task.Run(
            () =>
                ExecuteCore(
                    request,
                    progress,
                    cancellationToken),
            cancellationToken);
    }

    private MetadataApplicationPipelineResult ExecuteCore(
        MetadataApplyRequest request,
        IProgress<MetadataApplicationProgress>? progress,
        CancellationToken cancellationToken)
    {
        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch pipelineStopwatch =
            Stopwatch.StartNew();

        List<MetadataApplicationStageResult>
            stageResults =
                new();

        MetadataApplyValidationResult?
            validationResult =
                null;

        MetadataWriteResult?
            writeResult =
                null;

        MetadataBackupResult?
            backupResult =
                null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(
                progress,
                request,
                MetadataApplicationStage.Validation,
                10,
                "Validando la solicitud de aplicación.");

            MetadataApplicationStageResult
                validationStage =
                    ExecuteValidationStage(
                        request,
                        out validationResult,
                        cancellationToken);

            stageResults.Add(
                validationStage);

            if (!validationStage.WasSuccessful ||
                validationResult is null ||
                !validationResult.IsValid)
            {
                AddSkippedRemainingStages(
                    stageResults,
                    "El pipeline se detuvo porque la solicitud " +
                    "no superó la validación previa.");

                pipelineStopwatch.Stop();

                return BuildResult(
                    request,
                    startedAtUtc,
                    pipelineStopwatch.Elapsed,
                    MetadataApplicationStopReason
                        .ValidationFailed,
                    stageResults,
                    validationResult,
                    backupResult,
                    writeResult,
                    "La solicitud no superó la validación " +
                    "previa.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(
                progress,
                request,
                MetadataApplicationStage.Backup,
                30,
                "Creando y verificando la copia de seguridad.");

            MetadataBackupRequest backupRequest =
                new()
                {
                    ApplyRequestId =
                        request.RequestId,

                    PlanId =
                        request.PlanId,

                    SourceFilePath =
                        request.FilePath,

                    FileName =
                        request.FileName
                };

            Progress<MetadataBackupProgress> backupProgress =
                new(
                    update =>
                    {
                        double mappedPercentage =
                            30 +
                            update.NormalizedPercentage *
                            0.25;

                        ReportProgress(
                            progress,
                            request,
                            MetadataApplicationStage.Backup,
                            mappedPercentage,
                            update.Message);
                    });

            DateTimeOffset backupStartedAtUtc =
                DateTimeOffset.UtcNow;

            Stopwatch backupStopwatch =
                Stopwatch.StartNew();

            backupResult =
                _backupEngine.CreateBackupAsync(
                    backupRequest,
                    backupProgress,
                    cancellationToken)
                .GetAwaiter()
                .GetResult();

            backupStopwatch.Stop();

            MetadataApplicationStageStatus backupStageStatus =
                backupResult.WasSuccessful
                    ? MetadataApplicationStageStatus.Completed
                    : backupResult.Status ==
                      MetadataBackupStatus.Cancelled
                        ? MetadataApplicationStageStatus.Cancelled
                        : MetadataApplicationStageStatus.Failed;

            stageResults.Add(
                new MetadataApplicationStageResult
                {
                    Stage =
                        MetadataApplicationStage.Backup,

                    Status =
                        backupStageStatus,

                    StartedAtUtc =
                        backupStartedAtUtc,

                    CompletedAtUtc =
                        DateTimeOffset.UtcNow,

                    ElapsedTime =
                        backupStopwatch.Elapsed,

                    Message =
                        backupResult.Summary,

                    Details =
                        backupResult.Messages
                });

            if (!backupResult.WasSuccessful)
            {
                AddSkippedRemainingStages(
                    stageResults,
                    "El pipeline se detuvo porque no fue posible " +
                    "crear y verificar el respaldo obligatorio.");

                pipelineStopwatch.Stop();

                return BuildResult(
                    request,
                    startedAtUtc,
                    pipelineStopwatch.Elapsed,
                    backupResult.Status ==
                        MetadataBackupStatus.Cancelled
                            ? MetadataApplicationStopReason.Cancelled
                            : MetadataApplicationStopReason.BackupFailed,
                    stageResults,
                    validationResult,
                    backupResult,
                    writeResult,
                    backupResult.Summary);
            }

            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(
                progress,
                request,
                MetadataApplicationStage.MetadataWrite,
                60,
                "Resolviendo el escritor de metadatos.");

            MetadataWriteRequest writeRequest =
                new()
                {
                    ApplyRequestId =
                        request.RequestId,

                    PlanId =
                        request.PlanId,

                    FilePath =
                        request.FilePath,

                    FileName =
                        request.FileName,

                    VerifiedBackupPath =
                        backupResult.BackupFilePath,

                    Changes =
                        request.ValidChanges,

                    PreserveUnchangedMetadata =
                        true,

                    PreserveEmbeddedPictures =
                        true,

                    PreserveUnknownMetadata =
                        true
                };

            DateTimeOffset writingStartedAtUtc =
                DateTimeOffset.UtcNow;

            Stopwatch writingStopwatch =
                Stopwatch.StartNew();

            writeResult =
                _writerEngine.WriteAsync(
                    writeRequest,
                    cancellationToken)
                .GetAwaiter()
                .GetResult();

            writingStopwatch.Stop();

            bool diagnosticExecution =
                writeResult.Status ==
                MetadataWriteStatus.NoWritableChanges;

            MetadataApplicationStageStatus writingStageStatus =
                writeResult.WasSuccessful
                    ? MetadataApplicationStageStatus.Completed
                    : diagnosticExecution
                        ? MetadataApplicationStageStatus
                            .CompletedWithWarnings
                        : writeResult.Status ==
                          MetadataWriteStatus.Cancelled
                            ? MetadataApplicationStageStatus.Cancelled
                            : MetadataApplicationStageStatus.Failed;

            stageResults.Add(
                new MetadataApplicationStageResult
                {
                    Stage =
                        MetadataApplicationStage.MetadataWrite,

                    Status =
                        writingStageStatus,

                    StartedAtUtc =
                        writingStartedAtUtc,

                    CompletedAtUtc =
                        DateTimeOffset.UtcNow,

                    ElapsedTime =
                        writingStopwatch.Elapsed,

                    Message =
                        diagnosticExecution
                            ? "El escritor compatible fue resuelto. " +
                              "La ejecución fue diagnóstica y ningún " +
                              "archivo fue modificado."
                            : writeResult.Summary,

                    Details =
                        writeResult.Messages
                });

            if (!writeResult.WasSuccessful &&
                !diagnosticExecution)
            {
                AddSkippedRemainingStages(
                    stageResults,
                    "El pipeline se detuvo porque la escritura no " +
                    "pudo completarse.");

                pipelineStopwatch.Stop();

                return BuildResult(
                    request,
                    startedAtUtc,
                    pipelineStopwatch.Elapsed,
                    writeResult.Status ==
                        MetadataWriteStatus.Cancelled
                            ? MetadataApplicationStopReason.Cancelled
                            : MetadataApplicationStopReason
                                .MetadataWriteFailed,
                    stageResults,
                    validationResult,
                    backupResult,
                    writeResult,
                    writeResult.Summary);
            }

            ReportProgress(
                progress,
                request,
                MetadataApplicationStage
                    .PostWriteVerification,
                85,
                "La verificación posterior no se ejecutó.");

            stageResults.Add(
                CreateSkippedStage(
                    MetadataApplicationStage
                        .PostWriteVerification,
                    "Etapa omitida porque no se realizó " +
                    "ninguna escritura."));

            stageResults.Add(
                CreateCompletedStage(
                    MetadataApplicationStage.Finalization,
                    "La ejecución segura de diagnóstico " +
                    "terminó correctamente."));

            ReportProgress(
                progress,
                request,
                MetadataApplicationStage.Finalization,
                100,
                "Validación completada. Ningún archivo fue " +
                "modificado.");

            pipelineStopwatch.Stop();

            /*
             * El pipeline todavía no puede declararse
             * completamente terminado, porque respaldo,
             * escritura y verificación fueron omitidos.
             */
            return BuildResult(
                request,
                startedAtUtc,
                pipelineStopwatch.Elapsed,
                MetadataApplicationStopReason.None,
                stageResults,
                validationResult,
                backupResult,
                writeResult,
                string.Empty);
        }
        catch (OperationCanceledException)
        {
            AddCancelledOrSkippedStages(
                stageResults);

            pipelineStopwatch.Stop();

            return BuildResult(
                request,
                startedAtUtc,
                pipelineStopwatch.Elapsed,
                MetadataApplicationStopReason.Cancelled,
                stageResults,
                validationResult,
                backupResult,
                writeResult,
                "La ejecución fue cancelada.");
        }
        catch (Exception exception)
        {
            AddSkippedRemainingStages(
                stageResults,
                "Etapa omitida debido a un error inesperado.");

            pipelineStopwatch.Stop();

            return BuildResult(
                request,
                startedAtUtc,
                pipelineStopwatch.Elapsed,
                MetadataApplicationStopReason.UnexpectedError,
                stageResults,
                validationResult,
                backupResult,
                writeResult,
                $"Ocurrió un error inesperado: " +
                $"{exception.Message}");
        }
    }

    private MetadataApplicationStageResult
        ExecuteValidationStage(
            MetadataApplyRequest request,
            out MetadataApplyValidationResult validationResult,
            CancellationToken cancellationToken)
    {
        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        cancellationToken.ThrowIfCancellationRequested();

        validationResult =
            _requestValidator.Validate(
                request);

        stopwatch.Stop();

        MetadataApplicationStageStatus status =
            validationResult.IsValid
                ? validationResult.WarningCount > 0
                    ? MetadataApplicationStageStatus
                        .CompletedWithWarnings
                    : MetadataApplicationStageStatus.Completed
                : MetadataApplicationStageStatus.Failed;

        IReadOnlyList<string> details =
            validationResult.Issues
                .Select(
                    issue =>
                        $"[{issue.Code}] {issue.Summary}")
                .ToArray();

        return new MetadataApplicationStageResult
        {
            Stage =
                MetadataApplicationStage.Validation,

            Status =
                status,

            StartedAtUtc =
                startedAtUtc,

            CompletedAtUtc =
                DateTimeOffset.UtcNow,

            ElapsedTime =
                stopwatch.Elapsed,

            Message =
                validationResult.Summary,

            Details =
                details
        };
    }

    private static void AddSkippedRemainingStages(
        List<MetadataApplicationStageResult> stageResults,
        string message)
    {
        MetadataApplicationStage[] stages =
        {
            MetadataApplicationStage.Backup,
            MetadataApplicationStage.MetadataWrite,
            MetadataApplicationStage.PostWriteVerification,
            MetadataApplicationStage.Finalization
        };

        foreach (MetadataApplicationStage stage in stages)
        {
            if (stageResults.Any(
                    result =>
                        result.Stage == stage))
            {
                continue;
            }

            stageResults.Add(
                CreateSkippedStage(
                    stage,
                    message));
        }
    }

    private static void AddCancelledOrSkippedStages(
        List<MetadataApplicationStageResult> stageResults)
    {
        MetadataApplicationStage[] stages =
        {
            MetadataApplicationStage.Validation,
            MetadataApplicationStage.Backup,
            MetadataApplicationStage.MetadataWrite,
            MetadataApplicationStage.PostWriteVerification,
            MetadataApplicationStage.Finalization
        };

        bool cancelledStageAdded =
            stageResults.Any(
                result =>
                    result.Status ==
                    MetadataApplicationStageStatus.Cancelled);

        foreach (MetadataApplicationStage stage in stages)
        {
            if (stageResults.Any(
                    result =>
                        result.Stage == stage))
            {
                continue;
            }

            if (!cancelledStageAdded)
            {
                stageResults.Add(
                    CreateCancelledStage(
                        stage,
                        "La ejecución fue cancelada antes de " +
                        "completar esta etapa."));

                cancelledStageAdded =
                    true;

                continue;
            }

            stageResults.Add(
                CreateSkippedStage(
                    stage,
                    "Etapa omitida por cancelación."));
        }
    }

    private static MetadataApplicationStageResult
        CreateCompletedStage(
            MetadataApplicationStage stage,
            string message)
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        return new MetadataApplicationStageResult
        {
            Stage =
                stage,

            Status =
                MetadataApplicationStageStatus.Completed,

            StartedAtUtc =
                now,

            CompletedAtUtc =
                now,

            ElapsedTime =
                TimeSpan.Zero,

            Message =
                message
        };
    }

    private static MetadataApplicationStageResult
        CreateSkippedStage(
            MetadataApplicationStage stage,
            string message)
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        return new MetadataApplicationStageResult
        {
            Stage =
                stage,

            Status =
                MetadataApplicationStageStatus.Skipped,

            StartedAtUtc =
                now,

            CompletedAtUtc =
                now,

            ElapsedTime =
                TimeSpan.Zero,

            Message =
                message
        };
    }

    private static MetadataApplicationStageResult
        CreateCancelledStage(
            MetadataApplicationStage stage,
            string message)
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        return new MetadataApplicationStageResult
        {
            Stage =
                stage,

            Status =
                MetadataApplicationStageStatus.Cancelled,

            StartedAtUtc =
                now,

            CompletedAtUtc =
                now,

            ElapsedTime =
                TimeSpan.Zero,

            Message =
                message
        };
    }

    private static MetadataApplicationPipelineResult
        BuildResult(
            MetadataApplyRequest request,
            DateTimeOffset startedAtUtc,
            TimeSpan elapsedTime,
            MetadataApplicationStopReason stopReason,
            IReadOnlyList<MetadataApplicationStageResult>
                stageResults,
            MetadataApplyValidationResult? validationResult,
            MetadataBackupResult? backupResult,
            MetadataWriteResult? writeResult,
            string errorMessage)
    {
        return new MetadataApplicationPipelineResult
        {
            Request =
                request,

            StartedAtUtc =
                startedAtUtc,

            CompletedAtUtc =
                DateTimeOffset.UtcNow,

            ElapsedTime =
                elapsedTime,

            StopReason =
                stopReason,

            StageResults =
                stageResults.ToArray(),

            ValidationResult =
                validationResult,

            ApplyResult =
                null,

            ErrorMessage =
                errorMessage,

            BackupResult =
                backupResult,

            WriteResult =
                writeResult
        };
    }

    private static void ReportProgress(
        IProgress<MetadataApplicationProgress>? progress,
        MetadataApplyRequest request,
        MetadataApplicationStage stage,
        double percentage,
        string message)
    {
        progress?.Report(
            new MetadataApplicationProgress
            {
                Stage =
                    stage,

                Percentage =
                    percentage,

                Message =
                    message,

                FileName =
                    request.FileName
            });
    }
}