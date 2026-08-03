using System.IO;
using System.Security.Cryptography;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

/// <summary>
/// Calcula y compara hashes SHA-256 de archivos para comprobar
/// su integridad antes y después de operaciones controladas.
/// </summary>
public sealed class FileSha256Service
{
    /// <summary>
    /// Calcula el hash SHA-256 del archivo indicado.
    /// </summary>
    public async Task<string> ComputeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        string normalizedFilePath =
            NormalizeExistingFilePath(
                filePath);

        cancellationToken.ThrowIfCancellationRequested();

        await using FileStream stream =
            new(
                normalizedFilePath,
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

    /// <summary>
    /// Comprueba si dos archivos contienen exactamente los
    /// mismos bytes.
    /// </summary>
    public async Task<bool> FilesMatchAsync(
        string firstFilePath,
        string secondFilePath,
        CancellationToken cancellationToken = default)
    {
        string firstHash =
            await ComputeAsync(
                firstFilePath,
                cancellationToken);

        string secondHash =
            await ComputeAsync(
                secondFilePath,
                cancellationToken);

        return string.Equals(
            firstHash,
            secondHash,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Comprueba si el archivo coincide con un hash SHA-256
    /// previamente calculado.
    /// </summary>
    public async Task<bool> FileMatchesHashAsync(
        string filePath,
        string expectedHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(
                expectedHash))
        {
            throw new ArgumentException(
                "No se recibió un hash esperado válido.",
                nameof(expectedHash));
        }

        string actualHash =
            await ComputeAsync(
                filePath,
                cancellationToken);

        return string.Equals(
            actualHash,
            expectedHash.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeExistingFilePath(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            throw new ArgumentException(
                "No se recibió una ruta de archivo válida.",
                nameof(filePath));
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
                nameof(filePath),
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
}