using System.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources.Providers.Spotify;

namespace AudioMetadataManager.UI.Services.MetadataSources.Configuration;

/// <summary>
/// Servicio de configuración y prueba de conexión para Spotify.
/// </summary>
public sealed class SpotifyConfigurationService : IMetadataSourceConfigurationService
{
    private readonly ProviderTokenStore _tokenStore;

    public SpotifyConfigurationService()
    {
        _tokenStore = new ProviderTokenStore("Spotify");
    }

    public string SourceName => "Spotify";

    public MetadataSourceConfigurationResult GetStatus()
    {
        bool hasToken = _tokenStore.HasToken;
        if (hasToken)
        {
            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.Configured,
                "Credenciales de Spotify API configuradas de forma segura.");
        }

        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "Spotify activo en modo de catálogo de streaming (Credenciales opcionales).");
    }

    public MetadataSourceConfigurationResult SaveCredential(string credential)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                "Las credenciales de Spotify no pueden estar vacías.");
        }

        _tokenStore.SaveToken(credential);
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "Credenciales de Spotify guardadas correctamente.");
    }

    public MetadataSourceConfigurationResult DeleteCredential()
    {
        _tokenStore.DeleteToken();
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.NotConfigured,
            "Credenciales de Spotify eliminadas.");
    }

    public async Task<MetadataSourceConfigurationResult> TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var provider = new SpotifyMetadataProvider();
            var candidates = await provider.SearchTracksAsync(
                "Daft Punk",
                "One More Time",
                cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();

            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.ConnectionVerified,
                $"Conexión exitosa a Spotify ({stopwatch.ElapsedMilliseconds} ms). Servicio disponible.");
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                $"Error al conectar con Spotify ({stopwatch.ElapsedMilliseconds} ms): {exception.Message}");
        }
    }
}
