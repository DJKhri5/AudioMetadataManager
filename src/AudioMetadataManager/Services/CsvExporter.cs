using System.Text;
using AudioMetadataManager.Models;
namespace AudioMetadataManager.Services;
public static class CsvExporter
{
    public static async Task ExportAsync(string path, IEnumerable<AudioItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Archivo;Propuesto;Artista;Titulo;Version;Estado;Advertencias;Formato;TamanoBytes;Duracion;BitrateKbps;SampleRateHz;Bits;Canales;Duplicado");
        foreach (var x in items) sb.AppendLine(string.Join(';', new[] { x.RelativePath, x.ProposedFileName, x.Artist, x.Title, x.Version, x.Status, x.Warnings, x.Extension, x.SizeBytes.ToString(), x.Duration.ToString(), x.AudioBitrateKbps.ToString(), x.SampleRateHz.ToString(), x.BitsPerSample.ToString(), x.Channels.ToString(), x.DuplicateGroup }.Select(Escape)));
        await File.WriteAllTextAsync(path, sb.ToString(), new UTF8Encoding(true));
    }
    private static string Escape(string? value) => '"' + (value ?? "").Replace("\"", "\"\"") + '"';
}
