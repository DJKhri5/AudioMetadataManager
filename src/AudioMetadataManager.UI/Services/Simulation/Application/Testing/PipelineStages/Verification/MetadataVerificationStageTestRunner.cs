using System.IO;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Verification;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Verification;

/// <summary>
/// Ejecuta pruebas estructurales sobre la etapa concreta de
/// verificación posterior a la escritura.
///
/// Utiliza motores controlados y no abre ni modifica archivos
/// musicales reales.
/// </summary>
public sealed class MetadataVerificationStageTestRunner
{
    public async Task<MetadataVerificationStageTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        bool successfulVerificationWasCompleted =
            false;

        bool failedVerificationWasFailed =
            false;

        bool missingWriteResultWasRejected =
            false;

        bool noWritableChangesWasSkipped =
            false;

        bool cancelledWriteWasCancelled =
            false;

        bool failedWriteWasRejected =
            false;

        bool verificationResultWasStored =
            false;

        bool verificationInputsWereMapped =
            false;

        bool pictureCountBeforeWasForwarded =
            false;

        bool cancellationWasHonored =
            false;

        bool stageResultsWereAuditable =
            false;

        bool duplicateExecutionWasRejected =
            false;

        bool injectedEngineWasUsed =
            false;

        const int pictureCountBefore =
            4;

        MetadataApplyRequest successfulRequest =
            CreateApplyRequest(
                "verification-success.mp3");

        MetadataWriteResult successfulWriteResult =
            CreateWriteResult(
                successfulRequest,
                MetadataWriteStatus.Completed,
                pictureCountBefore,
                "Escritura controlada completada.");

        MetadataVerificationResult
            successfulVerificationResult =
                CreateVerificationResult(
                    successfulRequest.FilePath,
                    pictureCountBefore,
                    wasSuccessful:
                        true);

        ControlledMetadataWriterVerificationEngine
            successfulEngine =
                new(
                    successfulVerificationResult);

        MetadataVerificationStage successfulStage =
            new(
                successfulEngine);

        MetadataApplicationContext successfulContext =
            CreateContextWithWriteResult(
                successfulRequest,
                successfulWriteResult);

        await successfulStage.ExecuteAsync(
            successfulContext);

        MetadataApplicationStageResult?
            successfulStageResult =
                successfulContext.StageResults
                    .SingleOrDefault();

        successfulVerificationWasCompleted =
            successfulStageResult is not null &&
            successfulStageResult.Status ==
                MetadataApplicationStageStatus.Completed &&
            successfulVerificationResult.WasSuccessful &&
            successfulStageResult.Message ==
                successfulVerificationResult.Summary;

        messages.Add(
            successfulVerificationWasCompleted
                ? "La verificación exitosa fue registrada como completada."
                : "La verificación exitosa no produjo el estado esperado.");

        MetadataApplyRequest failedVerificationRequest =
            CreateApplyRequest(
                "verification-failure.mp3");

        MetadataWriteResult
            failedVerificationWriteResult =
                CreateWriteResult(
                    failedVerificationRequest,
                    MetadataWriteStatus.Completed,
                    pictureCountBefore,
                    "Escritura previa completada.");

        MetadataVerificationResult
            failedVerificationResult =
                CreateVerificationResult(
                    failedVerificationRequest.FilePath,
                    pictureCountBefore,
                    wasSuccessful:
                        false);

        ControlledMetadataWriterVerificationEngine
            failedVerificationEngine =
                new(
                    failedVerificationResult);

        MetadataVerificationStage failedVerificationStage =
            new(
                failedVerificationEngine);

        MetadataApplicationContext
            failedVerificationContext =
                CreateContextWithWriteResult(
                    failedVerificationRequest,
                    failedVerificationWriteResult);

        await failedVerificationStage.ExecuteAsync(
            failedVerificationContext);

        MetadataApplicationStageResult?
            failedVerificationStageResult =
                failedVerificationContext.StageResults
                    .SingleOrDefault();

        failedVerificationWasFailed =
            failedVerificationStageResult is not null &&
            failedVerificationStageResult.Status ==
                MetadataApplicationStageStatus.Failed &&
            !failedVerificationResult.WasSuccessful &&
            failedVerificationStageResult.Details.Any(
                detail =>
                    detail.Contains(
                        "no coincide",
                        StringComparison.OrdinalIgnoreCase));

        messages.Add(
            failedVerificationWasFailed
                ? "El fallo de verificación fue registrado correctamente."
                : "El fallo de verificación no produjo el estado esperado.");

        MetadataApplyRequest missingWriteRequest =
            CreateApplyRequest(
                "verification-without-write.mp3");

        ControlledMetadataWriterVerificationEngine
            missingWriteEngine =
                new(
                    CreateVerificationResult(
                        missingWriteRequest.FilePath,
                        pictureCountBefore,
                        wasSuccessful:
                            true));

        MetadataVerificationStage missingWriteStage =
            new(
                missingWriteEngine);

        MetadataApplicationContext missingWriteContext =
            new(
                missingWriteRequest);

        await missingWriteStage.ExecuteAsync(
            missingWriteContext);

        MetadataApplicationStageResult?
            missingWriteStageResult =
                missingWriteContext.StageResults
                    .SingleOrDefault();

        missingWriteResultWasRejected =
            missingWriteStageResult is not null &&
            missingWriteStageResult.Status ==
                MetadataApplicationStageStatus.Failed &&
            missingWriteContext.VerificationResult is null &&
            missingWriteEngine.CallCount == 0 &&
            missingWriteStageResult.Details.Any(
                detail =>
                    detail.Contains(
                        "no contiene un resultado",
                        StringComparison.OrdinalIgnoreCase));

        messages.Add(
            missingWriteResultWasRejected
                ? "La verificación sin resultado de escritura fue rechazada."
                : "La etapa permitió verificar sin resultado de escritura.");

        MetadataApplyRequest noWritableRequest =
            CreateApplyRequest(
                "verification-no-writable.mp3");

        MetadataWriteResult noWritableWriteResult =
            CreateWriteResult(
                noWritableRequest,
                MetadataWriteStatus.NoWritableChanges,
                pictureCountBefore,
                "No existieron cambios escribibles.");

        ControlledMetadataWriterVerificationEngine
            noWritableEngine =
                new(
                    CreateVerificationResult(
                        noWritableRequest.FilePath,
                        pictureCountBefore,
                        wasSuccessful:
                            true));

        MetadataVerificationStage noWritableStage =
            new(
                noWritableEngine);

        MetadataApplicationContext noWritableContext =
            CreateContextWithWriteResult(
                noWritableRequest,
                noWritableWriteResult);

        await noWritableStage.ExecuteAsync(
            noWritableContext);

        MetadataApplicationStageResult?
            noWritableStageResult =
                noWritableContext.StageResults
                    .SingleOrDefault();

        noWritableChangesWasSkipped =
            noWritableStageResult is not null &&
            noWritableStageResult.Status ==
                MetadataApplicationStageStatus.Skipped &&
            noWritableContext.VerificationResult is null &&
            noWritableEngine.CallCount == 0;

        messages.Add(
            noWritableChangesWasSkipped
                ? "La ausencia de cambios escritos omitió la verificación."
                : "NoWritableChanges no produjo el estado esperado.");

        MetadataApplyRequest cancelledWriteRequest =
            CreateApplyRequest(
                "verification-cancelled-write.mp3");

        MetadataWriteResult cancelledWriteResult =
            CreateWriteResult(
                cancelledWriteRequest,
                MetadataWriteStatus.Cancelled,
                pictureCountBefore,
                "La escritura previa fue cancelada.");

        ControlledMetadataWriterVerificationEngine
            cancelledWriteEngine =
                new(
                    CreateVerificationResult(
                        cancelledWriteRequest.FilePath,
                        pictureCountBefore,
                        wasSuccessful:
                            true));

        MetadataVerificationStage cancelledWriteStage =
            new(
                cancelledWriteEngine);

        MetadataApplicationContext cancelledWriteContext =
            CreateContextWithWriteResult(
                cancelledWriteRequest,
                cancelledWriteResult);

        await cancelledWriteStage.ExecuteAsync(
            cancelledWriteContext);

        MetadataApplicationStageResult?
            cancelledWriteStageResult =
                cancelledWriteContext.StageResults
                    .SingleOrDefault();

        cancelledWriteWasCancelled =
            cancelledWriteStageResult is not null &&
            cancelledWriteStageResult.Status ==
                MetadataApplicationStageStatus.Cancelled &&
            cancelledWriteContext.VerificationResult is null &&
            cancelledWriteEngine.CallCount == 0;

        messages.Add(
            cancelledWriteWasCancelled
                ? "La escritura cancelada impidió correctamente la verificación."
                : "La cancelación previa no produjo el estado esperado.");

        MetadataApplyRequest failedWriteRequest =
            CreateApplyRequest(
                "verification-failed-write.mp3");

        MetadataWriteResult failedWriteResult =
            CreateWriteResult(
                failedWriteRequest,
                MetadataWriteStatus.SaveFailed,
                pictureCountBefore,
                "La escritura previa terminó con un fallo.");

        ControlledMetadataWriterVerificationEngine
            failedWriteEngine =
                new(
                    CreateVerificationResult(
                        failedWriteRequest.FilePath,
                        pictureCountBefore,
                        wasSuccessful:
                            true));

        MetadataVerificationStage failedWriteStage =
            new(
                failedWriteEngine);

        MetadataApplicationContext failedWriteContext =
            CreateContextWithWriteResult(
                failedWriteRequest,
                failedWriteResult);

        await failedWriteStage.ExecuteAsync(
            failedWriteContext);

        MetadataApplicationStageResult?
            failedWriteStageResult =
                failedWriteContext.StageResults
                    .SingleOrDefault();

        failedWriteWasRejected =
            failedWriteStageResult is not null &&
            failedWriteStageResult.Status ==
                MetadataApplicationStageStatus.Failed &&
            failedWriteContext.VerificationResult is null &&
            failedWriteEngine.CallCount == 0 &&
            failedWriteStageResult.Details.Any(
                detail =>
                    detail.Contains(
                        "SaveFailed",
                        StringComparison.OrdinalIgnoreCase));

        messages.Add(
            failedWriteWasRejected
                ? "La escritura fallida fue rechazada antes de verificar."
                : "La etapa permitió verificar una escritura fallida.");

        verificationResultWasStored =
            ReferenceEquals(
                successfulContext.VerificationResult,
                successfulVerificationResult) &&
            ReferenceEquals(
                failedVerificationContext.VerificationResult,
                failedVerificationResult);

        messages.Add(
            verificationResultWasStored
                ? "Los resultados de verificación fueron almacenados."
                : "Algún resultado no fue almacenado en el contexto.");

        verificationInputsWereMapped =
            VerificationInputsWereMapped(
                successfulEngine,
                successfulRequest) &&
            VerificationInputsWereMapped(
                failedVerificationEngine,
                failedVerificationRequest);

        messages.Add(
            verificationInputsWereMapped
                ? "La ruta y los cambios fueron trasladados correctamente."
                : "Alguna entrada de verificación no coincide.");

        pictureCountBeforeWasForwarded =
            successfulEngine.LastPictureCountBefore ==
                pictureCountBefore &&
            failedVerificationEngine.LastPictureCountBefore ==
                pictureCountBefore;

        messages.Add(
            pictureCountBeforeWasForwarded
                ? "El conteo previo de imágenes fue conservado."
                : "El conteo previo de imágenes no fue trasladado.");

        MetadataApplyRequest cancellationRequest =
            CreateApplyRequest(
                "verification-token-cancelled.mp3");

        MetadataWriteResult cancellationWriteResult =
            CreateWriteResult(
                cancellationRequest,
                MetadataWriteStatus.Completed,
                pictureCountBefore,
                "Escritura previa completada.");

        ControlledMetadataWriterVerificationEngine
            cancellationEngine =
                new(
                    CreateVerificationResult(
                        cancellationRequest.FilePath,
                        pictureCountBefore,
                        wasSuccessful:
                            true));

        MetadataVerificationStage cancellationStage =
            new(
                cancellationEngine);

        using CancellationTokenSource
            cancellationTokenSource =
                new();

        cancellationTokenSource.Cancel();

        MetadataApplicationContext cancellationContext =
            CreateContextWithWriteResult(
                cancellationRequest,
                cancellationWriteResult,
                cancellationTokenSource.Token);

        try
        {
            await cancellationStage.ExecuteAsync(
                cancellationContext);

            messages.Add(
                "La etapa ignoró el token de cancelación.");
        }
        catch (OperationCanceledException)
        {
            cancellationWasHonored =
                cancellationEngine.CallCount == 0 &&
                cancellationContext.VerificationResult is null &&
                cancellationContext.StageResults.Count == 0;

            messages.Add(
                cancellationWasHonored
                    ? "La cancelación fue respetada antes de verificar."
                    : "La cancelación no detuvo la etapa correctamente.");
        }

        stageResultsWereAuditable =
            HasAuditableResult(
                successfulStageResult) &&
            HasAuditableResult(
                failedVerificationStageResult) &&
            HasAuditableResult(
                missingWriteStageResult) &&
            HasAuditableResult(
                noWritableStageResult) &&
            HasAuditableResult(
                cancelledWriteStageResult) &&
            HasAuditableResult(
                failedWriteStageResult) &&
            successfulStage.Stage ==
                MetadataApplicationStage
                    .PostWriteVerification &&
            successfulStage.Name ==
                "Verificación posterior a la escritura" &&
            successfulStage.ExecutionOrder ==
                400;

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
            failedVerificationEngine.CallCount == 1 &&
            missingWriteEngine.CallCount == 0 &&
            noWritableEngine.CallCount == 0 &&
            cancelledWriteEngine.CallCount == 0 &&
            failedWriteEngine.CallCount == 0 &&
            cancellationEngine.CallCount == 0;

        messages.Add(
            injectedEngineWasUsed
                ? "La etapa utilizó correctamente los motores controlados."
                : "La delegación a los motores no fue la esperada.");

        return new MetadataVerificationStageTestResult
        {
            SuccessfulVerificationWasCompleted =
                successfulVerificationWasCompleted,

            FailedVerificationWasFailed =
                failedVerificationWasFailed,

            MissingWriteResultWasRejected =
                missingWriteResultWasRejected,

            NoWritableChangesWasSkipped =
                noWritableChangesWasSkipped,

            CancelledWriteWasCancelled =
                cancelledWriteWasCancelled,

            FailedWriteWasRejected =
                failedWriteWasRejected,

            VerificationResultWasStored =
                verificationResultWasStored,

            VerificationInputsWereMapped =
                verificationInputsWereMapped,

            PictureCountBeforeWasForwarded =
                pictureCountBeforeWasForwarded,

            CancellationWasHonored =
                cancellationWasHonored,

            StageResultsWereAuditable =
                stageResultsWereAuditable,

            DuplicateExecutionWasRejected =
                duplicateExecutionWasRejected,

            InjectedEngineWasUsed =
                injectedEngineWasUsed,

            Messages =
                messages.ToArray()
        };
    }

    private static MetadataApplicationContext
        CreateContextWithWriteResult(
            MetadataApplyRequest applyRequest,
            MetadataWriteResult writeResult,
            CancellationToken cancellationToken = default)
    {
        MetadataApplicationContext context =
            new(
                applyRequest,
                cancellationToken);

        context.SetWriteResult(
            writeResult);

        return context;
    }

    private static bool VerificationInputsWereMapped(
        ControlledMetadataWriterVerificationEngine engine,
        MetadataApplyRequest applyRequest)
    {
        IReadOnlyList<MetadataFieldChange>
            expectedChanges =
                applyRequest.ValidChanges;

        return
            engine.LastFilePath ==
                applyRequest.FilePath &&
            engine.LastChanges.Count ==
                expectedChanges.Count &&
            engine.LastChanges
                .Select(change => change.Field)
                .SequenceEqual(
                    expectedChanges.Select(
                        change => change.Field)) &&
            engine.LastChanges
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
                MetadataApplicationStage
                    .PostWriteVerification &&
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
                    }
                }
        };
    }

    private static MetadataWriteResult CreateWriteResult(
        MetadataApplyRequest applyRequest,
        MetadataWriteStatus status,
        int pictureCountBefore,
        string message)
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        IReadOnlyList<MetadataFieldWriteResult>
            fieldResults =
                status ==
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
                                message
                        }
                    }
                    : Array.Empty<
                        MetadataFieldWriteResult>();

        return new MetadataWriteResult
        {
            WriteRequestId =
                Guid.NewGuid(),

            ApplyRequestId =
                applyRequest.RequestId,

            PlanId =
                applyRequest.PlanId,

            Status =
                status,

            FilePath =
                applyRequest.FilePath,

            WriterName =
                "ControlledWriter",

            PictureCountBefore =
                pictureCountBefore,

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
                    message
                }
        };
    }

    private static MetadataVerificationResult
        CreateVerificationResult(
            string filePath,
            int pictureCountBefore,
            bool wasSuccessful)
    {
        string persistedValue =
            wasSuccessful
                ? "Artista aprobado"
                : "Artista diferente";

        string message =
            wasSuccessful
                ? "El valor persistido coincide con el solicitado."
                : "El valor persistido no coincide con el solicitado.";

        return new MetadataVerificationResult
        {
            FilePath =
                filePath,

            FileOpened =
                true,

            FieldResults =
                new[]
                {
                    new MetadataFieldVerificationResult
                    {
                        Field =
                            MetadataField.Artist,

                        ExpectedValue =
                            "Artista aprobado",

                        PersistedValue =
                            persistedValue,

                        IsSupported =
                            true,

                        MatchesExpectedValue =
                            wasSuccessful,

                        Message =
                            message
                    }
                },

            PictureCountBefore =
                pictureCountBefore,

            PictureCountAfter =
                pictureCountBefore,

            Messages =
                new[]
                {
                    message
                }
        };
    }
}