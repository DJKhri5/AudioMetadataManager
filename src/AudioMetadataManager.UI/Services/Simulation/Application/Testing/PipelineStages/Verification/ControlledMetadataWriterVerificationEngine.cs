using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Engine;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Verification;

/// <summary>
/// Motor de verificación controlado utilizado por las pruebas
/// estructurales de MetadataVerificationStage.
/// </summary>
internal sealed class
    ControlledMetadataWriterVerificationEngine :
        IMetadataWriterVerificationEngine
{
    private readonly MetadataVerificationResult
        _resultToReturn;

    public ControlledMetadataWriterVerificationEngine(
        MetadataVerificationResult resultToReturn)
    {
        _resultToReturn =
            resultToReturn ??
            throw new ArgumentNullException(
                nameof(resultToReturn));
    }

    public int CallCount { get; private set; }

    public bool WasCalled =>
        CallCount > 0;

    public string LastFilePath { get; private set; } =
        string.Empty;

    public IReadOnlyList<MetadataFieldChange>
        LastChanges
    { get; private set; } =
            Array.Empty<MetadataFieldChange>();

    public int LastPictureCountBefore
    { get; private set; }

    /// <inheritdoc />
    public MetadataVerificationResult Verify(
        string? filePath,
        IEnumerable<MetadataFieldChange>? changes,
        int pictureCountBefore)
    {
        CallCount++;

        LastFilePath =
            filePath?.Trim() ??
            string.Empty;

        LastChanges =
            changes?.ToArray() ??
            Array.Empty<MetadataFieldChange>();

        LastPictureCountBefore =
            pictureCountBefore;

        return _resultToReturn;
    }
}