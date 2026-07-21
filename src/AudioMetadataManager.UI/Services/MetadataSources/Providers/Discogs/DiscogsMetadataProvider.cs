using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Execution;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs;

/// <summary>
/// Punto de entrada del proveedor de metadatos Discogs.
///
/// Valida la configuración y las solicitudes, y delega
/// la ejecución técnica en DiscogsSearchExecutor.
/// </summary>
public sealed class DiscogsMetadataProvider : IDisposable
{
    private readonly DiscogsOptions
        _options;

    private readonly DiscogsSearchExecutor
        _searchExecutor;

    private readonly bool
        _ownsSearchExecutor;

    private bool
        _disposed;

    /// <summary>
    /// Crea el proveedor y su infraestructura predeterminada.
    /// </summary>
    public DiscogsMetadataProvider(
        DiscogsOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _searchExecutor =
            new DiscogsSearchExecutor(
                _options);

        _ownsSearchExecutor =
            true;
    }

    /// <summary>
    /// Crea el proveedor con un ejecutor personalizado.
    /// </summary>
    public DiscogsMetadataProvider(
        DiscogsOptions options,
        DiscogsSearchExecutor searchExecutor)
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
    public DiscogsOptions Options =>
        _options;

    /// <summary>
    /// Valida y ejecuta una búsqueda en Discogs.
    /// </summary>
    public async Task<DiscogsSearchResult> SearchAsync(
        DiscogsSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        cancellationToken.ThrowIfCancellationRequested();

        DiscogsSearchResult? validationResult =
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

    private DiscogsSearchResult? ValidateRequest(
        DiscogsSearchRequest request)
    {
        if (!_options.IsValid)
        {
            return DiscogsSearchResult.InvalidConfiguration(
                request,
                "La configuración del proveedor Discogs no es válida.");
        }

        if (!request.HasMinimumSearchData)
        {
            return DiscogsSearchResult.InvalidRequest(
                request,
                "La búsqueda debe contener al menos Artist o Title.");
        }

        if (request.Page <= 0)
        {
            return DiscogsSearchResult.InvalidRequest(
                request,
                "El número de página debe ser mayor que cero.");
        }

        if (request.ResultsPerPage is <= 0 or > 100)
        {
            return DiscogsSearchResult.InvalidRequest(
                request,
                "La cantidad de resultados debe estar entre 1 y 100.");
        }

        if (!_options.HasUserToken)
        {
            return new DiscogsSearchResult
            {
                Status =
                    DiscogsProviderStatus.AuthenticationRequired,

                Request =
                    request,

                Message =
                    "El proveedor Discogs requiere un token de usuario."
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