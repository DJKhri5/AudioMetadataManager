namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Contiene una solicitud completa y aprobada para aplicar
/// cambios sobre un archivo.
///
/// Este objeto no ejecuta ninguna modificación.
/// </summary>
public sealed class MetadataApplyRequest
{
    /// <summary>
    /// Identificador único de la solicitud.
    /// </summary>
    public Guid RequestId { get; init; } =
        Guid.NewGuid();

    /// <summary>
    /// Identificador del plan de simulación del cual proviene.
    /// </summary>
    public Guid PlanId { get; init; }

    /// <summary>
    /// Momento UTC en que se creó la solicitud.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Ruta completa del archivo original.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre del archivo para presentación y auditoría.
    /// </summary>
    public string FileName { get; init; } =
        string.Empty;

    /// <summary>
    /// Cambios aprobados que se pretenden aplicar.
    /// </summary>
    public IReadOnlyList<MetadataFieldChange>
        Changes
    { get; init; } =
            Array.Empty<MetadataFieldChange>();

    /// <summary>
    /// Indica si la aplicación debe crear obligatoriamente una
    /// copia de seguridad antes de escribir.
    /// </summary>
    public bool RequireBackup { get; init; } =
        true;

    /// <summary>
    /// Indica si los valores escritos deberán volver a leerse y
    /// verificarse antes de considerar completada la operación.
    /// </summary>
    public bool RequirePostWriteVerification { get; init; } =
        true;

    /// <summary>
    /// Cambios válidos y diferentes del valor original.
    /// </summary>
    public IReadOnlyList<MetadataFieldChange>
        ValidChanges =>
            Changes
                .Where(change => change.IsValidChange)
                .ToArray();

    /// <summary>
    /// Indica si la solicitud contiene información suficiente
    /// para iniciar una validación previa.
    /// </summary>
    public bool IsStructurallyValid =>
        PlanId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(FilePath) &&
        ValidChanges.Count > 0;

    /// <summary>
    /// Cantidad de cambios aprobados utilizables.
    /// </summary>
    public int ValidChangeCount =>
        ValidChanges.Count;

    /// <summary>
    /// Resumen compacto de la solicitud.
    /// </summary>
    public string Summary =>
        $"{FileName}: {ValidChangeCount} cambio(s) " +
        "aprobado(s) para validación.";
}