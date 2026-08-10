using Xunit;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Coordination;

public sealed class
    MetadataProductiveApplicationBatchCoordinatorTests
{
    [Fact]
    public void NullIndividualCoordinator_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new MetadataProductiveApplicationBatchCoordinator(
                    null!));
    }

    [Fact]
    public async Task NullBatch_IsRejected()
    {
        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () =>
                await coordinator.ExecuteAsync(
                    null!,
                    MetadataPromotionDecision.Declined));
    }

    [Fact]
    public async Task InvalidBatch_ReturnsAuditableEmptyResult()
    {
        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        MetadataApplyBatchRequest batch =
            new();

        MetadataApplyBatchResult result =
            await coordinator.ExecuteAsync(
                batch,
                MetadataPromotionDecision.Declined);

        Assert.False(
            batch.IsStructurallyValid);

        Assert.Equal(
            0,
            result.TotalCount);

        Assert.NotEmpty(
            result.Messages);

        Assert.Equal(
            0,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            0,
            individualCoordinator.CompleteCallCount);
    }

    [Fact]
    public async Task UnsupportedDecision_IsRejectedBeforeExecution()
    {
        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                1);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await coordinator.ExecuteAsync(
                    batch,
                    MetadataPromotionDecision.Pending));

        Assert.Equal(
            0,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            0,
            individualCoordinator.CompleteCallCount);
    }

    [Fact]
    public async Task DeclinedDecision_IsForwardedToAllRequests()
    {
        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        MetadataApplyBatchResult result =
            await coordinator.ExecuteAsync(
                batch,
                MetadataPromotionDecision.Declined);

        Assert.Equal(
            3,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            3,
            individualCoordinator.CompleteCallCount);

        Assert.Equal(
            3,
            result.Results.Count);

        Assert.Equal(
            3,
            individualCoordinator.PromotionDecisions.Count);

        Assert.All(
            individualCoordinator.PromotionDecisions,
            decision =>
                Assert.Equal(
                    MetadataPromotionDecision.Declined,
                    decision));
    }

    [Fact]
    public async Task ApprovedDecision_IsForwardedToAllRequests()
    {
        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        MetadataApplyBatchResult result =
            await coordinator.ExecuteAsync(
                batch,
                MetadataPromotionDecision.Approved);

        Assert.Equal(
            3,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            3,
            individualCoordinator.CompleteCallCount);

        Assert.Equal(
            3,
            result.Results.Count);

        Assert.Equal(
            3,
            individualCoordinator.PromotionDecisions.Count);

        Assert.All(
            individualCoordinator.PromotionDecisions,
            decision =>
                Assert.Equal(
                    MetadataPromotionDecision.Approved,
                    decision));
    }

    [Fact]
    public async Task PrepareException_StopsBatchAndPreservesPartialResult()
    {
        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new()
                {
                    ThrowOnPrepareCall =
                        2
                };

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        MetadataApplyBatchResult result =
            await coordinator.ExecuteAsync(
                batch,
                MetadataPromotionDecision.Declined);

        Assert.Equal(
            2,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            1,
            individualCoordinator.CompleteCallCount);

        Assert.Equal(
            2,
            result.Results.Count);

        Assert.Equal(
            "Fallo simulado durante PrepareAsync.",
            result.Results[1].ErrorMessage);

        Assert.Contains(
            result.Messages,
            message =>
                message.Contains(
                    "1 solicitud(es) no fueron ejecutadas",
                    StringComparison.Ordinal));
    }

    [Fact]
    public async Task CompleteException_StopsBatchAndPreservesPartialResult()
    {
        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new()
                {
                    ThrowOnCompleteCall =
                        2
                };

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        MetadataApplyBatchResult result =
            await coordinator.ExecuteAsync(
                batch,
                MetadataPromotionDecision.Approved);

        Assert.Equal(
            2,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            2,
            individualCoordinator.CompleteCallCount);

        Assert.Equal(
            2,
            result.Results.Count);

        Assert.Equal(
            MetadataPromotionDecision.Approved,
            result.Results[0].PromotionDecision);

        Assert.Equal(
            "Fallo simulado durante CompleteAsync.",
            result.Results[1].ErrorMessage);

        Assert.Contains(
            result.Messages,
            message =>
                message.Contains(
                    "1 solicitud(es) no fueron ejecutadas",
                    StringComparison.Ordinal));
    }

    [Fact]
    public async Task MidBatchCancellation_StopsBeforeNextRequest()
    {
        using CancellationTokenSource
            cancellationSource =
                new();

        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new()
                {
                    CancellationSource =
                        cancellationSource,

                    CancelAfterCompleteCall =
                        1
                };

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        MetadataApplyBatchRequest batch =
            CreateValidBatch(
                3);

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () =>
                await coordinator.ExecuteAsync(
                    batch,
                    MetadataPromotionDecision.Declined,
                    cancellationSource.Token));

        Assert.True(
            cancellationSource.IsCancellationRequested);

        Assert.Equal(
            1,
            individualCoordinator.PrepareCallCount);

        Assert.Equal(
            1,
            individualCoordinator.CompleteCallCount);
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
                CreateValidRequest(
                    index));
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

    private static MetadataApplyRequest
        CreateValidRequest(
            int index)
    {
        return
            new MetadataApplyRequest
            {
                RequestId =
                    Guid.NewGuid(),

                PlanId =
                    Guid.NewGuid(),

                CreatedAtUtc =
                    DateTime.UtcNow,

                FilePath =
                    $@"C:\Tests\batch-coordinator-{index}.flac",

                FileName =
                    $"batch-coordinator-{index}.flac",

                RequireBackup =
                    true,

                RequirePostWriteVerification =
                    true,

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
                                $"Diagnostic Genre {index}",

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
            };
    }

    private sealed class
        RecordingProductiveApplicationCoordinator :
            IMetadataProductiveApplicationCoordinator
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

        public int? ThrowOnPrepareCall
        {
            get;
            init;
        }

        public int? ThrowOnCompleteCall
        {
            get;
            init;
        }

        public CancellationTokenSource?
            CancellationSource
        {
            get;
            init;
        }

        public int? CancelAfterCompleteCall
        {
            get;
            init;
        }

        public List<MetadataPromotionDecision>
            PromotionDecisions
        {
            get;
        } =
            new();

        public Task<MetadataProductiveApplicationResult>
            PrepareAsync(
                MetadataApplyRequest request,
                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            cancellationToken
                .ThrowIfCancellationRequested();

            PrepareCallCount++;

            if (ThrowOnPrepareCall ==
                PrepareCallCount)
            {
                throw new InvalidOperationException(
                    "Fallo simulado durante PrepareAsync.");
            }

            MetadataProductiveApplicationResult result =
                new()
                {
                    Messages =
                        new[]
                        {
                            "Preparación individual simulada."
                        }
                };

            return
                Task.FromResult(
                    result);
        }

        public Task<MetadataProductiveApplicationResult>
            CompleteAsync(
                MetadataProductiveApplicationResult
                    preparedResult,
                MetadataPromotionDecision promotionDecision,
                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                preparedResult);

            cancellationToken
                .ThrowIfCancellationRequested();

            CompleteCallCount++;

            if (ThrowOnCompleteCall ==
                CompleteCallCount)
            {
                throw new InvalidOperationException(
                    "Fallo simulado durante CompleteAsync.");
            }

            PromotionDecisions.Add(
                promotionDecision);

            MetadataProductiveApplicationResult result =
                new()
                {
                    PromotionDecision =
                        promotionDecision,

                    FinalCleanupWasAttempted =
                        true,

                    FinalCleanupWasSuccessful =
                        true,

                    Messages =
                        new[]
                        {
                            "Finalización individual simulada."
                        }
                };

            if (CancellationSource is not null &&
                CancelAfterCompleteCall ==
                    CompleteCallCount)
            {
                CancellationSource.Cancel();
            }

            return
                Task.FromResult(
                    result);
        }
    }
}