using AudioMetadataManager.UI.Services
    .MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Finalization;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Infrastructure;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing;

/// <summary>
/// Ejecuta una prueba controlada y sin escritura de archivos
/// sobre MetadataApplyResultBuilder.
/// </summary>
public sealed class MetadataApplyResultBuilderTestRunner
{
    private const string SharedMessage =
        "Mensaje auditable compartido.";

    private readonly IMetadataApplyResultBuilder
        _resultBuilder;

    /// <summary>
    /// Crea el runner con el constructor que se probará.
    /// </summary>
    public MetadataApplyResultBuilderTestRunner(
        IMetadataApplyResultBuilder resultBuilder)
    {
        _resultBuilder =
            resultBuilder ??
            throw new ArgumentNullException(
                nameof(resultBuilder));
    }

    /// <summary>
    /// Crea el runner utilizando la implementación
    /// predeterminada.
    /// </summary>
    public static MetadataApplyResultBuilderTestRunner
        CreateDefault()
    {
        return new MetadataApplyResultBuilderTestRunner(
            new MetadataApplyResultBuilder());
    }

    /// <summary>
    /// Ejecuta la prueba controlada del constructor.
    /// </summary>
    public MetadataApplyResultBuilderTestResult Run()
    {
        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        try
        {
            Guid requestId =
                Guid.NewGuid();

            Guid planId =
                Guid.NewGuid();

            const string filePath =
                @"C:\AudioMetadataManager\Tests\" +
                "builder-test.flac";

            const string fileName =
                "builder-test.flac";

            const string backupPath =
                @"C:\AudioMetadataManager\Tests\Backup\" +
                "builder-test.flac";

            MetadataFieldChange[] changes =
            {
                new()
                {
                    Field =
                        MetadataField.Genre,

                    OriginalValue =
                        DiagnosticMetadataTestValues.CreateGenre(),

                    NewValue =
                        "Trance",

                    WasManuallyApproved =
                        true,

                    Confidence =
                        1.0,

                    SupportingSources =
                        new[]
                        {
                            "Prueba controlada del constructor"
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
                            "Prueba controlada del constructor"
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
                        "ConstructorTestWriter",

                    FieldResults =
                        new[]
                        {
                            CreateWriteResult(
                                MetadataField.Genre,
                                DiagnosticMetadataTestValues.CreateGenre(),
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

            MetadataApplyResult applyResult =
                _resultBuilder.Build(
                    context);

            DateTimeOffset completedAtUtc =
                DateTimeOffset.UtcNow;

            bool identifiersPreserved =
                applyResult.RequestId ==
                    requestId &&
                applyResult.PlanId ==
                    planId;

            bool fileInformationPreserved =
                string.Equals(
                    applyResult.FilePath,
                    filePath,
                    StringComparison.Ordinal) &&
                string.Equals(
                    applyResult.FileName,
                    fileName,
                    StringComparison.Ordinal);

            bool backupPathPreserved =
                string.Equals(
                    applyResult.BackupPath,
                    backupPath,
                    StringComparison.Ordinal);

            bool fieldCountPreserved =
                applyResult.FieldResults.Count ==
                changes.Length;

            bool fieldValuesPreserved =
                changes.All(
                    change =>
                        HasPreservedFieldValues(
                            applyResult,
                            change));

            bool writeStatusPreserved =
                applyResult.FieldResults.All(
                    result =>
                        result.WriteSucceeded);

            bool verificationStatusPreserved =
                applyResult.FieldResults.All(
                    result =>
                        result.VerificationSucceeded);

            bool finalStatusCorrect =
                applyResult.Status ==
                    MetadataApplyStatus.Completed &&
                applyResult.WasSuccessful;

            bool messagesConsolidated =
                ContainsMessage(
                    applyResult.Messages,
                    context.ValidationResult?.Summary) &&
                ContainsMessage(
                    applyResult.Messages,
                    "Ruta de respaldo incorporada.") &&
                ContainsMessage(
                    applyResult.Messages,
                    "Escritura controlada completada.") &&
                ContainsMessage(
                    applyResult.Messages,
                    "Verificación controlada completada.") &&
                applyResult.FieldResults.All(
                    result =>
                        ContainsMessage(
                            applyResult.Messages,
                            result.Message));

            bool duplicateMessagesRemoved =
                applyResult.Messages.Count(
                    message =>
                        string.Equals(
                            message,
                            SharedMessage,
                            StringComparison.Ordinal)) ==
                    1 &&
                applyResult.Messages.Count ==
                applyResult.Messages
                    .Distinct(
                        StringComparer.Ordinal)
                    .Count();

            bool timingIsValid =
                applyResult.StartedAtUtc ==
                    context.StartedAtUtc &&
                applyResult.CompletedAtUtc >=
                    applyResult.StartedAtUtc &&
                applyResult.ElapsedTime ==
                    applyResult.CompletedAtUtc -
                    applyResult.StartedAtUtc &&
                completedAtUtc >=
                    startedAtUtc;

            return new MetadataApplyResultBuilderTestResult
            {
                ApplyResult =
                    applyResult,

                IdentifiersPreserved =
                    identifiersPreserved,

                FileInformationPreserved =
                    fileInformationPreserved,

                BackupPathPreserved =
                    backupPathPreserved,

                FieldCountPreserved =
                    fieldCountPreserved,

                FieldValuesPreserved =
                    fieldValuesPreserved,

                WriteStatusPreserved =
                    writeStatusPreserved,

                VerificationStatusPreserved =
                    verificationStatusPreserved,

                FinalStatusCorrect =
                    finalStatusCorrect,

                MessagesConsolidated =
                    messagesConsolidated,

                DuplicateMessagesRemoved =
                    duplicateMessagesRemoved,

                TimingIsValid =
                    timingIsValid,

                StartedAtUtc =
                    startedAtUtc,

                CompletedAtUtc =
                    completedAtUtc,

                ElapsedTime =
                    completedAtUtc -
                    startedAtUtc,

                Messages =
                    applyResult.Messages
            };
        }
        catch (Exception exception)
        {
            DateTimeOffset completedAtUtc =
                DateTimeOffset.UtcNow;

            return new MetadataApplyResultBuilderTestResult
            {
                StartedAtUtc =
                    startedAtUtc,

                CompletedAtUtc =
                    completedAtUtc,

                ElapsedTime =
                    completedAtUtc -
                    startedAtUtc,

                ErrorMessage =
                    exception.Message,

                ExceptionType =
                    exception.GetType().FullName ??
                    exception.GetType().Name,

                Messages =
                    new[]
                    {
                        exception.Message
                    }
            };
        }
    }

    private static MetadataFieldWriteResult
        CreateWriteResult(
            MetadataField field,
            string originalValue,
            string requestedValue)
    {
        return new MetadataFieldWriteResult
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
        return new MetadataFieldVerificationResult
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

    private static bool HasPreservedFieldValues(
        MetadataApplyResult applyResult,
        MetadataFieldChange change)
    {
        MetadataFieldApplyResult? fieldResult =
            applyResult.FieldResults
                .FirstOrDefault(
                    result =>
                        result.Field ==
                        change.Field);

        return fieldResult is not null &&
               string.Equals(
                   fieldResult.OriginalValue,
                   change.OriginalValue,
                   StringComparison.Ordinal) &&
               string.Equals(
                   fieldResult.RequestedValue,
                   change.NewValue,
                   StringComparison.Ordinal) &&
               string.Equals(
                   fieldResult.VerifiedValue,
                   change.NewValue,
                   StringComparison.Ordinal);
    }

    private static bool ContainsMessage(
        IReadOnlyList<string> messages,
        string? expectedMessage)
    {
        return !string.IsNullOrWhiteSpace(
                   expectedMessage) &&
               messages.Contains(
                   expectedMessage,
                   StringComparer.Ordinal);
    }
}