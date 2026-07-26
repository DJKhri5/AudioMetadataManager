using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Interfaces;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Resolution;

/// <summary>
/// Mantiene el registro de escritores disponibles y selecciona
/// el escritor compatible con una extensión.
/// </summary>
public sealed class MetadataWriterResolver
{
    private readonly IReadOnlyList<IMetadataFormatWriter>
        _writers;

    /// <summary>
    /// Crea el resolutor con la colección indicada.
    /// </summary>
    public MetadataWriterResolver(
        IEnumerable<IMetadataFormatWriter> writers)
    {
        ArgumentNullException.ThrowIfNull(
            writers);

        _writers =
            writers
                .Where(writer => writer is not null)
                .ToArray();
    }

    /// <summary>
    /// Escritores registrados.
    /// </summary>
    public IReadOnlyList<IMetadataFormatWriter>
        Writers =>
            _writers;

    /// <summary>
    /// Busca el escritor compatible con la extensión.
    /// </summary>
    public MetadataWriterResolutionResult Resolve(
        string? extension)
    {
        string normalizedExtension =
            NormalizeExtension(
                extension);

        IMetadataFormatWriter? writer =
            _writers.FirstOrDefault(
                candidate =>
                    candidate.CanWrite(
                        normalizedExtension));

        return new MetadataWriterResolutionResult
        {
            Extension =
                normalizedExtension,

            Writer =
                writer
        };
    }

    private static string NormalizeExtension(
        string? extension)
    {
        if (string.IsNullOrWhiteSpace(
                extension))
        {
            return string.Empty;
        }

        string normalized =
            extension.Trim();

        if (!normalized.StartsWith(
                '.'))
        {
            normalized =
                "." + normalized;
        }

        return normalized.ToLowerInvariant();
    }
}