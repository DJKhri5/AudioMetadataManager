using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.PathResolution;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Engine;

/// <summary>
/// Crea copias de seguridad verificables antes de modificar
/// archivos musicales.
///
/// La copia se escribe primero en un archivo temporal y sólo se
/// mueve al destino definitivo después de finalizar
/// correctamente.
/// </summary>
public sealed class MetadataBackupEngine
{
    private readonly MetadataBackupOptions
        _options;

    private readonly MetadataBackupPathResolver
        _pathResolver;

    /// <summary>
    /// Crea el motor con la configuración predeterminada.
    /// </summary>
    public MetadataBackupEngine()
        : this(
            new MetadataBackupOptions())
    {
    }

    /// <summary>
    /// Crea el motor con opciones personalizadas.
    /// </summary>
    public MetadataBackupEngine(
        MetadataBackupOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _pathResolver =
            new MetadataBackupPathResolver(
                _options);
    }

    /// <summary>
    /// Crea y verifica una copia de seguridad.
    /// </summary>
    public async Task<MetadataBackupResult> CreateBackupAsync(
        MetadataBackupRequest request,
        IProgress<MetadataBackupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        List<string> messages =
            new();

        string sourcePath =
            string.Empty;

        string backupDirectory =
            string.Empty;

        string backupFilePath =
            string.Empty;

        string temporaryFilePath =
            string.Empty;

        long sourceSize =
            -1;

        long backupSize =
            -1;

        string sourceHash =
            string.Empty;

        string backupHash =
            string.Empty;

        bool fileSizeVerified =
            false;

        bool hashVerified =
            false;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(
                progress,
                request,
                5,
                "Validando la solicitud de respaldo.");

            if (!request.IsStructurallyValid)
            {
                messages.Add(
                    "La solicitud de respaldo no contiene " +
                    "información estructural válida.");

                return BuildResult(
                    request,
                    MetadataBackupStatus.ValidationFailed,
                    sourcePath,
                    backupDirectory,
                    backupFilePath,
                    startedAtUtc,
                    stopwatch,
                    sourceSize,
                    backupSize,
                    sourceHash,
                    backupHash,
                    fileSizeVerified,
                    hashVerified,
                    messages);
            }

            if (!_options.IsValid)
            {
                messages.Add(
                    "La configuración del motor de respaldo " +
                    "no es válida.");

                return BuildResult(
                    request,
                    MetadataBackupStatus.ValidationFailed,
                    sourcePath,
                    backupDirectory,
                    backupFilePath,
                    startedAtUtc,
                    stopwatch,
                    sourceSize,
                    backupSize,
                    sourceHash,
                    backupHash,
                    fileSizeVerified,
                    hashVerified,
                    messages);
            }

            sourcePath =
                request.NormalizedSourceFilePath;

            if (!File.Exists(
                    sourcePath))
            {
                messages.Add(
                    "El archivo original no existe.");

                return BuildResult(
                    request,
                    MetadataBackupStatus.ValidationFailed,
                    sourcePath,
                    backupDirectory,
                    backupFilePath,
                    startedAtUtc,
                    stopwatch,
                    sourceSize,
                    backupSize,
                    sourceHash,
                    backupHash,
                    fileSizeVerified,
                    hashVerified,
                    messages);
            }

            FileInfo sourceInfo =
                new(
                    sourcePath);

            sourceSize =
                sourceInfo.Length;

            MetadataBackupPathResolutionResult pathResult =
                _pathResolver.Resolve(
                    request);

            if (!pathResult.IsValid)
            {
                messages.Add(
                    "No fue posible resolver la ruta final " +
                    "del respaldo.");

                return BuildResult(
                    request,
                    MetadataBackupStatus
                        .DestinationPreparationFailed,
                    sourcePath,
                    backupDirectory,
                    backupFilePath,
                    startedAtUtc,
                    stopwatch,
                    sourceSize,
                    backupSize,
                    sourceHash,
                    backupHash,
                    fileSizeVerified,
                    hashVerified,
                    messages);
            }

            backupDirectory =
                pathResult.BackupDirectoryPath;

            backupFilePath =
                pathResult.BackupFilePath;

            ReportProgress(
                progress,
                request,
                10,
                "Preparando la carpeta de respaldo.");

            Directory.CreateDirectory(
                backupDirectory);

            if (!Directory.Exists(
                    backupDirectory))
            {
                messages.Add(
                    "La carpeta de respaldo no pudo crearse.");

                return BuildResult(
                    request,
                    MetadataBackupStatus
                        .DestinationPreparationFailed,
                    sourcePath,
                    backupDirectory,
                    backupFilePath,
                    startedAtUtc,
                    stopwatch,
                    sourceSize,
                    backupSize,
                    sourceHash,
                    backupHash,
                    fileSizeVerified,
                    hashVerified,
                    messages);
            }

            temporaryFilePath =
                backupFilePath +
                $".{Guid.NewGuid():N}.tmp";

            ReportProgress(
                progress,
                request,
                15,
                "Copiando el archivo original.");

            await CopyFileAsync(
                sourcePath,
                temporaryFilePath,
                sourceSize,
                request.EffectiveFileName,
                progress,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(
                    temporaryFilePath))
            {
                messages.Add(
                    "La copia temporal no fue creada.");

                return BuildResult(
                    request,
                    MetadataBackupStatus.CopyFailed,
                    sourcePath,
                    backupDirectory,
                    backupFilePath,
                    startedAtUtc,
                    stopwatch,
                    sourceSize,
                    backupSize,
                    sourceHash,
                    backupHash,
                    fileSizeVerified,
                    hashVerified,
                    messages);
            }

            if (File.Exists(
                    backupFilePath))
            {
                if (!_options.AllowOverwrite)
                {
                    messages.Add(
                        "El destino definitivo ya existe y no " +
                        "se permite sobrescribirlo.");

                    return BuildResult(
                        request,
                        MetadataBackupStatus.CopyFailed,
                        sourcePath,
                        backupDirectory,
                        backupFilePath,
                        startedAtUtc,
                        stopwatch,
                        sourceSize,
                        backupSize,
                        sourceHash,
                        backupHash,
                        fileSizeVerified,
                        hashVerified,
                        messages);
                }

                File.Delete(
                    backupFilePath);
            }

            File.Move(
                temporaryFilePath,
                backupFilePath);

            temporaryFilePath =
                string.Empty;

            ReportProgress(
                progress,
                request,
                70,
                "Verificando el tamaño de la copia.");

            backupSize =
                new FileInfo(
                    backupFilePath)
                .Length;

            fileSizeVerified =
                !_options.VerifyFileSize ||
                sourceSize == backupSize;

            if (!fileSizeVerified)
            {
                messages.Add(
                    "El tamaño del respaldo no coincide con " +
                    "el archivo original.");

                return BuildResult(
                    request,
                    MetadataBackupStatus.VerificationFailed,
                    sourcePath,
                    backupDirectory,
                    backupFilePath,
                    startedAtUtc,
                    stopwatch,
                    sourceSize,
                    backupSize,
                    sourceHash,
                    backupHash,
                    fileSizeVerified,
                    hashVerified,
                    messages);
            }

            if (_options.VerifyHash)
            {
                ReportProgress(
                    progress,
                    request,
                    80,
                    "Calculando SHA-256 del archivo original.");

                sourceHash =
                    await ComputeHashAsync(
                        sourcePath,
                        cancellationToken);

                ReportProgress(
                    progress,
                    request,
                    90,
                    "Calculando SHA-256 del respaldo.");

                backupHash =
                    await ComputeHashAsync(
                        backupFilePath,
                        cancellationToken);

                hashVerified =
                    string.Equals(
                        sourceHash,
                        backupHash,
                        StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                hashVerified =
                    true;
            }

            if (!hashVerified)
            {
                messages.Add(
                    "La huella criptográfica del respaldo no " +
                    "coincide con la del archivo original.");

                return BuildResult(
                    request,
                    MetadataBackupStatus.VerificationFailed,
                    sourcePath,
                    backupDirectory,
                    backupFilePath,
                    startedAtUtc,
                    stopwatch,
                    sourceSize,
                    backupSize,
                    sourceHash,
                    backupHash,
                    fileSizeVerified,
                    hashVerified,
                    messages);
            }

            messages.Add(
                "El respaldo fue creado y verificado " +
                "correctamente.");

            if (pathResult.UsedUniqueFileName)
            {
                messages.Add(
                    "Se generó un nombre alternativo para " +
                    "evitar sobrescribir un respaldo anterior.");
            }

            ReportProgress(
                progress,
                request,
                100,
                "Respaldo creado y verificado.");

            return BuildResult(
                request,
                MetadataBackupStatus.Completed,
                sourcePath,
                backupDirectory,
                backupFilePath,
                startedAtUtc,
                stopwatch,
                sourceSize,
                backupSize,
                sourceHash,
                backupHash,
                fileSizeVerified,
                hashVerified,
                messages);
        }
        catch (OperationCanceledException)
        {
            messages.Add(
                "La operación de respaldo fue cancelada.");

            return BuildResult(
                request,
                MetadataBackupStatus.Cancelled,
                sourcePath,
                backupDirectory,
                backupFilePath,
                startedAtUtc,
                stopwatch,
                sourceSize,
                backupSize,
                sourceHash,
                backupHash,
                fileSizeVerified,
                hashVerified,
                messages);
        }
        catch (UnauthorizedAccessException exception)
        {
            messages.Add(
                "Windows rechazó el acceso necesario para " +
                $"crear el respaldo: {exception.Message}");

            return BuildResult(
                request,
                MetadataBackupStatus.CopyFailed,
                sourcePath,
                backupDirectory,
                backupFilePath,
                startedAtUtc,
                stopwatch,
                sourceSize,
                backupSize,
                sourceHash,
                backupHash,
                fileSizeVerified,
                hashVerified,
                messages);
        }
        catch (IOException exception)
        {
            messages.Add(
                "Ocurrió un error de entrada o salida durante " +
                $"el respaldo: {exception.Message}");

            return BuildResult(
                request,
                MetadataBackupStatus.CopyFailed,
                sourcePath,
                backupDirectory,
                backupFilePath,
                startedAtUtc,
                stopwatch,
                sourceSize,
                backupSize,
                sourceHash,
                backupHash,
                fileSizeVerified,
                hashVerified,
                messages);
        }
        catch (Exception exception)
        {
            messages.Add(
                "Ocurrió un error inesperado durante el " +
                $"respaldo: {exception.Message}");

            return BuildResult(
                request,
                MetadataBackupStatus.UnexpectedError,
                sourcePath,
                backupDirectory,
                backupFilePath,
                startedAtUtc,
                stopwatch,
                sourceSize,
                backupSize,
                sourceHash,
                backupHash,
                fileSizeVerified,
                hashVerified,
                messages);
        }
        finally
        {
            DeleteTemporaryFileSafely(
                temporaryFilePath);
        }
    }

    private async Task CopyFileAsync(
        string sourcePath,
        string destinationPath,
        long totalBytes,
        string fileName,
        IProgress<MetadataBackupProgress>? progress,
        CancellationToken cancellationToken)
    {
        FileOptions writeOptions =
            _options.FlushToDisk
                ? FileOptions.WriteThrough |
                  FileOptions.Asynchronous
                : FileOptions.Asynchronous;

        await using FileStream sourceStream =
            new(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                _options.CopyBufferSize,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

        await using FileStream destinationStream =
            new(
                destinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                _options.CopyBufferSize,
                writeOptions);

        byte[] buffer =
            new byte[_options.CopyBufferSize];

        long processedBytes =
            0;

        while (true)
        {
            int bytesRead =
                await sourceStream.ReadAsync(
                    buffer.AsMemory(
                        0,
                        buffer.Length),
                    cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            await destinationStream.WriteAsync(
                buffer.AsMemory(
                    0,
                    bytesRead),
                cancellationToken);

            processedBytes +=
                bytesRead;

            double copyProgress =
                totalBytes > 0
                    ? processedBytes /
                      (double)totalBytes
                    : 1;

            double percentage =
                15 +
                Math.Clamp(
                    copyProgress,
                    0,
                    1) *
                50;

            progress?.Report(
                new MetadataBackupProgress
                {
                    Percentage =
                        percentage,

                    Message =
                        "Copiando el archivo original.",

                    FileName =
                        fileName,

                    ProcessedBytes =
                        processedBytes,

                    TotalBytes =
                        totalBytes
                });
        }

        await destinationStream.FlushAsync(
            cancellationToken);

        if (_options.FlushToDisk)
        {
            destinationStream.Flush(
                flushToDisk: true);
        }
    }

    private static async Task<string> ComputeHashAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        await using FileStream stream =
            new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                1024 * 1024,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

        using SHA256 sha256 =
            SHA256.Create();

        byte[] hash =
            await sha256.ComputeHashAsync(
                stream,
                cancellationToken);

        return Convert.ToHexString(
            hash);
    }

    private static MetadataBackupResult BuildResult(
        MetadataBackupRequest request,
        MetadataBackupStatus status,
        string sourceFilePath,
        string backupDirectoryPath,
        string backupFilePath,
        DateTimeOffset startedAtUtc,
        Stopwatch stopwatch,
        long sourceFileSizeBytes,
        long backupFileSizeBytes,
        string sourceHash,
        string backupHash,
        bool fileSizeVerified,
        bool hashVerified,
        IReadOnlyList<string> messages)
    {
        stopwatch.Stop();

        return new MetadataBackupResult
        {
            BackupRequestId =
                request.BackupRequestId,

            ApplyRequestId =
                request.ApplyRequestId,

            PlanId =
                request.PlanId,

            Status =
                status,

            SourceFilePath =
                sourceFilePath,

            BackupFilePath =
                backupFilePath,

            BackupDirectoryPath =
                backupDirectoryPath,

            StartedAtUtc =
                startedAtUtc,

            CompletedAtUtc =
                DateTimeOffset.UtcNow,

            ElapsedTime =
                stopwatch.Elapsed,

            SourceFileSizeBytes =
                sourceFileSizeBytes,

            BackupFileSizeBytes =
                backupFileSizeBytes,

            SourceHash =
                sourceHash,

            BackupHash =
                backupHash,

            HashAlgorithmName =
                "SHA256",

            FileSizeVerified =
                fileSizeVerified,

            HashVerified =
                hashVerified,

            Messages =
                messages.ToArray()
        };
    }

    private static void ReportProgress(
        IProgress<MetadataBackupProgress>? progress,
        MetadataBackupRequest request,
        double percentage,
        string message)
    {
        progress?.Report(
            new MetadataBackupProgress
            {
                Percentage =
                    percentage,

                Message =
                    message,

                FileName =
                    request.EffectiveFileName
            });
    }

    private static void DeleteTemporaryFileSafely(
        string? temporaryFilePath)
    {
        if (string.IsNullOrWhiteSpace(
                temporaryFilePath))
        {
            return;
        }

        try
        {
            if (File.Exists(
                    temporaryFilePath))
            {
                File.Delete(
                    temporaryFilePath);
            }
        }
        catch
        {
            /*
             * La limpieza no debe ocultar el resultado original
             * de la operación.
             */
        }
    }
}