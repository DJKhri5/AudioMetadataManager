using System.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources.Providers.SoundCloud;

namespace AudioMetadataManager.UI.Services.MetadataSources.Configuration;

/// <summary>
/// Servicio de configuración y prueba de conexión para SoundCloud.
/// </summary>
public sealed class SoundCloudConfigurationService : IMetadataSourceConfigurationService
{
    private readonly ProviderTokenStore _tokenStore;

    public SoundCloudConfigurationService()
    {
        _tokenStore = new ProviderTokenStore("SoundCloud");
    }

    public string SourceName => "SoundCloud";

    public MetadataSourceConfigurationResult GetStatus()
    {
        bool hasToken = _tokenStore.HasToken;
        if (hasToken)
        {
            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.Configured,
                "Client ID / Token de SoundCloud guardado de forma segura.");
        }

        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "SoundCloud activo para búsqueda de remixes y bootlegs (Token opcional).");
    }

    public MetadataSourceConfigurationResult SaveCredential(string credential)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                "El token de SoundCloud no puede estar vacío.");
        }

        _tokenStore.SaveToken(credential);
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "Credencial de SoundCloud guardada correctamente.");
    }

    public MetadataSourceConfigurationResult DeleteCredential()
    {
        _tokenStore.DeleteToken();
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.NotConfigured,
            "Credencial de SoundCloud eliminada.");
    }

    public async Task<MetadataSourceConfigurationResult> TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var provider = new SoundCloudMetadataProvider();
            var candidates = await provider.SearchTracksAsync(
                "DJ Producer",
                "Summer Anthem",
                cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();

            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.ConnectionVerified,
                $"Conexión exitosa a SoundCloud ({stopwatch.ElapsedMilliseconds} ms). Servicio disponible.");
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                $"Error al conectar con SoundCloud ({stopwatch.ElapsedMilliseconds} ms): {exception.Message}");
        }
    }
}
