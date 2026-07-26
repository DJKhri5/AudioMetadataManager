namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

/// <summary>
/// Escritor de diagnóstico para archivos WAV.
/// </summary>
public sealed class DiagnosticWavMetadataWriter
    : DiagnosticMetadataFormatWriterBase
{
    public DiagnosticWavMetadataWriter()
        : base(
            "Diagnostic WAV Metadata Writer",
            new[]
            {
                ".wav"
            })
    {
    }
}