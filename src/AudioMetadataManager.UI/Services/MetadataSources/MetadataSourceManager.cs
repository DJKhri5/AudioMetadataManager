using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using System.Diagnostics;

namespace AudioMetadataManager.UI.Services.MetadataSources;

/// <summary>
/// Coordina las búsquedas realizadas en todas las fuentes
/// externas registradas en la aplicación.
/// </summary>
public class MetadataSourceManager
{
    private readonly IReadOnlyList<IMetadataSource> _sources;

    /// <summary>
    /// Recibe las fuentes disponibles y las ordena según
    /// la prioridad definida por cada proveedor.
    /// </summary>
    public MetadataSourceManager(
        IEnumerable<IMetadataSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        _sources = sources
            .OrderBy(source => source.Priority)
            .ToList();
    }

    /// <summary>
    /// Fuentes registradas, ordenadas por prioridad.
    /// </summary>
    public IReadOnlyList<IMetadataSource> Sources =>
        _sources;

    /// <summary>
    /// Fuentes que actualmente están configuradas
    /// y disponibles para realizar búsquedas.
    /// </summary>
    public IReadOnlyList<IMetadataSource> AvailableSources =>
        _sources
            .Where(source => source.IsAvailable)
            .ToList();

    /// <summary>
    /// Ejecuta las búsquedas de forma secuencial,
    /// respetando la prioridad de las plataformas.
    /// </summary>
    public async Task<IReadOnlyList<MetadataSearchResult>> SearchAllAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<MetadataSearchResult> results = new();

        foreach (IMetadataSource source in _sources)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!source.IsAvailable)
            {
                results.Add(
                    CreateUnavailableResult(
                        source,
                        request));

                continue;
            }

            MetadataSearchResult result =
                await SearchSourceSafelyAsync(
                    source,
                    request,
                    cancellationToken);

            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Ejecuta una fuente de manera protegida, evitando que
    /// un error en una plataforma interrumpa las demás.
    /// </summary>
    private static async Task<MetadataSearchResult>
        SearchSourceSafelyAsync(
            IMetadataSource source,
            MetadataSearchRequest request,
            CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            MetadataSearchResult result =
                await source.SearchAsync(
                    request,
                    cancellationToken);

            stopwatch.Stop();

            result.SourceName =
                string.IsNullOrWhiteSpace(result.SourceName)
                    ? source.Name
                    : result.SourceName;

            result.QueryUsed =
                string.IsNullOrWhiteSpace(result.QueryUsed)
                    ? request.PrimaryQuery
                    : result.QueryUsed;

            result.ElapsedTime =
                result.ElapsedTime > TimeSpan.Zero
                    ? result.ElapsedTime
                    : stopwatch.Elapsed;

            return result;
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
                    source.Name,

                Status =
                    MetadataSourceStatus.UnexpectedError,

                QueryUsed =
                    request.PrimaryQuery,

                WasSuccessful =
                    false,

                ErrorMessage =
                    $"Error al consultar {source.Name}: " +
                    exception.Message,

                ElapsedTime =
                    stopwatch.Elapsed,

                RequiresManualApproval =
                    source.RequiresManualApproval
            };
        }
    }

    /// <summary>
    /// Genera un resultado descriptivo cuando una fuente
    /// no está configurada o no puede utilizarse.
    /// </summary>
    private static MetadataSearchResult CreateUnavailableResult(
        IMetadataSource source,
        MetadataSearchRequest request)
    {
        return new MetadataSearchResult
        {
            SourceName =
                source.Name,

            Status =
                MetadataSourceStatus.InvalidConfiguration,

            QueryUsed =
                request.PrimaryQuery,

            WasSuccessful =
                false,

            ErrorMessage =
                $"{source.Name} no está disponible o configurado.",

            ElapsedTime =
                TimeSpan.Zero,

            RequiresManualApproval =
                source.RequiresManualApproval
        };
    }
}