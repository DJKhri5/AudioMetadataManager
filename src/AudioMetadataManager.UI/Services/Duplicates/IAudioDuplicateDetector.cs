using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Duplicates.Models;

namespace AudioMetadataManager.UI.Services.Duplicates;

public interface IAudioDuplicateDetector
{
    DuplicateDetectionResult DetectDuplicates(IEnumerable<AudioFile> files);
}
