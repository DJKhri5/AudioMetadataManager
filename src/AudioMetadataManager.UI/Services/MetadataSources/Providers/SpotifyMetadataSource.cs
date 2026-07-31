using System.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Mapping;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers;

/// <summary>
/// Adaptador de Spotify para el sistema genérico de fuentes
/// externas de metadatos.
///
/// Convierte solicitudes y resultados entre el contrato común
/// y los modelos específicos del módulo Spotify.
/// </summary>
public sealed class SpotifyMetadataSource
    : IMetadataSource, IDisposable
{
    private readonly SpotifyMetadataProvider
        _provider;

    private readonly SpotifyMetadataSearchResultMapper
        _resultMapper;

    private readonly bool
        _ownsProvider;

    private bool
        _disposed;

    /// <summary>
    /// Crea la fuente utilizando una configuración de Spotify.
    /// </summary>
    public SpotifyMetadataSource(
        SpotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _provider =
            new SpotifyMetadataProvider(
                options);

        _resultMapper =
            new SpotifyMetadataSearchResultMapper();

        _ownsProvider =
            true;
    }

    /// <summary>
    /// Crea la fuente con componentes personalizados.
    /// </summary>
    public SpotifyMetadataSource(
        SpotifyMetadataProvider provider,
        SpotifyMetadataSearchResultMapper resultMapper)
    {
        _provider =
            provider ??
            throw new ArgumentNullException(
                nameof(provider));

        _resultMapper =
            resultMapper ??
            throw new ArgumentNullException(
                nameof(resultMapper));

        _ownsProvider =
            false;
    }

    public string Name =>
        "Spotify";

    public int Priority =>
        3;

    public bool IsAvailable =>
        _provider.Options.IsValid &&
        _provider.Options.HasCredentials;

    public bool RequiresManualApproval =>
        false;

    /// <summary>
    /// Convierte la solicitud común, consulta Spotify y devuelve
    /// un resultado normalizado para MetadataSourceManager.
    /// </summary>
    public async Task<MetadataSearchResult> SearchAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        cancellationToken.ThrowIfCancellationRequested();

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        SpotifySearchRequest spotifyRequest =
            CreateSpotifyRequest(
                request);

        try
        {
            SpotifySearchResult spotifyResult =
                await _provider.SearchAsync(
                    spotifyRequest,
                    cancellationToken);

            stopwatch.Stop();

            return _resultMapper.Map(
                spotifyResult,
                request,
                stopwatch.Elapsed,
                RequiresManualApproval);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            return new MetadataSearchResult
            {
                SourceName =
                    Name,

                Status =
                    MetadataSourceStatus.UnexpectedError,

                QueryUsed =
                    spotifyRequest.SearchDisplay,

                WasSuccessful =
                    false,

                ErrorMessage =
                    $"Error inesperado al consultar Spotify: " +
                    $"{exception.Message}",

                ElapsedTime =
                    stopwatch.Elapsed,

                RequiresManualApproval =
                    RequiresManualApproval
            };
        }
    }

    private static SpotifySearchRequest CreateSpotifyRequest(
        MetadataSearchRequest request)
    {
        string artist =
            SelectPreferredValue(
                request.ParsedArtist,
                request.TaggedArtist);

        string title =
            SelectPreferredValue(
                request.ParsedTitle,
                request.TaggedTitle);

        string album =
            request.TaggedAlbum;

        return new SpotifySearchRequest
        {
            Artist =
                artist,

            Title =
                title,

            Album =
                album
        };
    }

    private static string SelectPreferredValue(
        string primaryValue,
        string fallbackValue)
    {
        if (!string.IsNullOrWhiteSpace(
                primaryValue))
        {
            return primaryValue.Trim();
        }

        return string.IsNullOrWhiteSpace(
                fallbackValue)
                    ? string.Empty
                    : fallbackValue.Trim();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsProvider)
        {
            _provider.Dispose();
        }

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}
