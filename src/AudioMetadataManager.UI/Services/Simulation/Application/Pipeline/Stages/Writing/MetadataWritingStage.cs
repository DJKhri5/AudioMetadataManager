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
    .Application.Writing.Engine;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Writing;

/// <summary>
/// Ejecuta la escritura de los metadatos aprobados después de
/// comprobar que existe un respaldo verificado.
/// </summary>
public sealed class MetadataWritingStage :
    MetadataApplicationStageBase
{
    private readonly IMetadataWriterEngine
        _writerEngine;

    /// <summary>
    /// Crea la etapa utilizando el motor de escritura
    /// predeterminado.
    /// </summary>
    public MetadataWritingStage()
        : this(
            new MetadataWriterEngine())
    {
    }

    /// <summary>
    /// Crea la etapa con un motor de escritura proporcionado.
    /// </summary>
    public MetadataWritingStage(
        IMetadataWriterEngine writerEngine)
    {
        _writerEngine =
            writerEngine ??
            throw new ArgumentNullException(
                nameof(writerEngine));
    }

    /// <inheritdoc />
    public override MetadataApplicationStage Stage =>
        MetadataApplicationStage.MetadataWrite;

    /// <inheritdoc />
    public override string Name =>
        "Escritura de metadatos aprobados";

    /// <inheritdoc />
    public override int ExecutionOrder =>
        300;

    /// <inheritdoc />
    protected override async
        Task<MetadataApplicationStageExecution>
        ExecuteCoreAsync(
            MetadataApplicationContext context)
    {
        MetadataApplyRequest applyRequest =
            context.Request;

        MetadataBackupResult? backupResult =
            context.BackupResult;

        if (backupResult?.WasSuccessful != true)
        {
            return Failed(
                "La escritura no puede comenzar sin un " +
                "respaldo creado y verificado.",
                backupResult is null
                    ? new[]
                    {
                        "El contexto no contiene un resultado " +
                        "de respaldo."
                    }
                    : new[]
                    {
                        backupResult.Summary
                    });
        }

        if (applyRequest.ValidChanges.Count == 0)
        {
            MetadataWriteResult noFieldChangesResult =
                new()
                {
                    WriteRequestId =
                        Guid.NewGuid(),

                    ApplyRequestId =
                        applyRequest.RequestId,

                    PlanId =
                        applyRequest.PlanId,

                    Status =
                        MetadataWriteStatus.NoWritableChanges,

                    FilePath =
                        applyRequest.FilePath,

                    StartedAtUtc =
                        DateTimeOffset.UtcNow,

                    CompletedAtUtc =
                        DateTimeOffset.UtcNow,

                    Messages =
                        new[]
                        {
                            "La solicitud no contiene cambios de " +
                            "campo; la escritura fue omitida."
                        }
                };

            context.SetWriteResult(
                noFieldChangesResult);

            return CompletedWithWarnings(
                "No hay cambios de campo para escribir en esta " +
                "solicitud.",
                noFieldChangesResult.Messages);
        }

        MetadataWriteRequest writeRequest =
            new()
            {
                ApplyRequestId =
                    applyRequest.RequestId,

                PlanId =
                    applyRequest.PlanId,

                FilePath =
                    applyRequest.FilePath,

                FileName =
                    applyRequest.FileName,

                VerifiedBackupPath =
                    backupResult.BackupFilePath,

                Changes =
                    applyRequest.ValidChanges,

                PreserveUnchangedMetadata =
                    true,

                PreserveEmbeddedPictures =
                    true,

                PreserveUnknownMetadata =
                    true
            };

        MetadataWriteResult writeResult =
            await _writerEngine.WriteAsync(
                writeRequest,
                context.CancellationToken);

        context.SetWriteResult(
            writeResult);

        if (writeResult.WasSuccessful)
        {
            return Completed(
                writeResult.Summary,
                writeResult.Messages);
        }

        if (writeResult.Status ==
            MetadataWriteStatus.NoWritableChanges)
        {
            return CompletedWithWarnings(
                "El escritor compatible fue resuelto, pero " +
                "ningún metadato pudo escribirse.",
                writeResult.Messages);
        }

        if (writeResult.Status ==
            MetadataWriteStatus.Cancelled)
        {
            return Cancelled(
                writeResult.Summary,
                writeResult.Messages);
        }

        return Failed(
            writeResult.Summary,
            writeResult.Messages);
    }
}