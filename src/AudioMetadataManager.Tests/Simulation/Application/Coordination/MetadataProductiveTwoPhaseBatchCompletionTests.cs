using Xunit;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Coordination;

public sealed class
    MetadataProductiveTwoPhaseBatchCompletionTests
{
    [Fact]
    public async Task UnsupportedDecision_IsRejected()
    {
        RecordingCoordinator individualCoordinator =
            new();

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataProductiveBatchPreparationResult preparation =
            CreateReadyPreparation(
                2);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () =>
                await coordinator.CompleteAsync(
                    preparation,
                    MetadataPromotionDecision.Pending));
    }

    [Fact]
    public async Task PreparationNotReady_IsRejected()
    {
        RecordingCoordinator individualCoordinator =
            new();

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataProductiveBatchPreparationResult preparation =
            new();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await coordinator.CompleteAsync(
                    preparation,
                    MetadataPromotionDecision.Approved));
    }

    [Fact]
    public async Task Declined_CompletesAllPreparationsSafely()
    {
        RecordingCoordinator individualCoordinator =
            new();

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataProductiveBatchPreparationResult preparation =
            CreateReadyPreparation(
                3);

        MetadataProductiveBatchCompletionResult result =
            await coordinator.CompleteAsync(
                preparation,
                MetadataPromotionDecision.Declined);

        Assert.True(
            result.WasSuccessful);

        Assert.Equal(
            3,
            result.DecisionResultCount);

        Assert.Equal(
            3,
            result.SuccessfulDecisionCount);

        Assert.Equal(
            0,
            result.FailedDecisionCount);

        Assert.Equal(
            0,
            result.CleanupResultCount);

        Assert.All(
            individualCoordinator.PromotionDecisions,
            decision =>
                Assert.Equal(
                    MetadataPromotionDecision.Declined,
                    decision));
    }

    [Fact]
    public async Task Approved_CompletesAllPreparationsSuccessfully()
    {
        RecordingCoordinator individualCoordinator =
            new();

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataProductiveBatchPreparationResult preparation =
            CreateReadyPreparation(
                3);

        MetadataProductiveBatchCompletionResult result =
            await coordinator.CompleteAsync(
                preparation,
                MetadataPromotionDecision.Approved);

        Assert.True(
            result.WasSuccessful);

        Assert.Equal(
            3,
            result.DecisionResultCount);

        Assert.Equal(
            3,
            result.SuccessfulDecisionCount);

        Assert.Equal(
            0,
            result.CleanupResultCount);

        Assert.All(
            individualCoordinator.PromotionDecisions,
            decision =>
                Assert.Equal(
                    MetadataPromotionDecision.Approved,
                    decision));
    }

    [Fact]
    public async Task ApprovedFailure_CleansRemainingPreparations()
    {
        RecordingCoordinator individualCoordinator =
            new()
            {
                ReturnFailedApprovedOnCall =
                    2
            };

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataProductiveBatchPreparationResult preparation =
            CreateReadyPreparation(
                3);

        MetadataProductiveBatchCompletionResult result =
            await coordinator.CompleteAsync(
                preparation,
                MetadataPromotionDecision.Approved);

        Assert.False(
            result.WasSuccessful);

        Assert.Equal(
            2,
            result.DecisionResultCount);

        Assert.Equal(
            1,
            result.SuccessfulDecisionCount);

        Assert.Equal(
            1,
            result.FailedDecisionCount);

        Assert.Equal(
            1,
            result.CleanupResultCount);

        Assert.Equal(
            MetadataPromotionDecision.Declined,
            result.CleanupResults[0].PromotionDecision);
    }

    [Fact]
    public async Task ApprovedException_CleansFailedAndRemainingPendingPreparations()
    {
        RecordingCoordinator individualCoordinator =
            new()
            {
                ThrowOnCompleteCall =
                    2
            };

        MetadataProductiveTwoPhaseBatchCoordinator coordinator =
            new(
                individualCoordinator);

        MetadataProductiveBatchPreparationResult preparation =
            CreateReadyPreparation(
                3);

        MetadataProductiveBatchCompletionResult result =
            await coordinator.CompleteAsync(
                preparation,
                MetadataPromotionDecision.Approved);

        Assert.False(
            result.WasSuccessful);

        Assert.Equal(
            2,
            result.DecisionResultCount);

        Assert.Equal(
            "Fallo simulado durante CompleteAsync.",
            result.DecisionResults[1].ErrorMessage);

        /*
         * Se limpian la preparación que falló y la posterior,
         * porque ninguna de las dos llegó a finalizar.
         */
        Assert.Equal(
            2,
            result.CleanupResultCount);
    }

    private static MetadataProductiveBatchPreparationResult
        CreateReadyPreparation(
            int count)
    {
        List<MetadataProductiveApplicationResult>
            preparations =
                new();

        for (int index = 0;
            index < count;
            index++)
        {
            preparations.Add(
                new MetadataProductiveApplicationResult
                {
                    IsolatedExecutionResult =
                        TestProductiveResultFactory
                            .CreateSuccessfulIsolatedExecution(),

                    PromotionDecision =
                        MetadataPromotionDecision.Pending
                });
        }

        return
            new MetadataProductiveBatchPreparationResult
            {
                BatchId =
                    Guid.NewGuid(),

                RequestedCount =
                    count,

                PreparationResults =
                    preparations
            };
    }

    private sealed class RecordingCoordinator :
        IMetadataProductiveApplicationCoordinator
    {
        public int CompleteCallCount { get; private set; }

        public int? ReturnFailedApprovedOnCall { get; init; }

        public int? ThrowOnCompleteCall { get; init; }

        public List<MetadataPromotionDecision>
            PromotionDecisions
        { get; } =
            new();

        public Task<MetadataProductiveApplicationResult>
            PrepareAsync(
                MetadataApplyRequest request,
                CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "Este test solamente utiliza CompleteAsync.");
        }

        public Task<MetadataProductiveApplicationResult>
            CompleteAsync(
                MetadataProductiveApplicationResult preparedResult,
                MetadataPromotionDecision promotionDecision,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CompleteCallCount++;

            if (ThrowOnCompleteCall == CompleteCallCount)
            {
                throw new InvalidOperationException(
                    "Fallo simulado durante CompleteAsync.");
            }

            PromotionDecisions.Add(
                promotionDecision);

            if (promotionDecision ==
                MetadataPromotionDecision.Approved)
            {
                if (ReturnFailedApprovedOnCall ==
                    CompleteCallCount)
                {
                    return Task.FromResult(
                        new MetadataProductiveApplicationResult
                        {
                            PromotionDecision =
                                MetadataPromotionDecision.Approved,

                            ErrorMessage =
                                "Promoción fallida simulada.",

                            FinalCleanupWasAttempted =
                                true,

                            FinalCleanupWasSuccessful =
                                true
                        });
                }

                return Task.FromResult(
                    TestProductiveResultFactory
                        .CreateSuccessfullyPromotedResult());
            }

            return Task.FromResult(
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
                });
        }
    }
}