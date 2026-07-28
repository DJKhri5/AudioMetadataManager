using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;

/// <summary>
/// Resume una ejecución realizada por
/// MetadataApplicationPipelineExecutor.
/// </summary>
public sealed class MetadataApplicationPipelineExecutionResult
{
    /// <summary>
    /// Contexto utilizado durante la ejecución.
    /// </summary>
    public MetadataApplicationContext Context { get; init; } =
        null!;

    /// <summary>
    /// Cantidad de etapas recibidas por el ejecutor.
    /// </summary>
    public int RegisteredStageCount { get; init; }

    /// <summary>
    /// Cantidad de etapas que llegaron a ejecutarse.
    /// </summary>
    public int ExecutedStageCount { get; init; }

    /// <summary>
    /// Etapa en la que se detuvo la ejecución.
    ///
    /// Permanece en None cuando se recorrieron todas las etapas.
    /// </summary>
    public MetadataApplicationStage StoppedAtStage
    { get; init; } =
            MetadataApplicationStage.None;

    /// <summary>
    /// Explicación de la detención.
    /// </summary>
    public string StopMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si todas las etapas registradas llegaron a
    /// ejecutarse.
    /// </summary>
    public bool AllStagesWereExecuted =>
        RegisteredStageCount > 0 &&
        ExecutedStageCount ==
            RegisteredStageCount;

    /// <summary>
    /// Indica si la ejecución se detuvo antes de recorrer todas
    /// las etapas.
    /// </summary>
    public bool WasStoppedEarly =>
        ExecutedStageCount <
        RegisteredStageCount;

    /// <summary>
    /// Indica si alguna etapa terminó con fallo bloqueante.
    /// </summary>
    public bool HasBlockingFailure =>
        Context.StageResults.Any(
            result =>
                result.IsBlockingFailure);

    /// <summary>
    /// Indica si alguna etapa quedó cancelada.
    /// </summary>
    public bool WasCancelled =>
        Context.StageResults.Any(
            result =>
                result.Status ==
                MetadataApplicationStageStatus.Cancelled);

    /// <summary>
    /// Indica si el ejecutor recorrió correctamente todas las
    /// etapas sin fallos bloqueantes ni cancelaciones.
    ///
    /// Esto no significa necesariamente que el contexto haya
    /// sido finalizado como una aplicación exitosa.
    /// </summary>
    public bool ExecutionWasSuccessful =>
        AllStagesWereExecuted &&
        !HasBlockingFailure &&
        !WasCancelled;

    /// <summary>
    /// Resumen compacto de la ejecución.
    /// </summary>
    public string Summary
    {
        get
        {
            if (ExecutionWasSuccessful)
            {
                return
                    $"Se ejecutaron correctamente " +
                    $"{ExecutedStageCount} etapa(s).";
            }

            if (WasCancelled)
            {
                return
                    $"La ejecución fue cancelada en la etapa " +
                    $"{StoppedAtStage}.";
            }

            if (HasBlockingFailure)
            {
                return
                    $"La ejecución se detuvo por un fallo en la " +
                    $"etapa {StoppedAtStage}.";
            }

            return
                $"La ejecución recorrió {ExecutedStageCount} de " +
                $"{RegisteredStageCount} etapa(s).";
        }
    }
}