using AudioMetadataManager.UI.Services.MetadataSources
    .Identification.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Execution;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Identification;

/// <summary>
/// Identifica automáticamente un archivo de audio combinando la
/// huella acústica generada por Chromaprint con la búsqueda de
/// grabaciones en AcoustID.
///
/// No consulta Discogs, Spotify ni ninguna otra fuente de
/// metadatos: solo produce el identificador de grabación (MBID)
/// de MusicBrainz que esas fuentes podrán utilizar más adelante.
/// </summary>
public sealed class AudioIdentificationOrchestrator : IDisposable
{
    private readonly ChromaprintFingerprintExecutor
        _fingerprintExecutor;

    private readonly AcoustIdLookupProvider
        _lookupProvider;

    private readonly bool
        _ownsLookupProvider;

    private bool
        _disposed;

    /// <summary>
    /// Crea el orquestador con la infraestructura predeterminada.
    /// </summary>
    public AudioIdentificationOrchestrator()
    {
        _fingerprintExecutor =
            new ChromaprintFingerprintExecutor(
                new ChromaprintOptions());

        _lookupProvider =
            new AcoustIdLookupProvider(
                AcoustIdOptionsFactory.CreateDefault());

        _ownsLookupProvider =
            true;
    }

    /// <summary>
    /// Crea el orquestador con componentes personalizados.
    ///
    /// Este constructor será útil para pruebas y futura
    /// inyección de dependencias.
    /// </summary>
    public AudioIdentificationOrchestrator(
        ChromaprintFingerprintExecutor fingerprintExecutor,
        AcoustIdLookupProvider lookupProvider)
    {
        _fingerprintExecutor =
            fingerprintExecutor ??
            throw new ArgumentNullException(
                nameof(fingerprintExecutor));

        _lookupProvider =
            lookupProvider ??
            throw new ArgumentNullException(
                nameof(lookupProvider));

        _ownsLookupProvider =
            false;
    }

    /// <summary>
    /// Genera la huella del archivo indicado y, si tiene éxito,
    /// consulta AcoustID para identificar la grabación asociada.
    /// </summary>
    public async Task<AudioIdentificationResult> IdentifyAsync(
        AudioIdentificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentNullException.ThrowIfNull(
            request);

        cancellationToken.ThrowIfCancellationRequested();

        if (!request.HasFilePath)
        {
            return AudioIdentificationResult.InvalidRequest(
                request.FilePath,
                "La solicitud no contiene una ruta de archivo.");
        }

        ChromaprintFingerprintResult fingerprintResult =
            await _fingerprintExecutor.ExecuteAsync(
                new ChromaprintFingerprintRequest
                {
                    FilePath =
                        request.FilePath
                },
                cancellationToken);

        if (!fingerprintResult.IsSuccess)
        {
            return new AudioIdentificationResult
            {
                Status =
                    fingerprintResult.Status ==
                        ChromaprintStatus.Cancelled
                        ? AudioIdentificationStatus.Cancelled
                        : AudioIdentificationStatus.FingerprintFailed,

                FilePath =
                    request.FilePath,

                FingerprintResult =
                    fingerprintResult,

                Message =
                    fingerprintResult.Message
            };
        }

        int durationSeconds =
            (int)Math.Round(
                fingerprintResult.Duration.TotalSeconds);

        AcoustIdLookupResult lookupResult =
            await _lookupProvider.LookupAsync(
                new AcoustIdLookupRequest
                {
                    Fingerprint =
                        fingerprintResult.Fingerprint,

                    DurationSeconds =
                        durationSeconds
                },
                cancellationToken);

        return BuildResult(
            request.FilePath,
            fingerprintResult,
            lookupResult);
    }

    private static AudioIdentificationResult BuildResult(
        string filePath,
        ChromaprintFingerprintResult fingerprintResult,
        AcoustIdLookupResult lookupResult)
    {
        if (!lookupResult.IsSuccess)
        {
            return new AudioIdentificationResult
            {
                Status =
                    AudioIdentificationStatus.LookupFailed,

                FilePath =
                    filePath,

                FingerprintResult =
                    fingerprintResult,

                LookupResult =
                    lookupResult,

                Message =
                    lookupResult.Message
            };
        }

        if (!lookupResult.HasCandidates)
        {
            return new AudioIdentificationResult
            {
                Status =
                    AudioIdentificationStatus.NoMatchFound,

                FilePath =
                    filePath,

                FingerprintResult =
                    fingerprintResult,

                LookupResult =
                    lookupResult,

                Message =
                    "La huella se generó correctamente, pero AcoustID " +
                    "no encontró grabaciones asociadas."
            };
        }

        return new AudioIdentificationResult
        {
            Status =
                AudioIdentificationStatus.Success,

            FilePath =
                filePath,

            FingerprintResult =
                fingerprintResult,

            LookupResult =
                lookupResult,

            Message =
                $"Se identificaron " +
                $"{lookupResult.Candidates.Count} grabación(es) candidata(s)."
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsLookupProvider)
        {
            _lookupProvider.Dispose();
        }

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}
