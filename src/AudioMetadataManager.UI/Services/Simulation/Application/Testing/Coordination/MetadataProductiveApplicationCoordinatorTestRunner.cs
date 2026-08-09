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
/// Ejecuta comprobaciones controladas del coordinador productivo
/// individual usando exclusivamente archivos temporales.
/// </summary>
public sealed class
    MetadataProductiveApplicationCoordinatorTestRunner
{
    private const string TestFolderName =
        "MetadataProductiveApplicationCoordinatorTests";

    private static readonly string TestGenre =
        DiagnosticMetadataTestValues.CreateGenre();

    private readonly FileSha256Service
        _fileSha256Service;

    /// <summary>
    /// Crea el corredor con las dependencias predeterminadas.
    /// </summary>
    public MetadataProductiveApplicationCoordinatorTestRunner()
        : this(
            new FileSha256Service())
    {
    }

    /// <summary>
    /// Crea el corredor con el servicio de hash proporcionado.
    /// </summary>
    public MetadataProductiveApplicationCoordinatorTestRunner(
        FileSha256Service fileSha256Service)
    {
        _fileSha256Service =
            fileSha256Service ??
            throw new ArgumentNullException(
                nameof(fileSha256Service));
    }

    /// <summary>
    /// Ejecuta las comprobaciones sobre una copia temporal del
    /// archivo indicado.
    /// </summary>
    public async Task<
        MetadataProductiveApplicationCoordinatorTestResult>
        RunAsync(
            string sourceFilePath,
            CancellationToken cancellationToken = default)
    {
        List<string> messages =
            new();

        string testDirectoryPath =
            string.Empty;

        string controlledDestinationPath =
            string.Empty;

        string controlledDestinationHashBefore =
            string.Empty;

        MetadataProductiveApplicationResult?
            preparedResult =
                null;

        MetadataProductiveApplicationResult?
            declinedResult =
                null;

        bool nullRequestWasRejected =
            false;

        bool verifiedCopyWasPrepared =
            false;

        bool promotionDecisionWasPending =
            false;

        bool originalRemainedUnchangedDuringPreparation =
            false;

        bool declinedDecisionWasHandled =
            false;

        bool declinedDecisionSkippedPromotion =
            false;

        bool declinedOriginalEndedInSafeState =
            false;

        bool declinedEnvironmentWasCleaned =
            false;

        bool declinedResultWasSuccessful =
            false;

        bool invalidDecisionWasRejected =
            false;

        bool reusedPreparationWasRejected =
            false;

        bool temporaryEnvironmentWasRemoved =
            false;

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

            controlledDestinationPath =
                Path.Combine(
                    testDirectoryPath,
                    $"controlled_destination{extension}");

            File.Copy(
                normalizedSourcePath,
                controlledDestinationPath,
                overwrite:
                    false);

            controlledDestinationHashBefore =
                await _fileSha256Service.ComputeAsync(
                    controlledDestinationPath,
                    cancellationToken);

            MetadataProductiveApplicationCoordinator
                coordinator =
                    new();

            try
            {
                await coordinator.PrepareAsync(
                    null!,
                    cancellationToken);

                messages.Add(
                    "El coordinador aceptó una solicitud nula.");
            }
            catch (ArgumentNullException exception)
                when (exception.ParamName == "request")
            {
                nullRequestWasRejected =
                    true;

                messages.Add(
                    "La solicitud nula fue rechazada " +
                    "correctamente.");
            }

            MetadataApplyRequest controlledRequest =
                CreateControlledRequest(
                    controlledDestinationPath);

            preparedResult =
                await coordinator.PrepareAsync(
                    controlledRequest,
                    cancellationToken);

            verifiedCopyWasPrepared =
                preparedResult.VerifiedCopyWasPrepared;

            promotionDecisionWasPending =
                preparedResult.PromotionDecision ==
                MetadataPromotionDecision.Pending;

            originalRemainedUnchangedDuringPreparation =
                await _fileSha256Service.FileMatchesHashAsync(
                    controlledDestinationPath,
                    controlledDestinationHashBefore,
                    CancellationToken.None);

            messages.Add(
                verifiedCopyWasPrepared
                    ? "La copia verificada fue preparada."
                    : "La copia verificada no fue preparada.");

            messages.Add(
                promotionDecisionWasPending
                    ? "La preparación quedó pendiente de una " +
                      "decisión."
                    : "La preparación no quedó pendiente.");

            messages.Add(
                originalRemainedUnchangedDuringPreparation
                    ? "El destino controlado permaneció intacto " +
                      "durante la preparación."
                    : "El destino controlado fue modificado " +
                      "durante la preparación.");

            MetadataProductiveApplicationResult
                invalidDecisionResult =
                    await coordinator.CompleteAsync(
                        preparedResult,
                        MetadataPromotionDecision.Pending,
                        cancellationToken);

            invalidDecisionWasRejected =
                !string.IsNullOrWhiteSpace(
                    invalidDecisionResult.ErrorMessage) &&
                invalidDecisionResult.ErrorMessage.Contains(
                    "Approved o Declined",
                    StringComparison.OrdinalIgnoreCase) &&
                invalidDecisionResult.PromotionResult is null;

            messages.Add(
                invalidDecisionWasRejected
                    ? "La decisión no permitida fue rechazada " +
                      "correctamente."
                    : "La decisión no permitida no fue rechazada " +
                      "correctamente.");

            declinedResult =
                await coordinator.CompleteAsync(
                    preparedResult,
                    MetadataPromotionDecision.Declined,
                    cancellationToken);

            declinedDecisionWasHandled =
                declinedResult.PromotionWasDeclined;

            declinedDecisionSkippedPromotion =
                declinedResult.PromotionResult is null;

            declinedOriginalEndedInSafeState =
                declinedResult.OriginalEndedInSafeState &&
                await _fileSha256Service.FileMatchesHashAsync(
                    controlledDestinationPath,
                    controlledDestinationHashBefore,
                    CancellationToken.None);

            declinedEnvironmentWasCleaned =
                declinedResult.FinalCleanupWasAttempted &&
                declinedResult.FinalCleanupWasSuccessful;

            declinedResultWasSuccessful =
                declinedResult.WasSafelyDeclined &&
                declinedResult.EndedInControlledState;

            messages.Add(
                declinedDecisionWasHandled
                    ? "La decisión Declined fue procesada."
                    : "La decisión Declined no fue procesada.");

            messages.Add(
                declinedDecisionSkippedPromotion
                    ? "El rechazo evitó ejecutar la promoción."
                    : "La promoción fue ejecutada pese al " +
                      "rechazo.");

            messages.Add(
                declinedOriginalEndedInSafeState
                    ? "El destino controlado terminó intacto."
                    : "El destino controlado no terminó en su " +
                      "estado inicial.");

            messages.Add(
                declinedEnvironmentWasCleaned
                    ? "El entorno aislado fue eliminado."
                    : "El entorno aislado no fue eliminado.");

            try
            {
                MetadataProductiveApplicationResult
                    reusedResult =
                        await coordinator.CompleteAsync(
                            preparedResult,
                            MetadataPromotionDecision.Declined,
                            cancellationToken);

                reusedPreparationWasRejected =
                    !string.IsNullOrWhiteSpace(
                        reusedResult.ErrorMessage) &&
                    reusedResult.ErrorMessage.Contains(
                        "ya fue finalizada",
                        StringComparison.OrdinalIgnoreCase);
            }
            catch (InvalidOperationException exception)
                when (exception.Message.Contains(
                    "ya fue finalizada",
                    StringComparison.OrdinalIgnoreCase))
            {
                reusedPreparationWasRejected =
                    true;
            }

            messages.Add(
                reusedPreparationWasRejected
                    ? "La preparación reutilizada fue rechazada."
                    : "La preparación pudo reutilizarse " +
                      "incorrectamente.");
        }
        catch (OperationCanceledException)
        {
            errorMessage =
                "La prueba del coordinador productivo fue " +
                "cancelada.";

            messages.Add(
                errorMessage);
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;

            messages.Add(
                "La prueba produjo un error inesperado: " +
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

            messages.Add(
                temporaryEnvironmentWasRemoved
                    ? "El entorno temporal general fue " +
                      "eliminado."
                    : "El entorno temporal general permaneció " +
                      "en el disco.");
        }

        return new
            MetadataProductiveApplicationCoordinatorTestResult
        {
            NullRequestWasRejected =
                nullRequestWasRejected,

            VerifiedCopyWasPrepared =
                verifiedCopyWasPrepared,

            PromotionDecisionWasPending =
                promotionDecisionWasPending,

            OriginalRemainedUnchangedDuringPreparation =
                originalRemainedUnchangedDuringPreparation,

            DeclinedDecisionWasHandled =
                declinedDecisionWasHandled,

            DeclinedDecisionSkippedPromotion =
                declinedDecisionSkippedPromotion,

            DeclinedOriginalEndedInSafeState =
                declinedOriginalEndedInSafeState,

            DeclinedEnvironmentWasCleaned =
                declinedEnvironmentWasCleaned,

            DeclinedResultWasSuccessful =
                declinedResultWasSuccessful,

            InvalidDecisionWasRejected =
                invalidDecisionWasRejected,

            ReusedPreparationWasRejected =
                reusedPreparationWasRejected,

            TemporaryEnvironmentWasRemoved =
                temporaryEnvironmentWasRemoved,

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
                            string.Empty,

                        NewValue =
                            TestGenre,

                        WasManuallyApproved =
                            true,

                        Confidence =
                            1.0,

                        SupportingSources =
                            new[]
                            {
                                "Prueba controlada del coordinador productivo"
                            }
                    }
                }
        };
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