using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Analysis;

namespace AudioMetadataManager.UI.Services;

public class MusicAnalysisEngine
{
    private readonly ConfidenceEngine _confidenceEngine = new();

    public AnalysisResult Analyze(AudioFile audioFile)
    {
        if (audioFile.ParsedName == null ||
            audioFile.Comparison == null)
        {
            return new AnalysisResult
            {
                ConfidenceScore = 0,
                ConfidenceLevel = "Sin información",
                RequiresManualReview = true,
                ArtistReliable = false,
                TitleReliable = false,
                VersionDetected = false,
                Summary =
                    "No existen resultados suficientes del parser o del comparador."
            };
        }

        AnalysisResult result =
            _confidenceEngine.Evaluate(audioFile);

        result.ArtistReliable =
            audioFile.Comparison.ArtistMatches;

        result.TitleReliable =
            audioFile.Comparison.TitleMatches;

        result.VersionDetected =
            !string.IsNullOrWhiteSpace(
                audioFile.ParsedName.Version);

        return result;
    }
}