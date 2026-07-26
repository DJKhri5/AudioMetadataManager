namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

/// <summary>
/// Contiene el resultado auditable de una etapa individual del
/// pipeline.
/// </summary>
public sealed class MetadataApplicationStageResult
{
    /// <summary>
    /// Etapa evaluada.
    /// </summary>
    public MetadataApplicationStage Stage { get; init; } =
        MetadataApplicationStage.None;

    /// <summary>
    /// Estado final de la etapa.
    /// </summary>
    public MetadataApplicationStageStatus Status { get; init; } =
        MetadataApplicationStageStatus.Pending;

    /// <summary>
    /// Momento UTC en que comenzó la etapa.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC en que terminó la etapa.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>
    /// Duración total de la etapa.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Mensaje principal de la etapa.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes adicionales, advertencias o detalles.
    /// </summary>
    public IReadOnlyList<string> Details { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si la etapa terminó de forma satisfactoria.
    /// </summary>
    public bool WasSuccessful =>
        Status is
            MetadataApplicationStageStatus.Completed or
            MetadataApplicationStageStatus
                .CompletedWithWarnings;

    /// <summary>
    /// Indica si esta etapa impidió continuar.
    /// </summary>
    public bool IsBlockingFailure =>
        Status is
            MetadataApplicationStageStatus.Failed or
            MetadataApplicationStageStatus.Cancelled;

    /// <summary>
    /// Nombre legible de la etapa.
    /// </summary>
    public string StageDisplay =>
        Stage switch
        {
            MetadataApplicationStage.Validation =>
                "Validación previa",

            MetadataApplicationStage.Backup =>
                "Copia de seguridad",

            MetadataApplicationStage.MetadataWrite =>
                "Escritura de metadatos",

            MetadataApplicationStage.PostWriteVerification =>
                "Verificación posterior",

            MetadataApplicationStage.Finalization =>
                "Finalización",

            _ =>
                "Sin etapa"
        };

    /// <summary>
    /// Resumen compacto del resultado.
    /// </summary>
    public string Summary =>
        $"{StageDisplay}: {Status}. " +
        $"{NormalizeMessage(Message)}";

    private static string NormalizeMessage(
        string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? "(sin mensaje)"
            : message.Trim();
    }
}