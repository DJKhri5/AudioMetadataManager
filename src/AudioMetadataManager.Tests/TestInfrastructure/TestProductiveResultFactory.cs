using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

namespace AudioMetadataManager.Tests;

internal static class TestProductiveResultFactory
{
    public static MetadataApplicationIsolatedExecutionResult
        CreateSuccessfulIsolatedExecution()
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

        FileIsolationVerificationResult verification =
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

        MetadataApplicationStageResult stageResult =
            new()
            {
                Stage =
                    MetadataApplicationStage.Finalization,

                Status =
                    MetadataApplicationStageStatus.Completed
            };

        MetadataApplicationPipelineResult pipelineResult =
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

        return
            new MetadataApplicationIsolatedExecutionResult
            {
                IsolationContext =
                    isolationContext,

                PipelineResult =
                    pipelineResult,

                IsolationVerification =
                    verification,

                EnvironmentWasPreserved =
                    true
            };
    }

    public static MetadataProductiveApplicationResult
        CreateSuccessfullyPromotedResult()
    {
        MetadataApplicationPromotionResult promotionResult =
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
                    CreateSuccessfulIsolatedExecution(),

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