using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta una prueba controlada del camino Approved del
/// coordinador productivo individual usando únicamente archivos
/// temporales.
/// </summary>
public sealed class
    MetadataProductiveApplicationApprovedTestRunner
{
    private const string TestFolderName =
        "MetadataProductiveApplicationApprovedTests";

    private const string TestGenre =
        "Electronic";

    private readonly FileSha256Service
        _fileSha256Service;

    /// <summary>
    /// Crea el corredor con las dependencias predeterminadas.
    /// </summary>
    public MetadataProductiveApplicationApprovedTestRunner()
        : this(
            new FileSha256Service())
    {
    }

    /// <summary>
    /// Crea el corredor con el servicio de hash proporcionado.
    /// </summary>
    public MetadataProductiveApplicationApprovedTestRunner(
        FileSha256Service fileSha256Service)
    {
        _fileSha256Service =
            fileSha256Service ??
            throw new ArgumentNullException(
                nameof(fileSha256Service));
    }

    /// <summary>
    /// Ejecuta la prueba sobre copias temporales del archivo
    /// indicado.
    /// </summary>
    public async Task<
        MetadataProductiveApplicationApprovedTestResult>
        RunAsync(
            string sourceFilePath,
            CancellationToken cancellationToken = default)
    {
        List<string> messages =
            new();

        string testDirectoryPath =
            string.Empty;

        string referenceOriginalPath =
            string.Empty;

        string controlledDestinationPath =
            string.Empty;

        string referenceOriginalHashBefore =
            string.Empty;

        string controlledDestinationHashBefore =
            string.Empty;

        string productiveBackupPath =
            string.Empty;

        bool testEnvironmentWasPrepared =
            false;

        bool verifiedCopyWasPrepared =
            false;

        bool promotionDecisionWasPending =
            false;

        bool destinationRemainedUnchangedDuringPreparation =
            false;

        bool approvedDecisionWasHandled =
            false;

        bool promotionWasSuccessful =
            false;

        bool productiveBackupWasCreated =
            false;

        bool productiveBackupWasVerified =
            false;

        bool replacementWasExecuted =
            false;

        bool promotedDestinationWasVerified =
            false;

        bool requestedGenreWasPersisted =
            false;

        bool rollbackWasNotRequired =
            false;

        bool referenceOriginalRemainedUnchanged =
            false;

        bool destinationEndedInSafeState =
            false;

        bool finalCleanupWasAttempted =
            false;

        bool finalCleanupWasSuccessful =
            false;

        bool productiveResultWasSuccessful =
            false;

        bool temporaryEnvironmentWasRemoved =
            false;

        bool temporaryProductiveBackupWasRemoved =
            false;

        string persistedGenre =
            string.Empty;

        string errorMessage =
            string.Empty;

        try
        {
            string normalizedSourcePath =
                NormalizeExistingFilePath(
                    sourceFilePath);

            string extension =
                Path.GetExtension(
                    normalizedSourcePath);

            testDirectoryPath =
                Path.Combine(
                    Path.GetTempPath(),
                    "AudioMetadataManager",
                    TestFolderName,
                    Guid.NewGuid().ToString(
                        "N"));

            Directory.CreateDirectory(
                testDirectoryPath);

            referenceOriginalPath =
                Path.Combine(
                    testDirectoryPath,
                    $"reference_original{extension}");

            controlledDestinationPath =
                Path.Combine(
                    testDirectoryPath,
                    $"controlled_destination{extension}");

            File.Copy(
                normalizedSourcePath,
                referenceOriginalPath,
                overwrite:
                    false);

            File.Copy(
                normalizedSourcePath,
                controlledDestinationPath,
                overwrite:
                    false);

            referenceOriginalHashBefore =
                await _fileSha256Service.ComputeAsync(
                    referenceOriginalPath,
                    cancellationToken);

            controlledDestinationHashBefore =
                await _fileSha256Service.ComputeAsync(
                    controlledDestinationPath,
                    cancellationToken);

            testEnvironmentWasPrepared =
                File.Exists(
                    referenceOriginalPath) &&
                File.Exists(
                    controlledDestinationPath);

            messages.Add(
                testEnvironmentWasPrepared
                    ? "El entorno temporal de aprobación fue " +
                      "preparado."
                    : "El entorno temporal de aprobación no pudo " +
                      "prepararse.");

            MetadataProductiveApplicationCoordinator
                coordinator =
                    new();

            MetadataApplyRequest controlledRequest =
                CreateControlledRequest(
                    controlledDestinationPath);

            MetadataProductiveApplicationResult
                preparedResult =
                    await coordinator.PrepareAsync(
                        controlledRequest,
                        cancellationToken);

            verifiedCopyWasPrepared =
                preparedResult.VerifiedCopyWasPrepared;

            promotionDecisionWasPending =
                preparedResult.PromotionDecision ==
                MetadataPromotionDecision.Pending;

            destinationRemainedUnchangedDuringPreparation =
                await _fileSha256Service.FileMatchesHashAsync(
                    controlledDestinationPath,
                    controlledDestinationHashBefore,
                    CancellationToken.None);

            messages.Add(
                verifiedCopyWasPrepared
                    ? "La copia verificada fue preparada y " +
                      "conservada."
                    : "La copia verificada no fue preparada.");

            messages.Add(
                promotionDecisionWasPending
                    ? "La preparación quedó pendiente de la " +
                      "decisión Approved."
                    : "La preparación no quedó pendiente.");

            messages.Add(
                destinationRemainedUnchangedDuringPreparation
                    ? "El destino temporal permaneció intacto " +
                      "durante la preparación."
                    : "El destino temporal cambió durante la " +
                      "preparación.");

            MetadataProductiveApplicationResult
                completedResult =
                    await coordinator.CompleteAsync(
                        preparedResult,
                        MetadataPromotionDecision.Approved,
                        cancellationToken);

            approvedDecisionWasHandled =
                completedResult.PromotionWasApproved;

            promotionWasSuccessful =
                completedResult.PromotionWasSuccessful;

            productiveBackupWasCreated =
                completedResult.PromotionResult?
                    .ProductiveBackupWasCreated == true;

            productiveBackupWasVerified =
                completedResult.PromotionResult?
                    .ProductiveBackupWasVerified == true;

            replacementWasExecuted =
                completedResult.PromotionResult?
                    .ReplacementWasExecuted == true;

            promotedDestinationWasVerified =
                completedResult.PromotionResult?
                    .PromotedFileWasVerified == true;

            rollbackWasNotRequired =
                completedResult.PromotionResult?
                    .RollbackWasAttempted != true;

            destinationEndedInSafeState =
                completedResult.OriginalEndedInSafeState;

            finalCleanupWasAttempted =
                completedResult.FinalCleanupWasAttempted;

            finalCleanupWasSuccessful =
                completedResult.FinalCleanupWasSuccessful;

            productiveResultWasSuccessful =
                completedResult.WasSuccessfullyPromoted &&
                completedResult.EndedInControlledState;

            productiveBackupPath =
                completedResult.PromotionResult?
                    .ProductiveBackupPath ??
                string.Empty;

            foreach (string message
                in completedResult.Messages)
            {
                messages.Add(
                    message);
            }

            persistedGenre =
                ReadGenre(
                    controlledDestinationPath);

            requestedGenreWasPersisted =
                string.Equals(
                    persistedGenre,
                    TestGenre,
                    StringComparison.OrdinalIgnoreCase);

            referenceOriginalRemainedUnchanged =
                await _fileSha256Service.FileMatchesHashAsync(
                    referenceOriginalPath,
                    referenceOriginalHashBefore,
                    CancellationToken.None);

            messages.Add(
                approvedDecisionWasHandled
                    ? "La decisión Approved fue procesada."
                    : "La decisión Approved no fue procesada.");

            messages.Add(
                promotionWasSuccessful
                    ? "La promoción productiva temporal terminó " +
                      "correctamente."
                    : "La promoción productiva temporal no " +
                      "terminó correctamente.");

            messages.Add(
                requestedGenreWasPersisted
                    ? "El género solicitado fue persistido en el " +
                      "destino temporal."
                    : "El género solicitado no fue persistido.");

            messages.Add(
                referenceOriginalRemainedUnchanged
                    ? "El original de referencia permaneció " +
                      "intacto."
                    : "El original de referencia fue modificado.");

            messages.Add(
                finalCleanupWasSuccessful
                    ? "El entorno aislado fue eliminado " +
                      "correctamente."
                    : "El entorno aislado no fue eliminado.");
        }
        catch (OperationCanceledException)
        {
            errorMessage =
                "La prueba Approved del coordinador productivo " +
                "fue cancelada.";

            messages.Add(
                errorMessage);
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;

            messages.Add(
                "La prueba Approved produjo un error inesperado: " +
                exception.Message);
        }
        finally
        {
            TryDeleteDirectory(
                testDirectoryPath);

            temporaryEnvironmentWasRemoved =
                !string.IsNullOrWhiteSpace(
                    testDirectoryPath) &&
                !Directory.Exists(
                    testDirectoryPath);

            temporaryProductiveBackupWasRemoved =
                string.IsNullOrWhiteSpace(
                    productiveBackupPath) ||
                !File.Exists(
                    productiveBackupPath);

            messages.Add(
                temporaryEnvironmentWasRemoved
                    ? "El entorno temporal general fue eliminado."
                    : "El entorno temporal general permaneció.");

            messages.Add(
                temporaryProductiveBackupWasRemoved
                    ? "El respaldo productivo temporal fue " +
                      "eliminado con el entorno de prueba."
                    : "El respaldo productivo temporal permaneció " +
                      "en el disco.");
        }

        return new
            MetadataProductiveApplicationApprovedTestResult
        {
            TestEnvironmentWasPrepared =
                testEnvironmentWasPrepared,

            VerifiedCopyWasPrepared =
                verifiedCopyWasPrepared,

            PromotionDecisionWasPending =
                promotionDecisionWasPending,

            DestinationRemainedUnchangedDuringPreparation =
                destinationRemainedUnchangedDuringPreparation,

            ApprovedDecisionWasHandled =
                approvedDecisionWasHandled,

            PromotionWasSuccessful =
                promotionWasSuccessful,

            ProductiveBackupWasCreated =
                productiveBackupWasCreated,

            ProductiveBackupWasVerified =
                productiveBackupWasVerified,

            ReplacementWasExecuted =
                replacementWasExecuted,

            PromotedDestinationWasVerified =
                promotedDestinationWasVerified,

            RequestedGenreWasPersisted =
                requestedGenreWasPersisted,

            RollbackWasNotRequired =
                rollbackWasNotRequired,

            ReferenceOriginalRemainedUnchanged =
                referenceOriginalRemainedUnchanged,

            DestinationEndedInSafeState =
                destinationEndedInSafeState,

            FinalCleanupWasAttempted =
                finalCleanupWasAttempted,

            FinalCleanupWasSuccessful =
                finalCleanupWasSuccessful,

            ProductiveResultWasSuccessful =
                productiveResultWasSuccessful,

            TemporaryEnvironmentWasRemoved =
                temporaryEnvironmentWasRemoved,

            TemporaryProductiveBackupWasRemoved =
                temporaryProductiveBackupWasRemoved,

            RequestedGenre =
                TestGenre,

            PersistedGenre =
                persistedGenre,

            ErrorMessage =
                errorMessage,

            Messages =
                messages.ToArray()
        };
    }

    private static MetadataApplyRequest
        CreateControlledRequest(
            string filePath)
    {
        return new MetadataApplyRequest
        {
            RequestId =
                Guid.NewGuid(),

            PlanId =
                Guid.NewGuid(),

            CreatedAtUtc =
                DateTime.UtcNow,

            FilePath =
                filePath,

            FileName =
                Path.GetFileName(
                    filePath),

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
                            ReadGenre(
                                filePath),

                        NewValue =
                            TestGenre,

                        WasManuallyApproved =
                            true,

                        Confidence =
                            1.0,

                        SupportingSources =
                            new[]
                            {
                                "Prueba controlada del camino " +
                                "Approved"
                            }
                    }
                }
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

    private static string NormalizeExistingFilePath(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            throw new ArgumentException(
                "No se recibió una ruta de archivo válida.",
                nameof(filePath));
        }

        string normalizedFilePath =
            Path.GetFullPath(
                filePath.Trim());

        if (!File.Exists(
                normalizedFilePath))
        {
            throw new FileNotFoundException(
                "El archivo indicado no existe.",
                normalizedFilePath);
        }

        return normalizedFilePath;
    }

    private static void TryDeleteDirectory(
        string? directoryPath)
    {
        if (string.IsNullOrWhiteSpace(
                directoryPath))
        {
            return;
        }

        try
        {
            if (Directory.Exists(
                    directoryPath))
            {
                Directory.Delete(
                    directoryPath,
                    recursive:
                        true);
            }
        }
        catch
        {
            // El resultado conservará evidencia si el entorno
            // temporal no puede eliminarse.
        }
    }
}