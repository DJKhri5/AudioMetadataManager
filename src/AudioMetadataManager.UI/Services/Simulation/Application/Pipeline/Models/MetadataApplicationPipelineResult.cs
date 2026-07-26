using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

/// <summary>
/// Contiene el resultado completo de una ejecución del pipeline
/// de aplicación.
/// </summary>
public sealed class MetadataApplicationPipelineResult
{
    /// <summary>
    /// Identificador único de la ejecución.
    /// </summary>
    public Guid ExecutionId { get; init; } =
        Guid.NewGuid();

    /// <summary>
    /// Solicitud que originó la ejecución.
    /// </summary>
    public MetadataApplyRequest Request { get; init; } =
        new();

    /// <summary>
    /// Momento UTC de inicio.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC de finalización.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>
    /// Duración total.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Razón por la que terminó el pipeline.
    /// </summary>
    public MetadataApplicationStopReason StopReason
    { get; init; } =
            MetadataApplicationStopReason.None;

    /// <summary>
    /// Resultados de las etapas ejecutadas.
    /// </summary>
    public IReadOnlyList<MetadataApplicationStageResult>
        StageResults
    { get; init; } =
            Array.Empty<MetadataApplicationStageResult>();

    /// <summary>
    /// Resultado de la validación previa.
    /// </summary>
    public MetadataApplyValidationResult?
        ValidationResult
    { get; init; }

    /// <summary>
    /// Resultado de la copia de seguridad obligatoria.
    /// </summary>
    public MetadataBackupResult?
        BackupResult
    { get; init; }

    /// <summary>
    /// Resultado producido por el motor de escritura.
    /// </summary>
    public MetadataWriteResult?
        WriteResult
    { get; init; }

    /// <summary>
    /// Resultado final de aplicación.
    ///
    /// Permanecerá nulo hasta que exista un escritor real.
    /// </summary>
    public MetadataApplyResult?
        ApplyResult
    { get; init; }

    /// <summary>
    /// Mensaje global de error.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la ejecución recorrió correctamente todas las
    /// etapas obligatorias.
    /// </summary>
    public bool WasSuccessful =>
        StopReason ==
            MetadataApplicationStopReason.Completed &&
        StageResults.Count > 0 &&
        StageResults.All(
            result =>
                result.WasSuccessful);

    /// <summary>
    /// Indica si la ejecución terminó por cancelación.
    /// </summary>
    public bool WasCancelled =>
        StopReason ==
        MetadataApplicationStopReason.Cancelled;

    /// <summary>
    /// Última etapa que llegó a ejecutarse.
    /// </summary>
    public MetadataApplicationStage LastExecutedStage =>
        StageResults
            .Where(
                result =>
                    result.Status !=
                    MetadataApplicationStageStatus.Skipped)
            .Select(
                result =>
                    result.Stage)
            .LastOrDefault();

    /// <summary>
    /// Cantidad de etapas terminadas correctamente.
    /// </summary>
    public int SuccessfulStageCount =>
        StageResults.Count(
            result =>
                result.WasSuccessful);

    /// <summary>
    /// Cantidad de etapas fallidas.
    /// </summary>
    public int FailedStageCount =>
        StageResults.Count(
            result =>
                result.IsBlockingFailure);

    /// <summary>
    /// Resumen compacto de la ejecución.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    $"{Request.FileName}: pipeline completado " +
                    $"correctamente en " +
                    $"{ElapsedTime.TotalMilliseconds:0} ms.";
            }

            if (WasCancelled)
            {
                return
                    $"{Request.FileName}: operación cancelada.";
            }

            return
                $"{Request.FileName}: pipeline detenido por " +
                $"{StopReason}. Etapas correctas: " +
                $"{SuccessfulStageCount}. Etapas fallidas: " +
                $"{FailedStageCount}.";
        }
    }
}