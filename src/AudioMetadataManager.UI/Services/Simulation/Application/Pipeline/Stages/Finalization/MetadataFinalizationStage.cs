using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Finalization;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Base;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Finalization;

/// <summary>
/// Construye y registra el resultado final consolidado de la
/// aplicación de metadatos.
/// </summary>
public sealed class MetadataFinalizationStage :
    MetadataApplicationStageBase
{
    private readonly IMetadataApplyResultBuilder
        _resultBuilder;

    /// <summary>
    /// Crea la etapa utilizando el constructor predeterminado.
    /// </summary>
    public MetadataFinalizationStage()
        : this(
            new MetadataApplyResultBuilder())
    {
    }

    /// <summary>
    /// Crea la etapa con el constructor proporcionado.
    /// </summary>
    public MetadataFinalizationStage(
        IMetadataApplyResultBuilder resultBuilder)
    {
        _resultBuilder =
            resultBuilder ??
            throw new ArgumentNullException(
                nameof(resultBuilder));
    }

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
    protected override
        Task<MetadataApplicationStageExecution>
        ExecuteCoreAsync(
            MetadataApplicationContext context)
    {
        context.ThrowIfCancellationRequested();

        MetadataApplyResult applyResult =
            _resultBuilder.Build(
                context);

        context.SetApplyResult(
            applyResult);

        if (applyResult.WasSuccessful)
        {
            return Task.FromResult(
                Completed(
                    applyResult.Summary,
                    applyResult.Messages));
        }

        if (applyResult.Status ==
            MetadataApplyStatus.PartiallyCompleted)
        {
            return Task.FromResult(
                CompletedWithWarnings(
                    applyResult.Summary,
                    applyResult.Messages));
        }

        return Task.FromResult(
            Failed(
                applyResult.Summary,
                applyResult.Messages));
    }
}