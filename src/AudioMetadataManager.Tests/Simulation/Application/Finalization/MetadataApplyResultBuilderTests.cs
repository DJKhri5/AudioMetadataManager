using Xunit;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Finalization;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Finalization;

public sealed class MetadataApplyResultBuilderTests
{
    private const string SharedMessage =
        "Mensaje auditable compartido.";

    [Fact]
    public void NullContext_IsRejected()
    {
        MetadataApplyResultBuilder builder =
            new();

        Assert.Throws<ArgumentNullException>(
            () =>
                builder.Build(
                    null!));
    }

    [Fact]
    public void Identifiers_ArePreserved()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.Equal(
            scenario.Request.RequestId,
            result.RequestId);

        Assert.Equal(
            scenario.Request.PlanId,
            result.PlanId);
    }

    [Fact]
    public void FileInformation_IsPreserved()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.Equal(
            scenario.Request.FilePath,
            result.FilePath);

        Assert.Equal(
            scenario.Request.FileName,
            result.FileName);
    }

    [Fact]
    public void BackupPath_IsPreserved()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.Equal(
            scenario.BackupPath,
            result.BackupPath);
    }

    [Fact]
    public void FieldCount_IsPreserved()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.Equal(
            scenario.Request.ValidChanges.Count,
            result.FieldResults.Count);
    }

    [Fact]
    public void FieldValues_ArePreserved()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        foreach (MetadataFieldChange change
                 in scenario.Request.ValidChanges)
        {
            MetadataFieldApplyResult? fieldResult =
                result.FieldResults
                    .FirstOrDefault(
                        item =>
                            item.Field ==
                            change.Field);

            Assert.NotNull(
                fieldResult);

            Assert.Equal(
                change.OriginalValue,
                fieldResult.OriginalValue);

            Assert.Equal(
                change.NewValue,
                fieldResult.RequestedValue);

            Assert.Equal(
                change.NewValue,
                fieldResult.VerifiedValue);
        }
    }

    [Fact]
    public void SuccessfulWrites_ArePreserved()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.All(
            result.FieldResults,
            fieldResult =>
                Assert.True(
                    fieldResult.WriteSucceeded));
    }

    [Fact]
    public void SuccessfulVerifications_ArePreserved()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.All(
            result.FieldResults,
            fieldResult =>
                Assert.True(
                    fieldResult.VerificationSucceeded));
    }

    [Fact]
    public void SuccessfulScenario_ProducesCompletedResult()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.Equal(
            MetadataApplyStatus.Completed,
            result.Status);

        Assert.True(
            result.WasSuccessful);
    }

    [Fact]
    public void Messages_AreConsolidated()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.Contains(
            scenario.Context.ValidationResult!.Summary,
            result.Messages);

        Assert.Contains(
            "Ruta de respaldo incorporada.",
            result.Messages);

        Assert.Contains(
            "Escritura controlada completada.",
            result.Messages);

        Assert.Contains(
            "Verificación controlada completada.",
            result.Messages);

        Assert.All(
            result.FieldResults,
            fieldResult =>
                Assert.Contains(
                    fieldResult.Message,
                    result.Messages));
    }

    [Fact]
    public void DuplicateMessages_AreRemoved()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.Equal(
            1,
            result.Messages.Count(
                message =>
                    string.Equals(
                        message,
                        SharedMessage,
                        StringComparison.Ordinal)));

        Assert.Equal(
            result.Messages.Count,
            result.Messages
                .Distinct(
                    StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public void Timing_IsDerivedFromContext()
    {
        TestScenario scenario =
            CreateSuccessfulScenario();

        MetadataApplyResult result =
            scenario.Builder.Build(
                scenario.Context);

        Assert.Equal(
            scenario.Context.StartedAtUtc,
            result.StartedAtUtc);

        Assert.True(
            result.CompletedAtUtc >=
            result.StartedAtUtc);

        Assert.Equal(
            result.CompletedAtUtc -
            result.StartedAtUtc,
            result.ElapsedTime);

        Assert.True(
            result.ElapsedTime >=
            TimeSpan.Zero);
    }

    private static TestScenario
        CreateSuccessfulScenario()
    {
        Guid requestId =
            Guid.NewGuid();

        Guid planId =
            Guid.NewGuid();

        const string filePath =
            @"C:\AudioMetadataManager\Tests\" +
            "builder-xunit-test.flac";

        const string fileName =
            "builder-xunit-test.flac";

        const string backupPath =
            @"C:\AudioMetadataManager\Tests\Backup\" +
            "builder-xunit-test.flac";

        MetadataFieldChange[] changes =
        {
            new()
            {
                Field =
                    MetadataField.Genre,

                OriginalValue =
                    "House",

                NewValue =
                    "Trance",

                WasManuallyApproved =
                    true,

                Confidence =
                    1.0,

                SupportingSources =
                    new[]
                    {
                        "xUnit"
                    }
            },

            new()
            {
                Field =
                    MetadataField.Album,

                OriginalValue =
                    "Original Album",

                NewValue =
                    "Verified Album",

                WasManuallyApproved =
                    true,

                Confidence =
                    1.0,

                SupportingSources =
                    new[]
                    {
                        "xUnit"
                    }
            }
        };

        MetadataApplyRequest request =
            new()
            {
                RequestId =
                    requestId,

                PlanId =
                    planId,

                FilePath =
                    filePath,

                FileName =
                    fileName,

                Changes =
                    changes,

                RequireBackup =
                    false,

                RequirePostWriteVerification =
                    true
            };

        MetadataApplicationContext context =
            new(
                request);

        context.SetValidationResult(
            new MetadataApplyValidationResult
            {
                Issues =
                    Array.Empty<
                        MetadataApplyValidationIssue>()
            });

        context.SetBackupResult(
            new MetadataBackupResult
            {
                ApplyRequestId =
                    requestId,

                PlanId =
                    planId,

                Status =
                    MetadataBackupStatus.Completed,

                SourceFilePath =
                    filePath,

                BackupFilePath =
                    backupPath,

                Messages =
                    new[]
                    {
                        SharedMessage,
                        "Ruta de respaldo incorporada."
                    }
            });

        context.SetWriteResult(
            new MetadataWriteResult
            {
                ApplyRequestId =
                    requestId,

                PlanId =
                    planId,

                Status =
                    MetadataWriteStatus.Completed,

                FilePath =
                    filePath,

                WriterName =
                    "xUnitWriter",

                FieldResults =
                    new[]
                    {
                        CreateWriteResult(
                            MetadataField.Genre,
                            "House",
                            "Trance"),

                        CreateWriteResult(
                            MetadataField.Album,
                            "Original Album",
                            "Verified Album")
                    },

                Messages =
                    new[]
                    {
                        SharedMessage,
                        "Escritura controlada completada."
                    }
            });

        context.SetVerificationResult(
            new MetadataVerificationResult
            {
                FilePath =
                    filePath,

                FileOpened =
                    true,

                FieldResults =
                    new[]
                    {
                        CreateVerificationResult(
                            MetadataField.Genre,
                            "Trance"),

                        CreateVerificationResult(
                            MetadataField.Album,
                            "Verified Album")
                    },

                PictureCountBefore =
                    2,

                PictureCountAfter =
                    2,

                Messages =
                    new[]
                    {
                        SharedMessage,
                        "Verificación controlada completada."
                    }
            });

        return
            new TestScenario(
                new MetadataApplyResultBuilder(),
                request,
                context,
                backupPath);
    }

    private static MetadataFieldWriteResult
        CreateWriteResult(
            MetadataField field,
            string originalValue,
            string requestedValue)
    {
        return
            new MetadataFieldWriteResult
            {
                Field =
                    field,

                OriginalValue =
                    originalValue,

                RequestedValue =
                    requestedValue,

                IsSupported =
                    true,

                ValuePrepared =
                    true,

                SaveSucceeded =
                    true,

                Message =
                    $"{field}: escritura consolidada."
            };
    }

    private static MetadataFieldVerificationResult
        CreateVerificationResult(
            MetadataField field,
            string persistedValue)
    {
        return
            new MetadataFieldVerificationResult
            {
                Field =
                    field,

                ExpectedValue =
                    persistedValue,

                PersistedValue =
                    persistedValue,

                IsSupported =
                    true,

                MatchesExpectedValue =
                    true,

                Message =
                    $"{field}: verificación consolidada."
            };
    }

    private sealed record TestScenario(
        MetadataApplyResultBuilder Builder,
        MetadataApplyRequest Request,
        MetadataApplicationContext Context,
        string BackupPath);
}