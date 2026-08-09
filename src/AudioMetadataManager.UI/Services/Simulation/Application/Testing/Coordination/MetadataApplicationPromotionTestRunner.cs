using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta una promoción controlada exclusivamente sobre copias
/// temporales, sin modificar el archivo original proporcionado.
/// </summary>
public sealed class MetadataApplicationPromotionTestRunner
{
    private const string TestFolderName =
        "MetadataApplicationPromotionTests";

    private static readonly string PromotedGenre =
        DiagnosticMetadataTestValues.CreateGenre();

    private readonly IMetadataApplicationPromotionService
        _promotionService;

    private readonly FileSha256Service
        _fileSha256Service;

    /// <summary>
    /// Crea el corredor con las dependencias predeterminadas.
    /// </summary>
    public MetadataApplicationPromotionTestRunner()
        : this(
            new MetadataApplicationPromotionService(),
            new FileSha256Service())
    {
    }

    /// <summary>
    /// Crea el corredor con las dependencias proporcionadas.
    /// </summary>
    public MetadataApplicationPromotionTestRunner(
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
    /// Ejecuta la prueba sobre archivos temporales derivados del
    /// archivo indicado.
    /// </summary>
    public async Task<MetadataApplicationPromotionTestResult>
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

        MetadataApplicationPromotionResult?
            promotionResult =
                null;

        bool testEnvironmentWasPrepared =
            false;

        bool referenceOriginalRemainedUnchanged =
            false;

        bool testEnvironmentWasRemoved =
            false;

        bool temporaryBackupWasRemoved =
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
                    "El entorno temporal de promoción no pudo " +
                    "ser preparado.");
            }

            messages.Add(
                "El entorno temporal de promoción fue preparado.");

            referenceHashBefore =
                await _fileSha256Service.ComputeAsync(
                    referenceOriginalPath,
                    cancellationToken);

            WriteGenre(
                verifiedCopyPath,
                PromotedGenre);

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
                "La copia verificada fue modificada de forma " +
                "controlada.");

            promotionResult =
                await _promotionService.PromoteAsync(
                    verifiedCopyPath,
                    destinationPath,
                    cancellationToken);

            referenceOriginalRemainedUnchanged =
                await _fileSha256Service
                    .FileMatchesHashAsync(
                        referenceOriginalPath,
                        referenceHashBefore,
                        CancellationToken.None);

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
            errorMessage =
                "La prueba de promoción fue cancelada.";

            messages.Add(
                errorMessage);
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;

            messages.Add(
                $"La prueba produjo un error: " +
                $"{exception.Message}");
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
                    ? "El entorno temporal de prueba fue " +
                      "eliminado."
                    : "El entorno temporal de prueba permaneció " +
                      "en el disco.");

            messages.Add(
                temporaryBackupWasRemoved
                    ? "El respaldo productivo temporal fue " +
                      "eliminado con el entorno de prueba."
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

        bool promotedFileWasVerified =
            promotionResult?.PromotedFileWasVerified == true;

        bool verifiedCopyWasPreserved =
            promotionResult?.VerifiedCopyWasPreserved == true;

        bool rollbackWasNotRequired =
            promotionResult is not null &&
            !promotionResult.RollbackWasAttempted;

        string finalErrorMessage =
            !string.IsNullOrWhiteSpace(
                errorMessage)
                ? errorMessage
                : promotionResult?.ErrorMessage ??
                  string.Empty;

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
                ? "La sustitución fue ejecutada."
                : "La sustitución no fue ejecutada.");

        messages.Add(
            promotedFileWasVerified
                ? "El destino coincide con la copia verificada."
                : "El destino no coincide con la copia verificada.");

        messages.Add(
            verifiedCopyWasPreserved
                ? "La copia verificada fue preservada."
                : "La copia verificada no fue preservada.");

        messages.Add(
            rollbackWasNotRequired
                ? "No fue necesario ejecutar una reversión."
                : "La operación requirió una reversión.");

        return new MetadataApplicationPromotionTestResult
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

            PromotedFileWasVerified =
                promotedFileWasVerified,

            ReferenceOriginalRemainedUnchanged =
                referenceOriginalRemainedUnchanged,

            VerifiedCopyWasPreserved =
                verifiedCopyWasPreserved,

            RollbackWasNotRequired =
                rollbackWasNotRequired,

            TestEnvironmentWasRemoved =
                testEnvironmentWasRemoved,

            TemporaryBackupWasRemoved =
                temporaryBackupWasRemoved,

            ErrorMessage =
                finalErrorMessage,

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
            // El resultado conservará la evidencia de que la
            // carpeta temporal no pudo ser eliminada.
        }
    }
}