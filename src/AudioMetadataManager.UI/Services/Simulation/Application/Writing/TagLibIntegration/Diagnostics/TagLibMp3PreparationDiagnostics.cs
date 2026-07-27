using System.Text;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Diagnostics;

/// <summary>
/// Genera un informe legible del proceso de preparación de
/// cambios MP3 exclusivamente en memoria.
/// </summary>
public static class TagLibMp3PreparationDiagnostics
{
    public static string BuildReport(
        TagLibMp3PreparationResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de preparación MP3 en memoria ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Archivo: {DisplayValue(result.FilePath)}");

        builder.AppendLine(
            $"Archivo abierto: {ToSpanish(result.FileOpened)}");

        builder.AppendLine(
            $"Preparación correcta: " +
            $"{ToSpanish(result.WasSuccessful)}");

        builder.AppendLine(
            $"Campos preparados: " +
            $"{result.SuccessfulFieldCount}");

        builder.AppendLine(
            $"Campos fallidos: {result.FailedFieldCount}");

        builder.AppendLine(
            $"Imágenes antes: {result.PictureCountBefore}");

        builder.AppendLine(
            $"Imágenes después: {result.PictureCountAfter}");

        builder.AppendLine(
            $"Imágenes preservadas: " +
            $"{ToSpanish(result.PicturesPreserved)}");

        builder.AppendLine(
            $"Save ejecutado: " +
            $"{ToSpanish(result.SaveWasExecuted)}");

        builder.AppendLine(
            $"Archivo físico intacto: " +
            $"{ToSpanish(result.PhysicalFileRemainedUnchanged)}");

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
            foreach (TagLibMp3FieldPreparationResult fieldResult
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
                    $"Valor preparado: " +
                    $"{DisplayValue(fieldResult.PreparedValue)}");

                builder.AppendLine(
                    $"Campo soportado: " +
                    $"{ToSpanish(fieldResult.IsSupported)}");

                builder.AppendLine(
                    $"Valor preparado correctamente: " +
                    $"{ToSpanish(fieldResult.WasPrepared)}");

                builder.AppendLine(
                    $"Coincide con lo solicitado: " +
                    $"{ToSpanish(fieldResult.MatchesRequestedValue)}");

                builder.AppendLine(
                    $"Resultado correcto: " +
                    $"{ToSpanish(fieldResult.WasSuccessful)}");

                builder.AppendLine(
                    $"Mensaje: " +
                    $"{DisplayValue(fieldResult.Message)}");

                builder.AppendLine();
            }
        }

        builder.AppendLine(
            "--- Mensajes generales ---");

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
            "=== Fin del diagnóstico de preparación MP3 ===");

        return builder.ToString();
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