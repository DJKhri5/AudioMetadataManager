using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Validation;

/// <summary>
/// Ejecuta pruebas estructurales sobre la etapa concreta de
/// validación.
///
/// Usa validadores controlados en memoria y no accede a archivos
/// reales ni ejecuta escritores.
/// </summary>
public sealed class MetadataValidationStageTestRunner
{
    public async Task<MetadataValidationStageTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        MetadataApplyRequest validRequest =
            CreateRequest(
                "validation-valid.mp3");

        MetadataApplyValidationResult validValidationResult =
            new();

        ControlledRequestValidator validValidator =
            new(
                validValidationResult);

        MetadataValidationStage validStage =
            new(
                validValidator);

        MetadataApplicationContext validContext =
            new(
                validRequest);

        await validStage.ExecuteAsync(
            validContext);

        MetadataApplicationStageResult? validStageResult =
            validContext.StageResults.SingleOrDefault();

        bool validResultWasCompleted =
            validStageResult is not null &&
            validStageResult.Status ==
                MetadataApplicationStageStatus.Completed &&
            validStageResult.Message ==
                validValidationResult.Summary;

        messages.Add(
            validResultWasCompleted
                ? "El resultado válido fue registrado como completado."
                : "El resultado válido no produjo el estado esperado.");

        MetadataApplyRequest warningRequest =
            CreateRequest(
                "validation-warning.mp3");

        MetadataApplyValidationResult warningValidationResult =
            new()
            {
                Issues =
                    new[]
                    {
                        CreateIssue(
                            MetadataApplyValidationIssueSeverity.Warning,
                            "CONTROLLED_WARNING",
                            "Advertencia controlada de prueba.")
                    }
            };

        ControlledRequestValidator warningValidator =
            new(
                warningValidationResult);

        MetadataValidationStage warningStage =
            new(
                warningValidator);

        MetadataApplicationContext warningContext =
            new(
                warningRequest);

        await warningStage.ExecuteAsync(
            warningContext);

        MetadataApplicationStageResult? warningStageResult =
            warningContext.StageResults.SingleOrDefault();

        bool warningResultWasCompletedWithWarnings =
            warningStageResult is not null &&
            warningStageResult.Status ==
                MetadataApplicationStageStatus
                    .CompletedWithWarnings &&
            warningStageResult.Details.Any(
                detail =>
                    detail.Contains(
                        "CONTROLLED_WARNING",
                        StringComparison.Ordinal));

        messages.Add(
            warningResultWasCompletedWithWarnings
                ? "La advertencia fue registrada sin bloquear la etapa."
                : "La advertencia no produjo el estado esperado.");

        MetadataApplyRequest invalidRequest =
            CreateRequest(
                "validation-invalid.mp3");

        MetadataApplyValidationResult invalidValidationResult =
            new()
            {
                Issues =
                    new[]
                    {
                        CreateIssue(
                            MetadataApplyValidationIssueSeverity.Error,
                            "CONTROLLED_ERROR",
                            "Error controlado de prueba.")
                    }
            };

        ControlledRequestValidator invalidValidator =
            new(
                invalidValidationResult);

        MetadataValidationStage invalidStage =
            new(
                invalidValidator);

        MetadataApplicationContext invalidContext =
            new(
                invalidRequest);

        await invalidStage.ExecuteAsync(
            invalidContext);

        MetadataApplicationStageResult? invalidStageResult =
            invalidContext.StageResults.SingleOrDefault();

        bool invalidResultWasFailed =
            invalidStageResult is not null &&
            invalidStageResult.Status ==
                MetadataApplicationStageStatus.Failed &&
            invalidStageResult.Details.Any(
                detail =>
                    detail.Contains(
                        "CONTROLLED_ERROR",
                        StringComparison.Ordinal));

        messages.Add(
            invalidResultWasFailed
                ? "El error bloqueante fue registrado como fallo."
                : "El error bloqueante no produjo el estado esperado.");

        bool validationResultsWereStored =
            ReferenceEquals(
                validContext.ValidationResult,
                validValidationResult) &&
            ReferenceEquals(
                warningContext.ValidationResult,
                warningValidationResult) &&
            ReferenceEquals(
                invalidContext.ValidationResult,
                invalidValidationResult);

        messages.Add(
            validationResultsWereStored
                ? "Los resultados fueron almacenados en sus contextos."
                : "Algún resultado no fue almacenado en el contexto.");

        bool stageResultsWereAuditable =
            HasAuditableResult(
                validStageResult) &&
            HasAuditableResult(
                warningStageResult) &&
            HasAuditableResult(
                invalidStageResult) &&
            validStage.Stage ==
                MetadataApplicationStage.Validation &&
            validStage.Name ==
                "Validación de solicitud de metadatos" &&
            validStage.ExecutionOrder ==
                100;

        messages.Add(
            stageResultsWereAuditable
                ? "Los resultados conservaron su identidad y tiempos."
                : "Los datos auditables de la etapa no coinciden.");

        bool duplicateExecutionWasRejected =
            false;

        try
        {
            await validStage.ExecuteAsync(
                validContext);

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

        bool injectedValidatorWasUsed =
            validValidator.CallCount == 1 &&
            ReferenceEquals(
                validValidator.LastRequest,
                validRequest) &&
            warningValidator.CallCount == 1 &&
            ReferenceEquals(
                warningValidator.LastRequest,
                warningRequest) &&
            invalidValidator.CallCount == 1 &&
            ReferenceEquals(
                invalidValidator.LastRequest,
                invalidRequest);

        messages.Add(
            injectedValidatorWasUsed
                ? "La etapa delegó en los validadores controlados."
                : "La delegación al validador no fue la esperada.");

        return new MetadataValidationStageTestResult
        {
            ValidResultWasCompleted =
                validResultWasCompleted,

            WarningResultWasCompletedWithWarnings =
                warningResultWasCompletedWithWarnings,

            InvalidResultWasFailed =
                invalidResultWasFailed,

            ValidationResultsWereStored =
                validationResultsWereStored,

            StageResultsWereAuditable =
                stageResultsWereAuditable,

            DuplicateExecutionWasRejected =
                duplicateExecutionWasRejected,

            InjectedValidatorWasUsed =
                injectedValidatorWasUsed,

            Messages =
                messages.ToArray()
        };
    }

    private static bool HasAuditableResult(
        MetadataApplicationStageResult? result)
    {
        return
            result is not null &&
            result.Stage ==
                MetadataApplicationStage.Validation &&
            result.StartedAtUtc != default &&
            result.CompletedAtUtc != default &&
            result.CompletedAtUtc >=
                result.StartedAtUtc &&
            result.ElapsedTime >=
                TimeSpan.Zero;
    }

    private static MetadataApplyRequest CreateRequest(
        string fileName)
    {
        return new MetadataApplyRequest
        {
            PlanId =
                Guid.NewGuid(),

            FilePath =
                @"Z:\AudioMetadataManager.StructuralTests\" +
                fileName,

            FileName =
                fileName,

            Changes =
                new[]
                {
                    new MetadataFieldChange
                    {
                        Field =
                            MetadataField.Title,

                        OriginalValue =
                            "Título original",

                        NewValue =
                            "Título validado",

                        WasManuallyApproved =
                            true,

                        Confidence =
                            1,

                        SupportingSources =
                            new[]
                            {
                                "Structural test"
                            }
                    }
                }
        };
    }

    private static MetadataApplyValidationIssue CreateIssue(
        MetadataApplyValidationIssueSeverity severity,
        string code,
        string message)
    {
        return new MetadataApplyValidationIssue
        {
            Severity =
                severity,

            Code =
                code,

            Message =
                message
        };
    }

    private sealed class ControlledRequestValidator :
        IMetadataApplyRequestValidator
    {
        private readonly MetadataApplyValidationResult
            _result;

        public ControlledRequestValidator(
            MetadataApplyValidationResult result)
        {
            _result =
                result ??
                throw new ArgumentNullException(
                    nameof(result));
        }

        public int CallCount { get; private set; }

        public MetadataApplyRequest? LastRequest
        { get; private set; }

        public MetadataApplyValidationResult Validate(
            MetadataApplyRequest request)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            CallCount++;

            LastRequest =
                request;

            return _result;
        }
    }
}