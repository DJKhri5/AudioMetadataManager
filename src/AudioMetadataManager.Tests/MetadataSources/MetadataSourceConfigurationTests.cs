using AudioMetadataManager.UI.Services.MetadataSources.Configuration;
using Xunit;

namespace AudioMetadataManager.Tests.MetadataSources;

public class MetadataSourceConfigurationTests
{
    [Fact]
    public void Registry_ContainsAllFiveProviders()
    {
        var registry = new MetadataSourceConfigurationRegistry();
        var services = registry.GetAllServices();

        Assert.Equal(5, services.Count);
        Assert.NotNull(registry.GetService("MusicBrainz"));
        Assert.NotNull(registry.GetService("Discogs"));
        Assert.NotNull(registry.GetService("Beatport"));
        Assert.NotNull(registry.GetService("Spotify"));
        Assert.NotNull(registry.GetService("SoundCloud"));
    }

    [Fact]
    public void MusicBrainzConfiguration_IsAlwaysConfigured()
    {
        var service = new MusicBrainzConfigurationService();
        var status = service.GetStatus();

        Assert.NotNull(status);
        Assert.Equal("MusicBrainz", status.SourceName);
        Assert.Equal(MetadataSourceConfigurationState.Configured, status.State);
    }

    [Fact]
    public void BeatportConfiguration_SaveAndDeleteToken_WorksCorrectly()
    {
        var service = new BeatportConfigurationService();
        var saveResult = service.SaveCredential("test_beatport_key_12345");
        Assert.True(saveResult.OperationSucceeded);

        var statusAfterSave = service.GetStatus();
        Assert.Equal(MetadataSourceConfigurationState.Configured, statusAfterSave.State);

        var deleteResult = service.DeleteCredential();
        Assert.True(deleteResult.OperationSucceeded);
    }
}
