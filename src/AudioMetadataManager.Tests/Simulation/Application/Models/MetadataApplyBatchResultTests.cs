using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using Xunit;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Models;

public sealed class MetadataApplyBatchResultTests
{
    [Fact]
    public void EmptyResult_IsNotSuccessful()
    {
        MetadataApplyBatchResult result =
            new();

        Assert.Equal(
            0,
            result.TotalCount);

        Assert.Equal(
            0,
            result.SuccessfulCount);

        Assert.Equal(
            0,
            result.FailedCount);

        Assert.False(
            result.WasSuccessful);
    }

    [Fact]
    public void SuccessfulResults_AreCountedCorrectly()
    {
        MetadataApplyBatchResult result =
            new()
            {
                Results =
                    new[]
                    {
                        CreateSuccessfulResult(),
                        CreateSuccessfulResult()
                    }
            };

        Assert.Equal(
            2,
            result.TotalCount);

        Assert.Equal(
            2,
            result.SuccessfulCount);

        Assert.Equal(
            0,
            result.FailedCount);

        Assert.True(
            result.WasSuccessful);
    }

    [Fact]
    public void PartialFailure_IsDetected()
    {
        MetadataApplyBatchResult result =
            new()
            {
                Results =
                    new[]
                    {
                        CreateSuccessfulResult(),
                        new MetadataProductiveApplicationResult()
                    }
            };

        Assert.Equal(
            2,
            result.TotalCount);

        Assert.Equal(
            1,
            result.SuccessfulCount);

        Assert.Equal(
            1,
            result.FailedCount);

        Assert.False(
            result.WasSuccessful);
    }

    [Fact]
    public void Duration_IsCalculatedFromRecordedTimes()
    {
        DateTime startedAtUtc =
            new(
                2026,
                8,
                9,
                12,
                0,
                0,
                DateTimeKind.Utc);

        MetadataApplyBatchResult result =
            new()
            {
                StartedAtUtc =
                    startedAtUtc,

                FinishedAtUtc =
                    startedAtUtc.AddSeconds(
                        5)
            };

        Assert.Equal(
            TimeSpan.FromSeconds(
                5),
            result.Duration);
    }

    [Fact]
    public void InvalidTimeRange_ProducesZeroDuration()
    {
        DateTime startedAtUtc =
            new(
                2026,
                8,
                9,
                12,
                0,
                5,
                DateTimeKind.Utc);

        MetadataApplyBatchResult result =
            new()
            {
                StartedAtUtc =
                    startedAtUtc,

                FinishedAtUtc =
                    startedAtUtc.AddSeconds(
                        -5)
            };

        Assert.Equal(
            TimeSpan.Zero,
            result.Duration);
    }

    private static MetadataProductiveApplicationResult
        CreateSuccessfulResult()
    {
        FileIsolationContext isolationContext =
            new()
            {
                OriginalFilePath =
                    @"C:\Tests\original.flac",

                OriginalFileName =
                    "original.flac",

                WorkingCopyPath =
                    @"C:\Tests\working.flac",

                WorkingBackupPath =
                    @"C:\Tests\backup.flac",

                TestDirectoryPath =
                    @"C:\Tests",

                OriginalHashBefore =
                    "ORIGINAL_HASH",

                WorkingCopyHashBefore =
                    "WORKING_HASH_BEFORE",

                WorkingBackupHash =
                    "WORKING_HASH_BEFORE"
            };

        FileIsolationVerificationResult
            isolationVerification =
                new()
                {
                    Context =
                        isolationContext,

                    OriginalHashAfter =
                        "ORIGINAL_HASH",

                    WorkingCopyHashAfter =
                        "WORKING_HASH_AFTER"
                };

        MetadataApplyResult applyResult =
            new()
            {
                Status =
                    MetadataApplyStatus.Completed
            };

        MetadataApplicationStageResult
            stageResult =
                new()
                {
                    Stage =
                        MetadataApplicationStage.Finalization,

                    Status =
                        MetadataApplicationStageStatus.Completed
                };

        MetadataApplicationPipelineResult
            pipelineResult =
                new()
                {
                    StopReason =
                        MetadataApplicationStopReason.Completed,

                    ApplyResult =
                        applyResult,

                    StageResults =
                        new[]
                        {
                            stageResult
                        }
                };

        MetadataApplicationIsolatedExecutionResult
            isolatedExecutionResult =
                new()
                {
                    IsolationContext =
                        isolationContext,

                    PipelineResult =
                        pipelineResult,

                    IsolationVerification =
                        isolationVerification,

                    EnvironmentWasPreserved =
                        true
                };

        MetadataApplicationPromotionResult
            promotionResult =
                new()
                {
                    InputsWereValidated =
                        true,

                    ProductiveBackupWasCreated =
                        true,

                    ProductiveBackupWasVerified =
                        true,

                    ReplacementWasExecuted =
                        true,

                    PromotedFileWasVerified =
                        true,

                    RollbackWasAttempted =
                        false
                };

        return
            new MetadataProductiveApplicationResult
            {
                IsolatedExecutionResult =
                    isolatedExecutionResult,

                PromotionDecision =
                    MetadataPromotionDecision.Approved,

                PromotionResult =
                    promotionResult,

                FinalCleanupWasAttempted =
                    true,

                FinalCleanupWasSuccessful =
                    true
            };
    }
}