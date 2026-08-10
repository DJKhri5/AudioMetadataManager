using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;

/// <summary>
/// Coordina una aplicación productiva por lote en dos fases:
///
/// 1. preparación verificada de todos los archivos;
/// 2. decisión global Approved o Declined.
/// </summary>
public sealed class MetadataProductiveTwoPhaseBatchCoordinator :
    IMetadataProductiveTwoPhaseBatchCoordinator
{
    private readonly IMetadataProductiveApplicationCoordinator
        _individualCoordinator;

    public MetadataProductiveTwoPhaseBatchCoordinator()
        : this(
            new MetadataProductiveApplicationCoordinator())
    {
    }

    public MetadataProductiveTwoPhaseBatchCoordinator(
        IMetadataProductiveApplicationCoordinator
            individualCoordinator)
    {
        _individualCoordinator =
            individualCoordinator ??
            throw new ArgumentNullException(
                nameof(individualCoordinator));
    }

    /// <inheritdoc />
    public async Task<MetadataProductiveBatchPreparationResult>
        PrepareAsync(
            MetadataApplyBatchRequest batchRequest,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            batchRequest);

        cancellationToken.ThrowIfCancellationRequested();

        DateTime startedAtUtc =
            DateTime.UtcNow;

        List<string> messages =
            new();

        List<MetadataProductiveApplicationResult>
            preparationResults =
                new();

        if (!batchRequest.IsStructurallyValid)
        {
            messages.Add(
                "El lote no superó las comprobaciones " +
                "estructurales y no será preparado.");

            return
                new MetadataProductiveBatchPreparationResult
                {
                    BatchId =
                        batchRequest.BatchId,

                    StartedAtUtc =
                        startedAtUtc,

                    FinishedAtUtc =
                        DateTime.UtcNow,

                    RequestedCount =
                        batchRequest.ValidRequestCount,

                    PreparationResults =
                        preparationResults,

                    Messages =
                        messages
                };
        }

        bool preparationFailed =
            false;

        foreach (MetadataApplyRequest request
            in batchRequest.ValidRequests)
        {
            MetadataProductiveApplicationResult
                preparedResult;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                preparedResult =
                    await _individualCoordinator
                        .PrepareAsync(
                            request,
                            cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await CleanupPendingPreparationsAsync(
                    preparationResults,
                    messages);

                throw;
            }
            catch (Exception exception)
            {
                messages.Add(
                    "La preparación individual produjo una " +
                    "excepción no controlada: " +
                    exception.Message);

                preparationFailed =
                    true;

                break;
            }

            preparationResults.Add(
                preparedResult);

            if (!preparedResult.VerifiedCopyWasPrepared ||
                preparedResult.PromotionDecision !=
                    MetadataPromotionDecision.Pending ||
                !string.IsNullOrWhiteSpace(
                    preparedResult.ErrorMessage))
            {
                messages.Add(
                    $"La preparación de '{request.FileName}' " +
                    "no produjo una copia verificada pendiente.");

                preparationFailed =
                    true;

                break;
            }

            messages.Add(
                $"Preparado correctamente: {request.FileName}.");
        }

        if (preparationFailed)
        {
            await CleanupPendingPreparationsAsync(
                preparationResults,
                messages);

            return
                new MetadataProductiveBatchPreparationResult
                {
                    BatchId =
                        batchRequest.BatchId,

                    StartedAtUtc =
                        startedAtUtc,

                    FinishedAtUtc =
                        DateTime.UtcNow,

                    RequestedCount =
                        batchRequest.ValidRequestCount,

                    PreparationResults =
                        preparationResults,

                    Messages =
                        messages,

                    WasAbortedAndCleanedUp =
                        true
                };
        }

        messages.Add(
            "Todas las solicitudes válidas fueron preparadas " +
            "sin modificar sus archivos originales.");

        return
            new MetadataProductiveBatchPreparationResult
            {
                BatchId =
                    batchRequest.BatchId,

                StartedAtUtc =
                    startedAtUtc,

                FinishedAtUtc =
                    DateTime.UtcNow,

                RequestedCount =
                    batchRequest.ValidRequestCount,

                PreparationResults =
                    preparationResults,

                Messages =
                    messages
            };
    }

    /// <inheritdoc />
    public async Task<MetadataProductiveBatchCompletionResult>
        CompleteAsync(
            MetadataProductiveBatchPreparationResult
                preparationResult,
            MetadataPromotionDecision promotionDecision,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            preparationResult);

        if (promotionDecision is not
            MetadataPromotionDecision.Approved and not
            MetadataPromotionDecision.Declined)
        {
            throw new ArgumentOutOfRangeException(
                nameof(promotionDecision),
                promotionDecision,
                "La finalización batch solamente acepta " +
                "Approved o Declined.");
        }

        if (!preparationResult.IsReadyForDecision)
        {
            throw new InvalidOperationException(
                "El lote preparado no se encuentra listo para " +
                "recibir una decisión global.");
        }

        DateTime startedAtUtc =
            DateTime.UtcNow;

        List<string> messages =
            preparationResult.Messages.ToList();

        List<MetadataProductiveApplicationResult>
            decisionResults =
                new();

        List<MetadataProductiveApplicationResult>
            cleanupResults =
                new();

        IReadOnlyList<MetadataProductiveApplicationResult>
            preparations =
                preparationResult.PreparationResults;

        for (int index = 0;
            index < preparations.Count;
            index++)
        {
            MetadataProductiveApplicationResult
                preparedResult =
                    preparations[index];

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                MetadataProductiveApplicationResult
                    completedResult =
                        await _individualCoordinator
                            .CompleteAsync(
                                preparedResult,
                                promotionDecision,
                                cancellationToken);

                decisionResults.Add(
                    completedResult);

                bool decisionWasSuccessful =
                    promotionDecision ==
                        MetadataPromotionDecision.Approved
                        ? completedResult.WasSuccessfullyPromoted
                        : completedResult.WasSafelyDeclined;

                if (!decisionWasSuccessful)
                {
                    messages.Add(
                        $"La finalización de la preparación " +
                        $"{index + 1} no terminó correctamente " +
                        $"con la decisión {promotionDecision}.");

                    await CleanupRemainingPreparationsAsync(
                        preparations,
                        index + 1,
                        cleanupResults,
                        messages);

                    break;
                }

                messages.Add(
                    $"Preparación {index + 1} finalizada " +
                    $"correctamente con {promotionDecision}.");
            }
            catch (OperationCanceledException)
            {
                messages.Add(
                    "La finalización batch fue cancelada. " +
                    "Las preparaciones todavía pendientes serán " +
                    "descartadas.");

                await CleanupRemainingPreparationsAsync(
                    preparations,
                    index,
                    cleanupResults,
                    messages);

                throw;
            }
            catch (Exception exception)
            {
                decisionResults.Add(
                    new MetadataProductiveApplicationResult
                    {
                        PromotionDecision =
                            promotionDecision,

                        ErrorMessage =
                            exception.Message,

                        Messages =
                            new[]
                            {
                                "La finalización individual produjo " +
                                "una excepción no controlada: " +
                                exception.Message
                            }
                    });

                messages.Add(
                    $"La preparación {index + 1} produjo una " +
                    "excepción durante la finalización. " +
                    "Las preparaciones pendientes serán descartadas.");

                await CleanupRemainingPreparationsAsync(
                    preparations,
                    index,
                    cleanupResults,
                    messages);

                break;
            }
        }

        return
            new MetadataProductiveBatchCompletionResult
            {
                BatchId =
                    preparationResult.BatchId,

                PromotionDecision =
                    promotionDecision,

                StartedAtUtc =
                    startedAtUtc,

                FinishedAtUtc =
                    DateTime.UtcNow,

                RequestedCount =
                    preparationResult.RequestedCount,

                DecisionResults =
                    decisionResults,

                CleanupResults =
                    cleanupResults,

                Messages =
                    messages
            };
    }

    /// <summary>
    /// Descarta de forma segura todas las preparaciones
    /// verificadas que todavía permanecen pendientes.
    /// </summary>
    private async Task CleanupPendingPreparationsAsync(
        IReadOnlyList<MetadataProductiveApplicationResult>
            preparationResults,
        List<string> messages)
    {
        List<MetadataProductiveApplicationResult>
            ignoredCleanupResults =
                new();

        await CleanupRemainingPreparationsAsync(
            preparationResults,
            0,
            ignoredCleanupResults,
            messages);
    }

    /// <summary>
    /// Finaliza con Declined las preparaciones pendientes a
    /// partir del índice indicado.
    ///
    /// CancellationToken.None es intencional: una cancelación
    /// externa no debe impedir la limpieza de los entornos que
    /// ya fueron creados.
    /// </summary>
    private async Task CleanupRemainingPreparationsAsync(
        IReadOnlyList<MetadataProductiveApplicationResult>
            preparationResults,
        int startIndex,
        List<MetadataProductiveApplicationResult>
            cleanupResults,
        List<string> messages)
    {
        for (int index = startIndex;
            index < preparationResults.Count;
            index++)
        {
            MetadataProductiveApplicationResult
                preparedResult =
                    preparationResults[index];

            if (preparedResult is null ||
                !preparedResult.VerifiedCopyWasPrepared ||
                preparedResult.PromotionDecision !=
                    MetadataPromotionDecision.Pending)
            {
                continue;
            }

            try
            {
                MetadataProductiveApplicationResult
                    cleanupResult =
                        await _individualCoordinator
                            .CompleteAsync(
                                preparedResult,
                                MetadataPromotionDecision.Declined,
                                CancellationToken.None);

                cleanupResults.Add(
                    cleanupResult);

                messages.Add(
                    cleanupResult.WasSafelyDeclined
                        ? $"La preparación pendiente {index + 1} " +
                          "fue descartada de forma segura."
                        : $"La preparación pendiente {index + 1} " +
                          "no pudo limpiarse de forma completamente " +
                          "verificada.");
            }
            catch (Exception exception)
            {
                cleanupResults.Add(
                    new MetadataProductiveApplicationResult
                    {
                        PromotionDecision =
                            MetadataPromotionDecision.Declined,

                        ErrorMessage =
                            exception.Message,

                        Messages =
                            new[]
                            {
                                "La limpieza de una preparación " +
                                "pendiente produjo una excepción: " +
                                exception.Message
                            }
                    });

                messages.Add(
                    $"La limpieza de la preparación pendiente " +
                    $"{index + 1} produjo una excepción: " +
                    exception.Message);
            }
        }
    }
}