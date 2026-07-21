using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Providers;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;

namespace AudioMetadataManager.UI.Services.MetadataSources;

/// <summary>
/// Crea y registra las fuentes externas disponibles
/// en Audio Metadata Manager.
/// </summary>
public static class MetadataSourceFactory
{
    /// <summary>
    /// Construye el administrador con todas las plataformas
    /// conocidas por la aplicación.
    ///
    /// Discogs utilizará una configuración predeterminada
    /// sin credenciales cuando no se proporcione una
    /// configuración externa.
    /// </summary>
    public static MetadataSourceManager CreateDefault(
        DiscogsOptions? discogsOptions = null)
    {
        DiscogsOptions effectiveDiscogsOptions =
            discogsOptions ??
            DiscogsOptionsFactory.CreateDefault();

        IReadOnlyList<IMetadataSource> sources =
            new List<IMetadataSource>
            {
                new DiscogsMetadataSource(
                    effectiveDiscogsOptions),

                new BeatportMetadataSource(),

                new SpotifyMetadataSource(),

                new SoundCloudMetadataSource()
            };

        return new MetadataSourceManager(
            sources);
    }
}