using System.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources.Providers.MusicBrainz;

namespace AudioMetadataManager.UI.Services.MetadataSources.Configuration;

/// <summary>
/// Servicio de configuración y diagnóstico de conexión para MusicBrainz.
/// MusicBrainz opera como una API abierta sin requerir clave de API.
/// </summary>
public sealed class MusicBrainzConfigurationService : IMetadataSourceConfigurationService
{
    public string SourceName => "MusicBrainz";

    public MetadataSourceConfigurationResult GetStatus()
    {
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "Conexión activa a la API pública abierta de MusicBrainz (no requiere clave de API).");
    }

    public MetadataSourceConfigurationResult SaveCredential(string credential)
    {
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "MusicBrainz es una API pública y no requiere almacenar credenciales.");
    }

    public MetadataSourceConfigurationResult DeleteCredential()
    {
        return MetadataSourceConfigurationResult.Success(
            SourceName,
            MetadataSourceConfigurationState.Configured,
            "MusicBrainz permanece disponible como API pública abierta.");
    }

    public async Task<MetadataSourceConfigurationResult> TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var provider = new MusicBrainzMetadataProvider();
            var candidates = await provider.SearchRecordingsAsync(
                "Tiësto",
                "Adagio for Strings",
                cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();

            if (candidates.Count > 0)
            {
                return MetadataSourceConfigurationResult.Success(
                    SourceName,
                    MetadataSourceConfigurationState.ConnectionVerified,
                    $"Conexión exitosa a MusicBrainz ({stopwatch.ElapsedMilliseconds} ms). Se recibieron {candidates.Count} resultado(s).");
            }

            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.ConnectionVerified,
                $"Conexión exitosa a MusicBrainz ({stopwatch.ElapsedMilliseconds} ms). Servicio disponible.");
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                $"Error al conectar con MusicBrainz ({stopwatch.ElapsedMilliseconds} ms): {exception.Message}");
        }
    }
}
