using AudioMetadataManager.UI.Services.Artwork;
using AudioMetadataManager.UI.Services.Artwork.Models;
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
    .Application.Pipeline.Stages.Artwork;

/// <summary>
/// Descarga e incrusta la carátula de la pista, cuando la
/// solicitud la pidió.
///
/// Es la única etapa opcional del pipeline: cuando la solicitud
/// no incluye una dirección de carátula, la etapa se omite sin
/// afectar el resto de la ejecución.
/// </summary>
public sealed class MetadataArtworkStage :
    MetadataApplicationStageBase
{
    private readonly TrackArtworkService
        _artworkService;

    /// <summary>
    /// Crea la etapa con el servicio de carátula predeterminado.
    /// </summary>
    public MetadataArtworkStage()
        : this(
            new TrackArtworkService())
    {
    }

    /// <summary>
    /// Crea la etapa con un servicio de carátula proporcionado.
    /// </summary>
    public MetadataArtworkStage(
        TrackArtworkService artworkService)
    {
        _artworkService =
            artworkService ??
            throw new ArgumentNullException(
                nameof(artworkService));
    }

    /// <inheritdoc />
    public override MetadataApplicationStage Stage =>
        MetadataApplicationStage.Artwork;

    /// <inheritdoc />
    public override string Name =>
        "Adquisición de carátula";

    /// <inheritdoc />
    public override int ExecutionOrder =>
        500;

    /// <inheritdoc />
    protected override async
        Task<MetadataApplicationStageExecution>
        ExecuteCoreAsync(
            MetadataApplicationContext context)
    {
        MetadataApplyRequest applyRequest =
            context.Request;

        if (!applyRequest.HasArtworkRequest)
        {
            return Skipped(
                "No se solicitó una carátula para esta pista.");
        }

        MetadataBackupResult? backupResult =
            context.BackupResult;

        if (backupResult?.WasSuccessful != true)
        {
            return Failed(
                "La adquisición de carátula no puede comenzar " +
                "sin un respaldo creado y verificado.",
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

        TrackArtworkResult artworkResult =
            await _artworkService.AcquireAsync(
                new TrackArtworkRequest
                {
                    FilePath =
                        applyRequest.FilePath,

                    VerifiedBackupPath =
                        backupResult.BackupFilePath,

                    ArtworkUrl =
                        applyRequest.ArtworkUrl!
                },
                context.CancellationToken);

        context.SetArtworkResult(
            artworkResult);

        if (artworkResult.IsSuccess)
        {
            return Completed(
                artworkResult.Message);
        }

        if (artworkResult.Status ==
            TrackArtworkStatus.Cancelled)
        {
            return Cancelled(
                artworkResult.Message);
        }

        return Failed(
            artworkResult.Message);
    }
}
