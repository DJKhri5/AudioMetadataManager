namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

/// <summary>
/// Representa una actualización de progreso emitida durante la
/// ejecución del pipeline.
/// </summary>
public sealed class MetadataApplicationProgress
{
    /// <summary>
    /// Etapa que se está ejecutando.
    /// </summary>
    public MetadataApplicationStage Stage { get; init; } =
        MetadataApplicationStage.None;

    /// <summary>
    /// Porcentaje global estimado, entre 0 y 100.
    /// </summary>
    public double Percentage { get; init; }

    /// <summary>
    /// Mensaje preparado para la interfaz.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre del archivo que se está procesando.
    /// </summary>
    public string FileName { get; init; } =
        string.Empty;

    /// <summary>
    /// Porcentaje limitado al intervalo admitido.
    /// </summary>
    public double NormalizedPercentage =>
        Math.Clamp(
            Percentage,
            0,
            100);

    /// <summary>
    /// Porcentaje preparado para mostrarse.
    /// </summary>
    public string PercentageDisplay =>
        $"{NormalizedPercentage:0}%";

    /// <summary>
    /// Resumen compacto para registros.
    /// </summary>
    public string Summary =>
        $"{PercentageDisplay} · {Stage}: " +
        $"{NormalizeMessage(Message)}";

    private static string NormalizeMessage(
        string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? "(sin mensaje)"
            : message.Trim();
    }
}