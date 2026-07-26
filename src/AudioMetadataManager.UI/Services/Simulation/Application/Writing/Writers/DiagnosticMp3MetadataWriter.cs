namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

/// <summary>
/// Escritor de diagnóstico para archivos MP3.
/// </summary>
public sealed class DiagnosticMp3MetadataWriter
    : DiagnosticMetadataFormatWriterBase
{
    public DiagnosticMp3MetadataWriter()
        : base(
            "Diagnostic MP3 Metadata Writer",
            new[]
            {
                ".mp3"
            })
    {
    }
}