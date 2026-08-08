using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Consolida el resultado de una futura ejecución productiva
/// compuesta por múltiples solicitudes individuales.
///
/// Este modelo no ejecuta operaciones por sí mismo.
/// Su responsabilidad es conservar identidad, estado,
/// resultados individuales, mensajes y tiempos del lote.
/// </summary>
public sealed class MetadataApplyBatchResult
{
    /// <summary>
    /// Identificador único del lote.
    /// </summary>
    public Guid BatchId { get; init; }

    /// <summary>
    /// Momento de inicio del procesamiento del lote.
    /// </summary>
    public DateTime StartedAtUtc { get; init; }

    /// <summary>
    /// Momento de finalización del procesamiento del lote.
    /// </summary>
    public DateTime FinishedAtUtc { get; init; }

    /// <summary>
    /// Resultados individuales producidos por las solicitudes
    /// procesadas dentro del lote.
    /// </summary>
    public IReadOnlyList<MetadataProductiveApplicationResult>
        Results
    { get; init; } =
            Array.Empty<MetadataProductiveApplicationResult>();

    /// <summary>
    /// Mensajes auditables asociados al procesamiento del lote.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Cantidad total de resultados individuales registrados.
    /// </summary>
    public int TotalCount =>
        Results.Count;

    /// <summary>
    /// Cantidad de resultados que terminaron correctamente.
    /// </summary>
    public int SuccessfulCount =>
        Results.Count(
            result =>
                result.WasSuccessfullyPromoted);

    /// <summary>
    /// Cantidad de resultados que no terminaron correctamente.
    /// </summary>
    public int FailedCount =>
        Results.Count(
            result =>
                !result.WasSuccessfullyPromoted);

    /// <summary>
    /// Indica si todos los resultados individuales fueron
    /// completados correctamente.
    /// </summary>
    public bool WasSuccessful =>
        TotalCount > 0 &&
        FailedCount == 0;

    /// <summary>
    /// Duración total registrada para el lote.
    /// </summary>
    public TimeSpan Duration =>
        FinishedAtUtc >= StartedAtUtc
            ? FinishedAtUtc - StartedAtUtc
            : TimeSpan.Zero;

    /// <summary>
    /// Resumen compacto del resultado por lote.
    /// </summary>
    public string Summary =>
        WasSuccessful
            ? $"{SuccessfulCount} de {TotalCount} solicitud(es) " +
              "terminaron correctamente."
            : $"{SuccessfulCount} de {TotalCount} solicitud(es) " +
              $"terminaron correctamente y {FailedCount} " +
              "presentaron un resultado no satisfactorio.";
}