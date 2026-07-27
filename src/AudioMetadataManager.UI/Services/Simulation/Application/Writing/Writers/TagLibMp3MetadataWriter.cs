using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Base;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

/// <summary>
/// Escritor real de metadatos MP3 mediante TagLibSharp.
/// </summary>
public sealed class TagLibMp3MetadataWriter
    : TagLibMetadataWriterBase
{
    public TagLibMp3MetadataWriter()
        : base(
            name:
                "TagLibSharp MP3 Metadata Writer",
            formatDisplayName:
                "MP3",
            supportedExtensions:
                new[]
                {
                    ".mp3"
                })
    {
    }
}