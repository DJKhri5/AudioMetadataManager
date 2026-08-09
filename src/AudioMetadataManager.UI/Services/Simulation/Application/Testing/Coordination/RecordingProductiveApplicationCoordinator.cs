using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Coordinador productivo individual controlado utilizado por
/// las pruebas de coordinación.
///
/// No accede al sistema de archivos ni ejecuta el pipeline real.
/// Produce resultados sintéticos capaces de representar una
/// preparación verificada pendiente.
/// </summary>
internal sealed class RecordingProductiveApplicationCoordinator :
    IMetadataProductiveApplicationCoordinator
{
    private readonly List<MetadataPromotionDecision>
        _promotionDecisions =
            new();

    public int PrepareCallCount { get; private set; }

    public int CompleteCallCount { get; private set; }

    public int? ThrowOnPrepareCall { get; init; }

    public int? ThrowOnCompleteCall { get; init; }

    public int? ReturnPrepareErrorOnCall { get; init; }

    public int? ReturnCompleteErrorOnCall { get; init; }

    public CancellationTokenSource?
        CancellationSource
    { get; init; }

    public int?
        CancelAfterCompleteCall
    { get; init; }

    public IReadOnlyList<MetadataPromotionDecision>
        PromotionDecisions =>
            _promotionDecisions;

    public MetadataPromotionDecision
        LastPromotionDecision
    { get; private set; } =
        MetadataPromotionDecision.NotRequested;

    public Task<MetadataProductiveApplicationResult>
        PrepareAsync(
            MetadataApplyRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        cancellationToken.ThrowIfCancellationRequested();

        PrepareCallCount++;

        if (ThrowOnPrepareCall ==
            PrepareCallCount)
        {
            throw new InvalidOperationException(
                "Fallo simulado durante PrepareAsync.");
        }

        if (ReturnPrepareErrorOnCall ==
            PrepareCallCount)
        {
            return Task.FromResult(
                new MetadataProductiveApplicationResult
                {
                    PromotionDecision =
                        MetadataPromotionDecision.Unavailable,

                    ErrorMessage =
                        "Fallo controlado durante PrepareAsync.",

                    Messages =
                        new[]
                        {
                            "Fallo controlado devuelto durante " +
                            "PrepareAsync."
                        }
                });
        }

        MetadataApplicationIsolatedExecutionResult
            isolatedExecutionResult =
                CreateVerifiedIsolatedExecutionResult(
                    request);

        MetadataProductiveApplicationResult result =
            new()
            {
                IsolatedExecutionResult =
                    isolatedExecutionResult,

                PromotionDecision =
                    MetadataPromotionDecision.Pending,

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
            MetadataProductiveApplicationResult preparedResult,
            MetadataPromotionDecision promotionDecision,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            preparedResult);

        cancellationToken.ThrowIfCancellationRequested();

        CompleteCallCount++;

        LastPromotionDecision =
            promotionDecision;

        _promotionDecisions.Add(
            promotionDecision);

        if (ThrowOnCompleteCall ==
            CompleteCallCount)
        {
            throw new InvalidOperationException(
                "Fallo simulado durante CompleteAsync.");
        }

        if (ReturnCompleteErrorOnCall ==
            CompleteCallCount)
        {
            return Task.FromResult(
                new MetadataProductiveApplicationResult
                {
                    IsolatedExecutionResult =
                        preparedResult.IsolatedExecutionResult,

                    PromotionDecision =
                        promotionDecision,

                    ErrorMessage =
                        "Fallo controlado durante CompleteAsync.",

                    Messages =
                        new[]
                        {
                            "Fallo controlado devuelto durante " +
                            "CompleteAsync."
                        }
                });
        }

        MetadataApplicationPromotionResult?
            promotionResult =
                promotionDecision ==
                    MetadataPromotionDecision.Approved
                    ? CreateSuccessfulPromotionResult(
                        preparedResult)
                    : null;

        MetadataProductiveApplicationResult result =
            new()
            {
                IsolatedExecutionResult =
                    preparedResult.IsolatedExecutionResult,

                PromotionDecision =
                    promotionDecision,

                PromotionResult =
                    promotionResult,

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

        if (CancelAfterCompleteCall ==
            CompleteCallCount)
        {
            CancellationSource?.Cancel();
        }

        return
            Task.FromResult(
                result);
    }

    /// <summary>
    /// Construye una ejecución aislada sintética que satisface
    /// todas las garantías necesarias para representar una copia
    /// verificada y preservada.
    /// </summary>
    private static MetadataApplicationIsolatedExecutionResult
        CreateVerifiedIsolatedExecutionResult(
            MetadataApplyRequest request)
    {
        string syntheticId =
            Guid.NewGuid().ToString(
                "N");

        string initialHash =
            $"initial-{syntheticId}";

        string modifiedHash =
            $"modified-{syntheticId}";

        FileIsolationContext isolationContext =
            new()
            {
                OriginalFilePath =
                    request.FilePath,

                OriginalFileName =
                    request.FileName,

                WorkingCopyPath =
                    $@"C:\AudioMetadataManager\Tests\{syntheticId}\working.tmp",

                WorkingBackupPath =
                    $@"C:\AudioMetadataManager\Tests\{syntheticId}\backup.tmp",

                TestDirectoryPath =
                    $@"C:\AudioMetadataManager\Tests\{syntheticId}",

                OriginalHashBefore =
                    initialHash,

                WorkingCopyHashBefore =
                    initialHash,

                WorkingBackupHash =
                    initialHash
            };

        FileIsolationVerificationResult
            verificationResult =
                new()
                {
                    Context =
                        isolationContext,

                    OriginalHashAfter =
                        initialHash,

                    WorkingCopyHashAfter =
                        modifiedHash
                };

        MetadataApplyResult applyResult =
            new()
            {
                RequestId =
                    request.RequestId,

                PlanId =
                    request.PlanId,

                FilePath =
                    request.FilePath,

                FileName =
                    request.FileName,

                Status =
                    MetadataApplyStatus.Completed
            };

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

                Message =
                    "Etapa sintética completada."
            };

        MetadataApplicationPipelineResult
            pipelineResult =
                new()
                {
                    Request =
                        request,

                    StartedAtUtc =
                        DateTimeOffset.UtcNow,

                    CompletedAtUtc =
                        DateTimeOffset.UtcNow,

                    StopReason =
                        MetadataApplicationStopReason.Completed,

                    StageResults =
                        new[]
                        {
                            stageResult
                        },

                    ApplyResult =
                        applyResult
                };

        return
            new MetadataApplicationIsolatedExecutionResult
            {
                IsolationContext =
                    isolationContext,

                PipelineResult =
                    pipelineResult,

                IsolationVerification =
                    verificationResult,

                EnvironmentWasPreserved =
                    true
            };
    }

    /// <summary>
    /// Construye una promoción sintética completamente correcta
    /// para las pruebas que necesitan representar Approved.
    /// </summary>
    private static MetadataApplicationPromotionResult
        CreateSuccessfulPromotionResult(
            MetadataProductiveApplicationResult
                preparedResult)
    {
        FileIsolationContext? isolationContext =
            preparedResult.IsolatedExecutionResult?
                .IsolationContext;

        string workingCopyPath =
            isolationContext?.WorkingCopyPath ??
            @"C:\AudioMetadataManager\Tests\working.tmp";

        string destinationPath =
            isolationContext?.OriginalFilePath ??
            @"C:\AudioMetadataManager\Tests\destination.tmp";

        string syntheticHash =
            Guid.NewGuid().ToString(
                "N");

        return
            new MetadataApplicationPromotionResult
            {
                VerifiedWorkingCopyPath =
                    workingCopyPath,

                DestinationFilePath =
                    destinationPath,

                ProductiveBackupPath =
                    destinationPath + ".backup",

                DestinationHashBefore =
                    "before-" + syntheticHash,

                VerifiedCopyHash =
                    "verified-" + syntheticHash,

                DestinationHashAfter =
                    "verified-" + syntheticHash,

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
                    false,

                RollbackWasSuccessful =
                    false,

                VerifiedCopyWasPreserved =
                    true
            };
    }
}