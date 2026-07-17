using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Quality;

public class AudioQualityAnalyzerService
{
    public AudioQualityResult Analyze(AudioFile audioFile)
    {
        AudioQualityResult result = new();

        // Información técnica básica
        result.CodecName =
            string.IsNullOrWhiteSpace(audioFile.CodecName)
                ? GetCodecName(audioFile.Extension)
                : audioFile.CodecName;

        result.CompressionType =
            GetCompressionType(audioFile.Extension);

        result.IsLossless =
            IsLosslessFormat(audioFile.Extension);

        result.BitsPerSample =
            audioFile.BitsPerSample;

        /*
         * Todavía no podemos determinar CBR o VBR con seguridad
         * únicamente con los datos actuales de TagLibSharp.
         */
        result.BitrateMode =
            GetProvisionalBitrateMode(audioFile.Extension);

        // Validaciones principales
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
        List<string> warnings = new();

        // Duración: 15 puntos
        if (result.HasValidDuration)
        {
            score += 15;
        }
        else
        {
            warnings.Add(
                "Duración no disponible o inválida");
        }

        // Frecuencia de muestreo: 20 puntos
        if (result.HasValidSampleRate)
        {
            score += 20;
        }
        else
        {
            warnings.Add(
                "Frecuencia de muestreo inusual");
        }

        // Canales: 10 puntos
        if (result.HasValidChannels)
        {
            score += 10;
        }
        else
        {
            warnings.Add(
                "Cantidad de canales inusual");
        }

        // Bitrate: 30 puntos
        if (result.HasPlausibleBitrate)
        {
            score += 30;
        }
        else
        {
            warnings.Add(
                "Bitrate técnicamente sospechoso");
        }

        // Coherencia codec/extensión: 15 puntos
        if (IsCodecConsistent(
                audioFile.Extension,
                result.CodecName))
        {
            score += 15;
        }
        else
        {
            warnings.Add(
                "El codec identificado no parece coherente con la extensión");
        }

        // Bits por muestra: 10 puntos
        if (HasPlausibleBitsPerSample(
                audioFile.Extension,
                audioFile.BitsPerSample))
        {
            score += 10;
        }
        else
        {
            warnings.Add(
                BuildBitsPerSampleWarning(audioFile));
        }

        result.QualityScore =
            Math.Clamp(score, 0, 100);

        result.QualityLevel =
            GetQualityLevel(result.QualityScore);

        result.HasTechnicalWarnings =
            warnings.Count > 0;

        result.TechnicalWarnings =
            warnings;

        result.RequiresManualReview =
            result.QualityScore < 80 ||
            result.HasTechnicalWarnings;

        result.SpectralAnalysisCompleted = false;

        details.Add(
            $"Codec: {result.CodecName}");

        details.Add(
            $"Compresión: {result.CompressionType}");

        details.Add(
            $"Bitrate: {GetBitrateDescription(audioFile.Bitrate)}");

        details.Add(
            $"Modo bitrate: {result.BitrateMode}");

        details.Add(
            $"Muestreo: {GetSampleRateDescription(audioFile.SampleRate)}");

        details.Add(
            $"Canales: {GetChannelDescription(audioFile.Channels)}");

        details.Add(
            $"Profundidad: {GetBitsPerSampleDescription(audioFile.BitsPerSample)}");

        if (warnings.Count == 0)
        {
            details.Add(
                "Los parámetros técnicos declarados son coherentes");
        }
        else
        {
            details.AddRange(
                warnings.Select(warning => $"Advertencia: {warning}"));
        }

        details.Add(
            "Análisis espectral pendiente");

        result.Summary =
            string.Join(" · ", details);

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
            NormalizeExtension(extension);

        return normalizedExtension switch
        {
            ".mp3" =>
                bitrate is >= 32 and <= 320,

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

    private static bool HasPlausibleBitsPerSample(
        string extension,
        int bitsPerSample)
    {
        string normalizedExtension =
            NormalizeExtension(extension);

        /*
         * En formatos comprimidos con pérdida, TagLibSharp puede
         * no entregar BitsPerSample. Un valor 0 no se considera
         * necesariamente un error.
         */
        if (normalizedExtension is
            ".mp3" or ".m4a" or ".aac" or ".ogg" or ".opus")
        {
            return bitsPerSample == 0 ||
                   bitsPerSample is 16 or 24 or 32;
        }

        if (normalizedExtension is
            ".wav" or ".aif" or ".aiff" or ".flac")
        {
            return bitsPerSample is 16 or 20 or 24 or 32;
        }

        return bitsPerSample >= 0;
    }

    private static bool IsLosslessFormat(string extension)
    {
        string normalizedExtension =
            NormalizeExtension(extension);

        return normalizedExtension is
            ".wav" or ".aif" or ".aiff" or ".flac";
    }

    private static string GetCompressionType(string extension)
    {
        string normalizedExtension =
            NormalizeExtension(extension);

        return normalizedExtension switch
        {
            ".wav" or ".aif" or ".aiff" =>
                "Sin pérdida / PCM",

            ".flac" =>
                "Sin pérdida comprimida",

            ".mp3" or ".aac" or ".ogg" or ".opus" =>
                "Con pérdida",

            ".m4a" =>
                "Con pérdida o sin pérdida",

            _ =>
                "Desconocida"
        };
    }

    private static string GetCodecName(string extension)
    {
        string normalizedExtension =
            NormalizeExtension(extension);

        return normalizedExtension switch
        {
            ".mp3" => "MPEG Layer III",
            ".wav" => "PCM / WAV",
            ".flac" => "FLAC",
            ".aif" => "AIFF",
            ".aiff" => "AIFF",
            ".m4a" => "AAC / ALAC",
            ".aac" => "AAC",
            ".ogg" => "Ogg Vorbis",
            ".opus" => "Opus",
            _ => "Desconocido"
        };
    }

    private static bool IsCodecConsistent(
        string extension,
        string codecName)
    {
        if (string.IsNullOrWhiteSpace(codecName) ||
            codecName.Equals(
                "Desconocido",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string expectedCodec =
            GetCodecName(extension);

        if (expectedCodec == "Desconocido")
        {
            return true;
        }

        return codecName.Equals(
            expectedCodec,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string GetProvisionalBitrateMode(
        string extension)
    {
        string normalizedExtension =
            NormalizeExtension(extension);

        return normalizedExtension switch
        {
            ".wav" or ".aif" or ".aiff" =>
                "PCM constante",

            ".flac" =>
                "Variable según compresión",

            ".mp3" or ".m4a" or ".aac" or ".ogg" or ".opus" =>
                "Sin determinar",

            _ =>
                "No aplicable"
        };
    }

    private static string BuildBitsPerSampleWarning(
        AudioFile audioFile)
    {
        if (audioFile.BitsPerSample <= 0)
        {
            return
                "Profundidad de bits no disponible";
        }

        return
            $"Profundidad de bits inusual: {audioFile.BitsPerSample} bits";
    }

    private static string GetBitrateDescription(int bitrate)
    {
        return bitrate > 0
            ? $"{bitrate} kbps"
            : "No disponible";
    }

    private static string GetSampleRateDescription(int sampleRate)
    {
        return sampleRate > 0
            ? $"{sampleRate / 1000.0:0.#} kHz"
            : "No disponible";
    }

    private static string GetBitsPerSampleDescription(
        int bitsPerSample)
    {
        return bitsPerSample > 0
            ? $"{bitsPerSample} bits"
            : "No disponible";
    }

    private static string GetChannelDescription(int channels)
    {
        return channels switch
        {
            1 => "Mono",
            2 => "Estéreo",
            > 2 => $"{channels} canales",
            _ => "No disponible"
        };
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        string normalized =
            extension.Trim().ToLowerInvariant();

        return normalized.StartsWith('.')
            ? normalized
            : $".{normalized}";
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