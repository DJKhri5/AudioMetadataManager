using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Artwork;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Backup;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Contracts;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Finalization;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Verification;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Writing;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Composition;

/// <summary>
/// Construye la composición funcional predeterminada del pipeline
/// de aplicación de metadatos.
/// </summary>
public static class MetadataApplicationPipelineFactory
{
    /// <summary>
    /// Crea el pipeline predeterminado con validación, respaldo,
    /// escritura, verificación posterior a la escritura,
    /// adquisición opcional de carátula y finalización.
    /// </summary>
    public static MetadataApplicationPipelineExecutor
        CreateDefault()
    {
        MetadataApplicationPipelineOptions options =
            new()
            {
                RejectDuplicateExecutionOrder =
                    true,

                CompleteContextAutomatically =
                    true
            };

        return Create(
            options);
    }

    /// <summary>
    /// Crea el pipeline predeterminado utilizando las opciones
    /// indicadas.
    /// </summary>
    public static MetadataApplicationPipelineExecutor Create(
        MetadataApplicationPipelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        IMetadataApplicationStage[] stages =
        {
            new MetadataValidationStage(),
            new MetadataBackupStage(),
            new MetadataWritingStage(),
            new MetadataVerificationStage(),
            new MetadataFinalizationStage(),
            new MetadataArtworkStage()
        };

        return new MetadataApplicationPipelineExecutor(
            stages,
            options);
    }
}