using System.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Contracts;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Base;

/// <summary>
/// Proporciona la infraestructura común para las etapas del
/// pipeline de aplicación.
///
/// La clase valida el contexto, comprueba la cancelación,
/// registra los tiempos de ejecución y transforma excepciones
/// en resultados auditables de etapa.
/// </summary>
public abstract class MetadataApplicationStageBase :
    IMetadataApplicationStage
{
    /// <inheritdoc />
    public abstract MetadataApplicationStage Stage { get; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract int ExecutionOrder { get; }

    /// <summary>
    /// Ejecuta la etapa y registra automáticamente su resultado
    /// en el contexto.
    /// </summary>
    public async Task ExecuteAsync(
        MetadataApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        if (context.IsCompleted)
        {
            throw new InvalidOperationException(
                $"La etapa {Name} no puede ejecutarse porque " +
                "el contexto ya fue finalizado.");
        }

        if (Stage ==
            MetadataApplicationStage.None)
        {
            throw new InvalidOperationException(
                $"La etapa {Name} no tiene una identidad válida.");
        }

        if (context.HasStage(
                Stage))
        {
            throw new InvalidOperationException(
                $"La etapa {Stage} ya fue ejecutada.");
        }

        context.ThrowIfCancellationRequested();

        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        MetadataApplicationStageResult stageResult;

        try
        {
            MetadataApplicationStageExecution execution =
                await ExecuteCoreAsync(
                    context);

            stopwatch.Stop();

            stageResult =
                new MetadataApplicationStageResult
                {
                    Stage =
                        Stage,

                    Status =
                        execution.Status,

                    StartedAtUtc =
                        startedAtUtc,

                    CompletedAtUtc =
                        DateTimeOffset.UtcNow,

                    ElapsedTime =
                        stopwatch.Elapsed,

                    Message =
                        execution.Message,

                    Details =
                        execution.Details
                };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            stageResult =
                new MetadataApplicationStageResult
                {
                    Stage =
                        Stage,

                    Status =
                        MetadataApplicationStageStatus.Cancelled,

                    StartedAtUtc =
                        startedAtUtc,

                    CompletedAtUtc =
                        DateTimeOffset.UtcNow,

                    ElapsedTime =
                        stopwatch.Elapsed,

                    Message =
                        $"La etapa {Name} fue cancelada.",

                    Details =
                        Array.Empty<string>()
                };

            context.AddStageResult(
                stageResult);

            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            stageResult =
                new MetadataApplicationStageResult
                {
                    Stage =
                        Stage,

                    Status =
                        MetadataApplicationStageStatus.Failed,

                    StartedAtUtc =
                        startedAtUtc,

                    CompletedAtUtc =
                        DateTimeOffset.UtcNow,

                    ElapsedTime =
                        stopwatch.Elapsed,

                    Message =
                        $"La etapa {Name} terminó con un error.",

                    Details =
                        new[]
                        {
                            exception.Message
                        }
                };
        }

        context.AddStageResult(
            stageResult);
    }

    /// <summary>
    /// Contiene la lógica específica de la etapa.
    ///
    /// Las clases derivadas no necesitan medir tiempos ni crear
    /// MetadataApplicationStageResult directamente.
    /// </summary>
    protected abstract Task<MetadataApplicationStageExecution>
        ExecuteCoreAsync(
            MetadataApplicationContext context);

    /// <summary>
    /// Construye un resultado interno de etapa completada.
    /// </summary>
    protected static MetadataApplicationStageExecution Completed(
        string message,
        IReadOnlyList<string>? details = null)
    {
        return new MetadataApplicationStageExecution
        {
            Status =
                MetadataApplicationStageStatus.Completed,

            Message =
                NormalizeMessage(
                    message),

            Details =
                details?.ToArray() ??
                Array.Empty<string>()
        };
    }

    /// <summary>
    /// Construye un resultado interno completado con
    /// advertencias.
    /// </summary>
    protected static MetadataApplicationStageExecution
        CompletedWithWarnings(
            string message,
            IReadOnlyList<string>? details = null)
    {
        return new MetadataApplicationStageExecution
        {
            Status =
                MetadataApplicationStageStatus
                    .CompletedWithWarnings,

            Message =
                NormalizeMessage(
                    message),

            Details =
                details?.ToArray() ??
                Array.Empty<string>()
        };
    }

    /// <summary>
    /// Construye un resultado interno fallido.
    /// </summary>
    protected static MetadataApplicationStageExecution Failed(
        string message,
        IReadOnlyList<string>? details = null)
    {
        return new MetadataApplicationStageExecution
        {
            Status =
                MetadataApplicationStageStatus.Failed,

            Message =
                NormalizeMessage(
                    message),

            Details =
                details?.ToArray() ??
                Array.Empty<string>()
        };
    }

    /// <summary>
    /// Construye un resultado interno cancelado.
    /// </summary>
    protected static MetadataApplicationStageExecution Cancelled(
        string message,
        IReadOnlyList<string>? details = null)
    {
        return new MetadataApplicationStageExecution
        {
            Status =
                MetadataApplicationStageStatus.Cancelled,

            Message =
                NormalizeMessage(
                    message),

            Details =
                details?.ToArray() ??
                Array.Empty<string>()
        };
    }

    /// <summary>
    /// Construye un resultado interno omitido.
    /// </summary>
    protected static MetadataApplicationStageExecution Skipped(
        string message,
        IReadOnlyList<string>? details = null)
    {
        return new MetadataApplicationStageExecution
        {
            Status =
                MetadataApplicationStageStatus.Skipped,

            Message =
                NormalizeMessage(
                    message),

            Details =
                details?.ToArray() ??
                Array.Empty<string>()
        };
    }

    private static string NormalizeMessage(
        string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? "La etapa no proporcionó un mensaje."
            : message.Trim();
    }

    /// <summary>
    /// Resultado interno utilizado por la clase base antes de
    /// construir el resultado auditable definitivo.
    /// </summary>
    protected sealed class MetadataApplicationStageExecution
    {
        public MetadataApplicationStageStatus Status
        { get; init; } =
                MetadataApplicationStageStatus.Pending;

        public string Message { get; init; } =
            string.Empty;

        public IReadOnlyList<string> Details { get; init; } =
            Array.Empty<string>();
    }
}