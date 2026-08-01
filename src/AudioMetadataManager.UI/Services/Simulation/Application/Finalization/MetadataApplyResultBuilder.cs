using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Finalization;

/// <summary>
/// Construye el resultado final, consolidado y auditable de una
/// aplicación de metadatos.
/// </summary>
public sealed class MetadataApplyResultBuilder :
    IMetadataApplyResultBuilder
{
    /// <inheritdoc />
    public MetadataApplyResult Build(
        MetadataApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        DateTimeOffset completedAtUtc =
            DateTimeOffset.UtcNow;

        MetadataFieldApplyResult[] fieldResults =
            BuildFieldResults(
                context);

        return new MetadataApplyResult
        {
            RequestId =
                context.Request.RequestId,

            PlanId =
                context.Request.PlanId,

            FilePath =
                context.Request.FilePath,

            FileName =
                context.Request.FileName,

            Status =
                DetermineStatus(
                    context,
                    fieldResults),

            StartedAtUtc =
                context.StartedAtUtc,

            CompletedAtUtc =
                completedAtUtc,

            ElapsedTime =
                completedAtUtc -
                context.StartedAtUtc,

            BackupPath =
                context.BackupResult?.BackupFilePath ??
                string.Empty,

            FieldResults =
                fieldResults,

            Messages =
                BuildMessages(
                    context,
                    fieldResults)
        };
    }

    private static MetadataFieldApplyResult[]
        BuildFieldResults(
            MetadataApplicationContext context)
    {
        return context.Request.ValidChanges
            .Select(
                change =>
                {
                    var writeFieldResult =
                        context.WriteResult?
                            .FieldResults
                            .FirstOrDefault(
                                result =>
                                    result.Field ==
                                    change.Field);

                    var verificationFieldResult =
                        context.VerificationResult?
                            .FieldResults
                            .FirstOrDefault(
                                result =>
                                    result.Field ==
                                    change.Field);

                    string message =
                        BuildFieldMessage(
                            writeFieldResult?.Message,
                            verificationFieldResult?.Message);

                    return new MetadataFieldApplyResult
                    {
                        Field =
                            change.Field,

                        OriginalValue =
                            change.OriginalValue,

                        RequestedValue =
                            change.NewValue,

                        VerifiedValue =
                            verificationFieldResult?
                                .PersistedValue ??
                            string.Empty,

                        WriteSucceeded =
                            writeFieldResult?
                                .WasWritten ==
                            true,

                        VerificationSucceeded =
                            verificationFieldResult?
                                .WasSuccessful ==
                            true,

                        Message =
                            message
                    };
                })
            .ToArray();
    }

    private static MetadataApplyStatus DetermineStatus(
        MetadataApplicationContext context,
        IReadOnlyList<MetadataFieldApplyResult>
            fieldResults)
    {
        if (context.WasCancelled)
        {
            return MetadataApplyStatus.Cancelled;
        }

        if (context.ValidationResult?.IsValid !=
            true)
        {
            return MetadataApplyStatus.ValidationFailed;
        }

        if (context.Request.RequireBackup &&
            context.BackupResult?.WasSuccessful !=
            true)
        {
            return MetadataApplyStatus.BackupFailed;
        }

        if (context.WriteResult?.WasSuccessful !=
            true)
        {
            return MetadataApplyStatus.WriteFailed;
        }

        int successfulFieldCount =
            fieldResults.Count(
                result =>
                    result.WasSuccessfullyApplied);

        if (context.VerificationResult?.WasSuccessful !=
            true)
        {
            return successfulFieldCount > 0
                ? MetadataApplyStatus.PartiallyCompleted
                : MetadataApplyStatus.VerificationFailed;
        }

        if (fieldResults.Count > 0 &&
            successfulFieldCount ==
            fieldResults.Count)
        {
            return MetadataApplyStatus.Completed;
        }

        return successfulFieldCount > 0
            ? MetadataApplyStatus.PartiallyCompleted
            : MetadataApplyStatus.VerificationFailed;
    }

    private static IReadOnlyList<string> BuildMessages(
        MetadataApplicationContext context,
        IReadOnlyList<MetadataFieldApplyResult>
            fieldResults)
    {
        List<string> messages =
            new();

        AddMessage(
            messages,
            context.ValidationResult?.Summary);

        AddMessages(
            messages,
            context.BackupResult?.Messages);

        AddMessages(
            messages,
            context.WriteResult?.Messages);

        AddMessages(
            messages,
            context.VerificationResult?.Messages);

        foreach (MetadataFieldApplyResult fieldResult
                 in fieldResults)
        {
            AddMessage(
                messages,
                fieldResult.Message);
        }

        return messages
            .Distinct(
                StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildFieldMessage(
        string? writeMessage,
        string? verificationMessage)
    {
        string[] messages =
        {
            NormalizeMessage(
                writeMessage),

            NormalizeMessage(
                verificationMessage)
        };

        return string.Join(
            " ",
            messages.Where(
                message =>
                    !string.IsNullOrWhiteSpace(
                        message)));
    }

    private static void AddMessages(
        ICollection<string> destination,
        IReadOnlyList<string>? source)
    {
        if (source is null)
        {
            return;
        }

        foreach (string message in source)
        {
            AddMessage(
                destination,
                message);
        }
    }

    private static void AddMessage(
        ICollection<string> destination,
        string? message)
    {
        string normalizedMessage =
            NormalizeMessage(
                message);

        if (!string.IsNullOrWhiteSpace(
                normalizedMessage))
        {
            destination.Add(
                normalizedMessage);
        }
    }

    private static string NormalizeMessage(
        string? message)
    {
        return string.IsNullOrWhiteSpace(
            message)
            ? string.Empty
            : message.Trim();
    }
}