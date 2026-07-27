using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Interfaces;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Resolution;

/// <summary>
/// Mantiene el registro de escritores disponibles y selecciona
/// el escritor compatible con una extensión.
///
/// Cuando existen varios escritores para el mismo formato, se
/// prioriza explícitamente el escritor real sobre el
/// diagnóstico.
/// </summary>
public sealed class MetadataWriterResolver
{
    private readonly IReadOnlyList<IMetadataFormatWriter>
        _writers;

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

    public IReadOnlyList<IMetadataFormatWriter>
        Writers =>
            _writers;

    public MetadataWriterResolutionResult Resolve(
        string? extension)
    {
        string normalizedExtension =
            NormalizeExtension(
                extension);

        IMetadataFormatWriter? writer =
            _writers
                .Where(candidate =>
                    candidate.CanWrite(
                        normalizedExtension))
                .OrderByDescending(
                    GetResolutionPriority)
                .ThenByDescending(
                    GetWriterKindPriority)
                .FirstOrDefault();

        return new MetadataWriterResolutionResult
        {
            Extension =
                normalizedExtension,

            Writer =
                writer
        };
    }

    private static int GetResolutionPriority(
        IMetadataFormatWriter writer)
    {
        return writer is IMetadataWriterDescriptor descriptor
            ? descriptor.ResolutionPriority
            : 0;
    }

    private static int GetWriterKindPriority(
        IMetadataFormatWriter writer)
    {
        if (writer is not IMetadataWriterDescriptor descriptor)
        {
            return 0;
        }

        return descriptor.WriterKind switch
        {
            MetadataWriterKind.Real =>
                2,

            MetadataWriterKind.Diagnostic =>
                1,

            _ =>
                0
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

        if (!normalized.StartsWith('.'))
        {
            normalized =
                "." + normalized;
        }

        return normalized.ToLowerInvariant();
    }
}