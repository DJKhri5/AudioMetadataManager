using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Base;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Finalization;

/// <summary>
/// Construye el resultado final y consolidado de la aplicación de
/// metadatos, y lo registra en el contexto compartido.
///
/// Replica fielmente la lógica de consolidación que ya usaba
/// MetadataApplicationPipeline (la clase monolítica), sin
/// duplicar ni rediseñar su comportamiento.
/// </summary>
public sealed class MetadataFinalizationStage :
    MetadataApplicationStageBase
{
    /// <inheritdoc />
    public override MetadataApplicationStage Stage =>
        MetadataApplicationStage.Finalization;

    /// <inheritdoc />
    public override string Name =>
        "Finalización de la aplicación";

    /// <inheritdoc />
    public override int ExecutionOrder =>
        500;

    /// <inheritdoc />
    protected override Task<MetadataApplicationStageExecution>
        ExecuteCoreAsync(
            MetadataApplicationContext context)
    {
        MetadataBackupResult? backupResult =
            context.BackupResult;

        MetadataWriteResult? writeResult =
            context.WriteResult;

        if (backupResult is null ||
            writeResult is null)
        {
            return Task.FromResult(
                Failed(
                    "La finalización no puede construir un " +
                    "resultado sin un respaldo y una escritura " +
                    "previos.",
                    new[]
                    {
                        "El contexto no contiene los resultados " +
                        "necesarios para finalizar."
                    }));
        }

        if (writeResult.Status ==
            MetadataWriteStatus.NoWritableChanges)
        {
            return Task.FromResult(
                Skipped(
                    "La finalización fue omitida porque no hubo " +
                    "una escritura real de metadatos.",
                    writeResult.Messages));
        }

        if (writeResult.Status ==
            MetadataWriteStatus.Cancelled)
        {
            return Task.FromResult(
                Cancelled(
                    "La finalización no se ejecutó porque la " +
                    "escritura fue cancelada.",
                    writeResult.Messages));
        }

        if (!writeResult.WasSuccessful)
        {
            return Task.FromResult(
                Failed(
                    "La finalización no puede construir un " +
                    "resultado porque la escritura no terminó " +
                    "correctamente.",
                    new[]
                    {
                        writeResult.Summary
                    }));
        }

        MetadataApplyResult applyResult =
            BuildApplyResult(
                context.Request,
                backupResult,
                writeResult,
                context.StartedAtUtc,
                context.ElapsedTime);

        context.SetApplyResult(
            applyResult);

        return Task.FromResult(
            applyResult.WasSuccessful
                ? Completed(
                    applyResult.Summary)
                : CompletedWithWarnings(
                    applyResult.Summary));
    }

    private static MetadataApplyResult BuildApplyResult(
        MetadataApplyRequest request,
        MetadataBackupResult backupResult,
        MetadataWriteResult writeResult,
        DateTimeOffset startedAtUtc,
        TimeSpan elapsedTime)
    {
        MetadataFieldApplyResult[] fieldResults =
            writeResult.FieldResults
                .Select(
                    result =>
                        new MetadataFieldApplyResult
                        {
                            Field =
                                result.Field,

                            OriginalValue =
                                result.OriginalValue,

                            RequestedValue =
                                result.RequestedValue,

                            VerifiedValue =
                                result.SaveSucceeded
                                    ? result.RequestedValue
                                    : string.Empty,

                            WriteSucceeded =
                                result.IsSupported &&
                                result.ValuePrepared,

                            VerificationSucceeded =
                                result.SaveSucceeded,

                            Message =
                                result.Message
                        })
                .ToArray();

        MetadataApplyStatus status;

        if (writeResult.WasSuccessful &&
            fieldResults.All(
                field =>
                    field.WasSuccessfullyApplied))
        {
            status =
                MetadataApplyStatus.Completed;
        }
        else if (fieldResults.Any(
                     field =>
                         field.WasSuccessfullyApplied))
        {
            status =
                MetadataApplyStatus.PartiallyCompleted;
        }
        else if (writeResult.Status ==
                 MetadataWriteStatus.Cancelled)
        {
            status =
                MetadataApplyStatus.Cancelled;
        }
        else if (writeResult.HasWrittenFields)
        {
            status =
                MetadataApplyStatus.VerificationFailed;
        }
        else
        {
            status =
                MetadataApplyStatus.WriteFailed;
        }

        return new MetadataApplyResult
        {
            RequestId =
                request.RequestId,

            PlanId =
                request.PlanId,

            FilePath =
                request.FilePath,

            FileName =
                request.FileName,

            Status =
                status,

            StartedAtUtc =
                startedAtUtc,

            CompletedAtUtc =
                DateTimeOffset.UtcNow,

            ElapsedTime =
                elapsedTime,

            BackupPath =
                backupResult.BackupFilePath,

            FieldResults =
                fieldResults,

            Messages =
                writeResult.Messages.ToArray()
        };
    }
}
