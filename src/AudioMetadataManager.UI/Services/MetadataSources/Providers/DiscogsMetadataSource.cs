using System.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Mapping;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers;

/// <summary>
/// Adaptador de Discogs para el sistema genérico de fuentes
/// externas de metadatos.
///
/// Convierte solicitudes y resultados entre el contrato común
/// y los modelos específicos del módulo Discogs.
/// </summary>
public sealed class DiscogsMetadataSource
    : IMetadataSource, IDisposable
{
    private readonly DiscogsMetadataProvider
        _provider;

    private readonly DiscogsMetadataSearchResultMapper
        _resultMapper;

    private readonly bool
        _ownsProvider;

    private bool
        _disposed;

    /// <summary>
    /// Crea la fuente utilizando una configuración de Discogs.
    /// </summary>
    public DiscogsMetadataSource(
        DiscogsOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        _provider =
            new DiscogsMetadataProvider(
                options);

        _resultMapper =
            new DiscogsMetadataSearchResultMapper();

        _ownsProvider =
            true;
    }

    /// <summary>
    /// Crea la fuente con componentes personalizados.
    /// </summary>
    public DiscogsMetadataSource(
        DiscogsMetadataProvider provider,
        DiscogsMetadataSearchResultMapper resultMapper)
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
        "Discogs";

    public int Priority =>
        1;

    public bool IsAvailable =>
        _provider.Options.IsValid &&
        _provider.Options.HasUserToken;

    public bool RequiresManualApproval =>
        false;

    /// <summary>
    /// Convierte la solicitud común, consulta Discogs y devuelve
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

        DiscogsSearchRequest discogsRequest =
            CreateDiscogsRequest(
                request);

        try
        {
            DiscogsSearchResult discogsResult =
                await _provider.SearchAsync(
                    discogsRequest,
                    cancellationToken);

            stopwatch.Stop();

            return _resultMapper.Map(
                discogsResult,
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
                    discogsRequest.SearchDisplay,

                WasSuccessful =
                    false,

                ErrorMessage =
                    $"Error inesperado al consultar Discogs: " +
                    $"{exception.Message}",

                ElapsedTime =
                    stopwatch.Elapsed,

                RequiresManualApproval =
                    RequiresManualApproval
            };
        }
    }

    private static DiscogsSearchRequest CreateDiscogsRequest(
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

        string version =
            request.HasParsedIdentity
                ? request.ParsedVersion
                : string.Empty;

        string album =
            request.TaggedAlbum;

        int? year =
            request.TaggedYear > 0 &&
            request.TaggedYear <= int.MaxValue
                ? (int)request.TaggedYear
                : null;

        return new DiscogsSearchRequest
        {
            Artist =
                artist,

            Title =
                title,

            Version =
                version,

            Album =
                album,

            Year =
                year,

            Page =
                1
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