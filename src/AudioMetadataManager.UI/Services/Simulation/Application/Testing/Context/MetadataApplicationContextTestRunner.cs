using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Context;

/// <summary>
/// Ejecuta comprobaciones estructurales sobre el ciclo de vida
/// de MetadataApplicationContext.
///
/// No accede al sistema de archivos y no ejecuta escritores.
/// </summary>
public sealed class MetadataApplicationContextTestRunner
{
    public MetadataApplicationContextTestResult Run()
    {
        List<string> messages =
            new();

        bool contextStartedActive =
            false;

        bool stageWasRegistered =
            false;

        bool duplicateStageWasRejected =
            false;

        bool prematureBuildWasRejected =
            false;

        bool contextWasFinalized =
            false;

        bool pipelineResultWasBuilt =
            false;

        bool postCompletionMutationWasRejected =
            false;

        MetadataApplyRequest request =
            new()
            {
                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    @"C:\Tests\context-test.mp3",

                FileName =
                    "context-test.mp3",

                Changes =
                    Array.Empty<MetadataFieldChange>()
            };

        MetadataApplicationContext context =
            new(
                request);

        contextStartedActive =
            !context.IsCompleted &&
            context.StopReason ==
                MetadataApplicationStopReason.None;

        messages.Add(
            contextStartedActive
                ? "El contexto comenzó activo."
                : "El contexto no comenzó en el estado esperado.");

        try
        {
            context.BuildResult();

            messages.Add(
                "El contexto permitió construir el resultado antes " +
                "de finalizar.");
        }
        catch (InvalidOperationException)
        {
            prematureBuildWasRejected =
                true;

            messages.Add(
                "La construcción prematura del resultado fue " +
                "rechazada correctamente.");
        }

        MetadataApplicationStageResult validationStage =
            new()
            {
                Stage =
                    MetadataApplicationStage.Validation,

                Status =
                    MetadataApplicationStageStatus.Completed,

                StartedAtUtc =
                    DateTimeOffset.UtcNow,

                CompletedAtUtc =
                    DateTimeOffset.UtcNow,

                ElapsedTime =
                    TimeSpan.Zero,

                Message =
                    "Etapa estructural de prueba completada."
            };

        context.AddStageResult(
            validationStage);

        stageWasRegistered =
            context.HasStage(
                MetadataApplicationStage.Validation) &&
            context.StageResults.Count == 1;

        messages.Add(
            stageWasRegistered
                ? "La etapa fue registrada correctamente."
                : "La etapa no fue registrada correctamente.");

        try
        {
            context.AddStageResult(
                validationStage);

            messages.Add(
                "El contexto permitió registrar una etapa " +
                "duplicada.");
        }
        catch (InvalidOperationException)
        {
            duplicateStageWasRejected =
                true;

            messages.Add(
                "La etapa duplicada fue rechazada " +
                "correctamente.");
        }

        context.Stop(
            MetadataApplicationStopReason.ValidationFailed,
            "Finalización estructural de prueba.");

        contextWasFinalized =
            context.IsCompleted &&
            context.StopReason ==
                MetadataApplicationStopReason.ValidationFailed &&
            !string.IsNullOrWhiteSpace(
                context.ErrorMessage);

        messages.Add(
            contextWasFinalized
                ? "El contexto fue finalizado correctamente."
                : "El contexto no se finalizó correctamente.");

        MetadataApplicationPipelineResult pipelineResult =
            context.BuildResult();

        pipelineResultWasBuilt =
            pipelineResult.ExecutionId ==
                context.ExecutionId &&
            pipelineResult.Request ==
                request &&
            pipelineResult.StopReason ==
                MetadataApplicationStopReason.ValidationFailed &&
            pipelineResult.StageResults.Count == 1;

        messages.Add(
            pipelineResultWasBuilt
                ? "El resultado inmutable fue construido " +
                  "correctamente."
                : "El resultado inmutable no coincide con el " +
                  "contexto.");

        try
        {
            context.AddStageResult(
                new MetadataApplicationStageResult
                {
                    Stage =
                        MetadataApplicationStage.Finalization,

                    Status =
                        MetadataApplicationStageStatus.Completed,

                    StartedAtUtc =
                        DateTimeOffset.UtcNow,

                    CompletedAtUtc =
                        DateTimeOffset.UtcNow,

                    ElapsedTime =
                        TimeSpan.Zero,

                    Message =
                        "Modificación posterior no permitida."
                });

            messages.Add(
                "El contexto permitió modificarse después de " +
                "finalizar.");
        }
        catch (InvalidOperationException)
        {
            postCompletionMutationWasRejected =
                true;

            messages.Add(
                "La modificación posterior a la finalización fue " +
                "rechazada correctamente.");
        }

        return new MetadataApplicationContextTestResult
        {
            ContextStartedActive =
                contextStartedActive,

            StageWasRegistered =
                stageWasRegistered,

            DuplicateStageWasRejected =
                duplicateStageWasRejected,

            PrematureBuildWasRejected =
                prematureBuildWasRejected,

            ContextWasFinalized =
                contextWasFinalized,

            PipelineResultWasBuilt =
                pipelineResultWasBuilt,

            PostCompletionMutationWasRejected =
                postCompletionMutationWasRejected,

            Messages =
                messages.ToArray()
        };
    }
}