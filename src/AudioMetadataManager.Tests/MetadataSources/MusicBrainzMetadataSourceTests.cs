using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Providers.MusicBrainz;
using Xunit;

namespace AudioMetadataManager.Tests.MetadataSources;

public class MusicBrainzMetadataSourceTests
{
    [Fact]
    public async Task SearchAsync_EmptyRequest_ReturnsZeroCandidatesSafely()
    {
        using var source = new MusicBrainzMetadataSource();
        var request = new MetadataSearchRequest();

        var result = await source.SearchAsync(request);

        Assert.NotNull(result);
        Assert.Equal("MusicBrainz", result.SourceName);
        Assert.False(result.WasSuccessful);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public void MusicBrainzSource_Properties_AreProperlyConfigured()
    {
        using var source = new MusicBrainzMetadataSource();

        Assert.Equal("MusicBrainz", source.Name);
        Assert.Equal(1, source.Priority);
        Assert.True(source.IsAvailable);
        Assert.False(source.RequiresManualApproval);
    }
}
