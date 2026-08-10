using Xunit;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Coordination;

public sealed class
    MetadataProductiveTwoPhaseBatchPreparationTests
{
    [Fact]
    public async Task InvalidBatch_IsNotReadyForDecision()
    {
        RecordingCoordinator individualCoordinator =
            new();

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataApplyBatchRequest batch =
            new();

        MetadataProductiveBatchPreparationResult result =
            await coordinator.PrepareAsync(
                batch);

        Assert.False(
            result.IsReadyForDecision);

        Assert.Equal(
            0,
            result.RequestedCount);

        Assert.Equal(
            0,
            result.ResultCount);

        Assert.Equal(
            0,
            individualCoordinator.PrepareCallCount);
    }

    [Fact]
    public async Task AllRequestsPrepared_BatchIsReadyForDecision()
    {
        RecordingCoordinator individualCoordinator =
            new();

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        MetadataProductiveBatchPreparationResult result =
            await coordinator.PrepareAsync(
                batch);

        Assert.True(
            result.IsReadyForDecision);

        Assert.False(
            result.WasAbortedAndCleanedUp);

        Assert.Equal(
            3,
            result.RequestedCount);

        Assert.Equal(
            3,
            result.ResultCount);

        Assert.Equal(
            3,
            result.VerifiedPreparationCount);

        Assert.Equal(
            3,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            0,
            individualCoordinator.CompleteCallCount);
    }

    [Fact]
    public async Task PrepareException_StopsAndCleansEarlierPreparations()
    {
        RecordingCoordinator individualCoordinator =
            new()
            {
                ThrowOnPrepareCall =
                    2
            };

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        MetadataProductiveBatchPreparationResult result =
            await coordinator.PrepareAsync(
                batch);

        Assert.False(
            result.IsReadyForDecision);

        Assert.True(
            result.WasAbortedAndCleanedUp);

        Assert.Equal(
            2,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            1,
            individualCoordinator.CompleteCallCount);

        Assert.Single(
            individualCoordinator.PromotionDecisions);

        Assert.Equal(
            MetadataPromotionDecision.Declined,
            individualCoordinator.PromotionDecisions[0]);
    }

    [Fact]
    public async Task InvalidPreparationResult_StopsAndCleansPendingPreparations()
    {
        RecordingCoordinator individualCoordinator =
            new()
            {
                ReturnInvalidPreparationOnCall =
                    2
            };

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        MetadataProductiveBatchPreparationResult result =
            await coordinator.PrepareAsync(
                batch);

        Assert.False(
            result.IsReadyForDecision);

        Assert.True(
            result.WasAbortedAndCleanedUp);

        Assert.Equal(
            2,
            individualCoordinator.PrepareCallCount);

        /*
         * Solo la primera preparación era realmente Pending
         * y verificable. La segunda es deliberadamente inválida.
         */
        Assert.Equal(
            1,
            individualCoordinator.CompleteCallCount);

        Assert.Single(
            individualCoordinator.PromotionDecisions);

        Assert.Equal(
            MetadataPromotionDecision.Declined,
            individualCoordinator.PromotionDecisions[0]);
    }

    [Fact]
    public async Task Cancellation_CleansPreparedItemsBeforeThrowing()
    {
        using CancellationTokenSource cancellationSource =
            new();

        RecordingCoordinator individualCoordinator =
            new()
            {
                CancellationSource =
                    cancellationSource,

                CancelAfterPrepareCall =
                    1
            };

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () =>
                await coordinator.PrepareAsync(
                    batch,
                    cancellationSource.Token));

        Assert.Equal(
            1,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            1,
            individualCoordinator.CompleteCallCount);

        Assert.Single(
            individualCoordinator.PromotionDecisions);

        Assert.Equal(
            MetadataPromotionDecision.Declined,
            individualCoordinator.PromotionDecisions[0]);
    }

    private static MetadataApplyBatchRequest
        CreateValidBatch(
            int requestCount)
    {
        List<MetadataApplyRequest> requests =
            new();

        for (int index = 1;
            index <= requestCount;
            index++)
        {
            requests.Add(
                new MetadataApplyRequest
                {
                    RequestId =
                        Guid.NewGuid(),

                    PlanId =
                        Guid.NewGuid(),

                    FilePath =
                        $@"C:\Tests\two-phase-{index}.flac",

                    FileName =
                        $"two-phase-{index}.flac",

                    Changes =
                        new[]
                        {
                            new MetadataFieldChange
                            {
                                Field =
                                    MetadataField.Genre,

                                OriginalValue =
                                    "House",

                                NewValue =
                                    $"Genre {index}",

                                WasManuallyApproved =
                                    true,

                                Confidence =
                                    1.0,

                                SupportingSources =
                                    new[]
                                    {
                                        "Automated xUnit test"
                                    }
                            }
                        }
                });
        }

        return
            new MetadataApplyBatchRequest
            {
                BatchId =
                    Guid.NewGuid(),

                Requests =
                    requests
            };
    }

    private sealed class RecordingCoordinator :
        IMetadataProductiveApplicationCoordinator
    {
        public int PrepareCallCount { get; private set; }

        public int CompleteCallCount { get; private set; }

        public int? ThrowOnPrepareCall { get; init; }

        public int? ReturnInvalidPreparationOnCall { get; init; }

        public CancellationTokenSource? CancellationSource { get; init; }

        public int? CancelAfterPrepareCall { get; init; }

        public List<MetadataPromotionDecision>
            PromotionDecisions
        { get; } =
            new();

        public Task<MetadataProductiveApplicationResult>
            PrepareAsync(
                MetadataApplyRequest request,
                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            cancellationToken.ThrowIfCancellationRequested();

            PrepareCallCount++;

            if (ThrowOnPrepareCall == PrepareCallCount)
            {
                throw new InvalidOperationException(
                    "Fallo simulado durante PrepareAsync.");
            }

            MetadataProductiveApplicationResult result =
                ReturnInvalidPreparationOnCall == PrepareCallCount
                    ? new MetadataProductiveApplicationResult
                    {
                        PromotionDecision =
                            MetadataPromotionDecision.NotRequested,

                        ErrorMessage =
                            "Preparación inválida simulada."
                    }
                    : CreatePendingPreparation();

            if (CancellationSource is not null &&
                CancelAfterPrepareCall == PrepareCallCount)
            {
                CancellationSource.Cancel();
            }

            return Task.FromResult(
                result);
        }

        public Task<MetadataProductiveApplicationResult>
            CompleteAsync(
                MetadataProductiveApplicationResult preparedResult,
                MetadataPromotionDecision promotionDecision,
                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                preparedResult);

            /*
             * La limpieza deliberadamente usa CancellationToken.None
             * desde el coordinador real.
             */
            cancellationToken.ThrowIfCancellationRequested();

            CompleteCallCount++;

            PromotionDecisions.Add(
                promotionDecision);

            return Task.FromResult(
                CreateSafelyDeclinedResult());
        }

        private static MetadataProductiveApplicationResult
            CreatePendingPreparation()
        {
            return
                new MetadataProductiveApplicationResult
                {
                    IsolatedExecutionResult =
                        TestProductiveResultFactory
                            .CreateSuccessfulIsolatedExecution(),

                    PromotionDecision =
                        MetadataPromotionDecision.Pending
                };
        }

        private static MetadataProductiveApplicationResult
            CreateSafelyDeclinedResult()
        {
            return
                new MetadataProductiveApplicationResult
                {
                    IsolatedExecutionResult =
                        TestProductiveResultFactory
                            .CreateSuccessfulIsolatedExecution(),

                    PromotionDecision =
                        MetadataPromotionDecision.Declined,

                    FinalCleanupWasAttempted =
                        true,

                    FinalCleanupWasSuccessful =
                        true
                };
        }
    }
}