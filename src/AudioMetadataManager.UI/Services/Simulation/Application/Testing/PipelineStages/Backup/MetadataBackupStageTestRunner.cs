using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Engine;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Backup;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Backup;

/// <summary>
/// Ejecuta pruebas estructurales sobre la etapa concreta de
/// respaldo.
///
/// Utiliza motores controlados. Sólo crea un archivo temporal
/// mínimo para representar un respaldo exitoso y lo elimina al
/// finalizar.
/// </summary>
public sealed class MetadataBackupStageTestRunner
{
    public async Task<MetadataBackupStageTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        string testDirectoryPath =
            Path.Combine(
                Path.GetTempPath(),
                "AudioMetadataManager",
                "MetadataBackupStageTests",
                Guid.NewGuid().ToString("N"));

        string successfulBackupPath =
            Path.Combine(
                testDirectoryPath,
                "controlled-backup.mp3");

        bool successfulResultWasCompleted =
            false;

        bool failedResultWasFailed =
            false;

        bool cancelledResultWasCancelled =
            false;

        bool backupResultsWereStored =
            false;

        bool backupRequestsWereMapped =
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
                successfulBackupPath,
                new byte[]
                {
                    0x41,
                    0x4D,
                    0x4D
                });

            MetadataApplyRequest successfulRequest =
                CreateApplyRequest(
                    "backup-success.mp3");

            MetadataBackupResult successfulBackupResult =
                CreateBackupResult(
                    successfulRequest,
                    MetadataBackupStatus.Completed,
                    successfulBackupPath,
                    fileSizeVerified: true,
                    hashVerified: true,
                    "Respaldo controlado completado.");

            ControlledBackupEngine successfulEngine =
                new(
                    successfulBackupResult);

            MetadataBackupStage successfulStage =
                new(
                    successfulEngine);

            using CancellationTokenSource
                cancellationTokenSource =
                    new();

            MetadataApplicationContext successfulContext =
                new(
                    successfulRequest,
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
                successfulStageResult.Message ==
                    successfulBackupResult.Summary;

            messages.Add(
                successfulResultWasCompleted
                    ? "El respaldo exitoso fue registrado como completado."
                    : "El respaldo exitoso no produjo el estado esperado.");

            MetadataApplyRequest failedRequest =
                CreateApplyRequest(
                    "backup-failure.mp3");

            MetadataBackupResult failedBackupResult =
                CreateBackupResult(
                    failedRequest,
                    MetadataBackupStatus.VerificationFailed,
                    string.Empty,
                    fileSizeVerified: false,
                    hashVerified: false,
                    "Fallo controlado de verificación.");

            ControlledBackupEngine failedEngine =
                new(
                    failedBackupResult);

            MetadataBackupStage failedStage =
                new(
                    failedEngine);

            MetadataApplicationContext failedContext =
                new(
                    failedRequest);

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
                    ? "El fallo del respaldo fue registrado correctamente."
                    : "El fallo del respaldo no produjo el estado esperado.");

            MetadataApplyRequest cancelledRequest =
                CreateApplyRequest(
                    "backup-cancelled.mp3");

            MetadataBackupResult cancelledBackupResult =
                CreateBackupResult(
                    cancelledRequest,
                    MetadataBackupStatus.Cancelled,
                    string.Empty,
                    fileSizeVerified: false,
                    hashVerified: false,
                    "Cancelación controlada del respaldo.");

            ControlledBackupEngine cancelledEngine =
                new(
                    cancelledBackupResult);

            MetadataBackupStage cancelledStage =
                new(
                    cancelledEngine);

            MetadataApplicationContext cancelledContext =
                new(
                    cancelledRequest);

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

            backupResultsWereStored =
                ReferenceEquals(
                    successfulContext.BackupResult,
                    successfulBackupResult) &&
                ReferenceEquals(
                    failedContext.BackupResult,
                    failedBackupResult) &&
                ReferenceEquals(
                    cancelledContext.BackupResult,
                    cancelledBackupResult);

            messages.Add(
                backupResultsWereStored
                    ? "Los resultados fueron almacenados en sus contextos."
                    : "Algún resultado no fue almacenado en el contexto.");

            backupRequestsWereMapped =
                RequestWasMapped(
                    successfulEngine.LastRequest,
                    successfulRequest) &&
                RequestWasMapped(
                    failedEngine.LastRequest,
                    failedRequest) &&
                RequestWasMapped(
                    cancelledEngine.LastRequest,
                    cancelledRequest);

            messages.Add(
                backupRequestsWereMapped
                    ? "Las solicitudes de respaldo fueron construidas correctamente."
                    : "Alguna solicitud de respaldo no conserva los datos esperados.");

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
                    failedStageResult) &&
                HasAuditableResult(
                    cancelledStageResult) &&
                successfulStage.Stage ==
                    MetadataApplicationStage.Backup &&
                successfulStage.Name ==
                    "Creación y verificación de respaldo" &&
                successfulStage.ExecutionOrder ==
                    200;

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
                failedEngine.CallCount == 1 &&
                cancelledEngine.CallCount == 1 &&
                successfulEngine.LastProgress is null &&
                failedEngine.LastProgress is null &&
                cancelledEngine.LastProgress is null;

            messages.Add(
                injectedEngineWasUsed
                    ? "La etapa delegó una vez en cada motor controlado."
                    : "La delegación al motor no fue la esperada.");
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

        return new MetadataBackupStageTestResult
        {
            SuccessfulResultWasCompleted =
                successfulResultWasCompleted,

            FailedResultWasFailed =
                failedResultWasFailed,

            CancelledResultWasCancelled =
                cancelledResultWasCancelled,

            BackupResultsWereStored =
                backupResultsWereStored,

            BackupRequestsWereMapped =
                backupRequestsWereMapped,

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

    private static bool RequestWasMapped(
        MetadataBackupRequest? backupRequest,
        MetadataApplyRequest applyRequest)
    {
        return
            backupRequest is not null &&
            backupRequest.ApplyRequestId ==
                applyRequest.RequestId &&
            backupRequest.PlanId ==
                applyRequest.PlanId &&
            backupRequest.SourceFilePath ==
                applyRequest.FilePath &&
            backupRequest.FileName ==
                applyRequest.FileName;
    }

    private static bool HasAuditableResult(
        MetadataApplicationStageResult? result)
    {
        return
            result is not null &&
            result.Stage ==
                MetadataApplicationStage.Backup &&
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
                Array.Empty<MetadataFieldChange>()
        };
    }

    private static MetadataBackupResult CreateBackupResult(
        MetadataApplyRequest applyRequest,
        MetadataBackupStatus status,
        string backupFilePath,
        bool fileSizeVerified,
        bool hashVerified,
        string message)
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        return new MetadataBackupResult
        {
            BackupRequestId =
                Guid.NewGuid(),

            ApplyRequestId =
                applyRequest.RequestId,

            PlanId =
                applyRequest.PlanId,

            Status =
                status,

            SourceFilePath =
                applyRequest.FilePath,

            BackupFilePath =
                backupFilePath,

            BackupDirectoryPath =
                string.IsNullOrWhiteSpace(
                    backupFilePath)
                        ? string.Empty
                        : Path.GetDirectoryName(
                            backupFilePath) ??
                          string.Empty,

            StartedAtUtc =
                now,

            CompletedAtUtc =
                now,

            ElapsedTime =
                TimeSpan.Zero,

            SourceFileSizeBytes =
                fileSizeVerified
                    ? 3
                    : -1,

            BackupFileSizeBytes =
                fileSizeVerified
                    ? 3
                    : -1,

            SourceHash =
                hashVerified
                    ? "CONTROLLED_HASH"
                    : string.Empty,

            BackupHash =
                hashVerified
                    ? "CONTROLLED_HASH"
                    : string.Empty,

            HashAlgorithmName =
                hashVerified
                    ? "SHA-256"
                    : string.Empty,

            FileSizeVerified =
                fileSizeVerified,

            HashVerified =
                hashVerified,

            Messages =
                new[]
                {
                    message
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

    private sealed class ControlledBackupEngine :
        IMetadataBackupEngine
    {
        private readonly MetadataBackupResult
            _result;

        public ControlledBackupEngine(
            MetadataBackupResult result)
        {
            _result =
                result ??
                throw new ArgumentNullException(
                    nameof(result));
        }

        public int CallCount { get; private set; }

        public MetadataBackupRequest? LastRequest
        { get; private set; }

        public IProgress<MetadataBackupProgress>? LastProgress
        { get; private set; }

        public CancellationToken LastCancellationToken
        { get; private set; }

        public Task<MetadataBackupResult> CreateBackupAsync(
            MetadataBackupRequest request,
            IProgress<MetadataBackupProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            CallCount++;

            LastRequest =
                request;

            LastProgress =
                progress;

            LastCancellationToken =
                cancellationToken;

            return Task.FromResult(
                _result);
        }
    }
}