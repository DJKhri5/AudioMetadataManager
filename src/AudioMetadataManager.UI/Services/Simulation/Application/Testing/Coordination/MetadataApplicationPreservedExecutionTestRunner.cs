using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using System.IO;
using TagLib;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Comprueba que una ejecución aislada satisfactoria pueda
/// conservarse temporalmente y limpiarse posteriormente.
/// </summary>
public sealed class
    MetadataApplicationPreservedExecutionTestRunner
{
    private const string TestGenre =
        "Electronic";

    public async Task<
        MetadataApplicationPreservedExecutionTestResult>
        RunAsync(
            string filePath,
            CancellationToken cancellationToken = default)
    {
        List<string> messages =
            new();

        FileIsolationTestHarness isolationHarness =
            new();

        MetadataApplicationIsolatedExecutor executor =
            new(
                isolationHarness,
                new MetadataApplyRequestIsolationFactory(),
                new MetadataApplicationCoordinator());

        MetadataApplicationIsolatedExecutionResult? result =
            null;

        bool workingCopyStillExisted =
            false;

        bool initialBackupStillExisted =
            false;

        bool manualCleanupWasSuccessful =
            false;

        bool temporaryDirectoryWasRemoved =
            false;

        string errorMessage =
            string.Empty;

        try
        {
            MetadataApplyRequest request =
                new()
                {
                    PlanId =
                        Guid.NewGuid(),

                    FilePath =
                        filePath,

                    FileName =
                        Path.GetFileName(
                            filePath),

                    Changes =
                        new[]
                        {
                            new MetadataFieldChange
                            {
                                Field =
                                    MetadataField.Genre,

                                OriginalValue =
                                    ReadGenre(
                                        filePath),

                                NewValue =
                                    TestGenre,

                                WasManuallyApproved =
                                    true,

                                Confidence =
                                    1.0
                            }
                        },

                    RequireBackup =
                        true,

                    RequirePostWriteVerification =
                        true
                };

            result =
                await executor.ExecuteAsync(
                    request,
                    MetadataApplicationIsolatedExecutionOptions
                        .PreserveSuccessfulExecution,
                    cancellationToken);

            FileIsolationContext? context =
                result.IsolationContext;

            if (context is not null)
            {
                workingCopyStillExisted =
                    System.IO.File.Exists(
                        context.WorkingCopyPath);

                initialBackupStillExisted =
                    System.IO.File.Exists(
                        context.WorkingBackupPath);

                manualCleanupWasSuccessful =
                    isolationHarness.TryCleanup(
                        context);

                temporaryDirectoryWasRemoved =
                    !Directory.Exists(
                        context.TestDirectoryPath);
            }
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;
        }

        bool executionWasSuccessful =
            result?.WasSuccessful == true;

        bool environmentWasPreserved =
            result?.EnvironmentWasPreserved == true;

        bool cleanupWasDeferred =
            result is not null &&
            !result.CleanupWasSuccessful;

        bool originalFileRemainedUnchanged =
            result?.OriginalFileRemainedUnchanged == true;

        bool workingCopyWasModified =
            result?.WorkingCopyWasModified == true;

        messages.Add(
            executionWasSuccessful
                ? "La ejecución aislada terminó correctamente."
                : "La ejecución aislada no terminó correctamente.");

        messages.Add(
            environmentWasPreserved
                ? "El entorno verificado fue conservado."
                : "El entorno verificado no fue conservado.");

        messages.Add(
            cleanupWasDeferred
                ? "La limpieza automática fue pospuesta."
                : "La limpieza automática no fue pospuesta.");

        messages.Add(
            workingCopyStillExisted
                ? "La copia verificada seguía disponible."
                : "La copia verificada no estaba disponible.");

        messages.Add(
            initialBackupStillExisted
                ? "El respaldo inicial seguía disponible."
                : "El respaldo inicial no estaba disponible.");

        messages.Add(
            originalFileRemainedUnchanged
                ? "El archivo original permaneció intacto."
                : "El archivo original no permaneció intacto.");

        messages.Add(
            workingCopyWasModified
                ? "La copia conservada contenía cambios."
                : "La copia conservada no contenía cambios.");

        messages.Add(
            manualCleanupWasSuccessful
                ? "La limpieza manual terminó correctamente."
                : "La limpieza manual no terminó correctamente.");

        messages.Add(
            temporaryDirectoryWasRemoved
                ? "La carpeta temporal fue eliminada."
                : "La carpeta temporal permaneció en el disco.");

        return new
            MetadataApplicationPreservedExecutionTestResult
        {
            ExecutionWasSuccessful =
                    executionWasSuccessful,

            EnvironmentWasPreserved =
                    environmentWasPreserved,

            CleanupWasDeferred =
                    cleanupWasDeferred,

            WorkingCopyStillExisted =
                    workingCopyStillExisted,

            InitialBackupStillExisted =
                    initialBackupStillExisted,

            OriginalFileRemainedUnchanged =
                    originalFileRemainedUnchanged,

            WorkingCopyWasModified =
                    workingCopyWasModified,

            ManualCleanupWasSuccessful =
                    manualCleanupWasSuccessful,

            TemporaryDirectoryWasRemoved =
                    temporaryDirectoryWasRemoved,

            ErrorMessage =
                    string.IsNullOrWhiteSpace(
                        errorMessage)
                            ? result?.ErrorMessage ??
                              string.Empty
                            : errorMessage,

            Messages =
                    messages.ToArray()
        };
    }

    private static string ReadGenre(
        string filePath)
    {
        using TagLib.File file =
            TagLib.File.Create(
                filePath);

        return file.Tag.Genres
            .FirstOrDefault() ??
            string.Empty;
    }
}