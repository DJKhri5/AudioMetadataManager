using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Execution;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId;

/// <summary>
/// Punto de entrada del proveedor de identificación AcoustID.
///
/// Valida la configuración y las solicitudes, y delega
/// la ejecución técnica en AcoustIdLookupExecutor.
/// </summary>
public sealed class AcoustIdLookupProvider : IDisposable
{
    private readonly AcoustIdOptions
        _options;

    private readonly AcoustIdLookupExecutor
        _lookupExecutor;

    private readonly bool
        _ownsLookupExecutor;

    private bool
        _disposed;

    /// <summary>
    /// Crea el proveedor y su infraestructura predeterminada.
    /// </summary>
    public AcoustIdLookupProvider(
        AcoustIdOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _lookupExecutor =
            new AcoustIdLookupExecutor(
                _options);

        _ownsLookupExecutor =
            true;
    }

    /// <summary>
    /// Crea el proveedor con un ejecutor personalizado.
    /// </summary>
    public AcoustIdLookupProvider(
        AcoustIdOptions options,
        AcoustIdLookupExecutor lookupExecutor)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _lookupExecutor =
            lookupExecutor ??
            throw new ArgumentNullException(
                nameof(lookupExecutor));

        _ownsLookupExecutor =
            false;
    }

    /// <summary>
    /// Configuración actualmente utilizada.
    /// </summary>
    public AcoustIdOptions Options =>
        _options;

    /// <summary>
    /// Indica si el proveedor tiene configuración y clave
    /// suficientes para consultar AcoustID.
    /// </summary>
    public bool IsAvailable =>
        _options.IsValid &&
        _options.HasClientApiKey;

    /// <summary>
    /// Valida y ejecuta una identificación de huella acústica.
    /// </summary>
    public async Task<AcoustIdLookupResult> LookupAsync(
        AcoustIdLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        cancellationToken.ThrowIfCancellationRequested();

        AcoustIdLookupResult? validationResult =
            ValidateRequest(
                request);

        if (validationResult is not null)
        {
            return validationResult;
        }

        return await _lookupExecutor.ExecuteAsync(
            request,
            cancellationToken);
    }

    private AcoustIdLookupResult? ValidateRequest(
        AcoustIdLookupRequest request)
    {
        if (!_options.IsValid)
        {
            return AcoustIdLookupResult.InvalidConfiguration(
                request,
                "La configuración del proveedor AcoustID no es válida.");
        }

        if (!request.HasMinimumData)
        {
            return AcoustIdLookupResult.InvalidRequest(
                request,
                "La solicitud debe contener una huella y una " +
                "duración mayor que cero.");
        }

        if (!_options.HasClientApiKey)
        {
            return new AcoustIdLookupResult
            {
                Status =
                    AcoustIdProviderStatus.AuthenticationFailed,

                Request =
                    request,

                Message =
                    "El proveedor AcoustID requiere una clave de cliente."
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

        if (_ownsLookupExecutor)
        {
            _lookupExecutor.Dispose();
        }

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}
