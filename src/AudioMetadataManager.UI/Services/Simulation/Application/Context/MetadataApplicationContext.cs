using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Context;

/// <summary>
/// Conserva el estado completo y mutable de una ejecución del
/// pipeline de aplicación de metadatos.
///
/// Cada instancia representa la ejecución correspondiente a un
/// único archivo.
///
/// Las distintas etapas del pipeline pueden registrar aquí sus
/// resultados sin necesitar recibir múltiples parámetros
/// independientes.
/// </summary>
public sealed class MetadataApplicationContext
{
    private readonly List<MetadataApplicationStageResult>
        _stageResults =
            new();

    /// <summary>
    /// Crea un contexto para una solicitud de aplicación.
    /// </summary>
    public MetadataApplicationContext(
        MetadataApplyRequest request,
        CancellationToken cancellationToken = default)
    {
        Request =
            request ??
            throw new ArgumentNullException(
                nameof(request));

        CancellationToken =
            cancellationToken;

        ExecutionId =
            Guid.NewGuid();

        StartedAtUtc =
            DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Identificador único de esta ejecución del pipeline.
    /// </summary>
    public Guid ExecutionId { get; }

    /// <summary>
    /// Solicitud que originó la ejecución.
    /// </summary>
    public MetadataApplyRequest Request { get; }

    /// <summary>
    /// Token utilizado para cancelar la ejecución.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Momento UTC en que se creó el contexto.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; }

    /// <summary>
    /// Momento UTC en que finalizó la ejecución.
    ///
    /// Permanece nulo mientras el contexto continúa activo.
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Resultado de la validación previa.
    /// </summary>
    public MetadataApplyValidationResult?
        ValidationResult
    { get; private set; }

    /// <summary>
    /// Resultado de la creación y verificación del respaldo.
    /// </summary>
    public MetadataBackupResult?
        BackupResult
    { get; private set; }

    /// <summary>
    /// Resultado producido por el motor de escritura.
    /// </summary>
    public MetadataWriteResult?
        WriteResult
    { get; private set; }

    /// <summary>
    /// Resultado final consolidado de la aplicación.
    /// </summary>
    public MetadataApplyResult?
        ApplyResult
    { get; private set; }

    /// <summary>
    /// Razón por la que terminó o se detuvo la ejecución.
    /// </summary>
    public MetadataApplicationStopReason StopReason
    { get; private set; } =
            MetadataApplicationStopReason.None;

    /// <summary>
    /// Mensaje global asociado a un error o detención.
    /// </summary>
    public string ErrorMessage { get; private set; } =
        string.Empty;

    /// <summary>
    /// Resultados de las etapas ejecutadas hasta el momento.
    /// </summary>
    public IReadOnlyList<MetadataApplicationStageResult>
        StageResults =>
            _stageResults.AsReadOnly();

    /// <summary>
    /// Indica si el contexto ya fue finalizado.
    /// </summary>
    public bool IsCompleted =>
        CompletedAtUtc.HasValue;

    /// <summary>
    /// Indica si la ejecución terminó correctamente.
    /// </summary>
    public bool WasSuccessful =>
        IsCompleted &&
        StopReason ==
            MetadataApplicationStopReason.Completed &&
        ApplyResult?.WasSuccessful == true &&
        _stageResults.Count > 0 &&
        _stageResults.All(
            result =>
                result.WasSuccessful);

    /// <summary>
    /// Indica si la ejecución fue cancelada.
    /// </summary>
    public bool WasCancelled =>
        StopReason ==
        MetadataApplicationStopReason.Cancelled;

    /// <summary>
    /// Duración transcurrida o duración final de la ejecución.
    /// </summary>
    public TimeSpan ElapsedTime =>
        (CompletedAtUtc ??
         DateTimeOffset.UtcNow) -
        StartedAtUtc;

    /// <summary>
    /// Última etapa que no fue omitida.
    /// </summary>
    public MetadataApplicationStage LastExecutedStage =>
        _stageResults
            .Where(
                result =>
                    result.Status !=
                    MetadataApplicationStageStatus.Skipped)
            .Select(
                result =>
                    result.Stage)
            .LastOrDefault();

    /// <summary>
    /// Cantidad de etapas que terminaron correctamente.
    /// </summary>
    public int SuccessfulStageCount =>
        _stageResults.Count(
            result =>
                result.WasSuccessful);

    /// <summary>
    /// Cantidad de etapas que terminaron con un fallo
    /// bloqueante.
    /// </summary>
    public int FailedStageCount =>
        _stageResults.Count(
            result =>
                result.IsBlockingFailure);

    /// <summary>
    /// Registra el resultado de la validación.
    /// </summary>
    public void SetValidationResult(
        MetadataApplyValidationResult result)
    {
        EnsureActive();

        ValidationResult =
            result ??
            throw new ArgumentNullException(
                nameof(result));
    }

    /// <summary>
    /// Registra el resultado del respaldo obligatorio.
    /// </summary>
    public void SetBackupResult(
        MetadataBackupResult result)
    {
        EnsureActive();

        BackupResult =
            result ??
            throw new ArgumentNullException(
                nameof(result));
    }

    /// <summary>
    /// Registra el resultado del motor de escritura.
    /// </summary>
    public void SetWriteResult(
        MetadataWriteResult result)
    {
        EnsureActive();

        WriteResult =
            result ??
            throw new ArgumentNullException(
                nameof(result));
    }

    /// <summary>
    /// Registra el resultado final de aplicación.
    /// </summary>
    public void SetApplyResult(
        MetadataApplyResult result)
    {
        EnsureActive();

        ApplyResult =
            result ??
            throw new ArgumentNullException(
                nameof(result));
    }

    /// <summary>
    /// Agrega el resultado de una etapa.
    ///
    /// Una misma etapa sólo puede registrarse una vez.
    /// </summary>
    public void AddStageResult(
        MetadataApplicationStageResult result)
    {
        EnsureActive();

        ArgumentNullException.ThrowIfNull(
            result);

        if (result.Stage ==
            MetadataApplicationStage.None)
        {
            throw new ArgumentException(
                "No es posible registrar una etapa sin " +
                "identificar.",
                nameof(result));
        }

        if (_stageResults.Any(
                existingResult =>
                    existingResult.Stage ==
                    result.Stage))
        {
            throw new InvalidOperationException(
                $"La etapa {result.Stage} ya fue registrada.");
        }

        _stageResults.Add(
            result);
    }

    /// <summary>
    /// Comprueba si una etapa ya fue registrada.
    /// </summary>
    public bool HasStage(
        MetadataApplicationStage stage)
    {
        return _stageResults.Any(
            result =>
                result.Stage == stage);
    }

    /// <summary>
    /// Solicita la cancelación si el token ya fue activado.
    /// </summary>
    public void ThrowIfCancellationRequested()
    {
        CancellationToken.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Finaliza correctamente la ejecución.
    /// </summary>
    public void Complete()
    {
        FinalizeExecution(
            MetadataApplicationStopReason.Completed,
            string.Empty);
    }

    /// <summary>
    /// Finaliza la ejecución con una razón específica.
    /// </summary>
    public void Stop(
        MetadataApplicationStopReason stopReason,
        string? errorMessage = null)
    {
        if (stopReason ==
            MetadataApplicationStopReason.None)
        {
            throw new ArgumentException(
                "La razón de detención no puede ser None.",
                nameof(stopReason));
        }

        FinalizeExecution(
            stopReason,
            errorMessage);
    }

    /// <summary>
    /// Construye el resultado inmutable correspondiente al
    /// estado actual del contexto.
    ///
    /// El contexto debe haber sido finalizado previamente.
    /// </summary>
    public MetadataApplicationPipelineResult BuildResult()
    {
        if (!IsCompleted)
        {
            throw new InvalidOperationException(
                "El contexto debe finalizarse antes de construir " +
                "el resultado del pipeline.");
        }

        return new MetadataApplicationPipelineResult
        {
            ExecutionId =
                ExecutionId,

            Request =
                Request,

            StartedAtUtc =
                StartedAtUtc,

            CompletedAtUtc =
                CompletedAtUtc!.Value,

            ElapsedTime =
                ElapsedTime,

            StopReason =
                StopReason,

            StageResults =
                _stageResults.ToArray(),

            ValidationResult =
                ValidationResult,

            BackupResult =
                BackupResult,

            WriteResult =
                WriteResult,

            ApplyResult =
                ApplyResult,

            ErrorMessage =
                ErrorMessage
        };
    }

    /// <summary>
    /// Resumen compacto del estado actual.
    /// </summary>
    public string Summary
    {
        get
        {
            if (!IsCompleted)
            {
                return
                    $"{Request.FileName}: ejecución activa. " +
                    $"Última etapa: {LastExecutedStage}.";
            }

            if (WasSuccessful)
            {
                return
                    $"{Request.FileName}: ejecución completada " +
                    $"correctamente en " +
                    $"{ElapsedTime.TotalMilliseconds:0} ms.";
            }

            if (WasCancelled)
            {
                return
                    $"{Request.FileName}: ejecución cancelada.";
            }

            return
                $"{Request.FileName}: ejecución terminada por " +
                $"{StopReason}. Etapas correctas: " +
                $"{SuccessfulStageCount}. Etapas fallidas: " +
                $"{FailedStageCount}.";
        }
    }

    private void FinalizeExecution(
        MetadataApplicationStopReason stopReason,
        string? errorMessage)
    {
        EnsureActive();

        StopReason =
            stopReason;

        ErrorMessage =
            string.IsNullOrWhiteSpace(errorMessage)
                ? string.Empty
                : errorMessage.Trim();

        CompletedAtUtc =
            DateTimeOffset.UtcNow;
    }

    private void EnsureActive()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException(
                "El contexto ya fue finalizado y no puede " +
                "modificarse.");
        }
    }
}