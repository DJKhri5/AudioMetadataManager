using System.Text;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Diagnostics;

/// <summary>
/// Construye el informe de una prueba real de escritura MP3
/// ejecutada sobre una copia aislada.
/// </summary>
public static class TagLibMp3IsolatedWriteTestDiagnostics
{
    public static string BuildReport(
        TagLibMp3IsolatedWriteTestResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Prueba aislada de escritura MP3 ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Archivo original: " +
            $"{DisplayValue(result.OriginalFilePath)}");

        builder.AppendLine(
            $"Copia de trabajo: " +
            $"{DisplayValue(result.WorkingCopyPath)}");

        builder.AppendLine(
            $"Respaldo de la copia: " +
            $"{DisplayValue(result.WorkingBackupPath)}");

        builder.AppendLine(
            $"Carpeta de prueba: " +
            $"{DisplayValue(result.TestDirectoryPath)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Comprobaciones de seguridad ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Original intacto: " +
            $"{ToSpanish(result.OriginalFileRemainedUnchanged)}");

        builder.AppendLine(
            $"Respaldo coincide con la copia inicial: " +
            $"{ToSpanish(result.BackupMatchesInitialWorkingCopy)}");

        builder.AppendLine(
            $"Copia modificada realmente: " +
            $"{ToSpanish(result.WorkingCopyWasModified)}");

        builder.AppendLine();

        builder.AppendLine(
            $"Hash original antes: " +
            $"{DisplayValue(result.OriginalHashBefore)}");

        builder.AppendLine(
            $"Hash original después: " +
            $"{DisplayValue(result.OriginalHashAfter)}");

        builder.AppendLine(
            $"Hash copia antes: " +
            $"{DisplayValue(result.WorkingCopyHashBefore)}");

        builder.AppendLine(
            $"Hash copia después: " +
            $"{DisplayValue(result.WorkingCopyHashAfter)}");

        builder.AppendLine(
            $"Hash respaldo: " +
            $"{DisplayValue(result.WorkingBackupHash)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Comprobación del género ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Género original: " +
            $"{DisplayValue(result.OriginalGenre)}");

        builder.AppendLine(
            $"Género solicitado: " +
            $"{DisplayValue(result.RequestedGenre)}");

        builder.AppendLine(
            $"Género persistido: " +
            $"{DisplayValue(result.PersistedGenre)}");

        builder.AppendLine(
            $"Género verificado: " +
            $"{ToSpanish(result.GenreWasPersisted)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Carátulas ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Cantidad antes: {result.PictureCountBefore}");

        builder.AppendLine(
            $"Cantidad después: {result.PictureCountAfter}");

        builder.AppendLine(
            $"Carátulas preservadas: " +
            $"{ToSpanish(result.PicturesWerePreserved)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Resultado del escritor ---");

        builder.AppendLine();

        if (result.WriteResult is null)
        {
            builder.AppendLine(
                "No se obtuvo un resultado del escritor.");
        }
        else
        {
            builder.AppendLine(
                MetadataWriterDiagnostics.BuildReport(
                    result.WriteResult));
        }

        builder.AppendLine(
            "--- Mensajes de la prueba ---");

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
            $"Prueba correcta: " +
            $"{ToSpanish(result.WasSuccessful)}");

        builder.AppendLine(
            $"Resumen: {result.Summary}");

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin de la prueba aislada MP3 ===");

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