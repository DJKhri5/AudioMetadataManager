using AudioMetadataManager.Services;
namespace AudioMetadataManager.Tests;
public sealed class FileNameParserTests
{
    [Fact] public void ParsesExampleAndPreservesConnectors() { var x = FileNameParser.Parse("Armin van Buuren & W&W - Late Checkout (Will Atkinson Remix).flac"); Assert.Equal("Armin van Buuren & W&W", x.Artist); Assert.Equal("Late Checkout", x.Title); Assert.Equal("Will Atkinson Remix", x.Version); }
    [Fact] public void RemovesTrackAndNoise() { var x = FileNameParser.Parse("01. Alex Di Stefano - Black Machina (Original Mix) 4clubbers.pl .mp3"); Assert.Equal("Alex Di Stefano", x.Artist); Assert.Contains("Black Machina", x.Title); Assert.DoesNotContain("4clubbers", x.CleanStem, StringComparison.OrdinalIgnoreCase); }
    [Fact] public void KeepsFeatVsAndX() { Assert.Contains("feat.", FileNameParser.Parse("A feat. B - Track.mp3").Artist); Assert.Contains("vs", FileNameParser.Parse("A vs B - Track.mp3").Artist); Assert.Contains("x", FileNameParser.Parse("A x B - Track.mp3").Artist); }
}
