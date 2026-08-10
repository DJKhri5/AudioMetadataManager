using System.IO;
using Xunit;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Verification;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Engine;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.Tests
    .Simulation.Application.PipelineStages.Verification;

public sealed class MetadataVerificationStageTests
{
    private const int PictureCountBefore = 4;

    [Fact]
    public void NullVerificationEngine_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new MetadataVerificationStage(
                    null!));
    }

    [Fact]
    public async Task SuccessfulVerification_CompletesStage()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-success.mp3");

        MetadataWriteResult writeResult =
            CreateWriteResult(
                request,
                MetadataWriteStatus.Completed,
                "Escritura controlada completada.");

        MetadataVerificationResult verificationResult =
            CreateVerificationResult(
                request.FilePath,
                wasSuccessful:
                    true);

        RecordingVerificationEngine engine =
            new(
                verificationResult);

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                writeResult);

        await stage.ExecuteAsync(
            context);

        MetadataApplicationStageResult result =
            Assert.Single(
                context.StageResults);

        Assert.Equal(
            MetadataApplicationStageStatus.Completed,
            result.Status);

        Assert.Equal(
            verificationResult.Summary,
            result.Message);

        Assert.Same(
            verificationResult,
            context.VerificationResult);

        Assert.Equal(
            1,
            engine.CallCount);
    }

    [Fact]
    public async Task FailedVerification_FailsStage()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-failure.mp3");

        MetadataWriteResult writeResult =
            CreateWriteResult(
                request,
                MetadataWriteStatus.Completed,
                "Escritura previa completada.");

        MetadataVerificationResult verificationResult =
            CreateVerificationResult(
                request.FilePath,
                wasSuccessful:
                    false);

        RecordingVerificationEngine engine =
            new(
                verificationResult);

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                writeResult);

        await stage.ExecuteAsync(
            context);

        MetadataApplicationStageResult result =
            Assert.Single(
                context.StageResults);

        Assert.Equal(
            MetadataApplicationStageStatus.Failed,
            result.Status);

        Assert.False(
            verificationResult.WasSuccessful);

        Assert.Contains(
            result.Details,
            detail =>
                detail.Contains(
                    "no coincide",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Same(
            verificationResult,
            context.VerificationResult);

        Assert.Equal(
            1,
            engine.CallCount);
    }

    [Fact]
    public async Task MissingWriteResult_IsRejectedBeforeVerification()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-without-write.mp3");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            new(
                request);

        await stage.ExecuteAsync(
            context);

        MetadataApplicationStageResult result =
            Assert.Single(
                context.StageResults);

        Assert.Equal(
            MetadataApplicationStageStatus.Failed,
            result.Status);

        Assert.Null(
            context.VerificationResult);

        Assert.Equal(
            0,
            engine.CallCount);

        Assert.Contains(
            result.Details,
            detail =>
                detail.Contains(
                    "no contiene un resultado",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task NoWritableChanges_SkipsVerification()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-no-writable.mp3");

        MetadataWriteResult writeResult =
            CreateWriteResult(
                request,
                MetadataWriteStatus.NoWritableChanges,
                "No existieron cambios escribibles.");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                writeResult);

        await stage.ExecuteAsync(
            context);

        MetadataApplicationStageResult result =
            Assert.Single(
                context.StageResults);

        Assert.Equal(
            MetadataApplicationStageStatus.Skipped,
            result.Status);

        Assert.Null(
            context.VerificationResult);

        Assert.Equal(
            0,
            engine.CallCount);
    }

    [Fact]
    public async Task CancelledWrite_CancelsVerificationStage()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-cancelled-write.mp3");

        MetadataWriteResult writeResult =
            CreateWriteResult(
                request,
                MetadataWriteStatus.Cancelled,
                "La escritura previa fue cancelada.");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                writeResult);

        await stage.ExecuteAsync(
            context);

        MetadataApplicationStageResult result =
            Assert.Single(
                context.StageResults);

        Assert.Equal(
            MetadataApplicationStageStatus.Cancelled,
            result.Status);

        Assert.Null(
            context.VerificationResult);

        Assert.Equal(
            0,
            engine.CallCount);
    }

    [Fact]
    public async Task FailedWrite_IsRejectedBeforeVerification()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-failed-write.mp3");

        MetadataWriteResult writeResult =
            CreateWriteResult(
                request,
                MetadataWriteStatus.SaveFailed,
                "La escritura previa terminó con un fallo.");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                writeResult);

        await stage.ExecuteAsync(
            context);

        MetadataApplicationStageResult result =
            Assert.Single(
                context.StageResults);

        Assert.Equal(
            MetadataApplicationStageStatus.Failed,
            result.Status);

        Assert.Null(
            context.VerificationResult);

        Assert.Equal(
            0,
            engine.CallCount);

        Assert.Contains(
            result.Details,
            detail =>
                detail.Contains(
                    "SaveFailed",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task VerificationResult_IsStoredInContext()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-result-storage.mp3");

        MetadataVerificationResult verificationResult =
            CreateVerificationResult(
                request.FilePath,
                wasSuccessful:
                    true);

        RecordingVerificationEngine engine =
            new(
                verificationResult);

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                CreateWriteResult(
                    request,
                    MetadataWriteStatus.Completed,
                    "Escritura completada."));

        await stage.ExecuteAsync(
            context);

        Assert.Same(
            verificationResult,
            context.VerificationResult);
    }

    [Fact]
    public async Task VerificationInputs_AreMappedFromRequest()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-inputs.mp3");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                CreateWriteResult(
                    request,
                    MetadataWriteStatus.Completed,
                    "Escritura completada."));

        await stage.ExecuteAsync(
            context);

        Assert.Equal(
            request.FilePath,
            engine.LastFilePath);

        Assert.Equal(
            request.ValidChanges.Count,
            engine.LastChanges.Count);

        Assert.Equal(
            request.ValidChanges
                .Select(
                    change =>
                        change.Field),
            engine.LastChanges
                .Select(
                    change =>
                        change.Field));

        Assert.Equal(
            request.ValidChanges
                .Select(
                    change =>
                        change.NewValue),
            engine.LastChanges
                .Select(
                    change =>
                        change.NewValue));
    }

    [Fact]
    public async Task PictureCountBefore_IsForwardedToEngine()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-picture-count.mp3");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                CreateWriteResult(
                    request,
                    MetadataWriteStatus.Completed,
                    "Escritura completada."));

        await stage.ExecuteAsync(
            context);

        Assert.Equal(
            PictureCountBefore,
            engine.LastPictureCountBefore);
    }

    [Fact]
    public async Task PreCancelledContext_StopsBeforeVerification()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-token-cancelled.mp3");

        using CancellationTokenSource cancellationSource =
            new();

        cancellationSource.Cancel();

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                CreateWriteResult(
                    request,
                    MetadataWriteStatus.Completed,
                    "Escritura previa completada."),
                cancellationSource.Token);

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () =>
                await stage.ExecuteAsync(
                    context));

        Assert.Equal(
            0,
            engine.CallCount);

        Assert.Null(
            context.VerificationResult);

        Assert.Empty(
            context.StageResults);
    }

    [Fact]
    public async Task StageResult_ContainsAuditableIdentityAndTiming()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-auditable.mp3");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                CreateWriteResult(
                    request,
                    MetadataWriteStatus.Completed,
                    "Escritura completada."));

        await stage.ExecuteAsync(
            context);

        MetadataApplicationStageResult result =
            Assert.Single(
                context.StageResults);

        Assert.Equal(
            MetadataApplicationStage.PostWriteVerification,
            stage.Stage);

        Assert.Equal(
            "Verificación posterior a la escritura",
            stage.Name);

        Assert.Equal(
            400,
            stage.ExecutionOrder);

        Assert.Equal(
            MetadataApplicationStage.PostWriteVerification,
            result.Stage);

        Assert.NotEqual(
            default,
            result.StartedAtUtc);

        Assert.NotEqual(
            default,
            result.CompletedAtUtc);

        Assert.True(
            result.CompletedAtUtc >=
            result.StartedAtUtc);

        Assert.True(
            result.ElapsedTime >=
            TimeSpan.Zero);
    }

    [Fact]
    public async Task DuplicateExecution_IsRejected()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-duplicate.mp3");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                CreateWriteResult(
                    request,
                    MetadataWriteStatus.Completed,
                    "Escritura completada."));

        await stage.ExecuteAsync(
            context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await stage.ExecuteAsync(
                    context));

        Assert.Single(
            context.StageResults);

        Assert.Equal(
            1,
            engine.CallCount);
    }

    [Fact]
    public async Task InjectedEngine_IsUsedExactlyOnceForValidWrite()
    {
        MetadataApplyRequest request =
            CreateApplyRequest(
                "verification-injected-engine.mp3");

        RecordingVerificationEngine engine =
            new(
                CreateVerificationResult(
                    request.FilePath,
                    wasSuccessful:
                        true));

        MetadataVerificationStage stage =
            new(
                engine);

        MetadataApplicationContext context =
            CreateContextWithWriteResult(
                request,
                CreateWriteResult(
                    request,
                    MetadataWriteStatus.Completed,
                    "Escritura completada."));

        await stage.ExecuteAsync(
            context);

        Assert.True(
            engine.WasCalled);

        Assert.Equal(
            1,
            engine.CallCount);
    }

    private static MetadataApplicationContext
        CreateContextWithWriteResult(
            MetadataApplyRequest applyRequest,
            MetadataWriteResult writeResult,
            CancellationToken cancellationToken = default)
    {
        MetadataApplicationContext context =
            new(
                applyRequest,
                cancellationToken);

        context.SetWriteResult(
            writeResult);

        return context;
    }

    private static MetadataApplyRequest
        CreateApplyRequest(
            string fileName)
    {
        return
            new MetadataApplyRequest
            {
                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    Path.Combine(
                        @"Z:\AudioMetadataManager.StructuralTests",
                        fileName),

                FileName =
                    fileName,

                Changes =
                    new[]
                    {
                        new MetadataFieldChange
                        {
                            Field =
                                MetadataField.Artist,

                            OriginalValue =
                                "Artista original",

                            NewValue =
                                "Artista aprobado",

                            WasManuallyApproved =
                                true,

                            Confidence =
                                1.0
                        }
                    }
            };
    }

    private static MetadataWriteResult
        CreateWriteResult(
            MetadataApplyRequest applyRequest,
            MetadataWriteStatus status,
            string message)
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        IReadOnlyList<MetadataFieldWriteResult>
            fieldResults =
                status ==
                MetadataWriteStatus.Completed
                    ? new[]
                    {
                        new MetadataFieldWriteResult
                        {
                            Field =
                                MetadataField.Artist,

                            OriginalValue =
                                "Artista original",

                            RequestedValue =
                                "Artista aprobado",

                            IsSupported =
                                true,

                            ValuePrepared =
                                true,

                            SaveSucceeded =
                                true,

                            Message =
                                message
                        }
                    }
                    : Array.Empty<
                        MetadataFieldWriteResult>();

        return
            new MetadataWriteResult
            {
                WriteRequestId =
                    Guid.NewGuid(),

                ApplyRequestId =
                    applyRequest.RequestId,

                PlanId =
                    applyRequest.PlanId,

                Status =
                    status,

                FilePath =
                    applyRequest.FilePath,

                WriterName =
                    "ControlledWriter",

                PictureCountBefore =
                    PictureCountBefore,

                StartedAtUtc =
                    now,

                CompletedAtUtc =
                    now,

                ElapsedTime =
                    TimeSpan.Zero,

                FieldResults =
                    fieldResults,

                Messages =
                    new[]
                    {
                        message
                    }
            };
    }

    private static MetadataVerificationResult
        CreateVerificationResult(
            string filePath,
            bool wasSuccessful)
    {
        string persistedValue =
            wasSuccessful
                ? "Artista aprobado"
                : "Artista diferente";

        string message =
            wasSuccessful
                ? "El valor persistido coincide con el solicitado."
                : "El valor persistido no coincide con el solicitado.";

        return
            new MetadataVerificationResult
            {
                FilePath =
                    filePath,

                FileOpened =
                    true,

                FieldResults =
                    new[]
                    {
                        new MetadataFieldVerificationResult
                        {
                            Field =
                                MetadataField.Artist,

                            ExpectedValue =
                                "Artista aprobado",

                            PersistedValue =
                                persistedValue,

                            IsSupported =
                                true,

                            MatchesExpectedValue =
                                wasSuccessful,

                            Message =
                                message
                        }
                    },

                PictureCountBefore =
                    PictureCountBefore,

                PictureCountAfter =
                    PictureCountBefore,

                Messages =
                    new[]
                    {
                        message
                    }
            };
    }

    private sealed class RecordingVerificationEngine :
        IMetadataWriterVerificationEngine
    {
        private readonly MetadataVerificationResult
            _resultToReturn;

        public RecordingVerificationEngine(
            MetadataVerificationResult resultToReturn)
        {
            _resultToReturn =
                resultToReturn ??
                throw new ArgumentNullException(
                    nameof(resultToReturn));
        }

        public int CallCount
        {
            get;
            private set;
        }

        public bool WasCalled =>
            CallCount > 0;

        public string LastFilePath
        {
            get;
            private set;
        } =
            string.Empty;

        public IReadOnlyList<MetadataFieldChange>
            LastChanges
        {
            get;
            private set;
        } =
            Array.Empty<MetadataFieldChange>();

        public int LastPictureCountBefore
        {
            get;
            private set;
        }

        public MetadataVerificationResult Verify(
            string? filePath,
            IEnumerable<MetadataFieldChange>? changes,
            int pictureCountBefore)
        {
            CallCount++;

            LastFilePath =
                filePath?.Trim() ??
                string.Empty;

            LastChanges =
                changes?.ToArray() ??
                Array.Empty<MetadataFieldChange>();

            LastPictureCountBefore =
                pictureCountBefore;

            return
                _resultToReturn;
        }
    }
}