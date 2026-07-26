using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

/// <summary>
/// Contiene una solicitud técnica para escribir metadatos
/// previamente aprobados sobre un archivo.
///
/// La existencia de este objeto no autoriza por sí sola la
/// escritura. El pipeline debe haber creado y verificado antes
/// una copia de seguridad.
/// </summary>
public sealed class MetadataWriteRequest
{
    /// <summary>
    /// Identificador único de la operación de escritura.
    /// </summary>
    public Guid WriteRequestId { get; init; } =
        Guid.NewGuid();

    /// <summary>
    /// Identificador de la solicitud general de aplicación.
    /// </summary>
    public Guid ApplyRequestId { get; init; }

    /// <summary>
    /// Identificador del plan de simulación.
    /// </summary>
    public Guid PlanId { get; init; }

    /// <summary>
    /// Ruta completa del archivo que podría modificarse.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre preparado para diagnósticos.
    /// </summary>
    public string FileName { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta del respaldo creado antes de esta solicitud.
    /// </summary>
    public string VerifiedBackupPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Cambios aprobados que deben escribirse.
    /// </summary>
    public IReadOnlyList<MetadataFieldChange>
        Changes
    { get; init; } =
            Array.Empty<MetadataFieldChange>();

    /// <summary>
    /// Momento UTC de creación.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Indica si el escritor debe preservar campos que no
    /// participan en la solicitud.
    /// </summary>
    public bool PreserveUnchangedMetadata { get; init; } =
        true;

    /// <summary>
    /// Indica si las imágenes incrustadas deben conservarse.
    /// </summary>
    public bool PreserveEmbeddedPictures { get; init; } =
        true;

    /// <summary>
    /// Indica si deben conservarse campos técnicos, privados o
    /// no administrados por esta versión del programa.
    /// </summary>
    public bool PreserveUnknownMetadata { get; init; } =
        true;

    /// <summary>
    /// Cambios válidos que representan diferencias reales.
    /// </summary>
    public IReadOnlyList<MetadataFieldChange>
        ValidChanges =>
            Changes
                .Where(change => change.IsValidChange)
                .ToArray();

    /// <summary>
    /// Ruta normalizada del archivo.
    /// </summary>
    public string NormalizedFilePath =>
        string.IsNullOrWhiteSpace(FilePath)
            ? string.Empty
            : Path.GetFullPath(FilePath.Trim());

    /// <summary>
    /// Extensión normalizada, incluyendo el punto inicial.
    /// </summary>
    public string NormalizedExtension =>
        string.IsNullOrWhiteSpace(NormalizedFilePath)
            ? string.Empty
            : Path.GetExtension(NormalizedFilePath)
                .ToLowerInvariant();

    /// <summary>
    /// Indica si existe un respaldo físico asociado.
    /// </summary>
    public bool HasVerifiedBackup =>
        !string.IsNullOrWhiteSpace(VerifiedBackupPath) &&
        File.Exists(VerifiedBackupPath);

    /// <summary>
    /// Indica si la solicitud contiene la identidad mínima.
    /// </summary>
    public bool IsStructurallyValid =>
        WriteRequestId != Guid.Empty &&
        ApplyRequestId != Guid.Empty &&
        PlanId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(NormalizedFilePath) &&
        File.Exists(NormalizedFilePath) &&
        HasVerifiedBackup &&
        ValidChanges.Count > 0;

    /// <summary>
    /// Resumen compacto.
    /// </summary>
    public string Summary =>
        $"{EffectiveFileName}: {ValidChanges.Count} cambio(s) " +
        $"preparado(s) para escritura.";

    /// <summary>
    /// Nombre efectivo del archivo.
    /// </summary>
    public string EffectiveFileName =>
        !string.IsNullOrWhiteSpace(FileName)
            ? FileName.Trim()
            : Path.GetFileName(NormalizedFilePath);
}