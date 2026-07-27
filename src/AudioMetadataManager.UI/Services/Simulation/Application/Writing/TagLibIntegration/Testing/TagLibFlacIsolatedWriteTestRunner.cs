using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Testing;

/// <summary>
/// Ejecuta la prueba real de escritura FLAC exclusivamente
/// sobre una copia temporal aislada.
/// </summary>
public sealed class TagLibFlacIsolatedWriteTestRunner
{
    private readonly TagLibIsolatedWriteTestRunner
        _commonRunner =
            new();

    public Task<TagLibIsolatedWriteTestResult>
        RunAsync(
            string? originalFilePath,
            string requestedGenre = "Electronic",
            CancellationToken cancellationToken = default)
    {
        return _commonRunner.RunAsync(
            originalFilePath,
            writer:
                new TagLibFlacMetadataWriter(),
            formatDisplayName:
                "FLAC",
            testFolderName:
                "TagLibFlacWriteTests",
            requestedGenre:
                requestedGenre,
            cancellationToken:
                cancellationToken);
    }
}