using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Agrupa solicitudes individuales de aplicación de metadatos
/// para una futura ejecución productiva por lote.
///
/// Este objeto no modifica archivos ni ejecuta el pipeline.
/// </summary>
public sealed class MetadataApplyBatchRequest
{
    /// <summary>
    /// Identificador único del lote.
    /// </summary>
    public Guid BatchId { get; init; } =
        Guid.NewGuid();

    /// <summary>
    /// Momento UTC en que se creó el lote.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Solicitudes individuales incluidas en el lote.
    /// </summary>
    public IReadOnlyList<MetadataApplyRequest>
        Requests
    { get; init; } =
            Array.Empty<MetadataApplyRequest>();

    /// <summary>
    /// Solicitudes estructuralmente válidas.
    /// </summary>
    public IReadOnlyList<MetadataApplyRequest>
        ValidRequests =>
            Requests
                .Where(request =>
                    request is not null &&
                    request.IsStructurallyValid)
                .ToArray();

    /// <summary>
    /// Rutas normalizadas repetidas dentro del lote.
    /// </summary>
    public IReadOnlyList<string>
        DuplicateFilePaths =>
            ValidRequests
                .Select(request =>
                    Path.GetFullPath(
                        request.FilePath))
                .GroupBy(
                    filePath => filePath,
                    StringComparer.OrdinalIgnoreCase)
                .Where(group =>
                    group.Count() > 1)
                .Select(group =>
                    group.Key)
                .ToArray();

    /// <summary>
    /// Indica si existen archivos repetidos dentro del lote.
    /// </summary>
    public bool HasDuplicateFilePaths =>
        DuplicateFilePaths.Count > 0;

    /// <summary>
    /// Indica si el lote posee información suficiente para una
    /// validación productiva posterior.
    /// </summary>
    public bool IsStructurallyValid =>
        BatchId != Guid.Empty &&
        ValidRequests.Count > 0 &&
        !HasDuplicateFilePaths;

    /// <summary>
    /// Cantidad total de solicitudes recibidas.
    /// </summary>
    public int RequestCount =>
        Requests.Count;

    /// <summary>
    /// Cantidad de solicitudes válidas.
    /// </summary>
    public int ValidRequestCount =>
        ValidRequests.Count;

    /// <summary>
    /// Cantidad total de cambios válidos contenidos en el lote.
    /// </summary>
    public int ValidChangeCount =>
        ValidRequests
            .Sum(request =>
                request.ValidChangeCount);

    /// <summary>
    /// Resumen compacto del lote.
    /// </summary>
    public string Summary =>
        IsStructurallyValid
            ? $"{ValidRequestCount} archivo(s) y " +
              $"{ValidChangeCount} cambio(s) " +
              "preparados para validación por lote."
            : "El lote no contiene solicitudes suficientes, " +
              "válidas y sin rutas duplicadas.";
}