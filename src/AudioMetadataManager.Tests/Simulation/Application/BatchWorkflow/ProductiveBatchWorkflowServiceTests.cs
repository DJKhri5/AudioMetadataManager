using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.BatchWorkflow;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.Tests;
using Xunit;

namespace AudioMetadataManager.Tests
    .Simulation.Application.BatchWorkflow;

public sealed class ProductiveBatchWorkflowServiceTests
{
    [Fact]
    public void NullCoordinator_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new ProductiveBatchWorkflowService(
                    null!));
    }

    [Fact]
    public async Task NullBatch_IsRejected()
    {
        ProductiveBatchWorkflowService service =
            new(
                new FakeTwoPhaseCoordinator());

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () =>
                await service.PrepareAsync(
                    null!));
    }

    [Fact]
    public async Task InvalidBatch_IsRejected()
    {
        ProductiveBatchWorkflowService service =
            new(
                new FakeTwoPhaseCoordinator());

        MetadataApplyBatchRequest batchRequest =
            new();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await service.PrepareAsync(
                    batchRequest));
    }

    [Fact]
    public async Task PrepareAsync_PreservesBatchAndResult()
    {
        MetadataApplyBatchRequest batchRequest =
            CreateValidBatch();

        FakeTwoPhaseCoordinator coordinator =
            new();

        ProductiveBatchWorkflowService service =
            new(
                coordinator);

        ProductiveBatchPreparation preparation =
            await service.PrepareAsync(
                batchRequest);

        Assert.Same(
            batchRequest,
            preparation.BatchRequest);

        Assert.Same(
            coordinator.PreparationResult,
            preparation.PreparationResult);

        Assert.True(
            preparation.IsReadyForDecision);

        Assert.Equal(
            1,
            coordinator.PrepareCallCount);
    }

    [Fact]
    public async Task CompleteAsync_ForwardsApprovedDecision()
    {
        MetadataApplyBatchRequest batchRequest =
            CreateValidBatch();

        FakeTwoPhaseCoordinator coordinator =
            new();

        ProductiveBatchWorkflowService service =
            new(
                coordinator);

        ProductiveBatchPreparation preparation =
            await service.PrepareAsync(
                batchRequest);

        MetadataProductiveBatchCompletionResult result =
            await service.CompleteAsync(
                preparation,
                MetadataPromotionDecision.Approved);

        Assert.Same(
            coordinator.CompletionResult,
            result);

        Assert.Equal(
            1,
            coordinator.CompleteCallCount);

        Assert.Equal(
            MetadataPromotionDecision.Approved,
            coordinator.LastDecision);
    }

    [Fact]
    public async Task UnsupportedDecision_IsRejected()
    {
        MetadataApplyBatchRequest batchRequest =
            CreateValidBatch();

        FakeTwoPhaseCoordinator coordinator =
            new();

        ProductiveBatchWorkflowService service =
            new(
                coordinator);

        ProductiveBatchPreparation preparation =
            await service.PrepareAsync(
                batchRequest);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () =>
                await service.CompleteAsync(
                    preparation,
                    MetadataPromotionDecision.Pending));

        Assert.Equal(
            0,
            coordinator.CompleteCallCount);
    }

    [Fact]
    public async Task CompleteAsync_ConsumesPreparation()
    {
        MetadataApplyBatchRequest batchRequest =
            CreateValidBatch();

        FakeTwoPhaseCoordinator coordinator =
            new();

        ProductiveBatchWorkflowService service =
            new(
                coordinator);

        ProductiveBatchPreparation preparation =
            await service.PrepareAsync(
                batchRequest);

        Assert.True(
            preparation.IsReadyForDecision);

        Assert.False(
            preparation.WasConsumed);

        await service.CompleteAsync(
            preparation,
            MetadataPromotionDecision.Approved);

        Assert.True(
            preparation.WasConsumed);

        Assert.False(
            preparation.IsReadyForDecision);

        Assert.Equal(
            1,
            coordinator.CompleteCallCount);
    }

    [Fact]
    public async Task ConsumedPreparation_CannotBeCompletedTwice()
    {
        MetadataApplyBatchRequest batchRequest =
            CreateValidBatch();

        FakeTwoPhaseCoordinator coordinator =
            new();

        ProductiveBatchWorkflowService service =
            new(
                coordinator);

        ProductiveBatchPreparation preparation =
            await service.PrepareAsync(
                batchRequest);

        await service.CompleteAsync(
            preparation,
            MetadataPromotionDecision.Approved);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await service.CompleteAsync(
                    preparation,
                    MetadataPromotionDecision.Approved));

        Assert.Equal(
            1,
            coordinator.CompleteCallCount);
    }

    private static MetadataApplyBatchRequest
        CreateValidBatch()
    {
        MetadataApplyRequest request =
            new()
            {
                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    @"C:\Tests\workflow-batch.mp3",

                FileName =
                    "workflow-batch.mp3",

                Changes =
                    new[]
                    {
                        new MetadataFieldChange
                        {
                            Field =
                                MetadataField.Genre,

                            OriginalValue =
                                "Old Genre",

                            NewValue =
                                "New Genre",

                            WasManuallyApproved =
                                true
                        }
                    }
            };

        return
            new MetadataApplyBatchRequest
            {
                Requests =
                    new[]
                    {
                        request
                    }
            };
    }

    private sealed class FakeTwoPhaseCoordinator :
        IMetadataProductiveTwoPhaseBatchCoordinator
    {
        public int PrepareCallCount
        {
            get;
            private set;
        }

        public int CompleteCallCount
        {
            get;
            private set;
        }

        public MetadataPromotionDecision LastDecision
        {
            get;
            private set;
        } =
            MetadataPromotionDecision.NotRequested;

        public MetadataProductiveBatchPreparationResult
            PreparationResult
        {
            get;
        } =
            new()
            {
                BatchId =
                    Guid.NewGuid(),

                RequestedCount =
                    1,

                StartedAtUtc =
                    DateTime.UtcNow,

                FinishedAtUtc =
                    DateTime.UtcNow,

                PreparationResults =
                    new[]
                    {
                        new MetadataProductiveApplicationResult
                        {
                            IsolatedExecutionResult =
                                TestProductiveResultFactory
                                    .CreateSuccessfulIsolatedExecution(),

                            PromotionDecision =
                                MetadataPromotionDecision.Pending
                        }
                    }
            };

        public MetadataProductiveBatchCompletionResult
            CompletionResult
        {
            get;
        } =
            new()
            {
                BatchId =
                    Guid.NewGuid(),

                RequestedCount =
                    1,

                PromotionDecision =
                    MetadataPromotionDecision.Approved,

                StartedAtUtc =
                    DateTime.UtcNow,

                FinishedAtUtc =
                    DateTime.UtcNow
            };

        public Task<MetadataProductiveBatchPreparationResult>
            PrepareAsync(
                MetadataApplyBatchRequest batchRequest,
                CancellationToken cancellationToken = default)
        {
            PrepareCallCount++;

            return
                Task.FromResult(
                    PreparationResult);
        }

        public Task<MetadataProductiveBatchCompletionResult>
            CompleteAsync(
                MetadataProductiveBatchPreparationResult
                    preparationResult,
                MetadataPromotionDecision promotionDecision,
                CancellationToken cancellationToken = default)
        {
            CompleteCallCount++;

            LastDecision =
                promotionDecision;

            return
                Task.FromResult(
                    CompletionResult);
        }
    }
}