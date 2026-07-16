using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Quality;

public class AudioQualityAnalyzerService
{
    public AudioQualityResult Analyze(AudioFile audioFile)
    {
        AudioQualityResult result = new();

        result.HasValidDuration =
            audioFile.Duration > TimeSpan.Zero;

        result.HasValidSampleRate =
            IsPlausibleSampleRate(audioFile.SampleRate);

        result.HasValidChannels =
            audioFile.Channels is >= 1 and <= 8;

        result.HasPlausibleBitrate =
            IsPlausibleBitrate(
                audioFile.Extension,
                audioFile.Bitrate);

        int score = 0;
        List<string> details = new();

        if (result.HasValidDuration)
        {
            score += 20;
        }
        else
        {
            details.Add("Duración no disponible o inválida");
        }

        if (result.HasValidSampleRate)
        {
            score += 25;
        }
        else
        {
            details.Add("Frecuencia de muestreo inusual");
        }

        if (result.HasValidChannels)
        {
            score += 15;
        }
        else
        {
            details.Add("Cantidad de canales inusual");
        }

        if (result.HasPlausibleBitrate)
        {
            score += 40;
        }
        else
        {
            details.Add("Bitrate técnicamente sospechoso");
        }

        result.QualityScore = score;
        result.QualityLevel = GetQualityLevel(score);
        result.RequiresManualReview = score < 80;

        result.SpectralAnalysisCompleted = false;

        if (details.Count == 0)
        {
            details.Add(
                "Los parámetros técnicos declarados son coherentes");
        }

        details.Add("Análisis espectral pendiente");

        result.Summary = string.Join(" · ", details);

        return result;
    }

    private static bool IsPlausibleSampleRate(int sampleRate)
    {
        int[] commonSampleRates =
        {
            8000,
            11025,
            16000,
            22050,
            32000,
            44100,
            48000,
            88200,
            96000,
            176400,
            192000
        };

        return commonSampleRates.Contains(sampleRate);
    }

    private static bool IsPlausibleBitrate(
        string extension,
        int bitrate)
    {
        string normalizedExtension =
            extension.Trim().ToLowerInvariant();

        return normalizedExtension switch
        {
            ".mp3" => bitrate is >= 32 and <= 320,

            ".m4a" or ".aac" =>
                bitrate is >= 32 and <= 1000,

            ".ogg" or ".opus" =>
                bitrate is >= 16 and <= 1000,

            ".wav" or ".aif" or ".aiff" =>
                bitrate >= 600,

            ".flac" =>
                bitrate >= 300,

            _ =>
                bitrate > 0
        };
    }

    private static string GetQualityLevel(int score)
    {
        return score switch
        {
            >= 95 => "Técnicamente consistente",
            >= 80 => "Consistente",
            >= 60 => "Revisión recomendada",
            >= 40 => "Sospechosa",
            _ => "No verificable"
        };
    }
}