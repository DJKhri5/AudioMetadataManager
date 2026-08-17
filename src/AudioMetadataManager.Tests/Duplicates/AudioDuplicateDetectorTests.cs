using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Duplicates;
using AudioMetadataManager.UI.Services.Duplicates.Models;
using AudioMetadataManager.UI.Services.Quality;
using Xunit;

namespace AudioMetadataManager.Tests.Duplicates;

public class AudioDuplicateDetectorTests
{
    [Fact]
    public void DetectDuplicates_NullOrSingleFile_ReturnsEmptyResult()
    {
        var detector = new AudioDuplicateDetector();

        var nullResult = detector.DetectDuplicates(null!);
        Assert.Empty(nullResult.Groups);
        Assert.Equal(0, nullResult.TotalDuplicateGroups);

        var singleFile = new AudioFile
        {
            FileName = "song.mp3",
            FullPath = @"C:\Music\song.mp3",
            Extension = ".mp3",
            FileSizeBytes = 5000000
        };

        var singleResult = detector.DetectDuplicates(new[] { singleFile });
        Assert.Empty(singleResult.Groups);
    }

    [Fact]
    public void DetectDuplicates_ExactBinaryDuplicates_GroupsCorrectly()
    {
        var detector = new AudioDuplicateDetector();

        var file1 = new AudioFile
        {
            FileName = "track1.mp3",
            FullPath = @"C:\Music\track1.mp3",
            Extension = ".mp3",
            FileSizeBytes = 10000000,
            Duration = TimeSpan.FromMinutes(3.5),
            Artist = "Armin van Buuren",
            Title = "Communication"
        };

        var file2 = new AudioFile
        {
            FileName = "track1_copy.mp3",
            FullPath = @"C:\Music\Other\track1_copy.mp3",
            Extension = ".mp3",
            FileSizeBytes = 10000000,
            Duration = TimeSpan.FromMinutes(3.5),
            Artist = "Armin van Buuren",
            Title = "Communication"
        };

        var result = detector.DetectDuplicates(new[] { file1, file2 });

        Assert.Single(result.Groups);
        var group = result.Groups[0];
        Assert.Equal(DuplicateMatchKind.ExactBinary, group.MatchKind);
        Assert.Equal(2, group.FileCount);
        Assert.True(group.Items[0].IsBestQualityCandidate);
        Assert.False(group.Items[1].IsBestQualityCandidate);
        Assert.Equal(10000000, group.PotentialReclaimableBytes);
    }

    [Fact]
    public void DetectDuplicates_ProbableMetadata_GroupsAndRanksFlacOverMp3()
    {
        var detector = new AudioDuplicateDetector();

        var flacFile = new AudioFile
        {
            FileName = "01 - Ben Nicky - Relapse.flac",
            FullPath = @"C:\Music\01 - Ben Nicky - Relapse.flac",
            Extension = ".flac",
            FileSizeBytes = 35000000,
            Duration = TimeSpan.FromMinutes(4),
            Bitrate = 1050,
            SampleRate = 44100,
            Artist = "Ben Nicky",
            Title = "Relapse",
            Version = "Extended Mix",
            QualityAnalysis = new AudioQualityResult
            {
                IsLossless = true,
                QualityScore = 95
            }
        };

        var mp3File = new AudioFile
        {
            FileName = "ben-nicky-relapse-extended.mp3",
            FullPath = @"C:\Music\ben-nicky-relapse-extended.mp3",
            Extension = ".mp3",
            FileSizeBytes = 9000000,
            Duration = TimeSpan.FromMinutes(4),
            Bitrate = 320,
            SampleRate = 44100,
            Artist = "Ben Nicky",
            Title = "Relapse",
            Version = "Extended Mix",
            QualityAnalysis = new AudioQualityResult
            {
                IsLossless = false,
                QualityScore = 75
            }
        };

        var result = detector.DetectDuplicates(new[] { flacFile, mp3File });

        Assert.Single(result.Groups);
        var group = result.Groups[0];
        Assert.Equal(DuplicateMatchKind.ProbableMetadata, group.MatchKind);
        Assert.Equal(2, group.FileCount);

        // La mejor calidad debe ser el FLAC
        Assert.Equal(".FLAC", group.Items[0].Extension);
        Assert.True(group.Items[0].IsBestQualityCandidate);
        Assert.Equal("Mejor versión recomendada", group.Items[0].QualityBadge);

        // La versión redundante debe ser el MP3
        Assert.Equal(".MP3", group.Items[1].Extension);
        Assert.False(group.Items[1].IsBestQualityCandidate);
        Assert.Equal("Copia redundante / menor calidad", group.Items[1].QualityBadge);
        Assert.Equal(9000000, group.PotentialReclaimableBytes);
    }

    [Fact]
    public void DetectDuplicates_DifferentTracks_AreNotGrouped()
    {
        var detector = new AudioDuplicateDetector();

        var trackA = new AudioFile
        {
            FileName = "trackA.mp3",
            FullPath = @"C:\Music\trackA.mp3",
            Extension = ".mp3",
            FileSizeBytes = 7000000,
            Duration = TimeSpan.FromMinutes(3),
            Artist = "Tiësto",
            Title = "Adagio for Strings"
        };

        var trackB = new AudioFile
        {
            FileName = "trackB.mp3",
            FullPath = @"C:\Music\trackB.mp3",
            Extension = ".mp3",
            FileSizeBytes = 8500000,
            Duration = TimeSpan.FromMinutes(4),
            Artist = "Paul van Dyk",
            Title = "For an Angel"
        };

        var result = detector.DetectDuplicates(new[] { trackA, trackB });

        Assert.Empty(result.Groups);
        Assert.Equal(0, result.TotalDuplicateGroups);
        Assert.Equal(0, result.TotalPotentialReclaimableBytes);
    }
}
