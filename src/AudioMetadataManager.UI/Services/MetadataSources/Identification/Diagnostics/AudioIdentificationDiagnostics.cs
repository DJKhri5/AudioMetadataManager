using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources
    .Identification.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Identification.Diagnostics;

/// <summary>
/// Ejecuta una identificación controlada de extremo a extremo
/// (Chromaprint seguido de AcoustID) contra un archivo real y
/// produce un informe legible.
/// </summary>
public sealed class AudioIdentificationDiagnostics
{
    private readonly AudioIdentificationOrchestrator
        _orchestrator;

    public AudioIdentificationDiagnostics()
        : this(
            new AudioIdentificationOrchestrator())
    {
    }

    public AudioIdentificationDiagnostics(
        AudioIdentificationOrchestrator orchestrator)
    {
        _orchestrator =
            orchestrator ??
            throw new ArgumentNullException(
                nameof(orchestrator));
    }

    /// <summary>
    /// Identifica el archivo indicado y devuelve un informe con
    /// el resultado de cada etapa.
    /// </summary>
    public async Task<string> RunAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        AudioIdentificationResult result =
            await _orchestrator.IdentifyAsync(
                new AudioIdentificationRequest
                {
                    FilePath =
                        filePath
                },
                cancellationToken);

        return BuildReport(
            result);
    }

    private static string BuildReport(
        AudioIdentificationResult result)
    {
        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de AudioIdentificationOrchestrator ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Archivo: " +
            $"{result.FilePath}");

        builder.AppendLine(
            $"Estado general: " +
            $"{result.Status}");

        builder.AppendLine(
            $"Mensaje: " +
            $"{result.Message}");

        builder.AppendLine();

        if (result.FingerprintResult is not null)
        {
            builder.AppendLine(
                "--- Chromaprint ---");

            builder.AppendLine(
                $"Estado: " +
                $"{result.FingerprintResult.Status}");

            builder.AppendLine(
                $"Duración informada: " +
                $"{result.FingerprintResult.Duration}");

            builder.AppendLine(
                $"Huella: " +
                $"{(result.FingerprintResult.HasFingerprint ? result.FingerprintResult.Fingerprint : "(sin huella)")}");

            builder.AppendLine();
        }

        if (result.LookupResult is not null)
        {
            builder.AppendLine(
                "--- AcoustID ---");

            builder.AppendLine(
                $"Estado: " +
                $"{result.LookupResult.Status}");

            builder.AppendLine(
                $"Grabaciones encontradas: " +
                $"{result.LookupResult.Candidates.Count}");

            foreach (AcoustIdRecordingCandidate candidate
                in result.LookupResult.Candidates)
            {
                builder.AppendLine(
                    $"- {candidate.DisplayName} " +
                    $"(MBID: {candidate.RecordingId}, " +
                    $"confianza: {candidate.Score * 100:0.0}%)");
            }

            builder.AppendLine();
        }

        builder.AppendLine(
            "=== Fin del diagnóstico ===");

        return builder.ToString();
    }
}
