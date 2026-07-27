using System.Text;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Diagnostics;

/// <summary>
/// Genera un informe legible de una inspección MP3 realizada
/// mediante TagLibSharp.
/// </summary>
public static class TagLibMp3InspectionDiagnostics
{
    public static string BuildReport(
        TagLibMp3InspectionResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de inspección MP3 ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Archivo: {DisplayValue(result.FilePath)}");

        builder.AppendLine(
            $"Inspección correcta: " +
            $"{ToSpanish(result.WasSuccessful)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Etiquetas ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Título: {DisplayValue(result.Title)}");

        builder.AppendLine(
            $"Artistas: {result.PerformersDisplay}");

        builder.AppendLine(
            $"Artistas del álbum: " +
            $"{DisplayList(result.AlbumArtists)}");

        builder.AppendLine(
            $"Álbum: {DisplayValue(result.Album)}");

        builder.AppendLine(
            $"Géneros: {result.GenresDisplay}");

        builder.AppendLine(
            $"Año: {DisplayNumber(result.Year)}");

        builder.AppendLine(
            $"Pista: {DisplayNumber(result.Track)}");

        builder.AppendLine(
            $"Total de pistas: " +
            $"{DisplayNumber(result.TrackCount)}");

        builder.AppendLine(
            $"Disco: {DisplayNumber(result.Disc)}");

        builder.AppendLine(
            $"Total de discos: " +
            $"{DisplayNumber(result.DiscCount)}");

        builder.AppendLine(
            $"Comentario: " +
            $"{DisplayValue(result.Comment)}");

        builder.AppendLine(
            $"Tipos de etiquetas: " +
            $"{DisplayValue(result.TagTypes)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Imágenes incrustadas ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Cantidad: {result.EmbeddedPictureCount}");

        builder.AppendLine(
            $"Contiene imágenes: " +
            $"{ToSpanish(result.HasEmbeddedPictures)}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Propiedades técnicas ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Duración: {result.Duration:hh\\:mm\\:ss}");

        builder.AppendLine(
            $"Bitrate: {result.AudioBitrateKbps} kbps");

        builder.AppendLine(
            $"Frecuencia: {result.AudioSampleRateHz} Hz");

        builder.AppendLine(
            $"Canales: {result.AudioChannels}");

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
            "=== Fin del diagnóstico de inspección MP3 ===");

        return builder.ToString();
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(sin información)"
            : value.Trim();
    }

    private static string DisplayList(
        IReadOnlyList<string> values)
    {
        return values.Count == 0
            ? "(sin información)"
            : string.Join(
                ", ",
                values);
    }

    private static string DisplayNumber(
        uint value)
    {
        return value == 0
            ? "(sin información)"
            : value.ToString();
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}