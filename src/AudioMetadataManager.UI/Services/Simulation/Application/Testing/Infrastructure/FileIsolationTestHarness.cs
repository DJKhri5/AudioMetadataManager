using System.IO;
using System.Security.Cryptography;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

/// <summary>
/// Crea y verifica entornos aislados para pruebas que pueden
/// modificar archivos.
///
/// El componente sometido a prueba sólo debe recibir la ruta
/// WorkingCopyPath. El archivo original nunca debe utilizarse
/// como destino de escritura.
/// </summary>
public sealed class FileIsolationTestHarness
{
    private const string DefaultRootFolderName =
        "AudioMetadataManager";

    private const string DefaultTestFolderName =
        "FileIsolationTests";

    /// <summary>
    /// Crea una copia aislada y un respaldo independiente antes
    /// de ejecutar cualquier operación de escritura.
    /// </summary>
    public async Task<FileIsolationContext> CreateAsync(
        string? originalFilePath,
        string? testFolderName,
        CancellationToken cancellationToken = default)
    {
        string normalizedOriginalPath =
            NormalizePath(
                originalFilePath);

        if (string.IsNullOrWhiteSpace(
                normalizedOriginalPath))
        {
            throw new ArgumentException(
                "No se recibió una ruta de archivo válida.",
                nameof(originalFilePath));
        }

        if (!File.Exists(
                normalizedOriginalPath))
        {
            throw new FileNotFoundException(
                "El archivo original indicado no existe.",
                normalizedOriginalPath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        string normalizedTestFolderName =
            NormalizeFolderName(
                testFolderName);

        string testDirectoryPath =
            Path.Combine(
                Path.GetTempPath(),
                DefaultRootFolderName,
                normalizedTestFolderName,
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            testDirectoryPath);

        string originalFileName =
            Path.GetFileName(
                normalizedOriginalPath);

        string workingCopyPath =
            Path.Combine(
                testDirectoryPath,
                "working_" + originalFileName);

        string workingBackupPath =
            Path.Combine(
                testDirectoryPath,
                "backup_" + originalFileName);

        try
        {
            File.Copy(
                normalizedOriginalPath,
                workingCopyPath,
                overwrite:
                    false);

            File.Copy(
                workingCopyPath,
                workingBackupPath,
                overwrite:
                    false);

            cancellationToken.ThrowIfCancellationRequested();

            string originalHashBefore =
                await ComputeSha256Async(
                    normalizedOriginalPath,
                    cancellationToken);

            string workingCopyHashBefore =
                await ComputeSha256Async(
                    workingCopyPath,
                    cancellationToken);

            string workingBackupHash =
                await ComputeSha256Async(
                    workingBackupPath,
                    cancellationToken);

            FileIsolationContext context =
                new()
                {
                    OriginalFilePath =
                        normalizedOriginalPath,

                    OriginalFileName =
                        originalFileName,

                    WorkingCopyPath =
                        workingCopyPath,

                    WorkingBackupPath =
                        workingBackupPath,

                    TestDirectoryPath =
                        testDirectoryPath,

                    OriginalHashBefore =
                        originalHashBefore,

                    WorkingCopyHashBefore =
                        workingCopyHashBefore,

                    WorkingBackupHash =
                        workingBackupHash
                };

            if (!context.BackupMatchesInitialWorkingCopy)
            {
                throw new InvalidOperationException(
                    "El respaldo creado no coincide con el " +
                    "estado inicial de la copia de trabajo.");
            }

            return context;
        }
        catch
        {
            TryDeleteDirectory(
                testDirectoryPath);

            throw;
        }
    }

    /// <summary>
    /// Calcula nuevamente los hashes después de ejecutar la
    /// operación sobre la copia de trabajo.
    /// </summary>
    public async Task<FileIsolationVerificationResult>
        VerifyAsync(
            FileIsolationContext context,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        List<string> messages =
            new();

        if (!context.IsCreated)
        {
            messages.Add(
                "El contexto de aislamiento no contiene todas " +
                "las rutas requeridas.");

            return new FileIsolationVerificationResult
            {
                Context =
                    context,

                Messages =
                    messages.ToArray()
            };
        }

        if (!File.Exists(
                context.OriginalFilePath))
        {
            messages.Add(
                "El archivo original ya no existe.");

            return new FileIsolationVerificationResult
            {
                Context =
                    context,

                Messages =
                    messages.ToArray()
            };
        }

        if (!File.Exists(
                context.WorkingCopyPath))
        {
            messages.Add(
                "La copia de trabajo ya no existe.");

            return new FileIsolationVerificationResult
            {
                Context =
                    context,

                Messages =
                    messages.ToArray()
            };
        }

        if (!File.Exists(
                context.WorkingBackupPath))
        {
            messages.Add(
                "El respaldo de la copia de trabajo ya no " +
                "existe.");

            return new FileIsolationVerificationResult
            {
                Context =
                    context,

                Messages =
                    messages.ToArray()
            };
        }

        cancellationToken.ThrowIfCancellationRequested();

        string originalHashAfter =
            await ComputeSha256Async(
                context.OriginalFilePath,
                cancellationToken);

        string workingCopyHashAfter =
            await ComputeSha256Async(
                context.WorkingCopyPath,
                cancellationToken);

        bool originalUnchanged =
            string.Equals(
                context.OriginalHashBefore,
                originalHashAfter,
                StringComparison.OrdinalIgnoreCase);

        bool workingCopyModified =
            !string.Equals(
                context.WorkingCopyHashBefore,
                workingCopyHashAfter,
                StringComparison.OrdinalIgnoreCase);

        messages.Add(
            originalUnchanged
                ? "El archivo original permaneció intacto."
                : "El archivo original fue modificado " +
                  "inesperadamente.");

        messages.Add(
            context.BackupMatchesInitialWorkingCopy
                ? "El respaldo coincide con el estado inicial " +
                  "de la copia."
                : "El respaldo no coincide con el estado " +
                  "inicial de la copia.");

        messages.Add(
            workingCopyModified
                ? "La copia de trabajo fue modificada."
                : "La copia de trabajo no presenta cambios.");

        return new FileIsolationVerificationResult
        {
            Context =
                context,

            OriginalHashAfter =
                originalHashAfter,

            WorkingCopyHashAfter =
                workingCopyHashAfter,

            Messages =
                messages.ToArray()
        };
    }

    /// <summary>
    /// Elimina una carpeta temporal creada por el harness.
    ///
    /// Sólo debe utilizarse cuando sus archivos ya no sean
    /// necesarios para diagnóstico.
    /// </summary>
    public bool TryCleanup(
        FileIsolationContext? context)
    {
        if (context is null ||
            string.IsNullOrWhiteSpace(
                context.TestDirectoryPath))
        {
            return false;
        }

        return TryDeleteDirectory(
            context.TestDirectoryPath);
    }

    private static async Task<string> ComputeSha256Async(
        string filePath,
        CancellationToken cancellationToken)
    {
        await using FileStream stream =
            new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize:
                    81920,
                useAsync:
                    true);

        using SHA256 sha256 =
            SHA256.Create();

        byte[] hash =
            await sha256.ComputeHashAsync(
                stream,
                cancellationToken);

        return Convert.ToHexString(
            hash);
    }

    private static string NormalizePath(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetFullPath(
                filePath.Trim());
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string NormalizeFolderName(
        string? value)
    {
        string normalized =
            string.IsNullOrWhiteSpace(value)
                ? DefaultTestFolderName
                : value.Trim();

        foreach (char invalidCharacter
            in Path.GetInvalidFileNameChars())
        {
            normalized =
                normalized.Replace(
                    invalidCharacter,
                    '_');
        }

        return string.IsNullOrWhiteSpace(normalized)
            ? DefaultTestFolderName
            : normalized;
    }

    private static bool TryDeleteDirectory(
        string directoryPath)
    {
        try
        {
            if (!Directory.Exists(
                    directoryPath))
            {
                return true;
            }

            Directory.Delete(
                directoryPath,
                recursive:
                    true);

            return true;
        }
        catch
        {
            return false;
        }
    }
}