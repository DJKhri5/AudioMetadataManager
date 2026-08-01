using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Composition;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;
using AudioMetadataManager.UI.Services.Simulation.Application.Pipeline.Integration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;
using System.Diagnostics;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineExecution;

/// <summary>
/// Ejecuta el pipeline completo sobre una copia aislada.
///
/// El archivo original nunca se entrega a ninguna etapa de
/// escritura.
/// </summary>
public sealed class
    MetadataApplicationPipelineIsolatedTestRunner
{
    private readonly FileIsolationTestHarness
        _isolationHarness;

    /// <summary>
    /// Crea el runner con la infraestructura predeterminada.
    /// </summary>
    public MetadataApplicationPipelineIsolatedTestRunner()
        : this(
            new FileIsolationTestHarness())
    {
    }

    /// <summary>
    /// Crea el runner con una infraestructura de aislamiento
    /// proporcionada.
    /// </summary>
    public MetadataApplicationPipelineIsolatedTestRunner(
        FileIsolationTestHarness isolationHarness)
    {
        _isolationHarness =
            isolationHarness ??
            throw new ArgumentNullException(
                nameof(isolationHarness));
    }

    /// <summary>
    /// Ejecuta las cinco etapas reales del pipeline sobre una
    /// copia temporal del archivo indicado.
    /// </summary>
    public async
        Task<MetadataApplicationPipelineIsolatedTestResult>
        RunAsync(
            string? originalFilePath,
            string requestedGenre = "Electronic",
            CancellationToken cancellationToken = default)
    {
        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        List<string> messages =
            new();

        string normalizedOriginalPath =
            NormalizePath(
                originalFilePath);

        string effectiveRequestedGenre =
            NormalizeValue(
                requestedGenre);

        FileIsolationContext? isolationContext =
            null;

        FileIsolationVerificationResult?
            isolationVerification =
                null;

        MetadataApplicationPipelineExecutionResult?
            pipelineExecutionResult =
                null;

        string originalGenre =
            string.Empty;

        string persistedGenre =
            string.Empty;

        string pipelineBackupFilePath =
            string.Empty;

        string pipelineBackupHash =
            string.Empty;

        int pictureCountBefore =
            0;

        int pictureCountAfter =
            0;

        bool pipelineBackupWasSuccessfulBeforeCleanup =
            false;

        bool cleanupWasAttempted =
            false;

        bool testDirectoryWasRemoved =
            false;

        string errorMessage =
            string.Empty;

        string exceptionType =
            string.Empty;

        try
        {
            isolationContext =
                await _isolationHarness.CreateAsync(
                    normalizedOriginalPath,
                    "MetadataApplicationPipeline",
                    cancellationToken);

            messages.Add(
                "El entorno aislado fue preparado correctamente.");

            using (TagLib.File tagFile =
                TagLib.File.Create(
                    isolationContext.WorkingCopyPath))
            {
                originalGenre =
                    JoinValues(
                        tagFile.Tag.Genres);
            }

            if (string.IsNullOrWhiteSpace(
                    effectiveRequestedGenre))
            {
                effectiveRequestedGenre =
                    "Electronic";
            }

            /*
             * La prueba necesita un cambio real. Si el archivo ya
             * contiene exactamente el género solicitado, se utiliza
             * un valor inequívocamente diferente.
             */
            if (string.Equals(
                    NormalizeValue(originalGenre),
                    effectiveRequestedGenre,
                    StringComparison.OrdinalIgnoreCase))
            {
                effectiveRequestedGenre +=
                    " (Pipeline Test)";
            }

            MetadataFieldChange genreChange =
                new()
                {
                    Field =
                        MetadataField.Genre,

                    OriginalValue =
                        originalGenre,

                    NewValue =
                        effectiveRequestedGenre,

                    WasManuallyApproved =
                        true,

                    Confidence =
                        1.0,

                    SupportingSources =
                        new[]
                        {
                            "Prueba integral aislada del pipeline"
                        }
                };

            MetadataApplyRequest applyRequest =
                new()
                {
                    RequestId =
                        Guid.NewGuid(),

                    PlanId =
                        Guid.NewGuid(),

                    FilePath =
                        isolationContext.WorkingCopyPath,

                    FileName =
                        Path.GetFileName(
                            isolationContext.WorkingCopyPath),

                    Changes =
                        new[]
                        {
                            genreChange
                        },

                    RequireBackup =
                        true,

                    RequirePostWriteVerification =
                        true
                };

            MetadataApplicationContext applicationContext =
                new(
                    applyRequest,
                    cancellationToken);

            MetadataApplicationPipelineExecutor executor =
                MetadataApplicationPipelineFactory
                    .CreateDefault();

            pipelineExecutionResult =
                await executor.ExecuteAsync(
                    applicationContext);

            /*
             * Estas propiedades dinámicas deben capturarse antes
             * de eliminar el entorno temporal.
             */
            pipelineBackupWasSuccessfulBeforeCleanup =
                applicationContext.BackupResult
                    ?.WasSuccessful ==
                true;

            pipelineBackupFilePath =
                applicationContext.BackupResult
                    ?.BackupFilePath ??
                string.Empty;

            pipelineBackupHash =
                applicationContext.BackupResult
                    ?.BackupHash ??
                string.Empty;

            pictureCountBefore =
                applicationContext.WriteResult
                    ?.PictureCountBefore ??
                0;

            pictureCountAfter =
                applicationContext.VerificationResult
                    ?.PictureCountAfter ??
                0;

            MetadataFieldVerificationResult?
                genreVerification =
                    applicationContext.VerificationResult
                        ?.FieldResults
                        .SingleOrDefault(
                            result =>
                                result.Field ==
                                MetadataField.Genre);

            persistedGenre =
                genreVerification?.PersistedValue ??
                string.Empty;

            messages.Add(
                pipelineExecutionResult.Summary);

            if (applicationContext.BackupResult is not null)
            {
                messages.Add(
                    applicationContext.BackupResult.Summary);
            }

            if (applicationContext.WriteResult is not null)
            {
                messages.Add(
                    applicationContext.WriteResult.Summary);
            }

            if (applicationContext.VerificationResult is not null)
            {
                messages.Add(
                    applicationContext.VerificationResult.Summary);
            }

            isolationVerification =
                await _isolationHarness.VerifyAsync(
                    isolationContext,
                    CancellationToken.None);

            messages.AddRange(
                isolationVerification.Messages);
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;

            exceptionType =
                exception.GetType().FullName ??
                exception.GetType().Name;

            messages.Add(
                $"La prueba integral terminó con un error: " +
                $"{exception.Message}");

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
                        "No fue posible verificar el entorno " +
                        "después del error: " +
                        verificationException.Message);
                }
            }
        }
        finally
        {
            if (isolationContext is not null)
            {
                cleanupWasAttempted =
                    true;

                bool harnessReportedCleanup =
                    _isolationHarness.TryCleanup(
                        isolationContext);

                testDirectoryWasRemoved =
                    harnessReportedCleanup &&
                    !Directory.Exists(
                        isolationContext.TestDirectoryPath);
            }

            stopwatch.Stop();
        }

        MetadataApplicationContext?
            completedContext =
                pipelineExecutionResult?.Context;

        MetadataFieldVerificationResult?
            completedGenreVerification =
                completedContext?.VerificationResult
                    ?.FieldResults
                    .SingleOrDefault(
                        result =>
                            result.Field ==
                            MetadataField.Genre);

        return new
            MetadataApplicationPipelineIsolatedTestResult
        {
            OriginalFilePath =
                    isolationContext?.OriginalFilePath ??
                    normalizedOriginalPath,

            WorkingCopyPath =
                    isolationContext?.WorkingCopyPath ??
                    string.Empty,

            WorkingBackupPath =
                    isolationContext?.WorkingBackupPath ??
                    string.Empty,

            PipelineBackupFilePath =
                    pipelineBackupFilePath,

            TestDirectoryPath =
                    isolationContext?.TestDirectoryPath ??
                    string.Empty,

            OriginalGenre =
                    originalGenre,

            RequestedGenre =
                    effectiveRequestedGenre,

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

            PipelineBackupHash =
                    pipelineBackupHash,

            PipelineExecutionResult =
                    pipelineExecutionResult,

            EnvironmentWasPrepared =
                    isolationContext?.IsCreated == true &&
                    isolationContext
                        .BackupMatchesInitialWorkingCopy,

            RegisteredStageCount =
                    pipelineExecutionResult
                        ?.RegisteredStageCount ??
                    0,

            ExecutedStageCount =
                    pipelineExecutionResult
                        ?.ExecutedStageCount ??
                    0,

            PipelineExecutionWasSuccessful =
                    pipelineExecutionResult
                        ?.ExecutionWasSuccessful ==
                    true,

            WriteWasSuccessful =
                    completedContext?.WriteResult
                        ?.WasSuccessful ==
                    true,

            VerificationWasSuccessful =
                    completedContext?.VerificationResult
                        ?.WasSuccessful ==
                    true,

            GenreVerificationWasSuccessful =
                    completedGenreVerification
                        ?.WasSuccessful ==
                    true,

            PipelineBackupWasSuccessfulBeforeCleanup =
                    pipelineBackupWasSuccessfulBeforeCleanup,

            CleanupWasAttempted =
                    cleanupWasAttempted,

            TestDirectoryWasRemoved =
                    testDirectoryWasRemoved,

            StartedAtUtc =
                    startedAtUtc,

            CompletedAtUtc =
                    DateTimeOffset.UtcNow,

            ElapsedTime =
                    stopwatch.Elapsed,

            ErrorMessage =
                    errorMessage,

            ExceptionType =
                    exceptionType,

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