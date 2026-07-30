using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Diagnostics;

/// <summary>
/// Ejecuta una identificación controlada contra AcoustID y
/// produce un informe legible.
/// </summary>
public sealed class AcoustIdLookupDiagnostics
{
    private readonly AcoustIdLookupProvider
        _provider;

    public AcoustIdLookupDiagnostics()
        : this(
            new AcoustIdLookupProvider(
                AcoustIdOptionsFactory.CreateDefault()))
    {
    }

    public AcoustIdLookupDiagnostics(
        AcoustIdLookupProvider provider)
    {
        _provider =
            provider ??
            throw new ArgumentNullException(
                nameof(provider));
    }

    /// <summary>
    /// Identifica la huella indicada y devuelve un informe
    /// con las grabaciones encontradas.
    /// </summary>
    public async Task<string> RunAsync(
        string fingerprint,
        int durationSeconds,
        CancellationToken cancellationToken = default)
    {
        AcoustIdLookupRequest request =
            new()
            {
                Fingerprint =
                    fingerprint,

                DurationSeconds =
                    durationSeconds
            };

        AcoustIdLookupResult result =
            await _provider.LookupAsync(
                request,
                cancellationToken);

        return BuildReport(
            result);
    }

    private static string BuildReport(
        AcoustIdLookupResult result)
    {
        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de AcoustIdLookupProvider ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Consulta: " +
            $"{result.Request?.SearchDisplay ?? "(sin solicitud)"}");

        builder.AppendLine(
            $"Estado: " +
            $"{result.Status}");

        builder.AppendLine(
            $"Mensaje: " +
            $"{result.Message}");

        builder.AppendLine(
            $"Grabaciones encontradas: " +
            $"{result.Candidates.Count}");

        builder.AppendLine();

        foreach (AcoustIdRecordingCandidate candidate in result.Candidates)
        {
            builder.AppendLine(
                $"- {candidate.DisplayName} " +
                $"(MBID: {candidate.RecordingId}, " +
                $"confianza: {candidate.Score * 100:0.0}%)");
        }

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin del diagnóstico ===");

        return builder.ToString();
    }
}
