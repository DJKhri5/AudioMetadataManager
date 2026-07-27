using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Base;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

/// <summary>
/// Escritor real de metadatos FLAC mediante TagLibSharp.
/// Reutiliza la validación, respaldo, escritura y verificación
/// posterior implementadas en TagLibMetadataWriterBase.
/// </summary>
public sealed class TagLibFlacMetadataWriter
    : TagLibMetadataWriterBase
{
    public TagLibFlacMetadataWriter()
        : base(
            name:
                "TagLibSharp FLAC Metadata Writer",
            formatDisplayName:
                "FLAC",
            supportedExtensions:
                new[]
                {
                    ".flac"
                })
    {
    }
}