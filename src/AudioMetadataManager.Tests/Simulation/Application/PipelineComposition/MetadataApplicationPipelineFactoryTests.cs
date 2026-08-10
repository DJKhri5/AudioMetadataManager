using Xunit;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Composition;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.Tests
    .Simulation.Application.PipelineComposition;

public sealed class MetadataApplicationPipelineFactoryTests
{
    [Fact]
    public void CreateDefault_ReturnsPipeline()
    {
        MetadataApplicationPipelineExecutor pipeline =
            MetadataApplicationPipelineFactory.CreateDefault();

        Assert.NotNull(
            pipeline);
    }

    [Fact]
    public void DefaultPipeline_HasExpectedStageCount()
    {
        MetadataApplicationPipelineExecutor pipeline =
            MetadataApplicationPipelineFactory.CreateDefault();

        Assert.Equal(
            5,
            pipeline.Stages.Count);
    }

    [Fact]
    public void DefaultPipeline_HasExpectedExecutionOrder()
    {
        MetadataApplicationPipelineExecutor pipeline =
            MetadataApplicationPipelineFactory.CreateDefault();

        int[] actualOrders =
            pipeline.Stages
                .Select(
                    stage =>
                        stage.ExecutionOrder)
                .ToArray();

        Assert.Equal(
            new[]
            {
                100,
                200,
                300,
                400,
                500
            },
            actualOrders);
    }

    [Fact]
    public void SuccessiveCreations_AreIndependent()
    {
        MetadataApplicationPipelineExecutor first =
            MetadataApplicationPipelineFactory.CreateDefault();

        MetadataApplicationPipelineExecutor second =
            MetadataApplicationPipelineFactory.CreateDefault();

        Assert.NotSame(
            first,
            second);

        Assert.NotSame(
            first.Options,
            second.Options);

        Assert.Equal(
            first.Stages.Count,
            second.Stages.Count);

        for (int index = 0;
            index < first.Stages.Count;
            index++)
        {
            Assert.NotSame(
                first.Stages[index],
                second.Stages[index]);
        }
    }

    [Fact]
    public void NullOptions_AreRejected()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                MetadataApplicationPipelineFactory.Create(
                    null!));
    }
}