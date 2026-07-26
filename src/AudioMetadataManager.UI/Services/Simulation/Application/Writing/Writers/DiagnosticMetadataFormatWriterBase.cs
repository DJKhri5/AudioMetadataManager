using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Interfaces;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

/// <summary>
/// Implementación base para escritores de diagnóstico.
///
/// Estos escritores no modifican archivos.
/// </summary>
public abstract class DiagnosticMetadataFormatWriterBase
    : IMetadataFormatWriter
{
    protected DiagnosticMetadataFormatWriterBase(
        string name,
        IEnumerable<string> supportedExtensions)
    {
        Name =
            name;

        SupportedExtensions =
            new HashSet<string>(
                supportedExtensions
                    .Select(
                        NormalizeExtension),
                StringComparer.OrdinalIgnoreCase);
    }

    public string Name { get; }

    public IReadOnlySet<string>
        SupportedExtensions
    { get; }

    public bool CanWrite(
        string extension)
    {
        return SupportedExtensions.Contains(
            NormalizeExtension(extension));
    }

    public Task<MetadataWriteResult> WriteAsync(
        MetadataWriteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<MetadataFieldWriteResult>
            fieldResults =
                request.ValidChanges
                    .Select(
                        change =>
                            new MetadataFieldWriteResult
                            {
                                Field =
                                    change.Field,

                                OriginalValue =
                                    change.OriginalValue,

                                RequestedValue =
                                    change.NewValue,

                                IsSupported =
                                    true,

                                ValuePrepared =
                                    false,

                                SaveSucceeded =
                                    false,

                                Message =
                                    "Escritor de diagnóstico: " +
                                    "ningún dato fue modificado."
                            })
                    .ToArray();

        MetadataWriteResult result =
            new()
            {
                WriteRequestId =
                    request.WriteRequestId,

                ApplyRequestId =
                    request.ApplyRequestId,

                PlanId =
                    request.PlanId,

                Status =
                    MetadataWriteStatus.NoWritableChanges,

                FilePath =
                    request.FilePath,

                WriterName =
                    Name,

                StartedAtUtc =
                    DateTimeOffset.UtcNow,

                CompletedAtUtc =
                    DateTimeOffset.UtcNow,

                ElapsedTime =
                    TimeSpan.Zero,

                FieldResults =
                    fieldResults,

                Messages =
                    new[]
                    {
                        "El escritor fue resuelto correctamente.",
                        "La implementación actual es solamente " +
                        "de diagnóstico.",
                        "Ningún archivo fue modificado."
                    }
            };

        return Task.FromResult(
            result);
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