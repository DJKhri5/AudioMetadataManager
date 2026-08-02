using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta comprobaciones controladas sobre el coordinador
/// productivo sin acceder ni modificar archivos de audio.
/// </summary>
public sealed class MetadataApplicationCoordinatorTestRunner
{
    /// <summary>
    /// Ejecuta las comprobaciones controladas del coordinador.
    /// </summary>
    public async Task<MetadataApplicationCoordinatorTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        bool nullRequestWasRejected =
            false;

        bool nullExecutorFactoryWasRejected =
            false;

        bool preCancelledExecutionWasHandled =
            false;

        bool cancellationStopReasonWasCorrect =
            false;

        bool nullExecutorWasHandled =
            false;

        bool nullExecutorStopReasonWasCorrect =
            false;

        bool factoryExceptionWasHandled =
            false;

        bool factoryExceptionStopReasonWasCorrect =
            false;

        MetadataApplicationPipelineResult?
            preCancelledResult =
                null;

        MetadataApplicationPipelineResult?
            nullExecutorResult =
                null;

        MetadataApplicationPipelineResult?
            factoryExceptionResult =
                null;

        MetadataApplicationCoordinator defaultCoordinator =
            new();

        try
        {
            await defaultCoordinator.ExecuteAsync(
                null!);

            messages.Add(
                "El coordinador aceptó una solicitud nula.");
        }
        catch (ArgumentNullException exception)
            when (exception.ParamName == "request")
        {
            nullRequestWasRejected =
                true;

            messages.Add(
                "La solicitud nula fue rechazada " +
                "correctamente.");
        }

        try
        {
            _ = new MetadataApplicationCoordinator(
                null!);

            messages.Add(
                "El coordinador aceptó una fábrica nula.");
        }
        catch (ArgumentNullException exception)
            when (exception.ParamName == "executorFactory")
        {
            nullExecutorFactoryWasRejected =
                true;

            messages.Add(
                "La fábrica nula fue rechazada " +
                "correctamente.");
        }

        MetadataApplyRequest controlledRequest =
            new()
            {
                RequestId =
                    Guid.NewGuid(),

                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    "coordinator-controlled-test.flac",

                FileName =
                    "coordinator-controlled-test.flac"
            };

        bool preCancelledFactoryWasInvoked =
            false;

        using (CancellationTokenSource cancellationSource =
            new())
        {
            cancellationSource.Cancel();

            MetadataApplicationCoordinator
                preCancelledCoordinator =
                    new(
                        () =>
                        {
                            preCancelledFactoryWasInvoked =
                                true;

                            throw new InvalidOperationException(
                                "La fábrica no debía ejecutarse.");
                        });

            preCancelledResult =
                await preCancelledCoordinator.ExecuteAsync(
                    controlledRequest,
                    cancellationSource.Token);
        }

        preCancelledExecutionWasHandled =
            preCancelledResult.WasCancelled &&
            !preCancelledFactoryWasInvoked;

        cancellationStopReasonWasCorrect =
            preCancelledResult.StopReason ==
            MetadataApplicationStopReason.Cancelled;

        messages.Add(
            preCancelledExecutionWasHandled &&
            cancellationStopReasonWasCorrect
                ? "La cancelación previa fue controlada " +
                  "correctamente."
                : "La cancelación previa no fue controlada " +
                  "correctamente.");

        MetadataApplicationCoordinator
            nullExecutorCoordinator =
                new(
                    () => null!);

        nullExecutorResult =
            await nullExecutorCoordinator.ExecuteAsync(
                controlledRequest);

        nullExecutorWasHandled =
            !nullExecutorResult.WasSuccessful &&
            !string.IsNullOrWhiteSpace(
                nullExecutorResult.ErrorMessage);

        nullExecutorStopReasonWasCorrect =
            nullExecutorResult.StopReason ==
            MetadataApplicationStopReason.UnexpectedError;

        messages.Add(
            nullExecutorWasHandled &&
            nullExecutorStopReasonWasCorrect
                ? "El ejecutor nulo fue controlado " +
                  "correctamente."
                : "El ejecutor nulo no fue controlado " +
                  "correctamente.");

        const string FactoryExceptionMessage =
            "Error controlado de la fábrica.";

        MetadataApplicationCoordinator
            throwingFactoryCoordinator =
                new(
                    () =>
                        throw new InvalidOperationException(
                            FactoryExceptionMessage));

        factoryExceptionResult =
            await throwingFactoryCoordinator.ExecuteAsync(
                controlledRequest);

        factoryExceptionWasHandled =
            !factoryExceptionResult.WasSuccessful &&
            string.Equals(
                factoryExceptionResult.ErrorMessage,
                FactoryExceptionMessage,
                StringComparison.Ordinal);

        factoryExceptionStopReasonWasCorrect =
            factoryExceptionResult.StopReason ==
            MetadataApplicationStopReason.UnexpectedError;

        messages.Add(
            factoryExceptionWasHandled &&
            factoryExceptionStopReasonWasCorrect
                ? "La excepción de fábrica fue controlada " +
                  "correctamente."
                : "La excepción de fábrica no fue controlada " +
                  "correctamente.");

        bool resultsWereFinalized =
            preCancelledResult.CompletedAtUtc != default &&
            nullExecutorResult.CompletedAtUtc != default &&
            factoryExceptionResult.CompletedAtUtc != default;

        messages.Add(
            resultsWereFinalized
                ? "Todos los resultados controlados fueron " +
                  "finalizados correctamente."
                : "Uno o más resultados controlados no fueron " +
                  "finalizados correctamente.");

        return new MetadataApplicationCoordinatorTestResult
        {
            NullRequestWasRejected =
                nullRequestWasRejected,

            NullExecutorFactoryWasRejected =
                nullExecutorFactoryWasRejected,

            PreCancelledExecutionWasHandled =
                preCancelledExecutionWasHandled,

            CancellationStopReasonWasCorrect =
                cancellationStopReasonWasCorrect,

            NullExecutorWasHandled =
                nullExecutorWasHandled,

            NullExecutorStopReasonWasCorrect =
                nullExecutorStopReasonWasCorrect,

            FactoryExceptionWasHandled =
                factoryExceptionWasHandled,

            FactoryExceptionStopReasonWasCorrect =
                factoryExceptionStopReasonWasCorrect,

            ResultsWereFinalized =
                resultsWereFinalized,

            Messages =
                messages.ToArray()
        };
    }
}