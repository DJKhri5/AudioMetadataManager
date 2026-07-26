namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Engine;

/// <summary>
/// Representa una actualización de progreso emitida durante la
/// creación y verificación de un respaldo.
/// </summary>
public sealed class MetadataBackupProgress
{
    /// <summary>
    /// Porcentaje estimado entre 0 y 100.
    /// </summary>
    public double Percentage { get; init; }

    /// <summary>
    /// Mensaje preparado para la interfaz y los registros.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre del archivo procesado.
    /// </summary>
    public string FileName { get; init; } =
        string.Empty;

    /// <summary>
    /// Bytes copiados hasta el momento.
    /// </summary>
    public long ProcessedBytes { get; init; }

    /// <summary>
    /// Tamaño total esperado.
    /// </summary>
    public long TotalBytes { get; init; }

    /// <summary>
    /// Porcentaje limitado al intervalo permitido.
    /// </summary>
    public double NormalizedPercentage =>
        Math.Clamp(
            Percentage,
            0,
            100);

    /// <summary>
    /// Texto compacto para registros.
    /// </summary>
    public string Summary =>
        $"{NormalizedPercentage:0}% · " +
        $"{NormalizeMessage(Message)}";

    private static string NormalizeMessage(
        string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? "(sin mensaje)"
            : message.Trim();
    }
}