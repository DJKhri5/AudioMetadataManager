using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Comprueba la reversión automática después de simular un fallo
/// de verificación posterior a una sustitución temporal.
/// </summary>
public sealed class MetadataApplicationRollbackTestRunner
{
    private const string TestFolderName =
        "MetadataApplicationRollbackTests";

    private const string ModifiedGenre =
        "Electronic";

    private const string ExpectedFailureText =
        "Fallo de verificación posterior simulado";

    private readonly IMetadataApplicationPromotionService
        _promotionService;

    private readonly FileSha256Service
        _fileSha256Service;

    /// <summary>
    /// Crea el corredor con las dependencias predeterminadas.
    /// </summary>
    public MetadataApplicationRollbackTestRunner()
        : this(
            new MetadataApplicationPromotionService(),
            new FileSha256Service())
    {
    }

    /// <summary>
    /// Crea el corredor con las dependencias proporcionadas.
    /// </summary>
    public MetadataApplicationRollbackTestRunner(
        IMetadataApplicationPromotionService promotionService,
        FileSha256Service fileSha256Service)
    {
        _promotionService =
            promotionService ??
            throw new ArgumentNullException(
                nameof(promotionService));

        _fileSha256Service =
            fileSha256Service ??
            throw new ArgumentNullException(
                nameof(fileSha256Service));
    }

    /// <summary>
    /// Ejecuta la prueba exclusivamente sobre archivos
    /// temporales derivados del archivo indicado.
    /// </summary>
    public async Task<MetadataApplicationRollbackTestResult>
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

        string destinationPath =
            string.Empty;

        string verifiedCopyPath =
            string.Empty;

        string referenceHashBefore =
            string.Empty;

        string destinationHashBefore =
            string.Empty;

        MetadataApplicationPromotionResult?
            promotionResult =
                null;

        bool testEnvironmentWasPrepared =
            false;

        bool destinationWasRestored =
            false;

        bool referenceOriginalRemainedUnchanged =
            false;

        bool verificationFailureWasSimulated =
            false;

        bool testEnvironmentWasRemoved =
            false;

        bool temporaryBackupWasRemoved =
            false;

        string unexpectedErrorMessage =
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

            destinationPath =
                Path.Combine(
                    testDirectoryPath,
                    $"destination{extension}");

            verifiedCopyPath =
                Path.Combine(
                    testDirectoryPath,
                    $"verified_copy{extension}");

            File.Copy(
                normalizedSourcePath,
                referenceOriginalPath,
                overwrite:
                    false);

            File.Copy(
                normalizedSourcePath,
                destinationPath,
                overwrite:
                    false);

            File.Copy(
                normalizedSourcePath,
                verifiedCopyPath,
                overwrite:
                    false);

            testEnvironmentWasPrepared =
                File.Exists(
                    referenceOriginalPath) &&
                File.Exists(
                    destinationPath) &&
                File.Exists(
                    verifiedCopyPath);

            if (!testEnvironmentWasPrepared)
            {
                throw new IOException(
                    "El entorno temporal de reversión no pudo " +
                    "ser preparado.");
            }

            messages.Add(
                "El entorno temporal de reversión fue preparado.");

            referenceHashBefore =
                await _fileSha256Service.ComputeAsync(
                    referenceOriginalPath,
                    cancellationToken);

            destinationHashBefore =
                await _fileSha256Service.ComputeAsync(
                    destinationPath,
                    cancellationToken);

            WriteGenre(
                verifiedCopyPath,
                ModifiedGenre);

            bool verifiedCopyDiffersFromDestination =
                !await _fileSha256Service.FilesMatchAsync(
                    verifiedCopyPath,
                    destinationPath,
                    cancellationToken);

            if (!verifiedCopyDiffersFromDestination)
            {
                throw new InvalidOperationException(
                    "La copia verificada no contiene cambios " +
                    "respecto del destino temporal.");
            }

            messages.Add(
                "La copia verificada fue modificada para la " +
                "prueba.");

            promotionResult =
                await _promotionService.PromoteAsync(
                    verifiedCopyPath,
                    destinationPath,
                    MetadataApplicationPromotionOptions
                        .SimulatedVerificationFailure,
                    cancellationToken);

            verificationFailureWasSimulated =
                promotionResult.Messages.Any(
                    message =>
                        message.Contains(
                            "simuló un fallo de verificación",
                            StringComparison.OrdinalIgnoreCase)) &&
                promotionResult.ErrorMessage.Contains(
                    ExpectedFailureText,
                    StringComparison.OrdinalIgnoreCase);

            destinationWasRestored =
                await _fileSha256Service.FileMatchesHashAsync(
                    destinationPath,
                    destinationHashBefore,
                    CancellationToken.None);

            referenceOriginalRemainedUnchanged =
                await _fileSha256Service.FileMatchesHashAsync(
                    referenceOriginalPath,
                    referenceHashBefore,
                    CancellationToken.None);

            messages.Add(
                verificationFailureWasSimulated
                    ? "El fallo de verificación fue simulado " +
                      "correctamente."
                    : "El fallo de verificación no fue " +
                      "detectado como simulado.");

            messages.Add(
                destinationWasRestored
                    ? "El destino temporal fue restaurado a su " +
                      "estado inicial."
                    : "El destino temporal no fue restaurado.");

            messages.Add(
                referenceOriginalRemainedUnchanged
                    ? "El original de referencia permaneció " +
                      "intacto."
                    : "El original de referencia fue modificado.");

            foreach (string message
                in promotionResult.Messages)
            {
                messages.Add(
                    message);
            }
        }
        catch (OperationCanceledException)
        {
            unexpectedErrorMessage =
                "La prueba de reversión fue cancelada.";

            messages.Add(
                unexpectedErrorMessage);
        }
        catch (Exception exception)
        {
            unexpectedErrorMessage =
                exception.Message;

            messages.Add(
                "La prueba produjo un error inesperado: " +
                exception.Message);
        }
        finally
        {
            string productiveBackupPath =
                promotionResult?
                    .ProductiveBackupPath ??
                string.Empty;

            TryDeleteDirectory(
                testDirectoryPath);

            testEnvironmentWasRemoved =
                !string.IsNullOrWhiteSpace(
                    testDirectoryPath) &&
                !Directory.Exists(
                    testDirectoryPath);

            temporaryBackupWasRemoved =
                string.IsNullOrWhiteSpace(
                    productiveBackupPath) ||
                !File.Exists(
                    productiveBackupPath);

            messages.Add(
                testEnvironmentWasRemoved
                    ? "El entorno temporal de reversión fue " +
                      "eliminado."
                    : "El entorno temporal de reversión " +
                      "permaneció en el disco.");

            messages.Add(
                temporaryBackupWasRemoved
                    ? "El respaldo productivo temporal fue " +
                      "eliminado."
                    : "El respaldo productivo temporal no fue " +
                      "eliminado.");
        }

        bool inputsWereValidated =
            promotionResult?.InputsWereValidated == true;

        bool productiveBackupWasCreated =
            promotionResult?
                .ProductiveBackupWasCreated == true;

        bool productiveBackupWasVerified =
            promotionResult?
                .ProductiveBackupWasVerified == true;

        bool replacementWasExecuted =
            promotionResult?.ReplacementWasExecuted == true;

        bool rollbackWasAttempted =
            promotionResult?.RollbackWasAttempted == true;

        bool rollbackWasSuccessful =
            promotionResult?.RollbackWasSuccessful == true;

        bool verifiedCopyWasPreserved =
            promotionResult?.VerifiedCopyWasPreserved == true;

        bool destinationEndedInSafeState =
            promotionResult?
                .DestinationEndedInSafeState == true;

        string expectedErrorMessage =
            string.IsNullOrWhiteSpace(
                unexpectedErrorMessage)
                ? promotionResult?.ErrorMessage ??
                  string.Empty
                : string.Empty;

        messages.Add(
            inputsWereValidated
                ? "Las entradas fueron validadas."
                : "Las entradas no fueron validadas.");

        messages.Add(
            productiveBackupWasCreated
                ? "El respaldo productivo fue creado."
                : "El respaldo productivo no fue creado.");

        messages.Add(
            productiveBackupWasVerified
                ? "El respaldo productivo fue verificado."
                : "El respaldo productivo no fue verificado.");

        messages.Add(
            replacementWasExecuted
                ? "La sustitución temporal fue ejecutada."
                : "La sustitución temporal no fue ejecutada.");

        messages.Add(
            rollbackWasAttempted
                ? "La reversión automática fue iniciada."
                : "La reversión automática no fue iniciada.");

        messages.Add(
            rollbackWasSuccessful
                ? "La reversión automática terminó " +
                  "correctamente."
                : "La reversión automática no terminó " +
                  "correctamente.");

        messages.Add(
            verifiedCopyWasPreserved
                ? "La copia verificada fue preservada."
                : "La copia verificada no fue preservada.");

        messages.Add(
            destinationEndedInSafeState
                ? "El destino terminó en un estado seguro."
                : "El destino no terminó en un estado seguro.");

        return new MetadataApplicationRollbackTestResult
        {
            TestEnvironmentWasPrepared =
                testEnvironmentWasPrepared,

            InputsWereValidated =
                inputsWereValidated,

            ProductiveBackupWasCreated =
                productiveBackupWasCreated,

            ProductiveBackupWasVerified =
                productiveBackupWasVerified,

            ReplacementWasExecuted =
                replacementWasExecuted,

            VerificationFailureWasSimulated =
                verificationFailureWasSimulated,

            RollbackWasAttempted =
                rollbackWasAttempted,

            RollbackWasSuccessful =
                rollbackWasSuccessful,

            DestinationWasRestored =
                destinationWasRestored,

            ReferenceOriginalRemainedUnchanged =
                referenceOriginalRemainedUnchanged,

            VerifiedCopyWasPreserved =
                verifiedCopyWasPreserved,

            DestinationEndedInSafeState =
                destinationEndedInSafeState,

            TestEnvironmentWasRemoved =
                testEnvironmentWasRemoved,

            TemporaryBackupWasRemoved =
                temporaryBackupWasRemoved,

            ExpectedErrorMessage =
                expectedErrorMessage,

            Messages =
                messages.ToArray()
        };
    }

    private static void WriteGenre(
        string filePath,
        string genre)
    {
        using TagLib.File file =
            TagLib.File.Create(
                filePath);

        file.Tag.Genres =
            new[]
            {
                genre
            };

        file.Save();
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