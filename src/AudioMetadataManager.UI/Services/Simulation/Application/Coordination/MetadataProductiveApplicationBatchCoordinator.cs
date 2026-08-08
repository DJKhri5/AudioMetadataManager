using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;

/// <summary>
/// Coordina de forma controlada una futura aplicación productiva
/// compuesta por múltiples solicitudes individuales.
///
/// Esta primera implementación establece únicamente la
/// infraestructura del coordinador por lote.
///
/// Todavía no ejecuta solicitudes productivas individuales.
/// </summary>
public sealed class MetadataProductiveApplicationBatchCoordinator :
    IMetadataProductiveApplicationBatchCoordinator
{
    private readonly IMetadataProductiveApplicationCoordinator
        _individualCoordinator;

    /// <summary>
    /// Crea el coordinador por lote utilizando el coordinador
    /// productivo individual predeterminado.
    /// </summary>
    public MetadataProductiveApplicationBatchCoordinator()
        : this(
            new MetadataProductiveApplicationCoordinator())
    {
    }

    /// <summary>
    /// Crea el coordinador por lote con el coordinador individual
    /// proporcionado.
    /// </summary>
    public MetadataProductiveApplicationBatchCoordinator(
        IMetadataProductiveApplicationCoordinator
            individualCoordinator)
    {
        _individualCoordinator =
            individualCoordinator ??
            throw new ArgumentNullException(
                nameof(individualCoordinator));
    }

    /// <inheritdoc />
    public async Task<MetadataApplyBatchResult>
            ExecuteAsync(
            MetadataApplyBatchRequest batchRequest,
            MetadataPromotionDecision promotionDecision,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            batchRequest);

        cancellationToken.ThrowIfCancellationRequested();

        if (promotionDecision !=
                MetadataPromotionDecision.Declined &&
            promotionDecision !=
                MetadataPromotionDecision.Approved)
        {
            throw new InvalidOperationException(
                "La ejecución productiva por lote solo admite " +
                "las decisiones Declined o Approved.");
        }

        DateTime startedAtUtc =
            DateTime.UtcNow;

        List<string> messages =
            new();

        if (!batchRequest.IsStructurallyValid)
        {
            messages.Add(
                "El lote fue rechazado porque no superó " +
                "las comprobaciones estructurales.");

            return
                new MetadataApplyBatchResult
                {
                    BatchId =
                        batchRequest.BatchId,

                    StartedAtUtc =
                        startedAtUtc,

                    FinishedAtUtc =
                        DateTime.UtcNow,

                    Results =
                        Array.Empty<
                            MetadataProductiveApplicationResult>(),

                    Messages =
                        messages
                };
        }

        messages.Add(
            $"El lote contiene " +
            $"{batchRequest.ValidRequests.Count} " +
            "solicitud(es) válida(s) para procesamiento.");

        List<MetadataProductiveApplicationResult>
            productiveResults =
                new();

        int inspectedRequestCount =
            0;

        foreach (MetadataApplyRequest request
            in batchRequest.ValidRequests)
        {
            cancellationToken.ThrowIfCancellationRequested();

            inspectedRequestCount++;

            messages.Add(
                $"Solicitud {inspectedRequestCount}: " +
                $"{request.FilePath}");

            try
            {
                MetadataProductiveApplicationResult
                    preparedResult =
                        await _individualCoordinator
                            .PrepareAsync(
                                request,
                                cancellationToken);

                if (!string.IsNullOrWhiteSpace(
                        preparedResult.ErrorMessage))
                {
                    productiveResults.Add(
                        preparedResult);

                    messages.Add(
                        $"La solicitud {inspectedRequestCount} " +
                        "falló durante la preparación: " +
                        preparedResult.ErrorMessage);

                    break;
                }

                MetadataProductiveApplicationResult
                    completedResult =
                        await _individualCoordinator
                            .CompleteAsync(
                                preparedResult,
                                promotionDecision,
                                cancellationToken);

                productiveResults.Add(
                    completedResult);

                if (!string.IsNullOrWhiteSpace(
                        completedResult.ErrorMessage))
                {
                    messages.Add(
                        $"La solicitud {inspectedRequestCount} " +
                        "falló durante la finalización: " +
                        completedResult.ErrorMessage);

                    break;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                MetadataProductiveApplicationResult
                    failedResult =
                        new()
                        {
                            ErrorMessage =
                                exception.Message,

                            Messages =
                                new[]
                                {
                            "La coordinación individual produjo " +
                            "una excepción no controlada: " +
                            exception.Message
                                }
                        };

                productiveResults.Add(
                    failedResult);

                messages.Add(
                    $"La solicitud {inspectedRequestCount} produjo " +
                    "una excepción no controlada. El lote se detuvo.");

                break;
            }
        }

        messages.Add(
            $"Se inspeccionaron " +
            $"{inspectedRequestCount} " +
            "solicitud(es) válidas.");

        messages.Add(
            "La ejecución productiva individual continúa " +
            "deshabilitada en esta etapa.");

        int notExecutedRequestCount =
            batchRequest.ValidRequests.Count -
            inspectedRequestCount;

        if (notExecutedRequestCount > 0)
        {
            messages.Add(
                $"{notExecutedRequestCount} solicitud(es) no fueron " +
                "ejecutadas porque el lote se detuvo anticipadamente.");
        }

        MetadataApplyBatchResult result =
            new()
            {
                BatchId =
                    batchRequest.BatchId,

                StartedAtUtc =
                    startedAtUtc,

                FinishedAtUtc =
                    DateTime.UtcNow,

                Results =
                    productiveResults,

                Messages =
                    messages
            };

        return
            result;
    }
}