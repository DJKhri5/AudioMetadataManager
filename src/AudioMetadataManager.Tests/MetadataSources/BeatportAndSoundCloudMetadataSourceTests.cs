using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Providers;
using Xunit;

namespace AudioMetadataManager.Tests.MetadataSources;

public class BeatportAndSoundCloudMetadataSourceTests
{
    [Fact]
    public async Task BeatportSource_ExtractsMixVersion_Correctly()
    {
        using var source = new BeatportMetadataSource();
        var request = new MetadataSearchRequest
        {
            ParsedArtist = "Armin van Buuren",
            ParsedTitle = "Communication (Extended Mix)"
        };

        var result = await source.SearchAsync(request);

        Assert.NotNull(result);
        Assert.True(result.WasSuccessful);
        Assert.NotEmpty(result.Candidates);
        var candidate = result.Candidates[0];
        Assert.Equal("Armin van Buuren", candidate.Artist);
        Assert.Equal("Communication", candidate.Title);
        Assert.Equal("Extended Mix", candidate.Version);
    }

    [Fact]
    public async Task SoundCloudSource_SetsManualApproval_Flag()
    {
        var source = new SoundCloudMetadataSource();
        Assert.True(source.RequiresManualApproval);

        var request = new MetadataSearchRequest
        {
            ParsedArtist = "DJ Producer",
            ParsedTitle = "Summer Anthem (VIP Bootleg)"
        };

        var result = await source.SearchAsync(request);

        Assert.NotNull(result);
        Assert.True(result.WasSuccessful);
        Assert.NotEmpty(result.Candidates);
        var candidate = result.Candidates[0];
        Assert.Equal("DJ Producer", candidate.Artist);
        Assert.Equal("Summer Anthem", candidate.Title);
        Assert.Equal("VIP Bootleg", candidate.Version);
    }
}
