using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Execution;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Diagnostics;

/// <summary>
/// Ejecuta una generación de huella controlada sobre un
/// archivo real y produce un informe legible.
///
/// No consulta AcoustID ni ninguna otra fuente externa.
/// </summary>
public sealed class ChromaprintFingerprintDiagnostics
{
    private readonly ChromaprintFingerprintExecutor
        _executor;

    public ChromaprintFingerprintDiagnostics()
        : this(
            new ChromaprintFingerprintExecutor(
                new ChromaprintOptions()))
    {
    }

    public ChromaprintFingerprintDiagnostics(
        ChromaprintFingerprintExecutor executor)
    {
        _executor =
            executor ??
            throw new ArgumentNullException(
                nameof(executor));
    }

    /// <summary>
    /// Genera la huella del archivo indicado y devuelve
    /// un informe con el resultado.
    /// </summary>
    public async Task<string> RunAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ChromaprintFingerprintRequest request =
            new()
            {
                FilePath =
                    filePath
            };

        ChromaprintFingerprintResult result =
            await _executor.ExecuteAsync(
                request,
                cancellationToken);

        return BuildReport(
            result);
    }

    private static string BuildReport(
        ChromaprintFingerprintResult result)
    {
        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de ChromaprintFingerprintExecutor ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Archivo: " +
            $"{result.FilePath}");

        builder.AppendLine(
            $"Estado: " +
            $"{result.Status}");

        builder.AppendLine(
            $"Duración informada: " +
            $"{result.Duration}");

        builder.AppendLine(
            $"Código de salida: " +
            $"{result.ExitCode?.ToString() ?? "(sin proceso)"}");

        builder.AppendLine(
            $"Huella: " +
            $"{(result.HasFingerprint ? result.Fingerprint : "(sin huella)")}");

        builder.AppendLine(
            $"Mensaje: " +
            $"{result.Message}");

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin del diagnóstico ===");

        return builder.ToString();
    }
}
