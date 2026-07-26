using System.Text;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Diagnostics;

/// <summary>
/// Construye un informe legible y auditable a partir del
/// resultado de una operación de respaldo.
/// </summary>
public static class MetadataBackupDiagnostics
{
    /// <summary>
    /// Genera el informe completo del respaldo.
    /// </summary>
    public static string BuildReport(
        MetadataBackupResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de la copia de seguridad ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Id de solicitud de respaldo: " +
            $"{result.BackupRequestId}");

        builder.AppendLine(
            $"Id de solicitud de aplicación: " +
            $"{result.ApplyRequestId}");

        builder.AppendLine(
            $"Id del plan: {result.PlanId}");

        builder.AppendLine(
            $"Estado: " +
            $"{GetStatusDisplay(result.Status)}");

        builder.AppendLine(
            $"Operación correcta: " +
            $"{ToSpanish(result.WasSuccessful)}");

        builder.AppendLine(
            $"Archivo de respaldo disponible: " +
            $"{ToSpanish(result.HasBackupFile)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Ubicaciones ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Archivo original: " +
            $"{DisplayPath(result.SourceFilePath)}");

        builder.AppendLine(
            $"Carpeta de respaldo: " +
            $"{DisplayPath(result.BackupDirectoryPath)}");

        builder.AppendLine(
            $"Archivo respaldado: " +
            $"{DisplayPath(result.BackupFilePath)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Verificación de integridad ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Tamaño original: " +
            $"{result.SourceFileSizeDisplay}");

        builder.AppendLine(
            $"Tamaño del respaldo: " +
            $"{result.BackupFileSizeDisplay}");

        builder.AppendLine(
            $"Los tamaños coinciden: " +
            $"{ToSpanish(result.SizesMatch)}");

        builder.AppendLine(
            $"Tamaño verificado: " +
            $"{ToSpanish(result.FileSizeVerified)}");

        builder.AppendLine();

        builder.AppendLine(
            $"Algoritmo de hash: " +
            $"{DisplayValue(result.HashAlgorithmName)}");

        builder.AppendLine(
            $"SHA-256 original: " +
            $"{DisplayHash(result.SourceHash)}");

        builder.AppendLine(
            $"SHA-256 respaldo: " +
            $"{DisplayHash(result.BackupHash)}");

        builder.AppendLine(
            $"Las huellas coinciden: " +
            $"{ToSpanish(result.HashesMatch)}");

        builder.AppendLine(
            $"Hash verificado: " +
            $"{ToSpanish(result.HashVerified)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Tiempos ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Inicio UTC: {result.StartedAtUtc:O}");

        builder.AppendLine(
            $"Finalización UTC: " +
            $"{result.CompletedAtUtc:O}");

        builder.AppendLine(
            $"Duración total: " +
            $"{result.ElapsedTime.TotalMilliseconds:0} ms");

        builder.AppendLine();

        builder.AppendLine(
            "--- Mensajes ---");

        builder.AppendLine();

        if (result.Messages.Count == 0)
        {
            builder.AppendLine(
                "No se registraron mensajes.");
        }
        else
        {
            foreach (string message
                in result.Messages)
            {
                builder.AppendLine(
                    $"- {message}");
            }
        }

        builder.AppendLine();

        builder.AppendLine(
            $"Resumen: {result.Summary}");

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin del diagnóstico de la copia de seguridad ===");

        return builder.ToString();
    }

    private static string GetStatusDisplay(
        MetadataBackupStatus status)
    {
        return status switch
        {
            MetadataBackupStatus.Pending =>
                "Pendiente",

            MetadataBackupStatus.Validated =>
                "Solicitud validada",

            MetadataBackupStatus.DestinationPrepared =>
                "Destino preparado",

            MetadataBackupStatus.Copied =>
                "Archivo copiado",

            MetadataBackupStatus.Verified =>
                "Copia verificada",

            MetadataBackupStatus.Completed =>
                "Completado",

            MetadataBackupStatus.ValidationFailed =>
                "Validación rechazada",

            MetadataBackupStatus
                .DestinationPreparationFailed =>
                    "No se pudo preparar el destino",

            MetadataBackupStatus.CopyFailed =>
                "Error durante la copia",

            MetadataBackupStatus.VerificationFailed =>
                "Verificación fallida",

            MetadataBackupStatus.Cancelled =>
                "Operación cancelada",

            MetadataBackupStatus.UnexpectedError =>
                "Error inesperado",

            _ =>
                status.ToString()
        };
    }

    private static string DisplayPath(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
                ? "(ruta no disponible)"
                : value.Trim();
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
                ? "(sin información)"
                : value.Trim();
    }

    private static string DisplayHash(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
                ? "(no calculado)"
                : value.Trim();
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}