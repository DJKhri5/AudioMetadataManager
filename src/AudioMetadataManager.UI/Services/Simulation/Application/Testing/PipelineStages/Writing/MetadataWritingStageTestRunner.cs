using System.IO;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Writing;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Engine;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Writing;

/// <summary>
/// Ejecuta pruebas estructurales sobre la etapa concreta de
/// escritura.
///
/// Utiliza motores controlados y un archivo temporal mínimo
/// únicamente para representar un respaldo verificado.
/// Ningún archivo musical real es abierto o modificado.
/// </summary>
public sealed class MetadataWritingStageTestRunner
{
    public async Task<MetadataWritingStageTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        string testDirectoryPath =
            Path.Combine(
                Path.GetTempPath(),
                "AudioMetadataManager",
                "MetadataWritingStageTests",
                Guid.NewGuid().ToString("N"));

        string verifiedBackupPath =
            Path.Combine(
                testDirectoryPath,
                "verified-backup.mp3");

        bool successfulResultWasCompleted =
            false;

        bool noWritableChangesHadWarnings =
            false;

        bool cancelledResultWasCancelled =
            false;

        bool failedResultWasFailed =
            false;

        bool missingBackupWasRejected =
            false;

        bool writerResultsWereStored =
            false;

        bool writeRequestsWereMapped =
            false;

        bool cancellationTokenWasForwarded =
            false;

        bool stageResultsWereAuditable =
            false;

        bool duplicateExecutionWasRejected =
            false;

        bool injectedEngineWasUsed =
            false;

        bool temporaryFilesWereCleaned =
            false;

        try
        {
            Directory.CreateDirectory(
                testDirectoryPath);

            File.WriteAllBytes(
                verifiedBackupPath,
                new byte[]
                {
                    0x41,
                    0x4D,
                    0x4D
                });

            MetadataApplyRequest successfulRequest =
                CreateApplyRequest(
                    "writing-success.mp3");

            ControlledWriterEngine successfulEngine =
                new(
                    MetadataWriteStatus.Completed,
                    "Escritura controlada completada.");

            MetadataWritingStage successfulStage =
                new(
                    successfulEngine);

            using CancellationTokenSource
                cancellationTokenSource =
                    new();

            MetadataApplicationContext successfulContext =
                CreateContextWithVerifiedBackup(
                    successfulRequest,
                    verifiedBackupPath,
                    cancellationTokenSource.Token);

            await successfulStage.ExecuteAsync(
                successfulContext);

            MetadataApplicationStageResult?
                successfulStageResult =
                    successfulContext.StageResults
                        .SingleOrDefault();

            successfulResultWasCompleted =
                successfulStageResult is not null &&
                successfulStageResult.Status ==
                    MetadataApplicationStageStatus.Completed &&
                successfulEngine.LastResult?.WasSuccessful ==
                    true &&
                successfulStageResult.Message ==
                    successfulEngine.LastResult.Summary;

            messages.Add(
                successfulResultWasCompleted
                    ? "La escritura exitosa fue registrada como completada."
                    : "La escritura exitosa no produjo el estado esperado.");

            MetadataApplyRequest noWritableRequest =
                CreateApplyRequest(
                    "writing-no-writable.mp3");

            ControlledWriterEngine noWritableEngine =
                new(
                    MetadataWriteStatus.NoWritableChanges,
                    "Ningún cambio controlado pudo escribirse.");

            MetadataWritingStage noWritableStage =
                new(
                    noWritableEngine);

            MetadataApplicationContext noWritableContext =
                CreateContextWithVerifiedBackup(
                    noWritableRequest,
                    verifiedBackupPath);

            await noWritableStage.ExecuteAsync(
                noWritableContext);

            MetadataApplicationStageResult?
                noWritableStageResult =
                    noWritableContext.StageResults
                        .SingleOrDefault();

            noWritableChangesHadWarnings =
                noWritableStageResult is not null &&
                noWritableStageResult.Status ==
                    MetadataApplicationStageStatus
                        .CompletedWithWarnings &&
                noWritableStageResult.Details.Any(
                    detail =>
                        detail.Contains(
                            "Ningún cambio controlado",
                            StringComparison.Ordinal));

            messages.Add(
                noWritableChangesHadWarnings
                    ? "La ausencia de cambios escribibles produjo una advertencia."
                    : "NoWritableChanges no produjo el estado esperado.");

            MetadataApplyRequest cancelledRequest =
                CreateApplyRequest(
                    "writing-cancelled.mp3");

            ControlledWriterEngine cancelledEngine =
                new(
                    MetadataWriteStatus.Cancelled,
                    "Cancelación controlada de la escritura.");

            MetadataWritingStage cancelledStage =
                new(
                    cancelledEngine);

            MetadataApplicationContext cancelledContext =
                CreateContextWithVerifiedBackup(
                    cancelledRequest,
                    verifiedBackupPath);

            await cancelledStage.ExecuteAsync(
                cancelledContext);

            MetadataApplicationStageResult?
                cancelledStageResult =
                    cancelledContext.StageResults
                        .SingleOrDefault();

            cancelledResultWasCancelled =
                cancelledStageResult is not null &&
                cancelledStageResult.Status ==
                    MetadataApplicationStageStatus.Cancelled &&
                cancelledStageResult.Details.Any(
                    detail =>
                        detail.Contains(
                            "Cancelación controlada",
                            StringComparison.Ordinal));

            messages.Add(
                cancelledResultWasCancelled
                    ? "La cancelación fue registrada correctamente."
                    : "La cancelación no produjo el estado esperado.");

            MetadataApplyRequest failedRequest =
                CreateApplyRequest(
                    "writing-failure.mp3");

            ControlledWriterEngine failedEngine =
                new(
                    MetadataWriteStatus.SaveFailed,
                    "Fallo controlado al guardar metadatos.");

            MetadataWritingStage failedStage =
                new(
                    failedEngine);

            MetadataApplicationContext failedContext =
                CreateContextWithVerifiedBackup(
                    failedRequest,
                    verifiedBackupPath);

            await failedStage.ExecuteAsync(
                failedContext);

            MetadataApplicationStageResult?
                failedStageResult =
                    failedContext.StageResults
                        .SingleOrDefault();

            failedResultWasFailed =
                failedStageResult is not null &&
                failedStageResult.Status ==
                    MetadataApplicationStageStatus.Failed &&
                failedStageResult.Details.Any(
                    detail =>
                        detail.Contains(
                            "Fallo controlado",
                            StringComparison.Ordinal));

            messages.Add(
                failedResultWasFailed
                    ? "El fallo de escritura fue registrado correctamente."
                    : "El fallo de escritura no produjo el estado esperado.");

            MetadataApplyRequest missingBackupRequest =
                CreateApplyRequest(
                    "writing-without-backup.mp3");

            ControlledWriterEngine missingBackupEngine =
                new(
                    MetadataWriteStatus.Completed,
                    "Este motor no debe ejecutarse.");

            MetadataWritingStage missingBackupStage =
                new(
                    missingBackupEngine);

            MetadataApplicationContext missingBackupContext =
                new(
                    missingBackupRequest);

            await missingBackupStage.ExecuteAsync(
                missingBackupContext);

            MetadataApplicationStageResult?
                missingBackupStageResult =
                    missingBackupContext.StageResults
                        .SingleOrDefault();

            missingBackupWasRejected =
                missingBackupStageResult is not null &&
                missingBackupStageResult.Status ==
                    MetadataApplicationStageStatus.Failed &&
                missingBackupContext.WriteResult is null &&
                missingBackupEngine.CallCount == 0 &&
                missingBackupStageResult.Details.Any(
                    detail =>
                        detail.Contains(
                            "no contiene un resultado",
                            StringComparison.OrdinalIgnoreCase));

            messages.Add(
                missingBackupWasRejected
                    ? "La escritura sin respaldo verificado fue rechazada."
                    : "La etapa permitió continuar sin respaldo verificado.");

            writerResultsWereStored =
                ReferenceEquals(
                    successfulContext.WriteResult,
                    successfulEngine.LastResult) &&
                ReferenceEquals(
                    noWritableContext.WriteResult,
                    noWritableEngine.LastResult) &&
                ReferenceEquals(
                    cancelledContext.WriteResult,
                    cancelledEngine.LastResult) &&
                ReferenceEquals(
                    failedContext.WriteResult,
                    failedEngine.LastResult);

            messages.Add(
                writerResultsWereStored
                    ? "Los resultados fueron almacenados en sus contextos."
                    : "Algún resultado no fue almacenado en el contexto.");

            writeRequestsWereMapped =
                RequestWasMapped(
                    successfulEngine.LastRequest,
                    successfulRequest,
                    verifiedBackupPath) &&
                RequestWasMapped(
                    noWritableEngine.LastRequest,
                    noWritableRequest,
                    verifiedBackupPath) &&
                RequestWasMapped(
                    cancelledEngine.LastRequest,
                    cancelledRequest,
                    verifiedBackupPath) &&
                RequestWasMapped(
                    failedEngine.LastRequest,
                    failedRequest,
                    verifiedBackupPath);

            messages.Add(
                writeRequestsWereMapped
                    ? "Las solicitudes de escritura fueron construidas correctamente."
                    : "Alguna solicitud no conserva los datos esperados.");

            cancellationTokenWasForwarded =
                successfulEngine.LastCancellationToken ==
                    cancellationTokenSource.Token;

            messages.Add(
                cancellationTokenWasForwarded
                    ? "El token de cancelación fue entregado al motor."
                    : "El token de cancelación no fue entregado correctamente.");

            stageResultsWereAuditable =
                HasAuditableResult(
                    successfulStageResult) &&
                HasAuditableResult(
                    noWritableStageResult) &&
                HasAuditableResult(
                    cancelledStageResult) &&
                HasAuditableResult(
                    failedStageResult) &&
                HasAuditableResult(
                    missingBackupStageResult) &&
                successfulStage.Stage ==
                    MetadataApplicationStage.MetadataWrite &&
                successfulStage.Name ==
                    "Escritura de metadatos aprobados" &&
                successfulStage.ExecutionOrder ==
                    300;

            messages.Add(
                stageResultsWereAuditable
                    ? "Los resultados conservaron su identidad y tiempos."
                    : "Los datos auditables de la etapa no coinciden.");

            try
            {
                await successfulStage.ExecuteAsync(
                    successfulContext);

                messages.Add(
                    "La segunda ejecución de la etapa fue permitida.");
            }
            catch (InvalidOperationException)
            {
                duplicateExecutionWasRejected =
                    true;

                messages.Add(
                    "La segunda ejecución de la etapa fue rechazada.");
            }

            injectedEngineWasUsed =
                successfulEngine.CallCount == 1 &&
                noWritableEngine.CallCount == 1 &&
                cancelledEngine.CallCount == 1 &&
                failedEngine.CallCount == 1 &&
                missingBackupEngine.CallCount == 0;

            messages.Add(
                injectedEngineWasUsed
                    ? "La etapa utilizó correctamente los motores controlados."
                    : "La delegación a los motores no fue la esperada.");
        }
        finally
        {
            temporaryFilesWereCleaned =
                TryDeleteDirectory(
                    testDirectoryPath);
        }

        messages.Add(
            temporaryFilesWereCleaned
                ? "Los archivos temporales fueron eliminados."
                : "No fue posible eliminar todos los archivos temporales.");

        return new MetadataWritingStageTestResult
        {
            SuccessfulResultWasCompleted =
                successfulResultWasCompleted,

            NoWritableChangesHadWarnings =
                noWritableChangesHadWarnings,

            CancelledResultWasCancelled =
                cancelledResultWasCancelled,

            FailedResultWasFailed =
                failedResultWasFailed,

            MissingBackupWasRejected =
                missingBackupWasRejected,

            WriterResultsWereStored =
                writerResultsWereStored,

            WriteRequestsWereMapped =
                writeRequestsWereMapped,

            CancellationTokenWasForwarded =
                cancellationTokenWasForwarded,

            StageResultsWereAuditable =
                stageResultsWereAuditable,

            DuplicateExecutionWasRejected =
                duplicateExecutionWasRejected,

            InjectedEngineWasUsed =
                injectedEngineWasUsed,

            TemporaryFilesWereCleaned =
                temporaryFilesWereCleaned,

            Messages =
                messages.ToArray()
        };
    }

    private static MetadataApplicationContext
        CreateContextWithVerifiedBackup(
            MetadataApplyRequest applyRequest,
            string verifiedBackupPath,
            CancellationToken cancellationToken = default)
    {
        MetadataApplicationContext context =
            new(
                applyRequest,
                cancellationToken);

        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        context.SetBackupResult(
            new MetadataBackupResult
            {
                BackupRequestId =
                    Guid.NewGuid(),

                ApplyRequestId =
                    applyRequest.RequestId,

                PlanId =
                    applyRequest.PlanId,

                Status =
                    MetadataBackupStatus.Completed,

                SourceFilePath =
                    applyRequest.FilePath,

                BackupFilePath =
                    verifiedBackupPath,

                BackupDirectoryPath =
                    Path.GetDirectoryName(
                        verifiedBackupPath) ??
                    string.Empty,

                StartedAtUtc =
                    now,

                CompletedAtUtc =
                    now,

                ElapsedTime =
                    TimeSpan.Zero,

                SourceFileSizeBytes =
                    3,

                BackupFileSizeBytes =
                    3,

                SourceHash =
                    "CONTROLLED_HASH",

                BackupHash =
                    "CONTROLLED_HASH",

                HashAlgorithmName =
                    "SHA-256",

                FileSizeVerified =
                    true,

                HashVerified =
                    true,

                Messages =
                    new[]
                    {
                        "Respaldo controlado verificado."
                    }
            });

        return context;
    }

    private static bool RequestWasMapped(
        MetadataWriteRequest? writeRequest,
        MetadataApplyRequest applyRequest,
        string verifiedBackupPath)
    {
        IReadOnlyList<MetadataFieldChange>
            expectedChanges =
                applyRequest.ValidChanges;

        return
            writeRequest is not null &&
            writeRequest.ApplyRequestId ==
                applyRequest.RequestId &&
            writeRequest.PlanId ==
                applyRequest.PlanId &&
            writeRequest.FilePath ==
                applyRequest.FilePath &&
            writeRequest.FileName ==
                applyRequest.FileName &&
            writeRequest.VerifiedBackupPath ==
                verifiedBackupPath &&
            writeRequest.PreserveUnchangedMetadata &&
            writeRequest.PreserveEmbeddedPictures &&
            writeRequest.PreserveUnknownMetadata &&
            writeRequest.Changes.Count ==
                expectedChanges.Count &&
            writeRequest.Changes
                .Select(change => change.Field)
                .SequenceEqual(
                    expectedChanges.Select(
                        change => change.Field)) &&
            writeRequest.Changes
                .Select(change => change.NewValue)
                .SequenceEqual(
                    expectedChanges.Select(
                        change => change.NewValue));
    }

    private static bool HasAuditableResult(
        MetadataApplicationStageResult? result)
    {
        return
            result is not null &&
            result.Stage ==
                MetadataApplicationStage.MetadataWrite &&
            result.StartedAtUtc != default &&
            result.CompletedAtUtc != default &&
            result.CompletedAtUtc >=
                result.StartedAtUtc &&
            result.ElapsedTime >=
                TimeSpan.Zero;
    }

    private static MetadataApplyRequest CreateApplyRequest(
        string fileName)
    {
        return new MetadataApplyRequest
        {
            PlanId =
                Guid.NewGuid(),

            FilePath =
                Path.Combine(
                    @"Z:\AudioMetadataManager.StructuralTests",
                    fileName),

            FileName =
                fileName,

            Changes =
                new[]
                {
                    new MetadataFieldChange
                    {
                        Field =
                            MetadataField.Artist,

                        OriginalValue =
                            "Artista original",

                        NewValue =
                            "Artista aprobado",

                        WasManuallyApproved =
                            true,

                        Confidence =
                            1.0
                    },

                    new MetadataFieldChange
                    {
                        Field =
                            MetadataField.Title,

                        OriginalValue =
                            "Título sin cambio",

                        NewValue =
                            "Título sin cambio",

                        WasManuallyApproved =
                            true,

                        Confidence =
                            1.0
                    }
                }
        };
    }

    private static bool TryDeleteDirectory(
        string directoryPath)
    {
        try
        {
            if (Directory.Exists(
                    directoryPath))
            {
                Directory.Delete(
                    directoryPath,
                    recursive: true);
            }

            return !Directory.Exists(
                directoryPath);
        }
        catch
        {
            return false;
        }
    }

    private sealed class ControlledWriterEngine :
        IMetadataWriterEngine
    {
        private readonly MetadataWriteStatus
            _status;

        private readonly string
            _message;

        public ControlledWriterEngine(
            MetadataWriteStatus status,
            string message)
        {
            _status =
                status;

            _message =
                message;
        }

        public int CallCount { get; private set; }

        public MetadataWriteRequest? LastRequest
        { get; private set; }

        public MetadataWriteResult? LastResult
        { get; private set; }

        public CancellationToken LastCancellationToken
        { get; private set; }

        public Task<MetadataWriteResult> WriteAsync(
            MetadataWriteRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            CallCount++;

            LastRequest =
                request;

            LastCancellationToken =
                cancellationToken;

            DateTimeOffset now =
                DateTimeOffset.UtcNow;

            IReadOnlyList<MetadataFieldWriteResult>
                fieldResults =
                    _status ==
                    MetadataWriteStatus.Completed
                        ? new[]
                        {
                            new MetadataFieldWriteResult
                            {
                                Field =
                                    MetadataField.Artist,

                                OriginalValue =
                                    "Artista original",

                                RequestedValue =
                                    "Artista aprobado",

                                IsSupported =
                                    true,

                                ValuePrepared =
                                    true,

                                SaveSucceeded =
                                    true,

                                Message =
                                    _message
                            }
                        }
                        : Array.Empty<
                            MetadataFieldWriteResult>();

            LastResult =
                new MetadataWriteResult
                {
                    WriteRequestId =
                        request.WriteRequestId,

                    ApplyRequestId =
                        request.ApplyRequestId,

                    PlanId =
                        request.PlanId,

                    Status =
                        _status,

                    FilePath =
                        request.FilePath,

                    WriterName =
                        "ControlledWriter",

                    StartedAtUtc =
                        now,

                    CompletedAtUtc =
                        now,

                    ElapsedTime =
                        TimeSpan.Zero,

                    FieldResults =
                        fieldResults,

                    Messages =
                        new[]
                        {
                            _message
                        }
                };

            return Task.FromResult(
                LastResult);
        }
    }
}