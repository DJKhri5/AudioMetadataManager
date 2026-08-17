using AudioMetadataManager.UI.Services.MetadataSources.Providers.Discogs.Configuration;

namespace AudioMetadataManager.UI.Services.MetadataSources.Configuration;

/// <summary>
/// Registro central de servicios de configuración para todas las plataformas externas.
/// </summary>
public sealed class MetadataSourceConfigurationRegistry
{
    private readonly IReadOnlyList<IMetadataSourceConfigurationService> _services;

    public MetadataSourceConfigurationRegistry()
    {
        _services = new List<IMetadataSourceConfigurationService>
        {
            new MusicBrainzConfigurationService(),
            new DiscogsConfigurationService(),
            new BeatportConfigurationService(),
            new SpotifyConfigurationService(),
            new SoundCloudConfigurationService()
        };
    }

    public MetadataSourceConfigurationRegistry(IReadOnlyList<IMetadataSourceConfigurationService> services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IReadOnlyList<IMetadataSourceConfigurationService> GetAllServices() => _services;

    public IMetadataSourceConfigurationService? GetService(string sourceName)
    {
        return _services.FirstOrDefault(s => string.Equals(s.SourceName, sourceName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<MetadataSourceConfigurationResult>> TestAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _services.Select(service => service.TestConnectionAsync(cancellationToken));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results;
    }
}
