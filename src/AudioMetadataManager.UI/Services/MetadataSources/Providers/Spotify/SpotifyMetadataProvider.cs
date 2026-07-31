using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Execution;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify;

/// <summary>
/// Punto de entrada del proveedor de metadatos Spotify.
///
/// Valida la configuración y las solicitudes, y delega
/// la ejecución técnica en SpotifySearchExecutor.
/// </summary>
public sealed class SpotifyMetadataProvider : IDisposable
{
    private readonly SpotifyOptions
        _options;

    private readonly SpotifySearchExecutor
        _searchExecutor;

    private readonly bool
        _ownsSearchExecutor;

    private bool
        _disposed;

    /// <summary>
    /// Crea el proveedor y su infraestructura predeterminada.
    /// </summary>
    public SpotifyMetadataProvider(
        SpotifyOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _searchExecutor =
            new SpotifySearchExecutor(
                _options);

        _ownsSearchExecutor =
            true;
    }

    /// <summary>
    /// Crea el proveedor con un ejecutor personalizado.
    /// </summary>
    public SpotifyMetadataProvider(
        SpotifyOptions options,
        SpotifySearchExecutor searchExecutor)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _searchExecutor =
            searchExecutor ??
            throw new ArgumentNullException(
                nameof(searchExecutor));

        _ownsSearchExecutor =
            false;
    }

    /// <summary>
    /// Configuración actualmente utilizada.
    /// </summary>
    public SpotifyOptions Options =>
        _options;

    /// <summary>
    /// Indica si el proveedor tiene configuración y credenciales
    /// suficientes para consultar Spotify.
    /// </summary>
    public bool IsAvailable =>
        _options.IsValid &&
        _options.HasCredentials;

    /// <summary>
    /// Valida y ejecuta una búsqueda en Spotify.
    /// </summary>
    public async Task<SpotifySearchResult> SearchAsync(
        SpotifySearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        cancellationToken.ThrowIfCancellationRequested();

        SpotifySearchResult? validationResult =
            ValidateRequest(
                request);

        if (validationResult is not null)
        {
            return validationResult;
        }

        return await _searchExecutor.ExecuteAsync(
            request,
            cancellationToken);
    }

    private SpotifySearchResult? ValidateRequest(
        SpotifySearchRequest request)
    {
        if (!_options.IsValid)
        {
            return SpotifySearchResult.InvalidConfiguration(
                request,
                "La configuración del proveedor Spotify no es " +
                "válida.");
        }

        if (!request.HasMinimumSearchData)
        {
            return SpotifySearchResult.InvalidRequest(
                request,
                "La búsqueda debe contener al menos Artist o " +
                "Title.");
        }

        if (!_options.HasCredentials)
        {
            return new SpotifySearchResult
            {
                Status =
                    SpotifyProviderStatus.AuthenticationFailed,

                Request =
                    request,

                Message =
                    "El proveedor Spotify requiere un Client ID " +
                    "y un Client Secret."
            };
        }

        return null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsSearchExecutor)
        {
            _searchExecutor.Dispose();
        }

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}
