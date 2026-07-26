using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;

/// <summary>
/// Contiene el resultado completo y verificable de una
/// operación de respaldo.
/// </summary>
public sealed class MetadataBackupResult
{
    /// <summary>
    /// Identificador de la solicitud procesada.
    /// </summary>
    public Guid BackupRequestId { get; init; }

    /// <summary>
    /// Identificador de la solicitud de aplicación.
    /// </summary>
    public Guid ApplyRequestId { get; init; }

    /// <summary>
    /// Identificador del plan de simulación.
    /// </summary>
    public Guid PlanId { get; init; }

    /// <summary>
    /// Estado final de la operación.
    /// </summary>
    public MetadataBackupStatus Status { get; init; } =
        MetadataBackupStatus.Pending;

    /// <summary>
    /// Ruta del archivo original.
    /// </summary>
    public string SourceFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta final del respaldo.
    /// </summary>
    public string BackupFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Carpeta utilizada para el respaldo.
    /// </summary>
    public string BackupDirectoryPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Momento UTC de inicio.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC de finalización.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>
    /// Duración total.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Tamaño del archivo original.
    /// </summary>
    public long SourceFileSizeBytes { get; init; }

    /// <summary>
    /// Tamaño del archivo copiado.
    /// </summary>
    public long BackupFileSizeBytes { get; init; }

    /// <summary>
    /// Hash calculado para el archivo original.
    /// </summary>
    public string SourceHash { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash calculado para el respaldo.
    /// </summary>
    public string BackupHash { get; init; } =
        string.Empty;

    /// <summary>
    /// Algoritmo utilizado para calcular las huellas.
    /// </summary>
    public string HashAlgorithmName { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la comprobación de tamaño fue satisfactoria.
    /// </summary>
    public bool FileSizeVerified { get; init; }

    /// <summary>
    /// Indica si la comprobación de hash fue satisfactoria.
    /// </summary>
    public bool HashVerified { get; init; }

    /// <summary>
    /// Mensajes, advertencias y errores registrados.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si existe una ruta de respaldo utilizable.
    /// </summary>
    public bool HasBackupFile =>
        !string.IsNullOrWhiteSpace(
            BackupFilePath) &&
        File.Exists(
            BackupFilePath);

    /// <summary>
    /// Indica si ambos tamaños coinciden.
    /// </summary>
    public bool SizesMatch =>
        SourceFileSizeBytes >= 0 &&
        SourceFileSizeBytes ==
        BackupFileSizeBytes;

    /// <summary>
    /// Indica si ambos hashes existen y coinciden.
    /// </summary>
    public bool HashesMatch =>
        !string.IsNullOrWhiteSpace(
            SourceHash) &&
        !string.IsNullOrWhiteSpace(
            BackupHash) &&
        string.Equals(
            SourceHash,
            BackupHash,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si el respaldo terminó completamente verificado.
    /// </summary>
    public bool WasSuccessful =>
        Status ==
            MetadataBackupStatus.Completed &&
        HasBackupFile &&
        FileSizeVerified &&
        HashVerified;

    /// <summary>
    /// Tamaño preparado para diagnóstico.
    /// </summary>
    public string SourceFileSizeDisplay =>
        FormatFileSize(
            SourceFileSizeBytes);

    /// <summary>
    /// Tamaño del respaldo preparado para diagnóstico.
    /// </summary>
    public string BackupFileSizeDisplay =>
        FormatFileSize(
            BackupFileSizeBytes);

    /// <summary>
    /// Resumen legible.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    $"Respaldo creado y verificado: " +
                    $"{BackupFilePath}.";
            }

            return
                $"El respaldo terminó con estado {Status}. " +
                $"Tamaño verificado: " +
                $"{ToSpanish(FileSizeVerified)}. " +
                $"Hash verificado: " +
                $"{ToSpanish(HashVerified)}.";
        }
    }

    private static string FormatFileSize(
        long bytes)
    {
        if (bytes < 0)
        {
            return "(sin información)";
        }

        string[] units =
        {
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        };

        double size =
            bytes;

        int unitIndex =
            0;

        while (size >= 1024 &&
               unitIndex <
               units.Length - 1)
        {
            size /=
                1024;

            unitIndex++;
        }

        return
            $"{size:0.00} {units[unitIndex]}";
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}