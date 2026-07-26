namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

/// <summary>
/// Escritor de diagnóstico para archivos FLAC.
/// </summary>
public sealed class DiagnosticFlacMetadataWriter
    : DiagnosticMetadataFormatWriterBase
{
    public DiagnosticFlacMetadataWriter()
        : base(
            "Diagnostic FLAC Metadata Writer",
            new[]
            {
                ".flac"
            })
    {
    }
}