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

            audioFile.Album =
                file.Tag.Album ?? "";

            audioFile.Genre =
                file.Tag.FirstGenre ?? "";

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

            audioFile.Status = "Analizado";
        }
        catch
        {
            audioFile.Status = "Error";
        }
    }
}