using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.PathResolution;

/// <summary>
/// Construye rutas seguras y deterministas para los respaldos,
/// evitando sobrescrituras accidentales.
/// </summary>
public sealed class MetadataBackupPathResolver
{
    private readonly MetadataBackupOptions
        _options;

    /// <summary>
    /// Crea el resolutor con la configuración predeterminada.
    /// </summary>
    public MetadataBackupPathResolver()
        : this(
            new MetadataBackupOptions())
    {
    }

    /// <summary>
    /// Crea el resolutor con opciones personalizadas.
    /// </summary>
    public MetadataBackupPathResolver(
        MetadataBackupOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));
    }

    /// <summary>
    /// Resuelve la ruta final donde debe crearse el respaldo.
    /// </summary>
    public MetadataBackupPathResolutionResult Resolve(
        MetadataBackupRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        if (!request.IsStructurallyValid ||
            !_options.IsValid)
        {
            return new MetadataBackupPathResolutionResult();
        }

        string sourcePath =
            request.NormalizedSourceFilePath;

        if (string.IsNullOrWhiteSpace(
                sourcePath))
        {
            return new MetadataBackupPathResolutionResult();
        }

        string rootDirectory =
            ResolveRootDirectory(
                request,
                sourcePath);

        string backupDirectory =
            BuildBackupDirectory(
                request,
                rootDirectory);

        string sourceFileName =
            ResolveFileName(
                request,
                sourcePath);

        string requestedFilePath =
            Path.Combine(
                backupDirectory,
                sourceFileName);

        string finalFilePath =
            ResolveCollision(
                requestedFilePath);

        bool usedUniqueFileName =
            !string.Equals(
                requestedFilePath,
                finalFilePath,
                StringComparison.OrdinalIgnoreCase);

        return new MetadataBackupPathResolutionResult
        {
            RootBackupDirectory =
                rootDirectory,

            BackupDirectoryPath =
                backupDirectory,

            BackupFilePath =
                finalFilePath,

            UsedUniqueFileName =
                usedUniqueFileName
        };
    }

    private string ResolveRootDirectory(
        MetadataBackupRequest request,
        string sourcePath)
    {
        if (!string.IsNullOrWhiteSpace(
                request.RequestedBackupRootPath))
        {
            return Path.GetFullPath(
                request.RequestedBackupRootPath.Trim());
        }

        if (!string.IsNullOrWhiteSpace(
                _options.NormalizedRootBackupDirectory))
        {
            return
                _options.NormalizedRootBackupDirectory;
        }

        string? sourceDirectory =
            Path.GetDirectoryName(
                sourcePath);

        if (string.IsNullOrWhiteSpace(
                sourceDirectory))
        {
            sourceDirectory =
                Environment.CurrentDirectory;
        }

        return Path.Combine(
            sourceDirectory,
            _options.NormalizedBackupFolderName);
    }

    private string BuildBackupDirectory(
        MetadataBackupRequest request,
        string rootDirectory)
    {
        string result =
            rootDirectory;

        if (_options.OrganizeByDate)
        {
            string dateFolder =
                DateTimeOffset.Now.ToString(
                    _options.DateFolderFormat);

            result =
                Path.Combine(
                    result,
                    SanitizePathSegment(
                        dateFolder));
        }

        if (_options.OrganizeByPlanId)
        {
            result =
                Path.Combine(
                    result,
                    request.PlanId.ToString("N"));
        }

        return result;
    }

    private static string ResolveFileName(
        MetadataBackupRequest request,
        string sourcePath)
    {
        string fileName =
            request.EffectiveFileName;

        if (string.IsNullOrWhiteSpace(
                fileName))
        {
            fileName =
                Path.GetFileName(
                    sourcePath);
        }

        return SanitizeFileName(
            fileName);
    }

    private string ResolveCollision(
        string requestedFilePath)
    {
        if (!File.Exists(
                requestedFilePath))
        {
            return requestedFilePath;
        }

        if (_options.AllowOverwrite)
        {
            return requestedFilePath;
        }

        if (!_options.GenerateUniqueNameOnCollision)
        {
            return string.Empty;
        }

        string? directory =
            Path.GetDirectoryName(
                requestedFilePath);

        string fileNameWithoutExtension =
            Path.GetFileNameWithoutExtension(
                requestedFilePath);

        string extension =
            Path.GetExtension(
                requestedFilePath);

        int counter =
            1;

        while (true)
        {
            string candidateName =
                $"{fileNameWithoutExtension} " +
                $"({counter}){extension}";

            string candidatePath =
                Path.Combine(
                    directory ??
                    string.Empty,
                    candidateName);

            if (!File.Exists(
                    candidatePath))
            {
                return candidatePath;
            }

            counter++;
        }
    }

    private static string SanitizeFileName(
        string fileName)
    {
        string result =
            fileName.Trim();

        foreach (char invalidCharacter
            in Path.GetInvalidFileNameChars())
        {
            result =
                result.Replace(
                    invalidCharacter,
                    '_');
        }

        return string.IsNullOrWhiteSpace(
                result)
            ? "archivo_sin_nombre"
            : result;
    }

    private static string SanitizePathSegment(
        string value)
    {
        string result =
            value.Trim();

        foreach (char invalidCharacter
            in Path.GetInvalidFileNameChars())
        {
            result =
                result.Replace(
                    invalidCharacter,
                    '_');
        }

        return string.IsNullOrWhiteSpace(
                result)
            ? "sin_identificar"
            : result;
    }
}