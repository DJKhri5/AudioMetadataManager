using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Infrastructure;
using System.IO;
using TagLib;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta una prueba integral del ejecutor aislado sobre un
/// archivo de audio real, sin modificar el original.
/// </summary>
public sealed class MetadataApplicationIsolatedExecutorTestRunner
{
    private static readonly string TestGenre =
        DiagnosticMetadataTestValues.CreateGenre();

    private readonly MetadataApplicationIsolatedExecutor
        _isolatedExecutor;

    public MetadataApplicationIsolatedExecutorTestRunner()
        : this(
            new MetadataApplicationIsolatedExecutor())
    {
    }

    public MetadataApplicationIsolatedExecutorTestRunner(
        MetadataApplicationIsolatedExecutor
            isolatedExecutor)
    {
        _isolatedExecutor =
            isolatedExecutor ??
            throw new ArgumentNullException(
                nameof(isolatedExecutor));
    }

    public async Task<MetadataApplicationIsolatedExecutorTestResult>
        RunAsync(
            string filePath,
            CancellationToken cancellationToken = default)
    {
        List<string> messages =
            new();

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

        MetadataApplicationIsolatedExecutionResult result =
            await _isolatedExecutor.ExecuteAsync(
                request,
                cancellationToken);

        string persistedGenre =
            result.PipelineResult?
                .VerificationResult?
                .FieldResults
                .FirstOrDefault(
                    fieldResult =>
                        fieldResult.Field ==
                        MetadataField.Genre)?
                .PersistedValue ??
            string.Empty;

        messages.Add(
            result.IsolationWasPrepared
                ? "El entorno aislado fue preparado."
                : "El entorno aislado no fue preparado.");

        messages.Add(
            result.PipelineWasSuccessful
                ? "El pipeline terminó correctamente."
                : "El pipeline no terminó correctamente.");

        messages.Add(
            result.OriginalFileRemainedUnchanged
                ? "El archivo original permaneció intacto."
                : "El archivo original fue modificado.");

        messages.Add(
            result.WorkingCopyWasModified
                ? "La copia temporal fue modificada."
                : "La copia temporal no fue modificada.");

        messages.Add(
            result.InitialBackupWasPreserved
                ? "El respaldo inicial fue preservado."
                : "El respaldo inicial no fue preservado.");

        messages.Add(
            result.CleanupWasSuccessful
                ? "El entorno temporal fue eliminado."
                : "El entorno temporal no pudo eliminarse.");

        messages.Add(
            string.Equals(
                TestGenre,
                persistedGenre,
                StringComparison.OrdinalIgnoreCase)
                ? "El género solicitado fue persistido."
                : "El género solicitado no fue persistido.");

        return new MetadataApplicationIsolatedExecutorTestResult
        {
            IsolationWasPrepared =
                result.IsolationWasPrepared,

            PipelineWasSuccessful =
                result.PipelineWasSuccessful,

            OriginalFileRemainedUnchanged =
                result.OriginalFileRemainedUnchanged,

            WorkingCopyWasModified =
                result.WorkingCopyWasModified,

            InitialBackupWasPreserved =
                result.InitialBackupWasPreserved,

            CleanupWasSuccessful =
                result.CleanupWasSuccessful,

            RequestedGenre =
                TestGenre,

            PersistedGenre =
                persistedGenre,

            ErrorMessage =
                result.ErrorMessage,

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