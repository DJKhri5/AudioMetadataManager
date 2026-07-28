using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Base;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Validation;

/// <summary>
/// Ejecuta la validación previa de la solicitud y conserva su
/// resultado en el contexto compartido del pipeline.
/// </summary>
public sealed class MetadataValidationStage :
    MetadataApplicationStageBase
{
    private readonly IMetadataApplyRequestValidator
        _requestValidator;

    /// <summary>
    /// Crea la etapa con el validador predeterminado.
    /// </summary>
    public MetadataValidationStage()
        : this(
            new MetadataApplyRequestValidator())
    {
    }

    /// <summary>
    /// Crea la etapa con un validador proporcionado.
    /// </summary>
    public MetadataValidationStage(
        IMetadataApplyRequestValidator requestValidator)
    {
        _requestValidator =
            requestValidator ??
            throw new ArgumentNullException(
                nameof(requestValidator));
    }

    /// <inheritdoc />
    public override MetadataApplicationStage Stage =>
        MetadataApplicationStage.Validation;

    /// <inheritdoc />
    public override string Name =>
        "Validación de solicitud de metadatos";

    /// <inheritdoc />
    public override int ExecutionOrder =>
        100;

    /// <inheritdoc />
    protected override Task<MetadataApplicationStageExecution>
        ExecuteCoreAsync(
            MetadataApplicationContext context)
    {
        MetadataApplyValidationResult validationResult =
            _requestValidator.Validate(
                context.Request);

        context.SetValidationResult(
            validationResult);

        IReadOnlyList<string> details =
            validationResult.Issues
                .Select(
                    issue =>
                        $"[{issue.Code}] {issue.Summary}")
                .ToArray();

        MetadataApplicationStageExecution execution =
            !validationResult.IsValid
                ? Failed(
                    validationResult.Summary,
                    details)
                : validationResult.WarningCount > 0
                    ? CompletedWithWarnings(
                        validationResult.Summary,
                        details)
                    : Completed(
                        validationResult.Summary,
                        details);

        return Task.FromResult(
            execution);
    }
}