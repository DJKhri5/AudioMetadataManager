using AudioMetadataManager.UI.Services.Renaming;
using Xunit;

namespace AudioMetadataManager.Tests.Renaming;

public class SafeFileNameSanitizerTests
{
    private readonly SafeFileNameSanitizer _sanitizer = new();

    [Theory]
    [InlineData("Artist - Title: Subtitle.mp3", ".mp3", "Artist - Title_ Subtitle.mp3")]
    [InlineData("Artist/Band - Track? Name.flac", ".flac", "Artist_Band - Track_ Name.flac")]
    [InlineData("Artist * \"Special\" <Mix> | Edit.mp3", ".mp3", "Artist _ _Special_ _Mix_ _ Edit.mp3")]
    public void Sanitize_ReplacesInvalidCharacters_WithUnderscores(string input, string ext, string expected)
    {
        string result = _sanitizer.Sanitize(input, ext);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Track Name...   ", ".mp3", "Track Name.mp3")]
    [InlineData("Artist - Title.   .flac", ".flac", "Artist - Title.flac")]
    public void Sanitize_TrimsTrailingPeriodsAndSpaces(string input, string ext, string expected)
    {
        string result = _sanitizer.Sanitize(input, ext);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("CON", ".mp3", "_CON_.mp3")]
    [InlineData("AUX.mp3", ".mp3", "_AUX_.mp3")]
    [InlineData("NUL", ".flac", "_NUL_.flac")]
    [InlineData("PRN", ".mp3", "_PRN_.mp3")]
    [InlineData("COM1", ".mp3", "_COM1_.mp3")]
    [InlineData("LPT1", ".mp3", "_LPT1_.mp3")]
    public void Sanitize_GuardsReservedWindowsDeviceNames(string input, string ext, string expected)
    {
        string result = _sanitizer.Sanitize(input, ext);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Sanitize_PreservesNormalizedExtension()
    {
        string result = _sanitizer.Sanitize("Artist - Title", "MP3");
        Assert.Equal("Artist - Title.mp3", result);
    }

    [Fact]
    public void IsValidFileName_ReturnsFalse_ForInvalidWindowsCharacters()
    {
        bool isValid = _sanitizer.IsValidFileName("Artist: Title.mp3", out string error);
        Assert.False(isValid);
        Assert.Contains("caracteres no permitidos", error);
    }

    [Fact]
    public void IsValidFileName_ReturnsTrue_ForCleanName()
    {
        bool isValid = _sanitizer.IsValidFileName("Artist - Title (Extended Mix).mp3", out string error);
        Assert.True(isValid);
        Assert.Empty(error);
    }
}
