using AudioMetadataManager.UI.Models;
using TagLib;

namespace AudioMetadataManager.UI.Services;

public class MetadataReaderService
{
    public void ReadMetadata(AudioFile audioFile)
    {
        try
        {
            using var file = TagLib.File.Create(audioFile.FullPath);

            audioFile.Title = file.Tag.Title ?? "";

            audioFile.Artist =
                file.Tag.FirstPerformer ?? "";

            audioFile.Version =
                file.Tag.Subtitle ?? "";

            audioFile.Album =
                file.Tag.Album ?? "";

            audioFile.Genre =
                file.Tag.FirstGenre ?? "";

            audioFile.Label =
                file.Tag.Publisher ?? "";

            audioFile.Year =
                file.Tag.Year;

            audioFile.Duration =
                file.Properties.Duration;

            audioFile.Bitrate =
                file.Properties.AudioBitrate;

            audioFile.SampleRate =
                file.Properties.AudioSampleRate;

            audioFile.Channels =
                file.Properties.AudioChannels;

            audioFile.BitsPerSample =
                file.Properties.BitsPerSample;

            audioFile.CodecName =
                GetCodecName(audioFile.Extension);

            audioFile.Status = "Analizado";
        }
        catch
        {
            audioFile.Status = "Error";
        }
    }
    private static string GetCodecName(string extension)
    {
        return extension.Trim().ToLowerInvariant() switch
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
}
