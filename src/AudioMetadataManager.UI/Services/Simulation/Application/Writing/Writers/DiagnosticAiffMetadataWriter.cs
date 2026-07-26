namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

/// <summary>
/// Escritor de diagnóstico para archivos AIFF y AIF.
/// </summary>
public sealed class DiagnosticAiffMetadataWriter
    : DiagnosticMetadataFormatWriterBase
{
    public DiagnosticAiffMetadataWriter()
        : base(
            "Diagnostic AIFF Metadata Writer",
            new[]
            {
                ".aif",
                ".aiff"
            })
    {
    }
}