using System.Text;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Diagnostics;

/// <summary>
/// Construye un informe legible de la verificación posterior.
/// </summary>
public static class MetadataVerificationDiagnostics
{
    public static string BuildReport(
        MetadataVerificationResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Verificación posterior de metadatos ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Archivo: {DisplayValue(result.FilePath)}");

        builder.AppendLine(
            $"Archivo abierto: {ToSpanish(result.FileOpened)}");

        builder.AppendLine(
            $"Verificación correcta: " +
            $"{ToSpanish(result.WasSuccessful)}");

        builder.AppendLine(
            $"Campos correctos: {result.SuccessfulFieldCount}");

        builder.AppendLine(
            $"Campos fallidos: {result.FailedFieldCount}");

        builder.AppendLine(
            $"Imágenes antes: {result.PictureCountBefore}");

        builder.AppendLine(
            $"Imágenes después: {result.PictureCountAfter}");

        builder.AppendLine(
            $"Imágenes preservadas: " +
            $"{ToSpanish(result.PicturesPreserved)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Resultados por campo ---");

        builder.AppendLine();

        if (result.FieldResults.Count == 0)
        {
            builder.AppendLine(
                "No se registraron campos para verificar.");
        }
        else
        {
            foreach (MetadataFieldVerificationResult fieldResult
                in result.FieldResults)
            {
                builder.AppendLine(
                    $"[{fieldResult.Field}]");

                builder.AppendLine(
                    $"Valor esperado: " +
                    $"{DisplayValue(fieldResult.ExpectedValue)}");

                builder.AppendLine(
                    $"Valor persistido: " +
                    $"{DisplayValue(fieldResult.PersistedValue)}");

                builder.AppendLine(
                    $"Campo soportado: " +
                    $"{ToSpanish(fieldResult.IsSupported)}");

                builder.AppendLine(
                    $"Coincidencia: " +
                    $"{ToSpanish(
                        fieldResult.MatchesExpectedValue)}");

                builder.AppendLine(
                    $"Mensaje: " +
                    $"{DisplayValue(fieldResult.Message)}");

                builder.AppendLine();
            }
        }

        builder.AppendLine(
            "--- Mensajes generales ---");

        builder.AppendLine();

        foreach (string message in result.Messages)
        {
            builder.AppendLine(
                $"- {message}");
        }

        builder.AppendLine();

        builder.AppendLine(
            $"Resumen: {result.Summary}");

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin de la verificación posterior ===");

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