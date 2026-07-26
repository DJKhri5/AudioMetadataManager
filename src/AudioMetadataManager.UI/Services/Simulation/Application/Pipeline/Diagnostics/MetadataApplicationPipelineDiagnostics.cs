using System.Text;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Diagnostics;

/// <summary>
/// Construye un informe legible de una ejecución del pipeline
/// de aplicación.
///
/// El diagnóstico no modifica archivos.
/// </summary>
public static class MetadataApplicationPipelineDiagnostics
{
    /// <summary>
    /// Construye el informe completo de una ejecución.
    /// </summary>
    public static string BuildReport(
        MetadataApplicationPipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico del pipeline de aplicación ===");

        builder.AppendLine();

        AppendGeneralInformation(
            builder,
            result);

        AppendRequest(
            builder,
            result);

        AppendStages(
            builder,
            result);

        AppendValidation(
            builder,
            result);

        AppendBackup(
            builder,
            result);

        builder.AppendLine(
            "=== Fin del diagnóstico del pipeline de aplicación ===");

        return builder.ToString();
    }

    private static void AppendGeneralInformation(
        StringBuilder builder,
        MetadataApplicationPipelineResult result)
    {
        builder.AppendLine(
            $"Id de ejecución: {result.ExecutionId}");

        builder.AppendLine(
            $"Archivo: {DisplayValue(result.Request.FileName)}");

        builder.AppendLine(
            $"Ruta: {DisplayValue(result.Request.FilePath)}");

        builder.AppendLine(
            $"Inicio UTC: {result.StartedAtUtc:O}");

        builder.AppendLine(
            $"Finalización UTC: {result.CompletedAtUtc:O}");

        builder.AppendLine(
            $"Duración total: " +
            $"{result.ElapsedTime.TotalMilliseconds:0} ms");

        builder.AppendLine(
            $"Motivo de detención: " +
            $"{GetStopReasonDisplay(result.StopReason)}");

        builder.AppendLine(
            $"Ejecución completa: " +
            $"{ToSpanish(result.WasSuccessful)}");

        builder.AppendLine(
            $"Cancelada: " +
            $"{ToSpanish(result.WasCancelled)}");

        builder.AppendLine(
            $"Última etapa ejecutada: " +
            $"{GetStageDisplay(result.LastExecutedStage)}");

        builder.AppendLine(
            $"Etapas correctas: " +
            $"{result.SuccessfulStageCount}");

        builder.AppendLine(
            $"Etapas fallidas: " +
            $"{result.FailedStageCount}");

        builder.AppendLine(
            $"Resumen: {result.Summary}");

        if (!string.IsNullOrWhiteSpace(
                result.ErrorMessage))
        {
            builder.AppendLine(
                $"Error global: {result.ErrorMessage}");
        }

        builder.AppendLine();
    }

    private static void AppendRequest(
        StringBuilder builder,
        MetadataApplicationPipelineResult result)
    {
        builder.AppendLine(
            "--- Solicitud de aplicación ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Id de solicitud: " +
            $"{result.Request.RequestId}");

        builder.AppendLine(
            $"Id del plan: " +
            $"{result.Request.PlanId}");

        builder.AppendLine(
            $"Cambios válidos: " +
            $"{result.Request.ValidChangeCount}");

        builder.AppendLine(
            $"Respaldo obligatorio: " +
            $"{ToSpanish(result.Request.RequireBackup)}");

        builder.AppendLine(
            $"Verificación posterior obligatoria: " +
            $"{ToSpanish(
                result.Request.RequirePostWriteVerification)}");

        if (result.Request.ValidChanges.Count == 0)
        {
            builder.AppendLine(
                "Cambios aprobados: ninguno.");
        }
        else
        {
            builder.AppendLine(
                "Cambios aprobados:");

            foreach (var change
                in result.Request.ValidChanges)
            {
                builder.AppendLine(
                    $"- {change.Summary} " +
                    $"Confianza: " +
                    $"{change.ConfidenceDisplay} " +
                    $"Aprobación manual: " +
                    $"{ToSpanish(
                        change.WasManuallyApproved)}");
            }
        }

        builder.AppendLine();
    }

    private static void AppendStages(
        StringBuilder builder,
        MetadataApplicationPipelineResult result)
    {
        builder.AppendLine(
            "--- Etapas del pipeline ---");

        builder.AppendLine();

        if (result.StageResults.Count == 0)
        {
            builder.AppendLine(
                "No se registraron etapas.");

            builder.AppendLine();

            return;
        }

        foreach (
            MetadataApplicationStageResult stageResult
            in result.StageResults)
        {
            builder.AppendLine(
                $"[{stageResult.StageDisplay}]");

            builder.AppendLine(
                $"Estado: " +
                $"{GetStageStatusDisplay(
                    stageResult.Status)}");

            builder.AppendLine(
                $"Duración: " +
                $"{stageResult.ElapsedTime
                    .TotalMilliseconds:0} ms");

            builder.AppendLine(
                $"Mensaje: " +
                $"{DisplayValue(stageResult.Message)}");

            if (stageResult.Details.Count > 0)
            {
                builder.AppendLine(
                    "Detalles:");

                foreach (string detail
                    in stageResult.Details)
                {
                    builder.AppendLine(
                        $"- {detail}");
                }
            }

            builder.AppendLine();
        }
    }

    private static void AppendValidation(
        StringBuilder builder,
        MetadataApplicationPipelineResult result)
    {
        builder.AppendLine(
            "--- Resultado de validación ---");

        builder.AppendLine();

        if (result.ValidationResult is null)
        {
            builder.AppendLine(
                "La validación no produjo un resultado.");

            builder.AppendLine();

            return;
        }

        builder.AppendLine(
            $"Solicitud válida: " +
            $"{ToSpanish(
                result.ValidationResult.IsValid)}");

        builder.AppendLine(
            $"Errores: " +
            $"{result.ValidationResult.ErrorCount}");

        builder.AppendLine(
            $"Advertencias: " +
            $"{result.ValidationResult.WarningCount}");

        builder.AppendLine(
            $"Resumen: " +
            $"{result.ValidationResult.Summary}");

        if (result.ValidationResult.Issues.Count > 0)
        {
            builder.AppendLine(
                "Problemas y mensajes:");

            foreach (var issue
                in result.ValidationResult.Issues)
            {
                builder.AppendLine(
                    $"- [{issue.Code}] " +
                    $"{issue.Summary}");
            }
        }

        builder.AppendLine();
    }

    /// <summary>
    /// Agrega el resultado detallado de la copia de seguridad.
    /// </summary>
    private static void AppendBackup(
        StringBuilder builder,
        MetadataApplicationPipelineResult result)
    {
        builder.AppendLine(
            "--- Resultado del respaldo ---");

        builder.AppendLine();

        if (result.BackupResult is null)
        {
            builder.AppendLine(
                "El pipeline no produjo un resultado de respaldo.");

            builder.AppendLine();

            return;
        }

        string backupReport =
            MetadataBackupDiagnostics.BuildReport(
                result.BackupResult);

        builder.AppendLine(
            backupReport);
    }

    private static string GetStopReasonDisplay(
        MetadataApplicationStopReason reason)
    {
        return reason switch
        {
            MetadataApplicationStopReason.None =>
                "Ejecución diagnóstica terminada",

            MetadataApplicationStopReason.Completed =>
                "Proceso completo",

            MetadataApplicationStopReason.ValidationFailed =>
                "Validación rechazada",

            MetadataApplicationStopReason.BackupFailed =>
                "Error al crear el respaldo",

            MetadataApplicationStopReason.MetadataWriteFailed =>
                "Error durante la escritura",

            MetadataApplicationStopReason.VerificationFailed =>
                "Error durante la verificación",

            MetadataApplicationStopReason.Cancelled =>
                "Operación cancelada",

            MetadataApplicationStopReason.UnexpectedError =>
                "Error inesperado",

            _ =>
                reason.ToString()
        };
    }

    private static string GetStageDisplay(
        MetadataApplicationStage stage)
    {
        return stage switch
        {
            MetadataApplicationStage.Validation =>
                "Validación previa",

            MetadataApplicationStage.Backup =>
                "Copia de seguridad",

            MetadataApplicationStage.MetadataWrite =>
                "Escritura de metadatos",

            MetadataApplicationStage.PostWriteVerification =>
                "Verificación posterior",

            MetadataApplicationStage.Finalization =>
                "Finalización",

            _ =>
                "Ninguna"
        };
    }

    private static string GetStageStatusDisplay(
        MetadataApplicationStageStatus status)
    {
        return status switch
        {
            MetadataApplicationStageStatus.Pending =>
                "Pendiente",

            MetadataApplicationStageStatus.Running =>
                "En ejecución",

            MetadataApplicationStageStatus.Completed =>
                "Completada",

            MetadataApplicationStageStatus.CompletedWithWarnings =>
                "Completada con advertencias",

            MetadataApplicationStageStatus.Failed =>
                "Fallida",

            MetadataApplicationStageStatus.Skipped =>
                "Omitida",

            MetadataApplicationStageStatus.Cancelled =>
                "Cancelada",

            _ =>
                status.ToString()
        };
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(sin información)"
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