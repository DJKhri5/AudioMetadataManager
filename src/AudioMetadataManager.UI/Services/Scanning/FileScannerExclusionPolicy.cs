using System.IO;

namespace AudioMetadataManager.UI.Services.Scanning;

/// <summary>
/// Define las reglas y nombres de carpetas excluidas durante el escaneo
/// de bibliotecas para evitar ingerir respaldos o archivos de sistema.
/// </summary>
public sealed class FileScannerExclusionPolicy
{
    private static readonly HashSet<string> DefaultExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "AudioMetadataManager_Backup",
        "AMM_Backups",
        "AMM_Staging",
        "Backups",
        "Backup",
        "_backups",
        "_backup",
        ".backup",
        ".backups",
        ".git",
        ".vs",
        ".idea",
        "$RECYCLE.BIN",
        "System Volume Information"
    };

    private readonly HashSet<string> _excludedDirectoryNames;

    public FileScannerExclusionPolicy()
        : this(DefaultExcludedDirectoryNames)
    {
    }

    public FileScannerExclusionPolicy(IEnumerable<string> excludedDirectoryNames)
    {
        ArgumentNullException.ThrowIfNull(excludedDirectoryNames);
        _excludedDirectoryNames = new HashSet<string>(excludedDirectoryNames, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determina si un directorio debe ser ignorado durante el escaneo recursivo.
    /// </summary>
    public bool ShouldExcludeDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return true;
        }

        string directoryName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        if (string.IsNullOrWhiteSpace(directoryName))
        {
            return false;
        }

        if (_excludedDirectoryNames.Contains(directoryName))
        {
            return true;
        }

        // Excluir cualquier subcarpeta que comience con prefijo de respaldo de AMM
        if (directoryName.StartsWith("AudioMetadataManager_Backup", StringComparison.OrdinalIgnoreCase) ||
            directoryName.StartsWith("AMM_Backup", StringComparison.OrdinalIgnoreCase) ||
            directoryName.StartsWith(".backup", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
