using Xunit;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Context;

public sealed class MetadataApplicationContextTests
{
    [Fact]
    public void NewContext_StartsActive()
    {
        MetadataApplyRequest request =
            CreateRequest();

        MetadataApplicationContext context =
            new(
                request);

        Assert.False(
            context.IsCompleted);

        Assert.Equal(
            MetadataApplicationStopReason.None,
            context.StopReason);

        Assert.Null(
            context.CompletedAtUtc);

        Assert.False(
            context.WasCancelled);

        Assert.NotEqual(
            Guid.Empty,
            context.ExecutionId);

        Assert.Same(
            request,
            context.Request);
    }

    [Fact]
    public void StageResult_IsRegistered()
    {
        MetadataApplicationContext context =
            CreateContext();

        MetadataApplicationStageResult stageResult =
            CreateValidationStageResult();

        context.AddStageResult(
            stageResult);

        Assert.True(
            context.HasStage(
                MetadataApplicationStage.Validation));

        Assert.Single(
            context.StageResults);

        Assert.Same(
            stageResult,
            context.StageResults[0]);
    }

    [Fact]
    public void DuplicateStage_IsRejected()
    {
        MetadataApplicationContext context =
            CreateContext();

        MetadataApplicationStageResult stageResult =
            CreateValidationStageResult();

        context.AddStageResult(
            stageResult);

        Assert.Throws<InvalidOperationException>(
            () =>
                context.AddStageResult(
                    stageResult));

        Assert.Single(
            context.StageResults);
    }

    [Fact]
    public void BuildResult_BeforeCompletion_IsRejected()
    {
        MetadataApplicationContext context =
            CreateContext();

        Assert.Throws<InvalidOperationException>(
            () =>
                context.BuildResult());
    }

    [Fact]
    public void Stop_FinalizesContext()
    {
        MetadataApplicationContext context =
            CreateContext();

        context.Stop(
            MetadataApplicationStopReason.ValidationFailed,
            " Finalización estructural de prueba. ");

        Assert.True(
            context.IsCompleted);

        Assert.Equal(
            MetadataApplicationStopReason.ValidationFailed,
            context.StopReason);

        Assert.Equal(
            "Finalización estructural de prueba.",
            context.ErrorMessage);

        Assert.NotNull(
            context.CompletedAtUtc);

        Assert.True(
            context.CompletedAtUtc >=
            context.StartedAtUtc);

        Assert.True(
            context.ElapsedTime >=
            TimeSpan.Zero);
    }

    [Fact]
    public void CompletedContext_BuildsImmutablePipelineResult()
    {
        MetadataApplyRequest request =
            CreateRequest();

        MetadataApplicationContext context =
            new(
                request);

        MetadataApplicationStageResult stageResult =
            CreateValidationStageResult();

        context.AddStageResult(
            stageResult);

        context.Stop(
            MetadataApplicationStopReason.ValidationFailed,
            "Finalización estructural de prueba.");

        MetadataApplicationPipelineResult result =
            context.BuildResult();

        Assert.Equal(
            context.ExecutionId,
            result.ExecutionId);

        Assert.Same(
            request,
            result.Request);

        Assert.Equal(
            context.StartedAtUtc,
            result.StartedAtUtc);

        Assert.Equal(
            context.CompletedAtUtc,
            result.CompletedAtUtc);

        Assert.Equal(
            context.ElapsedTime,
            result.ElapsedTime);

        Assert.Equal(
            MetadataApplicationStopReason.ValidationFailed,
            result.StopReason);

        Assert.Single(
            result.StageResults);

        Assert.Same(
            stageResult,
            result.StageResults[0]);

        Assert.Equal(
            context.ErrorMessage,
            result.ErrorMessage);
    }

    [Fact]
    public void Mutation_AfterCompletion_IsRejected()
    {
        MetadataApplicationContext context =
            CreateContext();

        context.Stop(
            MetadataApplicationStopReason.ValidationFailed,
            "Finalización estructural de prueba.");

        MetadataApplicationStageResult stageResult =
            new()
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
            };

        Assert.Throws<InvalidOperationException>(
            () =>
                context.AddStageResult(
                    stageResult));
    }

    private static MetadataApplicationContext
        CreateContext()
    {
        return
            new MetadataApplicationContext(
                CreateRequest());
    }

    private static MetadataApplyRequest
        CreateRequest()
    {
        return
            new MetadataApplyRequest
            {
                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    @"C:\Tests\context-xunit-test.mp3",

                FileName =
                    "context-xunit-test.mp3",

                Changes =
                    Array.Empty<MetadataFieldChange>()
            };
    }

    private static MetadataApplicationStageResult
        CreateValidationStageResult()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        return
            new MetadataApplicationStageResult
            {
                Stage =
                    MetadataApplicationStage.Validation,

                Status =
                    MetadataApplicationStageStatus.Completed,

                StartedAtUtc =
                    now,

                CompletedAtUtc =
                    now,

                ElapsedTime =
                    TimeSpan.Zero,

                Message =
                    "Etapa estructural de prueba completada."
            };
    }
}