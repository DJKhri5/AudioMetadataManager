using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Interfaces;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Resolution;

/// <summary>
/// Contiene el resultado de buscar un escritor compatible con
/// una extensión de archivo.
/// </summary>
public sealed class MetadataWriterResolutionResult
{
    /// <summary>
    /// Extensión solicitada.
    /// </summary>
    public string Extension { get; init; } =
        string.Empty;

    /// <summary>
    /// Escritor seleccionado.
    /// </summary>
    public IMetadataFormatWriter? Writer { get; init; }

    /// <summary>
    /// Indica si se encontró un escritor compatible.
    /// </summary>
    public bool WasResolved =>
        Writer is not null;

    /// <summary>
    /// Nombre del escritor preparado para diagnóstico.
    /// </summary>
    public string WriterName =>
        Writer is null ||
        string.IsNullOrWhiteSpace(Writer.Name)
            ? "(escritor no disponible)"
            : Writer.Name.Trim();

    /// <summary>
    /// Resumen compacto.
    /// </summary>
    public string Summary =>
        WasResolved
            ? $"La extensión '{Extension}' será procesada " +
              $"por {WriterName}."
            : $"No existe un escritor compatible con la " +
              $"extensión '{Extension}'.";
}