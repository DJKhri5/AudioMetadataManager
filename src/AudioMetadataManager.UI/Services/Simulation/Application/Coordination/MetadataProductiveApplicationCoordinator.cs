using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;

/// <summary>
/// Coordina una aplicación productiva individual mediante una
/// ejecución aislada preservada, una decisión explícita de
/// promoción y una limpieza final controlada.
/// </summary>
public sealed class MetadataProductiveApplicationCoordinator :
    IMetadataProductiveApplicationCoordinator
{
    private readonly MetadataApplicationIsolatedExecutor
        _isolatedExecutor;

    private readonly IMetadataApplicationPromotionService
        _promotionService;

    private readonly FileIsolationTestHarness
        _isolationHarness;

    private readonly object
        _completionSync =
            new();

    private readonly HashSet<string>
        _completedPreparationPaths =
            new(
                StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Crea el coordinador con las dependencias predeterminadas.
    /// </summary>
    public MetadataProductiveApplicationCoordinator()
        : this(
            new MetadataApplicationIsolatedExecutor(),
            new MetadataApplicationPromotionService(),
            new FileIsolationTestHarness())
    {
    }

    /// <summary>
    /// Crea el coordinador con las dependencias proporcionadas.
    /// </summary>
    public MetadataProductiveApplicationCoordinator(
        MetadataApplicationIsolatedExecutor isolatedExecutor,
        IMetadataApplicationPromotionService promotionService,
        FileIsolationTestHarness isolationHarness)
    {
        _isolatedExecutor =
            isolatedExecutor ??
            throw new ArgumentNullException(
                nameof(isolatedExecutor));

        _promotionService =
            promotionService ??
            throw new ArgumentNullException(
                nameof(promotionService));

        _isolationHarness =
            isolationHarness ??
            throw new ArgumentNullException(
                nameof(isolationHarness));
    }

    /// <inheritdoc />
    public async Task<MetadataProductiveApplicationResult>
        PrepareAsync(
            MetadataApplyRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        List<string> messages =
            new();

        MetadataApplicationIsolatedExecutionResult?
            isolatedExecutionResult =
                null;

        string errorMessage =
            string.Empty;

        try
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            messages.Add(
                "Se inició la preparación productiva sobre una " +
                "copia temporal aislada.");

            isolatedExecutionResult =
                await _isolatedExecutor.ExecuteAsync(
                    request,
                    MetadataApplicationIsolatedExecutionOptions
                        .PreserveSuccessfulExecution,
                    cancellationToken);

            messages.Add(
                isolatedExecutionResult.WasSuccessful
                    ? "La copia verificada fue preparada y " +
                      "conservada correctamente."
                    : "La ejecución aislada no produjo una copia " +
                      "verificada promovible.");

            if (!string.IsNullOrWhiteSpace(
                    isolatedExecutionResult.ErrorMessage))
            {
                messages.Add(
                    "La ejecución aislada informó: " +
                    isolatedExecutionResult.ErrorMessage);
            }
        }
        catch (OperationCanceledException)
        {
            errorMessage =
                "La preparación productiva fue cancelada.";

            messages.Add(
                errorMessage);
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;

            messages.Add(
                "La preparación productiva produjo un error: " +
                exception.Message);
        }

        bool verifiedCopyWasPrepared =
            isolatedExecutionResult?.WasSuccessful == true &&
            isolatedExecutionResult.EnvironmentWasPreserved &&
            isolatedExecutionResult.IsolationContext is not null;

        return new MetadataProductiveApplicationResult
        {
            IsolatedExecutionResult =
                isolatedExecutionResult,

            PromotionDecision =
                verifiedCopyWasPrepared
                    ? MetadataPromotionDecision.Pending
                    : MetadataPromotionDecision.Unavailable,

            ErrorMessage =
                errorMessage,

            Messages =
                messages.ToArray()
        };
    }

    /// <inheritdoc />
    public async Task<MetadataProductiveApplicationResult>
        CompleteAsync(
            MetadataProductiveApplicationResult preparedResult,
            MetadataPromotionDecision promotionDecision,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            preparedResult);

        List<string> messages =
            preparedResult.Messages.ToList();

        MetadataApplicationPromotionResult?
            promotionResult =
                null;

        bool finalCleanupWasAttempted =
            false;

        bool finalCleanupWasSuccessful =
            false;

        bool completionWasReserved =
            false;

        string errorMessage =
            string.Empty;

        MetadataApplicationIsolatedExecutionResult?
            isolatedExecutionResult =
                preparedResult.IsolatedExecutionResult;

        FileIsolationContext? isolationContext =
            isolatedExecutionResult?.IsolationContext;

        try
        {
            ReserveCompletion(
                preparedResult,
                promotionDecision,
                isolationContext);

            completionWasReserved =
                true;

            cancellationToken
                .ThrowIfCancellationRequested();

            if (promotionDecision ==
                MetadataPromotionDecision.Declined)
            {
                messages.Add(
                    "El usuario rechazó la promoción. El archivo " +
                    "original no fue entregado al servicio de " +
                    "promoción.");
            }
            else
            {
                if (isolationContext is null)
                {
                    throw new InvalidOperationException(
                        "No existe un entorno aislado disponible " +
                        "para completar la promoción.");
                }

                messages.Add(
                    "El usuario aprobó la promoción de la copia " +
                    "verificada hacia el archivo original.");

                promotionResult =
                    await _promotionService.PromoteAsync(
                        isolationContext.WorkingCopyPath,
                        isolationContext.OriginalFilePath,
                        cancellationToken);

                foreach (string message
                    in promotionResult.Messages)
                {
                    messages.Add(
                        message);
                }

                messages.Add(
                    promotionResult.WasSuccessful
                        ? "La promoción productiva terminó " +
                          "correctamente."
                        : promotionResult.WasSafelyRolledBack
                            ? "La promoción no pudo completarse, " +
                              "pero el archivo original fue " +
                              "restaurado correctamente."
                            : "La promoción no terminó en un " +
                              "estado seguro verificado.");
            }
        }
        catch (OperationCanceledException)
        {
            errorMessage =
                "La finalización productiva fue cancelada.";

            messages.Add(
                errorMessage);
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;

            messages.Add(
                "La finalización productiva produjo un error: " +
                exception.Message);
        }
        finally
        {
            if (completionWasReserved &&
                isolationContext is not null)
            {
                finalCleanupWasAttempted =
                    true;

                finalCleanupWasSuccessful =
                    _isolationHarness.TryCleanup(
                        isolationContext);

                messages.Add(
                    finalCleanupWasSuccessful
                        ? "El entorno aislado fue eliminado " +
                          "correctamente."
                        : "El entorno aislado no pudo ser " +
                          "eliminado completamente.");
            }
        }

        return new MetadataProductiveApplicationResult
        {
            IsolatedExecutionResult =
                isolatedExecutionResult,

            PromotionDecision =
                promotionDecision,

            PromotionResult =
                promotionResult,

            FinalCleanupWasAttempted =
                finalCleanupWasAttempted,

            FinalCleanupWasSuccessful =
                finalCleanupWasSuccessful,

            ErrorMessage =
                errorMessage,

            Messages =
                messages.ToArray()
        };
    }

    private void ReserveCompletion(
        MetadataProductiveApplicationResult preparedResult,
        MetadataPromotionDecision promotionDecision,
        FileIsolationContext? isolationContext)
    {
        ValidateCompletionRequest(
            preparedResult,
            promotionDecision);

        if (isolationContext is null ||
            string.IsNullOrWhiteSpace(
                isolationContext.TestDirectoryPath))
        {
            throw new InvalidOperationException(
                "La preparación no contiene un entorno aislado " +
                "válido para finalizar.");
        }

        string preparationPath =
            isolationContext.TestDirectoryPath;

        lock (_completionSync)
        {
            if (!_completedPreparationPaths.Add(
                    preparationPath))
            {
                throw new InvalidOperationException(
                    "La preparación productiva ya fue finalizada " +
                    "anteriormente y no puede reutilizarse.");
            }
        }
    }

    private static void ValidateCompletionRequest(
        MetadataProductiveApplicationResult preparedResult,
        MetadataPromotionDecision promotionDecision)
    {
        if (!preparedResult.VerifiedCopyWasPrepared)
        {
            throw new InvalidOperationException(
                "La preparación anterior no contiene una copia " +
                "verificada disponible para promoción.");
        }

        if (preparedResult.PromotionDecision !=
            MetadataPromotionDecision.Pending)
        {
            throw new InvalidOperationException(
                "La preparación anterior no se encuentra " +
                "pendiente de una decisión de promoción.");
        }

        if (promotionDecision is not
            MetadataPromotionDecision.Approved and not
            MetadataPromotionDecision.Declined)
        {
            throw new ArgumentOutOfRangeException(
                nameof(promotionDecision),
                promotionDecision,
                "La finalización solamente acepta una decisión " +
                "Approved o Declined.");
        }
    }
}