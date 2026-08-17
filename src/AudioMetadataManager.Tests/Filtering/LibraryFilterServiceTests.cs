using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Filtering;
using AudioMetadataManager.UI.Services.Filtering.Models;
using Xunit;

namespace AudioMetadataManager.Tests.Filtering;

public class LibraryFilterServiceTests
{
    [Fact]
    public void NormalizeString_RemovesAccentsAndLowercases()
    {
        string input = "Tiësto - Canción Éxito & Mañana";
        string normalized = LibraryFilterService.NormalizeString(input);

        Assert.Equal("tiesto - cancion exito & manana", normalized);
    }

    [Fact]
    public void Matches_SearchText_MatchesAccentAndCaseInsensitive()
    {
        var service = new LibraryFilterService();
        var file = new AudioFile
        {
            FileName = "01 - Tiesto - Adagio.flac",
            FullPath = @"C:\Music\01 - Tiesto - Adagio.flac",
            Extension = ".flac",
            Artist = "Tiësto",
            Title = "Adagio for Strings",
            Label = "Magik Muzik"
        };

        // Búsqueda por artista con y sin tilde/diéresis
        Assert.True(service.Matches(file, new LibraryFilterCriteria { SearchText = "tiesto" }));
        Assert.True(service.Matches(file, new LibraryFilterCriteria { SearchText = "Tiësto" }));

        // Búsqueda por título
        Assert.True(service.Matches(file, new LibraryFilterCriteria { SearchText = "adagio" }));

        // Búsqueda por sello discográfico
        Assert.True(service.Matches(file, new LibraryFilterCriteria { SearchText = "magik muzik" }));

        // Búsqueda multi-término
        Assert.True(service.Matches(file, new LibraryFilterCriteria { SearchText = "tiesto magik" }));

        // Búsqueda que no coincide
        Assert.False(service.Matches(file, new LibraryFilterCriteria { SearchText = "armin" }));
    }

    [Fact]
    public void Matches_FormatFilter_FiltersCorrectly()
    {
        var service = new LibraryFilterService();
        var flacFile = new AudioFile { Extension = ".flac", FileName = "track.flac" };
        var mp3File = new AudioFile { Extension = ".mp3", FileName = "track.mp3" };

        Assert.True(service.Matches(flacFile, new LibraryFilterCriteria { FormatFilter = "FLAC" }));
        Assert.False(service.Matches(mp3File, new LibraryFilterCriteria { FormatFilter = "FLAC" }));

        Assert.True(service.Matches(mp3File, new LibraryFilterCriteria { FormatFilter = "MP3" }));
        Assert.False(service.Matches(flacFile, new LibraryFilterCriteria { FormatFilter = "MP3" }));

        Assert.True(service.Matches(flacFile, new LibraryFilterCriteria { FormatFilter = "Todos" }));
        Assert.True(service.Matches(mp3File, new LibraryFilterCriteria { FormatFilter = "Todos" }));
    }

    [Fact]
    public void Matches_QualityFilter_FiltersLosslessAndBitrates()
    {
        var service = new LibraryFilterService();

        var flacFile = new AudioFile
        {
            Extension = ".flac",
            Bitrate = 1050,
            QualityAnalysis = new AudioQualityResult { IsLossless = true }
        };

        var mp3File320 = new AudioFile
        {
            Extension = ".mp3",
            Bitrate = 320,
            QualityAnalysis = new AudioQualityResult { IsLossless = false }
        };

        var mp3File128 = new AudioFile
        {
            Extension = ".mp3",
            Bitrate = 128,
            QualityAnalysis = new AudioQualityResult { IsLossless = false }
        };

        // Lossless
        Assert.True(service.Matches(flacFile, new LibraryFilterCriteria { QualityFilter = "Lossless" }));
        Assert.False(service.Matches(mp3File320, new LibraryFilterCriteria { QualityFilter = "Lossless" }));

        // >= 320 kbps
        Assert.True(service.Matches(mp3File320, new LibraryFilterCriteria { QualityFilter = "≥ 320 kbps" }));
        Assert.False(service.Matches(mp3File128, new LibraryFilterCriteria { QualityFilter = "≥ 320 kbps" }));

        // < 320 kbps
        Assert.True(service.Matches(mp3File128, new LibraryFilterCriteria { QualityFilter = "< 320 kbps" }));
        Assert.False(service.Matches(mp3File320, new LibraryFilterCriteria { QualityFilter = "< 320 kbps" }));
    }

    [Fact]
    public void Filter_CombinesMultipleCriteriaCorrectly()
    {
        var service = new LibraryFilterService();

        var file1 = new AudioFile
        {
            FileName = "01. Tess - Shooting Stars.flac",
            Extension = ".flac",
            Artist = "Tess",
            Title = "Shooting Stars",
            Bitrate = 950
        };

        var file2 = new AudioFile
        {
            FileName = "02. Tess - Shooting Stars.mp3",
            Extension = ".mp3",
            Artist = "Tess",
            Title = "Shooting Stars",
            Bitrate = 320
        };

        var file3 = new AudioFile
        {
            FileName = "03. Portex - Don't Switch.flac",
            Extension = ".flac",
            Artist = "Portex",
            Title = "Don't Switch",
            Bitrate = 1020
        };

        var files = new[] { file1, file2, file3 };

        // Filtrar por artista "Tess" AND formato "FLAC"
        var criteria = new LibraryFilterCriteria
        {
            SearchText = "Tess",
            FormatFilter = "FLAC"
        };

        var result = service.Filter(files, criteria);

        Assert.Single(result);
        Assert.Equal("01. Tess - Shooting Stars.flac", result[0].FileName);
    }
}
