using System.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources.Providers.Beatport;

namespace AudioMetadataManager.UI.Services.MetadataSources.Configuration;

/// <summary>
/// Servicio de configuración y prueba de conexión para Beatport.
/// </summary>
public sealed class BeatportConfigurationService : IMetadataSourceConfigurationService
{
    private readonly ProviderTokenStore _tokenStore;

    public BeatportConfigurationService()
    {
        _tokenStore = new ProviderTokenStore("Beatport");
    }

    public string SourceName => "Beatport";

    public MetadataSourceConfigurationResult GetStatus()
    {
        bool hasToken = _tokenStore.HasToken;
        if (hasToken)
        {
            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.Configured,
                "API Key / Token de Beatport guardado de forma segura.");
        }

        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "Beatport activo en modo de búsqueda estándar (API Key opcional).");
    }

    public MetadataSourceConfigurationResult SaveCredential(string credential)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                "El token de Beatport no puede estar vacío.");
        }

        _tokenStore.SaveToken(credential);
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "Credencial de Beatport guardada correctamente.");
    }

    public MetadataSourceConfigurationResult DeleteCredential()
    {
        _tokenStore.DeleteToken();
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.NotConfigured,
            "Credencial de Beatport eliminada.");
    }

    public async Task<MetadataSourceConfigurationResult> TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var provider = new BeatportMetadataProvider();
            var candidates = await provider.SearchTracksAsync(
                "Armin van Buuren",
                "Communication",
                cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();

            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.ConnectionVerified,
                $"Conexión exitosa a Beatport ({stopwatch.ElapsedMilliseconds} ms). Servicio disponible.");
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                $"Error al conectar con Beatport ({stopwatch.ElapsedMilliseconds} ms): {exception.Message}");
        }
    }
}
