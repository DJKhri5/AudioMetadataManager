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
    .Application.Pipeline.Stages.Base;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Backup;

/// <summary>
/// Crea y verifica el respaldo obligatorio antes de modificar
/// los metadatos del archivo original.
/// </summary>
public sealed class MetadataBackupStage :
    MetadataApplicationStageBase
{
    private readonly IMetadataBackupEngine
        _backupEngine;

    /// <summary>
    /// Crea la etapa con el motor de respaldo predeterminado.
    /// </summary>
    public MetadataBackupStage()
        : this(
            new MetadataBackupEngine())
    {
    }

    /// <summary>
    /// Crea la etapa con un motor de respaldo proporcionado.
    /// </summary>
    public MetadataBackupStage(
        IMetadataBackupEngine backupEngine)
    {
        _backupEngine =
            backupEngine ??
            throw new ArgumentNullException(
                nameof(backupEngine));
    }

    /// <inheritdoc />
    public override MetadataApplicationStage Stage =>
        MetadataApplicationStage.Backup;

    /// <inheritdoc />
    public override string Name =>
        "Creación y verificación de respaldo";

    /// <inheritdoc />
    public override int ExecutionOrder =>
        200;

    /// <inheritdoc />
    protected override async
        Task<MetadataApplicationStageExecution>
        ExecuteCoreAsync(
            MetadataApplicationContext context)
    {
        MetadataApplyRequest applyRequest =
            context.Request;

        MetadataBackupRequest backupRequest =
            new()
            {
                ApplyRequestId =
                    applyRequest.RequestId,

                PlanId =
                    applyRequest.PlanId,

                SourceFilePath =
                    applyRequest.FilePath,

                FileName =
                    applyRequest.FileName
            };

        MetadataBackupResult backupResult =
            await _backupEngine.CreateBackupAsync(
                backupRequest,
                progress: null,
                context.CancellationToken);

        context.SetBackupResult(
            backupResult);

        if (backupResult.WasSuccessful)
        {
            return Completed(
                backupResult.Summary,
                backupResult.Messages);
        }

        if (backupResult.Status ==
            MetadataBackupStatus.Cancelled)
        {
            return Cancelled(
                backupResult.Summary,
                backupResult.Messages);
        }

        return Failed(
            backupResult.Summary,
            backupResult.Messages);
    }
}