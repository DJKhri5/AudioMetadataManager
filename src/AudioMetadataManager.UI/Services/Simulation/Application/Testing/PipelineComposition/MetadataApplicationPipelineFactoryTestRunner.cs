using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Composition;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Backup;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Finalization;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Verification;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Writing;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineComposition;

/// <summary>
/// Ejecuta pruebas estructurales sobre la composición
/// predeterminada del pipeline de aplicación de metadatos.
/// </summary>
public sealed class MetadataApplicationPipelineFactoryTestRunner
{
    /// <summary>
    /// Comprueba las etapas, sus identidades, órdenes, opciones
    /// seguras y la independencia entre creaciones sucesivas.
    /// </summary>
    public Task<MetadataApplicationPipelineFactoryTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        MetadataApplicationPipelineExecutor firstExecutor =
            MetadataApplicationPipelineFactory.CreateDefault();

        MetadataApplicationPipelineExecutor secondExecutor =
            MetadataApplicationPipelineFactory.CreateDefault();

        bool exactlyFiveStagesWereRegistered =
            firstExecutor.Stages.Count == 5;

        if (!exactlyFiveStagesWereRegistered)
        {
            messages.Add(
                "La fábrica no registró exactamente cinco etapas.");
        }

        Type[] expectedStageTypes =
        {
            typeof(MetadataValidationStage),
            typeof(MetadataBackupStage),
            typeof(MetadataWritingStage),
            typeof(MetadataVerificationStage),
            typeof(MetadataFinalizationStage)
        };

        bool concreteStageTypesWereCorrect =
            firstExecutor.Stages
                .Select(
                    stage =>
                        stage.GetType())
                .SequenceEqual(
                    expectedStageTypes);

        if (!concreteStageTypesWereCorrect)
        {
            messages.Add(
                "Los tipos concretos de las etapas no coinciden " +
                "con la composición esperada.");
        }

        MetadataApplicationStage[] expectedStageIdentities =
        {
            MetadataApplicationStage.Validation,
            MetadataApplicationStage.Backup,
            MetadataApplicationStage.MetadataWrite,
            MetadataApplicationStage.PostWriteVerification,
            MetadataApplicationStage.Finalization
        };

        MetadataApplicationStage[] actualStageIdentities =
            firstExecutor.Stages
                .Select(
                    stage =>
                        stage.Stage)
                .ToArray();

        bool stageIdentitiesWereCorrect =
            actualStageIdentities
                .OrderBy(
                    stage =>
                        stage)
                .SequenceEqual(
                    expectedStageIdentities
                        .OrderBy(
                            stage =>
                                stage));

        if (!stageIdentitiesWereCorrect)
        {
            messages.Add(
                "Las identidades funcionales registradas no son " +
                "las esperadas.");
        }

        int[] expectedExecutionOrders =
        {
            100,
            200,
            300,
            400,
            500
        };

        bool executionOrdersWereCorrect =
            firstExecutor.Stages
                .Select(
                    stage =>
                        stage.ExecutionOrder)
                .OrderBy(
                    order =>
                        order)
                .SequenceEqual(
                    expectedExecutionOrders);

        if (!executionOrdersWereCorrect)
        {
            messages.Add(
                "Los órdenes de ejecución no corresponden a 100, " +
                "200, 300, 400 y 500.");
        }

        bool finalStageOrderWasCorrect =
            actualStageIdentities.SequenceEqual(
                expectedStageIdentities);

        if (!finalStageOrderWasCorrect)
        {
            messages.Add(
                "El orden final del pipeline no corresponde a " +
                "validación, respaldo, escritura, verificación y " +
                "finalización.");
        }

        MetadataApplicationPipelineOptions options =
            firstExecutor.Options;

        bool defaultOptionsWereSafe =
            options.StopOnBlockingFailure &&
            options.StopOnCancellation &&
            !options.StopOnSkippedStage &&
            options.RejectDuplicateExecutionOrder &&
            !options.CompleteContextAutomatically;

        if (!defaultOptionsWereSafe)
        {
            messages.Add(
                "La configuración predeterminada del pipeline no " +
                "cumple las condiciones seguras esperadas.");
        }

        bool stagesWereIndependent =
            firstExecutor.Stages
                .Zip(
                    secondExecutor.Stages,
                    (firstStage, secondStage) =>
                        !ReferenceEquals(
                            firstStage,
                            secondStage))
                .All(
                    wereIndependent =>
                        wereIndependent);

        bool successiveCreationsWereIndependent =
            !ReferenceEquals(
                firstExecutor,
                secondExecutor) &&
            !ReferenceEquals(
                firstExecutor.Options,
                secondExecutor.Options) &&
            stagesWereIndependent;

        if (!successiveCreationsWereIndependent)
        {
            messages.Add(
                "Las creaciones sucesivas compartieron alguna " +
                "instancia mutable del pipeline.");
        }

        bool nullOptionsWereRejected =
            false;

        try
        {
            MetadataApplicationPipelineFactory.Create(
                null!);
        }
        catch (ArgumentNullException)
        {
            nullOptionsWereRejected =
                true;
        }

        if (!nullOptionsWereRejected)
        {
            messages.Add(
                "La fábrica no rechazó una configuración nula.");
        }

        MetadataApplicationPipelineFactoryTestResult result =
            new()
            {
                ExactlyFiveStagesWereRegistered =
                    exactlyFiveStagesWereRegistered,

                ConcreteStageTypesWereCorrect =
                    concreteStageTypesWereCorrect,

                StageIdentitiesWereCorrect =
                    stageIdentitiesWereCorrect,

                ExecutionOrdersWereCorrect =
                    executionOrdersWereCorrect,

                FinalStageOrderWasCorrect =
                    finalStageOrderWasCorrect,

                DefaultOptionsWereSafe =
                    defaultOptionsWereSafe,

                SuccessiveCreationsWereIndependent =
                    successiveCreationsWereIndependent,

                NullOptionsWereRejected =
                    nullOptionsWereRejected,

                Messages =
                    messages
            };

        return Task.FromResult(
            result);
    }
}