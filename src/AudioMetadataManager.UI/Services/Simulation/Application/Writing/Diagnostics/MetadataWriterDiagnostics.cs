using System.Text;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Diagnostics;

/// <summary>
/// Construye un informe legible de una ejecución del motor de
/// escritura de metadatos.
/// </summary>
public static class MetadataWriterDiagnostics
{
    public static string BuildReport(
        MetadataWriteResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico del motor de escritura ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Id de solicitud: {result.WriteRequestId}");

        builder.AppendLine(
            $"Id de aplicación: {result.ApplyRequestId}");

        builder.AppendLine(
            $"Id del plan: {result.PlanId}");

        builder.AppendLine(
            $"Archivo: {DisplayValue(result.FilePath)}");

        builder.AppendLine(
            $"Escritor: {DisplayValue(result.WriterName)}");

        builder.AppendLine(
            $"Estado: {GetStatusDisplay(result.Status)}");

        builder.AppendLine(
            $"Operación correcta: " +
            $"{ToSpanish(result.WasSuccessful)}");

        builder.AppendLine(
            $"Campos escritos: {result.WrittenFieldCount}");

        builder.AppendLine(
            $"Campos fallidos: {result.FailedFieldCount}");

        builder.AppendLine(
            $"Duración: " +
            $"{result.ElapsedTime.TotalMilliseconds:0} ms");

        builder.AppendLine();

        builder.AppendLine(
            "--- Resultados por campo ---");

        builder.AppendLine();

        if (result.FieldResults.Count == 0)
        {
            builder.AppendLine(
                "No se registraron resultados por campo.");
        }
        else
        {
            foreach (MetadataFieldWriteResult fieldResult
                in result.FieldResults)
            {
                builder.AppendLine(
                    $"[{fieldResult.Field}]");

                builder.AppendLine(
                    $"Valor original: " +
                    $"{DisplayValue(fieldResult.OriginalValue)}");

                builder.AppendLine(
                    $"Valor solicitado: " +
                    $"{DisplayValue(fieldResult.RequestedValue)}");

                builder.AppendLine(
                    $"Campo soportado: " +
                    $"{ToSpanish(fieldResult.IsSupported)}");

                builder.AppendLine(
                    $"Valor preparado: " +
                    $"{ToSpanish(fieldResult.ValuePrepared)}");

                builder.AppendLine(
                    $"Guardado correcto: " +
                    $"{ToSpanish(fieldResult.SaveSucceeded)}");

                builder.AppendLine(
                    $"Mensaje: " +
                    $"{DisplayValue(fieldResult.Message)}");

                builder.AppendLine();
            }
        }

        builder.AppendLine(
            "--- Mensajes globales ---");

        builder.AppendLine();

        if (result.Messages.Count == 0)
        {
            builder.AppendLine(
                "No se registraron mensajes.");
        }
        else
        {
            foreach (string message in result.Messages)
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
            "=== Fin del diagnóstico del motor de escritura ===");

        return builder.ToString();
    }

    private static string GetStatusDisplay(
        MetadataWriteStatus status)
    {
        return status switch
        {
            MetadataWriteStatus.Pending =>
                "Pendiente",

            MetadataWriteStatus.Validated =>
                "Validada",

            MetadataWriteStatus.WriterResolved =>
                "Escritor resuelto",

            MetadataWriteStatus.FileOpened =>
                "Archivo abierto",

            MetadataWriteStatus.ValuesPrepared =>
                "Valores preparados",

            MetadataWriteStatus.Saved =>
                "Archivo guardado",

            MetadataWriteStatus.Completed =>
                "Completada",

            MetadataWriteStatus.ValidationFailed =>
                "Validación rechazada",

            MetadataWriteStatus.UnsupportedFormat =>
                "Formato no soportado",

            MetadataWriteStatus.FileOpenFailed =>
                "Error al abrir el archivo",

            MetadataWriteStatus.NoWritableChanges =>
                "Ejecución diagnóstica sin escritura",

            MetadataWriteStatus.PartiallyCompleted =>
                "Completada parcialmente",

            MetadataWriteStatus.SaveFailed =>
                "Error al guardar",

            MetadataWriteStatus.Cancelled =>
                "Cancelada",

            MetadataWriteStatus.UnexpectedError =>
                "Error inesperado",

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