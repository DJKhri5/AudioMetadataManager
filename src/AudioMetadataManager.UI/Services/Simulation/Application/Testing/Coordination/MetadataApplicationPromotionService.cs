using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Promueve una copia previamente verificada hacia un archivo de
/// destino mediante respaldo productivo, sustitución controlada,
/// verificación SHA-256 y reversión ante fallos.
/// </summary>
public sealed class MetadataApplicationPromotionService :
    IMetadataApplicationPromotionService
{
    private const string BackupRootFolderName =
        "AudioMetadataManager_Backup";

    private readonly FileSha256Service
        _fileSha256Service;

    /// <summary>
    /// Crea el servicio con la implementación predeterminada de
    /// verificación SHA-256.
    /// </summary>
    public MetadataApplicationPromotionService()
        : this(
            new FileSha256Service())
    {
    }

    /// <summary>
    /// Crea el servicio con el verificador proporcionado.
    /// </summary>
    public MetadataApplicationPromotionService(
        FileSha256Service fileSha256Service)
    {
        _fileSha256Service =
            fileSha256Service ??
            throw new ArgumentNullException(
                nameof(fileSha256Service));
    }

    /// <inheritdoc />
    public Task<MetadataApplicationPromotionResult>
        PromoteAsync(
            string verifiedWorkingCopyPath,
            string destinationFilePath,
            CancellationToken cancellationToken = default)
    {
        return PromoteAsync(
            verifiedWorkingCopyPath,
            destinationFilePath,
            MetadataApplicationPromotionOptions.SafeDefault,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MetadataApplicationPromotionResult>
        PromoteAsync(
            string verifiedWorkingCopyPath,
            string destinationFilePath,
            MetadataApplicationPromotionOptions options,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        List<string> messages =
            new();

        string normalizedVerifiedCopyPath =
            string.Empty;

        string normalizedDestinationPath =
            string.Empty;

        string productiveBackupPath =
            string.Empty;

        string destinationHashBefore =
            string.Empty;

        string verifiedCopyHash =
            string.Empty;

        string destinationHashAfter =
            string.Empty;

        string stagingPath =
            string.Empty;

        string rollbackStagingPath =
            string.Empty;

        bool inputsWereValidated =
            false;

        bool productiveBackupWasCreated =
            false;

        bool productiveBackupWasVerified =
            false;

        bool replacementWasExecuted =
            false;

        bool promotedFileWasVerified =
            false;

        bool rollbackWasAttempted =
            false;

        bool rollbackWasSuccessful =
            false;

        string errorMessage =
            string.Empty;

        try
        {
            normalizedVerifiedCopyPath =
                NormalizeExistingFilePath(
                    verifiedWorkingCopyPath,
                    nameof(verifiedWorkingCopyPath));

            normalizedDestinationPath =
                NormalizeExistingFilePath(
                    destinationFilePath,
                    nameof(destinationFilePath));

            ValidateDifferentPaths(
                normalizedVerifiedCopyPath,
                normalizedDestinationPath);

            ValidateMatchingExtensions(
                normalizedVerifiedCopyPath,
                normalizedDestinationPath);

            inputsWereValidated =
                true;

            messages.Add(
                "Las rutas y los archivos de entrada fueron " +
                "validados.");

            cancellationToken
                .ThrowIfCancellationRequested();

            destinationHashBefore =
                await _fileSha256Service.ComputeAsync(
                    normalizedDestinationPath,
                    cancellationToken);

            verifiedCopyHash =
                await _fileSha256Service.ComputeAsync(
                    normalizedVerifiedCopyPath,
                    cancellationToken);

            productiveBackupPath =
                CreateProductiveBackupPath(
                    normalizedDestinationPath);

            string? backupDirectoryPath =
                Path.GetDirectoryName(
                    productiveBackupPath);

            if (string.IsNullOrWhiteSpace(
                    backupDirectoryPath))
            {
                throw new InvalidOperationException(
                    "No fue posible determinar la carpeta del " +
                    "respaldo productivo.");
            }

            Directory.CreateDirectory(
                backupDirectoryPath);

            File.Copy(
                normalizedDestinationPath,
                productiveBackupPath,
                overwrite:
                    false);

            productiveBackupWasCreated =
                File.Exists(
                    productiveBackupPath);

            if (!productiveBackupWasCreated)
            {
                throw new IOException(
                    "El respaldo productivo no fue creado.");
            }

            messages.Add(
                "El respaldo productivo fue creado.");

            productiveBackupWasVerified =
                await _fileSha256Service
                    .FileMatchesHashAsync(
                        productiveBackupPath,
                        destinationHashBefore,
                        cancellationToken);

            if (!productiveBackupWasVerified)
            {
                throw new InvalidOperationException(
                    "El respaldo productivo no coincide con el " +
                    "estado original del destino.");
            }

            messages.Add(
                "El respaldo productivo fue verificado.");

            string destinationDirectoryPath =
                Path.GetDirectoryName(
                    normalizedDestinationPath) ??
                throw new InvalidOperationException(
                    "No fue posible determinar la carpeta del " +
                    "archivo de destino.");

            string destinationFileName =
                Path.GetFileName(
                    normalizedDestinationPath);

            stagingPath =
                Path.Combine(
                    destinationDirectoryPath,
                    $".{destinationFileName}." +
                    $"{Guid.NewGuid():N}.promotion.tmp");

            File.Copy(
                normalizedVerifiedCopyPath,
                stagingPath,
                overwrite:
                    false);

            bool stagingMatchesVerifiedCopy =
                await _fileSha256Service.FilesMatchAsync(
                    normalizedVerifiedCopyPath,
                    stagingPath,
                    cancellationToken);

            if (!stagingMatchesVerifiedCopy)
            {
                throw new InvalidOperationException(
                    "El archivo preparado para la sustitución " +
                    "no coincide con la copia verificada.");
            }

            messages.Add(
                "El archivo de preparación fue creado y " +
                "verificado junto al destino.");

            cancellationToken
                .ThrowIfCancellationRequested();

            File.Replace(
                stagingPath,
                normalizedDestinationPath,
                destinationBackupFileName:
                    null,
                ignoreMetadataErrors:
                    true);

            replacementWasExecuted =
                true;

            stagingPath =
                string.Empty;

            messages.Add(
                "La sustitución controlada fue ejecutada.");

            destinationHashAfter =
                await _fileSha256Service.ComputeAsync(
                    normalizedDestinationPath,
                    cancellationToken);

            bool promotedHashMatchesVerifiedCopy =
                string.Equals(
                    destinationHashAfter,
                    verifiedCopyHash,
                    StringComparison.OrdinalIgnoreCase);

            if (options
                .SimulatePostReplacementVerificationFailure)
            {
                messages.Add(
                    "Se simuló un fallo de verificación después de " +
                    "la sustitución.");

                throw new InvalidOperationException(
                    "Fallo de verificación posterior simulado para " +
                    "comprobar la reversión automática.");
            }

            promotedFileWasVerified =
                promotedHashMatchesVerifiedCopy;

            if (!promotedFileWasVerified)
            {
                throw new InvalidOperationException(
                    "El archivo promovido no coincide con la " +
                    "copia verificada.");
            }

            messages.Add(
                "El archivo promovido coincide con la copia " +
                "verificada.");
        }
        catch (OperationCanceledException)
        {
            errorMessage =
                "La promoción controlada fue cancelada.";

            messages.Add(
                errorMessage);
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;

            messages.Add(
                $"La promoción produjo un error: " +
                $"{exception.Message}");
        }

        if (!promotedFileWasVerified &&
            productiveBackupWasVerified &&
            replacementWasExecuted)
        {
            rollbackWasAttempted =
                true;

            messages.Add(
                "Se inició la reversión desde el respaldo " +
                "productivo.");

            try
            {
                string destinationDirectoryPath =
                    Path.GetDirectoryName(
                        normalizedDestinationPath) ??
                    throw new InvalidOperationException(
                        "No fue posible determinar la carpeta " +
                        "del destino durante la reversión.");

                string destinationFileName =
                    Path.GetFileName(
                        normalizedDestinationPath);

                rollbackStagingPath =
                    Path.Combine(
                        destinationDirectoryPath,
                        $".{destinationFileName}." +
                        $"{Guid.NewGuid():N}.rollback.tmp");

                File.Copy(
                    productiveBackupPath,
                    rollbackStagingPath,
                    overwrite:
                        false);

                bool rollbackStagingWasVerified =
                    await _fileSha256Service
                        .FileMatchesHashAsync(
                            rollbackStagingPath,
                            destinationHashBefore,
                            CancellationToken.None);

                if (!rollbackStagingWasVerified)
                {
                    throw new InvalidOperationException(
                        "El archivo preparado para la reversión " +
                        "no coincide con el respaldo productivo.");
                }

                File.Replace(
                    rollbackStagingPath,
                    normalizedDestinationPath,
                    destinationBackupFileName:
                        null,
                    ignoreMetadataErrors:
                        true);

                rollbackStagingPath =
                    string.Empty;

                rollbackWasSuccessful =
                    await _fileSha256Service
                        .FileMatchesHashAsync(
                            normalizedDestinationPath,
                            destinationHashBefore,
                            CancellationToken.None);

                messages.Add(
                    rollbackWasSuccessful
                        ? "El archivo de destino fue restaurado " +
                          "correctamente."
                        : "La reversión no pudo ser verificada.");
            }
            catch (Exception rollbackException)
            {
                rollbackWasSuccessful =
                    false;

                messages.Add(
                    "La reversión produjo un error: " +
                    rollbackException.Message);

                if (string.IsNullOrWhiteSpace(
                        errorMessage))
                {
                    errorMessage =
                        rollbackException.Message;
                }
            }
        }

        TryDeleteFile(
            stagingPath);

        TryDeleteFile(
            rollbackStagingPath);

        if (!options.PreserveVerifiedWorkingCopy)
        {
            TryDeleteFile(
                normalizedVerifiedCopyPath);
        }

        bool verifiedCopyWasPreserved =
            !string.IsNullOrWhiteSpace(
                normalizedVerifiedCopyPath) &&
            File.Exists(
                normalizedVerifiedCopyPath);

        return new MetadataApplicationPromotionResult
        {
            VerifiedWorkingCopyPath =
                normalizedVerifiedCopyPath,

            DestinationFilePath =
                normalizedDestinationPath,

            ProductiveBackupPath =
                productiveBackupPath,

            DestinationHashBefore =
                destinationHashBefore,

            VerifiedCopyHash =
                verifiedCopyHash,

            DestinationHashAfter =
                destinationHashAfter,

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

            RollbackWasAttempted =
                rollbackWasAttempted,

            RollbackWasSuccessful =
                rollbackWasSuccessful,

            VerifiedCopyWasPreserved =
                verifiedCopyWasPreserved,

            ErrorMessage =
                errorMessage,

            Messages =
                messages.ToArray()
        };
    }

    private static string NormalizeExistingFilePath(
        string? filePath,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            throw new ArgumentException(
                "No se recibió una ruta de archivo válida.",
                parameterName);
        }

        string normalizedFilePath;

        try
        {
            normalizedFilePath =
                Path.GetFullPath(
                    filePath.Trim());
        }
        catch (Exception exception)
            when (exception is ArgumentException or
                  NotSupportedException or
                  PathTooLongException)
        {
            throw new ArgumentException(
                "La ruta del archivo no es válida.",
                parameterName,
                exception);
        }

        if (!File.Exists(
                normalizedFilePath))
        {
            throw new FileNotFoundException(
                "El archivo indicado no existe.",
                normalizedFilePath);
        }

        return normalizedFilePath;
    }

    private static void ValidateDifferentPaths(
        string verifiedWorkingCopyPath,
        string destinationFilePath)
    {
        if (string.Equals(
                verifiedWorkingCopyPath,
                destinationFilePath,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "La copia verificada y el archivo de destino " +
                "deben ser archivos diferentes.");
        }
    }

    private static void ValidateMatchingExtensions(
        string verifiedWorkingCopyPath,
        string destinationFilePath)
    {
        string verifiedExtension =
            Path.GetExtension(
                verifiedWorkingCopyPath);

        string destinationExtension =
            Path.GetExtension(
                destinationFilePath);

        if (!string.Equals(
                verifiedExtension,
                destinationExtension,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "La copia verificada y el destino deben tener " +
                "la misma extensión.");
        }
    }

    private static string CreateProductiveBackupPath(
        string destinationFilePath)
    {
        string destinationDirectoryPath =
            Path.GetDirectoryName(
                destinationFilePath) ??
            throw new InvalidOperationException(
                "No fue posible determinar la carpeta del " +
                "archivo de destino.");

        string destinationFileName =
            Path.GetFileName(
                destinationFilePath);

        return Path.Combine(
            destinationDirectoryPath,
            BackupRootFolderName,
            DateTime.UtcNow.ToString(
                "yyyy-MM-dd"),
            Guid.NewGuid().ToString(
                "N"),
            destinationFileName);
    }

    private static void TryDeleteFile(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return;
        }

        try
        {
            if (File.Exists(
                    filePath))
            {
                File.Delete(
                    filePath);
            }
        }
        catch
        {
            // La limpieza auxiliar no debe ocultar el resultado
            // principal de la promoción o de la reversión.
        }
    }
}